using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

public class AnimatorKeyboardController : MonoBehaviour
{
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private Rig headRig;
    [SerializeField] private float clapDuration = 5.0f;
    
    // Added variables for animation smoothing
    [SerializeField] private Transform destination;
    private float _rotationSpeed = 1f;
    private float _strafeSpeed = 0.35f;
    private float _dampingTime = 0.1f;

    private Animator _animator;
    private NavMeshAgent _agent;
    private AnimatorStateInfo _currentState;
    private bool _isPerformingAction;

    // Animator parameter hashes
    private static readonly int VelocityXHash = Animator.StringToHash("Velocity X");
    private static readonly int VelocityZHash = Animator.StringToHash("Velocity Z");
    private static readonly int FlipTriggerHash = Animator.StringToHash("FlipTrigger");
    private static readonly int LayTriggerHash = Animator.StringToHash("LayTrigger");
    private static readonly int UpTriggerHash = Animator.StringToHash("UpTrigger");
    private static readonly int AngryTriggerHash = Animator.StringToHash("AngryTrigger");
    private static readonly int DanceTriggerHash = Animator.StringToHash("DanceTrigger");
    private static readonly int SurpriseTriggerHash = Animator.StringToHash("SurpriseTrigger");
    private static readonly int IsClappingHash = Animator.StringToHash("IsClapping");

    // Animator state names
    private const string BlendTreeStateName = "2D Blend Tree";
    private const string LayingDownStateName = "Laying Down";
    private const string ClappingStateName = "Clapping";
    private const string DancingStateName = "Dancing";
    private const string FlipStateName = "Flip";

    private Coroutine _clappingCoroutine;
    
    private enum EmotionType
    {
        Happy,
        Upset,
        Surprised,
        Supportive,
        Cheerful,
        Angry
    }

    // Mapping of keys to emotions
    private readonly Dictionary<KeyCode, EmotionType> _keyEmotionMap = new Dictionary<KeyCode, EmotionType>
    {
        { KeyCode.Space, EmotionType.Happy },
        { KeyCode.Z, EmotionType.Upset },
        { KeyCode.X, EmotionType.Surprised },
        { KeyCode.C, EmotionType.Supportive },
        { KeyCode.V, EmotionType.Cheerful },
        { KeyCode.B, EmotionType.Angry },
    };
    

    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        // Assign components if not set in the Inspector
        if (emotionController == null)
        {
            emotionController = GetComponentInChildren<EmotionController>();
        }

        if (headRig == null)
        {
            headRig = GetComponentInChildren<Rig>();
        }

        // Disable automatic rotation of NavMeshAgent
        if (_agent != null)
        {
            _agent.updateRotation = false;
        }

        // Assign destination if not set
        if (destination == null)
        {
            NavMeshAIController aiController = GetComponent<NavMeshAIController>();
            if (aiController != null)
            {
                destination = aiController.debugDestination;
            }
        }
    }

    void Update()
    {
        _currentState = _animator.GetCurrentAnimatorStateInfo(0);
        _isPerformingAction = !_currentState.IsName(BlendTreeStateName);

        UpdateEmotion(_isPerformingAction);
        UpdateBlendTree(_isPerformingAction);
        HandleLayDownCompletion();
        ControlNavMeshAgent(_isPerformingAction);
    }

    private void UpdateEmotion(bool isPerformingAction)
    {
        if (isPerformingAction)
        {
            HandleOngoingEmotion();
            return;
        }
        
        foreach (var keyEmotionPair in _keyEmotionMap)
        {
            if (Input.GetKeyDown(keyEmotionPair.Key))
            {
                TriggerEmotion(keyEmotionPair.Value);
                break;
            }
        }
    }

    private void HandleOngoingEmotion()
    {
        // Special: Clapping is in loop
        if (_currentState.IsName(ClappingStateName) && !_animator.IsInTransition(0))
        {
            if (!_animator.GetBool(IsClappingHash))
            {
                emotionController.Default();
            }
        }
        // Special: Exit time = 0.6
        else if (_currentState.IsName(FlipStateName) && _currentState.normalizedTime >= 0.58f && !_animator.IsInTransition(0))
        {
            emotionController.Default();
        }
        // Other emotions
        else if (_currentState.normalizedTime >= 0.9f && !_animator.IsInTransition(0))
        {
            if (_currentState.IsName(DancingStateName))
            {
                headRig.weight = 1;
            }

            emotionController.Default();
        }
    }

    private void TriggerEmotion(EmotionType emotion)
    {
        switch (emotion)
        {
            case EmotionType.Happy:
                emotionController.Happy();
                _animator.SetTrigger(FlipTriggerHash);
                ResetVelocity();
                break;
            case EmotionType.Upset:
                emotionController.Upset();
                _animator.SetTrigger(LayTriggerHash);
                ResetVelocity();
                break;
            case EmotionType.Surprised:
                emotionController.Surprised();
                _animator.SetTrigger(SurpriseTriggerHash);
                ResetVelocity();
                break;
            case EmotionType.Supportive:
                emotionController.Supportive();
                StartClapping();
                break;
            case EmotionType.Cheerful:
                headRig.weight = 0;
                emotionController.Cheerful();
                _animator.SetTrigger(DanceTriggerHash);
                ResetVelocity();
                break;
            case EmotionType.Angry:
                emotionController.Angry();
                _animator.SetTrigger(AngryTriggerHash);
                ResetVelocity();
                break;
        }
    }

    private void StartClapping()
    {
        if (_clappingCoroutine != null)
            StopCoroutine(_clappingCoroutine);

        _animator.SetBool(IsClappingHash, true);
        _clappingCoroutine = StartCoroutine(ClappingRoutine());
    }

    private IEnumerator ClappingRoutine()
    {
        yield return new WaitForSeconds(clapDuration);
        _animator.SetBool(IsClappingHash, false);
        _clappingCoroutine = null;
    }

    private void UpdateBlendTree(bool isPerformingAction)
    {
        //float dampingTime = 0.1f; // Smooth transitions
        float velocityThreshold = 0.05f; // Threshold to prevent twitching

        if (_agent != null && !_agent.isStopped && _agent.remainingDistance > _agent.stoppingDistance && !isPerformingAction)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);

            Vector3 directionToTarget = destination.position - transform.position;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            float angleThreshold = 100f; // Angle to determine if target is behind

            if (angleToTarget > angleThreshold)
            {
                // Strafe to turn towards the target
                Vector3 cross = Vector3.Cross(transform.forward, directionToTarget.normalized);
                float direction = cross.y > 0 ? 1f : -1f; // 1 for right, -1 for left

                _animator.SetFloat(VelocityXHash, direction * _strafeSpeed, _dampingTime, Time.deltaTime);
                _animator.SetFloat(VelocityZHash, 0f, _dampingTime, Time.deltaTime);

                // Rotate the character
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            }
            else
            {
                // Apply threshold to prevent twitching
                float velocityX = Mathf.Abs(localVelocity.x) > velocityThreshold ? localVelocity.x : 0f;
                float velocityZ = Mathf.Abs(localVelocity.z) > velocityThreshold ? localVelocity.z : 0f;

                _animator.SetFloat(VelocityXHash, velocityX, _dampingTime, Time.deltaTime);
                _animator.SetFloat(VelocityZHash, velocityZ, _dampingTime, Time.deltaTime);

                // Rotate character to move direction
                if (localVelocity != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_agent.velocity.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
                }
            }
        }
        else if (!isPerformingAction)
        {
            ResetVelocity();
        }
    }

    private void HandleLayDownCompletion()
    {
        if (_currentState.IsName(LayingDownStateName) && _currentState.normalizedTime >= 1.0f && !_animator.IsInTransition(0))
        {
            _animator.SetTrigger(UpTriggerHash);
        }
    }

    private void ControlNavMeshAgent(bool isPerformingAction)
    {
        if (_agent != null)
        {
            _agent.isStopped = isPerformingAction;
        }
    }

    private void ResetVelocity()
    {
        _animator.SetFloat(VelocityXHash, 0f, _dampingTime, Time.deltaTime);
        _animator.SetFloat(VelocityZHash, 0f, _dampingTime, Time.deltaTime);
    }
}

using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Animations.Rigging;

public class AnimatorAIController : MonoBehaviour
{
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private Rig headRig;
    [SerializeField] private float clapDuration = 5.0f;

    private Animator _animator;
    private NavMeshAgent _agent;
    private AnimatorStateInfo _currentState;
    private bool _isPerformingAction;
    private Coroutine _clappingCoroutine;

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
    private const string FlipStateName = "Flip"; // Added FlipStateName
    
    private enum EmotionType
    {
        Happy,
        Upset,
        Surprised,
        Supportive,
        Cheerful,
        Angry
    }
    
    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        if (emotionController == null)
        {
            emotionController = GetComponentInChildren<EmotionController>();
        }

        if (headRig == null)
        {
            headRig = GetComponentInChildren<Rig>();
        }
    }

    void Update()
    {
        _currentState = _animator.GetCurrentAnimatorStateInfo(0);
        _isPerformingAction = !_currentState.IsName(BlendTreeStateName);

        HandleOngoingEmotion();
        HandleLayDownCompletion();

        if (!_isPerformingAction)
        {
            UpdateBlendTree();
        }
        else
        {
            ResetVelocity();
        }
    }

    // Public emotion functions to be called by PetManager
    public void Default()
    {
        emotionController.Default();

        // Ensure animator returns to 2D Blend Tree
        if (!_currentState.IsName(BlendTreeStateName))
        {
            _animator.Play(BlendTreeStateName);
        }

        // Ensure NavMeshAgent can move
        if (_agent != null)
        {
            _agent.isStopped = false;
        }
    }

    public void Happy()
    {
        TriggerEmotion(EmotionType.Happy);
    }

    public void Upset()
    {
        TriggerEmotion(EmotionType.Upset);
    }

    public void Surprised()
    {
        TriggerEmotion(EmotionType.Surprised);
    }

    public void Supportive()
    {
        TriggerEmotion(EmotionType.Supportive);
    }

    public void Cheerful()
    {
        TriggerEmotion(EmotionType.Cheerful);
    }

    public void Angry()
    {
        TriggerEmotion(EmotionType.Angry);
    }

    private void TriggerEmotion(EmotionType emotion)
    {
        // Stop the NavMeshAgent when performing an action
        if (_agent != null)
        {
            _agent.isStopped = true;
        }

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
                if (headRig != null)
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
        emotionController.Default();
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
        else if (_currentState.normalizedTime >= 0.9f && !_animator.IsInTransition(0))
        {
            if (_currentState.IsName(DancingStateName) && headRig != null)
            {
                headRig.weight = 1;
            }

            emotionController.Default();
        }
    }

    private void HandleLayDownCompletion()
    {
        if (_currentState.IsName(LayingDownStateName) && _currentState.normalizedTime >= 1.0f && !_animator.IsInTransition(0))
        {
            _animator.SetTrigger(UpTriggerHash);
        }
    }

    private void UpdateBlendTree()
    {
        if (_agent != null && !_agent.isStopped && _agent.remainingDistance > _agent.stoppingDistance)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);
            _animator.SetFloat(VelocityXHash, localVelocity.x);
            _animator.SetFloat(VelocityZHash, localVelocity.z);
        }
        else
        {
            ResetVelocity();
        }
    }

    private void ResetVelocity()
    {
        _animator.SetFloat(VelocityXHash, 0f);
        _animator.SetFloat(VelocityZHash, 0f);
    }
}

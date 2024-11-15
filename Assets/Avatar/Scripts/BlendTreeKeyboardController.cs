using UnityEngine;
using UnityEngine.AI;

public class BlendTreeKeyboardController : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    
    private static readonly int VelocityXHash = Animator.StringToHash("Velocity X");
    private static readonly int VelocityZHash = Animator.StringToHash("Velocity Z");
    private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int IsLayingHash = Animator.StringToHash("isLaying");
    private static readonly int IsGettingUpHash = Animator.StringToHash("isUp");
    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        bool isJumping = _animator.GetBool(IsJumpingHash);
        bool isLaying = _animator.GetBool(IsLayingHash);
        bool isGettingUp = _animator.GetBool(IsGettingUpHash);
        bool isPerformingAction = isJumping || isLaying || isGettingUp;
        
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        HandleInput(isJumping, isLaying, isGettingUp);
        UpdateAnimatorParameters(isPerformingAction);
        HandleFlipCompletion(isJumping, stateInfo);
        HandleLayDownCompletion(isLaying, isGettingUp, stateInfo);
        ControlNavMeshAgent(isPerformingAction);
    }
    
    private void HandleInput(bool isJumping, bool isLaying, bool isGettingUp)
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartFlip();
        }

        if (Input.GetKeyDown(KeyCode.H) && !isLaying && !isGettingUp)
        {
            StartLayDown();
        }
    }
    
    private void UpdateAnimatorParameters(bool isPerformingAction)
    {
        if (_agent != null && !_agent.isStopped && _agent.remainingDistance > _agent.stoppingDistance && !isPerformingAction)
        {
            // Update velocities from NavMeshAgent
            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);

            // Update animator parameters
            _animator.SetFloat(VelocityXHash, localVelocity.x);
            _animator.SetFloat(VelocityZHash, localVelocity.z);
        }
        else if (!isPerformingAction)
        {
            // Reset velocities if agent is not moving and no action is being performed
            ResetVelocity();
        }
    }
    
    private void StartFlip()
    {
        _animator.SetBool(IsJumpingHash, true);
        ResetVelocity(); // Ensure velocity is zero before the flip
    }
    
    private void HandleFlipCompletion(bool isJumping, AnimatorStateInfo stateInfo)
    {
        if (isJumping && stateInfo.IsName("Flip") && stateInfo.normalizedTime >= 1.0f)
        {
            _animator.SetBool(IsJumpingHash, false);
        }
    }
    
    private void StartLayDown()
    {
        _animator.SetBool(IsLayingHash, true);
        ResetVelocity(); // Ensure velocity is zero before laying down
    }
    
    private void HandleLayDownCompletion(bool isLaying, bool isGettingUp, AnimatorStateInfo stateInfo)
    {
        if (isLaying && stateInfo.IsName("Laying Down") && stateInfo.normalizedTime >= 1.0f)
        {
            _animator.SetBool(IsLayingHash, false);
            _animator.SetBool(IsGettingUpHash, true);
        }

        if (isGettingUp && stateInfo.IsName("Getting Up") && stateInfo.normalizedTime >= 1.0f)
        {
            _animator.SetBool(IsGettingUpHash, false);
        }
    }
    
    private void ControlNavMeshAgent(bool isPerformingAction)
    {
        if (_agent == null) return;

        _agent.isStopped = isPerformingAction;
    }
    
    private void ResetVelocity()
    {
        _animator.SetFloat(VelocityXHash, 0f);
        _animator.SetFloat(VelocityZHash, 0f);
    }

    // These methods can be expanded or removed based on your needs
    public void PositiveReaction()
    {
        ResetVelocity();
    }

    public void NegativeReaction()
    {
        ResetVelocity();
    }
}

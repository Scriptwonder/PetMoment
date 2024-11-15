using UnityEngine;
using UnityEngine.AI;

public class NavMeshAIController : MonoBehaviour
{
    public Transform destination;
    
    [SerializeField]
    private float stoppingOffset = 5.0f;
    
    private NavMeshAgent _agent;
    private Animator _animator;
    
    private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int IsLayingHash = Animator.StringToHash("isLaying");
    private static readonly int IsGettingUpHash = Animator.StringToHash("isUp");

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (destination == null || _agent == null || _animator == null)
            return;

        // Set the agent's destination
        _agent.destination = destination.position;

        // Check if the character is performing animations that should stop movement
        bool isPerformingAction = _animator.GetBool(IsJumpingHash) ||
                                  _animator.GetBool(IsLayingHash) ||
                                  _animator.GetBool(IsGettingUpHash);

        // Determine whether to stop the agent
        if (isPerformingAction || IsNearDestination())
        {
            _agent.isStopped = true; // Stop NavMeshAgent
        }
        else
        {
            _agent.isStopped = false; // Resume NavMeshAgent
        }
    }
    
    private bool IsNearDestination()
    {
        if (_agent.pathPending)
            return false;

        float distanceToTarget = Vector3.Distance(_agent.transform.position, destination.position);
        return distanceToTarget <= (_agent.stoppingDistance + stoppingOffset);
    }
}

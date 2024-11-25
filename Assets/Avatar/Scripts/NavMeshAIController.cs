using UnityEngine;
using UnityEngine.AI;

public class NavMeshAIController : MonoBehaviour
{
    public Transform destination;

    [SerializeField]
    private float stoppingOffset = 5.0f;

    private NavMeshAgent _agent;
    private Animator _animator;

    // Animator state names
    private const string FlipStateName = "Flip";
    private const string LayingDownStateName = "Laying Down";
    private const string GettingUpStateName = "Getting Up";
    private const string SurprisedStateName = "Surprised";
    private const string AngryStateName = "Angry";
    private const string DancingStateName = "Dancing";
    private const string ClappingStateName = "Clapping";

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
        bool isPerformingAction = IsAnyActionActive();

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

    /// <summary>
    /// Checks if any action is currently active based on Animator states.
    /// </summary>
    private bool IsAnyActionActive()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName(FlipStateName) ||
               _animator.GetCurrentAnimatorStateInfo(0).IsName(LayingDownStateName) ||
               _animator.GetCurrentAnimatorStateInfo(0).IsName(GettingUpStateName) ||
               _animator.GetCurrentAnimatorStateInfo(0).IsName(SurprisedStateName) ||
               _animator.GetCurrentAnimatorStateInfo(0).IsName(AngryStateName) ||
               _animator.GetCurrentAnimatorStateInfo(0).IsName(DancingStateName) ||
               _animator.GetCurrentAnimatorStateInfo(0).IsName(ClappingStateName);
    }

    /// <summary>
    /// Determines if the agent is near the destination.
    /// </summary>
    private bool IsNearDestination()
    {
        if (_agent.pathPending)
            return false;

        float distanceToTarget = Vector3.Distance(_agent.transform.position, destination.position);
        return distanceToTarget <= (_agent.stoppingDistance + stoppingOffset);
    }
}

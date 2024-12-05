using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class NavMeshAIController : MonoBehaviour
{
    public Transform debugDestination; // only used in AnimatorKeyboardController.cs for debug
    [SerializeField] private Transform destination;

    //[SerializeField]
    private float stoppingOffset = 0.5f;

    private NavMeshAgent _agent;
    private Animator _animator;

    // Animator parameter hash
    private static readonly int IsNearTargetHash = Animator.StringToHash("IsNearTarget");

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

        // Disable automatic rotation
        if (_agent != null)
        {
            _agent.updateRotation = false;
        }

        StartCoroutine(AssignHeadAimTarget());
    }

    private IEnumerator AssignHeadAimTarget()
    {
        while (Camera.main == null)
        {
            yield return null;
        }
        
        destination = Camera.main.transform;
        Debug.Log("Main Camera assigned - NavMeshAIController");
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
        bool nearDestination = IsNearDestination();

        // Update animator parameter
        _animator.SetBool(IsNearTargetHash, nearDestination);

        if (isPerformingAction || nearDestination)
        {
            _agent.isStopped = true; // Stop NavMeshAgent
            _agent.velocity = Vector3.zero; // Ensure velocity is zero
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
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsName(FlipStateName) ||
               stateInfo.IsName(LayingDownStateName) ||
               stateInfo.IsName(GettingUpStateName) ||
               stateInfo.IsName(SurprisedStateName) ||
               stateInfo.IsName(AngryStateName) ||
               stateInfo.IsName(DancingStateName) ||
               stateInfo.IsName(ClappingStateName);
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

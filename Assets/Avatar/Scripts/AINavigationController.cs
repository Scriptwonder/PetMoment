using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class AINavigationController : MonoBehaviour
{
    public Transform destination;
    private NavMeshAgent _agent;
    public float stoppingOffset = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (destination != null)
        {
            _agent.destination = destination.position;

            // "player" destination to destory itself when the character arrives with an variable offset
            float distanceToTarget = Vector3.Distance(transform.position, destination.position);
            if (distanceToTarget <= _agent.stoppingDistance + stoppingOffset)
            {
                Destroy(destination.gameObject);
                destination = null;
            }
        }
    }
}
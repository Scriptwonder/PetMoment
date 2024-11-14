using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBlendTreeController : MonoBehaviour
{
    Animator _animator;
    NavMeshAgent _agent;

    int _velocityXHash;
    int _velocityZHash;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        _velocityXHash = Animator.StringToHash("Velocity X");
        _velocityZHash = Animator.StringToHash("Velocity Z");
    }

    public void resetVelocity()
    {
        _animator.SetFloat(_velocityXHash, 0f);
        _animator.SetFloat(_velocityZHash, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_agent != null && _agent.remainingDistance > _agent.stoppingDistance)
        {
            // Update velocity from NavMeshAgent
            Vector3 agentVelocity = transform.InverseTransformDirection(_agent.velocity);
            float velocityX = agentVelocity.x;
            float velocityZ = agentVelocity.z;

            // Update animator
            _animator.SetFloat(_velocityXHash, velocityX);
            _animator.SetFloat(_velocityZHash, velocityZ);
        }
        else
        {
            // Reset animator values to zero if agent is not moving
            _animator.SetFloat(_velocityXHash, 0f);
            _animator.SetFloat(_velocityZHash, 0f);
        }
    }
}
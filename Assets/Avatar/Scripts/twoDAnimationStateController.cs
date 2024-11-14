using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoDAnimationStateController : MonoBehaviour
{
    Animator _animator;
    
    float _velocityX = 0.0f;
    float _velocityZ = 0.0f;
    int _velocityXHash;
    int _velocityZHash;
    
    public float acceleration = 2.0f;
    public float deceleration = 4.0f;
    public float maxminWalkVelocity = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();

        _velocityXHash = Animator.StringToHash("Velocity X");
        _velocityZHash = Animator.StringToHash("Velocity Z");
    }

    void ChangeVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, float maxVelocity)
    {
        if (forwardPressed && _velocityZ < maxVelocity)
        {
            _velocityZ += Time.deltaTime * acceleration;
        }

        if (leftPressed && _velocityX > -maxVelocity)
        {
            _velocityX -= Time.deltaTime * acceleration;
        }

        if (rightPressed && _velocityX < maxVelocity)
        {
            _velocityX += Time.deltaTime * acceleration;
        }

        if (!forwardPressed && _velocityZ > 0.0f)
        {
            _velocityZ -= Time.deltaTime * deceleration;
        }

        if (!leftPressed && _velocityX < 0.0f)
        {
            _velocityX += Time.deltaTime * deceleration;
        }

        if (!rightPressed && _velocityX > 0.0f)
        {
            _velocityX -= Time.deltaTime * deceleration;
        }
    }

    void LockOrResetVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, float maxVelocity)
    {
        // Lock forward movement
        if (forwardPressed && _velocityZ > maxVelocity)
        {
            _velocityZ = maxVelocity;
        }
        else if (forwardPressed && _velocityZ < maxVelocity && _velocityZ > (maxVelocity - 0.05f))
        {
            _velocityZ = maxVelocity;
        }

        // Lock left movement
        if (leftPressed && _velocityX < -maxVelocity)
        {
            _velocityX = -maxVelocity;
        }
        else if (leftPressed && _velocityX > -maxVelocity && _velocityX < (-maxVelocity + 0.05f))
        {
            _velocityX = -maxVelocity;
        }

        // Lock right movement
        if (rightPressed && _velocityX > maxVelocity)
        {
            _velocityX = maxVelocity;
        }
        else if (rightPressed && _velocityX < maxVelocity && _velocityX > (maxVelocity - 0.05f))
        {
            _velocityX = maxVelocity;
        }

        // Reset velocityZ if not moving forward
        if (!forwardPressed && Mathf.Abs(_velocityZ) < 0.05f)
        {
            _velocityZ = 0.0f;
        }

        // Reset velocityX if neither left nor right is pressed
        if (!leftPressed && !rightPressed && Mathf.Abs(_velocityX) < 0.05f)
        {
            _velocityX = 0.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Keyboard Input System
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);

        ChangeVelocity(forwardPressed, leftPressed, rightPressed, maxminWalkVelocity);
        LockOrResetVelocity(forwardPressed, leftPressed, rightPressed, maxminWalkVelocity);

        _animator.SetFloat(_velocityXHash, _velocityX);
        _animator.SetFloat(_velocityZHash, _velocityZ);
    }
}

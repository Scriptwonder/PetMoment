using UnityEngine;

public class TargetController : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        // Capture WASD inputs
        if (Input.GetKey(KeyCode.W))
        {
            moveVertical += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVertical -= 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveHorizontal -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveHorizontal += 1f;
        }

        // Normalize movement vector to prevent faster diagonal movement
        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical).normalized * speed * Time.deltaTime;

        // Move the sphere in world space
        transform.Translate(movement, Space.World);
    }
}
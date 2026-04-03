using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float pushForce = 8f;        // strength of each push
    public float maxSpeed = 6f;         
    public float friction = 4f;         // how fast you slow down
    public float pushCooldown = 0.4f;   // time between pushes

    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    private Vector3 velocity;
    private float pushTimer;

    void Update()
    {
        pushTimer -= Time.deltaTime;

        if (IsGrounded())
        {
            HandlePushInput();
        }

        ApplyFriction();
        Move();
    }

    void HandlePushInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && pushTimer <= 0f)
        {
            Vector3 forward = transform.forward;
            forward.y = 0;

            velocity += forward.normalized * pushForce;

            // clamp speed
            if (velocity.magnitude > maxSpeed)
                velocity = velocity.normalized * maxSpeed;

            pushTimer = pushCooldown;
        }
    }

    void ApplyFriction()
    {
        velocity = Vector3.Lerp(velocity, Vector3.zero, friction * Time.deltaTime);
    }

    void Move()
    {
        transform.position += velocity * Time.deltaTime;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
}
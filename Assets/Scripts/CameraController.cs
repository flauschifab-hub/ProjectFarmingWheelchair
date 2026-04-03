using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;            
    public LayerMask groundLayer;            
    public float groundCheckDistance = 0.1f;  

    void Update()
    {
        if (IsGrounded())
        {
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 forward = transform.forward; 
                forward.y = 0; 
                transform.position += forward.normalized * moveSpeed * Time.deltaTime;
            }
        }
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
}
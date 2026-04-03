using UnityEngine;

public class WheelChairController : MonoBehaviour
{
    [Header("Movement")]
    public float maxForwardSpeed = 5f;
    public float forwardAcceleration = 3f;
    public float deceleration = 2f;
    public float turnSpeed = 90f; 
    public float turnLerpSpeed = 5f; 
    public float driftFactor = 0.1f;
    public float maxForwardHoldTime = 3f;

    [Header("Collision")]
    public Rigidbody cameraRigidbody; 
    public float collisionForceMultiplier = 0.5f; 
    public float flingMultiplier = 2f;           
    public float rotationTorque = 5f;
    public float xResetDelay = 3f;
    public float xResetSpeed = 90f;

    [Header("Camera Look")]
    public float lookSensitivity = 2f;
    private Transform cameraTransform;
    private float pitch = 0f;
    private float yaw = 0f;

    private Rigidbody rb;
    private float currentForwardSpeed = 0f;
    private float targetTurnDirection = 0f; 
    private float currentTurnSpeed = 0f;
    private float forwardHoldTimer = 0f;
    private bool controlEnabled = true;

    private float xResetTimer = 0f;
    private bool startXReset = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        if (cameraRigidbody != null)
            cameraRigidbody.isKinematic = true;

        if (cameraRigidbody != null && cameraRigidbody.transform.IsChildOf(transform))
        {
            cameraTransform = cameraRigidbody.transform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw = cameraTransform.eulerAngles.y;
            pitch = cameraTransform.localEulerAngles.x;
        }
    }

    void FixedUpdate()
    {
        if (!controlEnabled) return;

        bool pressingLeft = Input.GetKey(KeyCode.A);
        bool pressingRight = Input.GetKey(KeyCode.D);

        if (pressingLeft && pressingRight)
        {
            forwardHoldTimer += Time.fixedDeltaTime;
            if (forwardHoldTimer <= maxForwardHoldTime)
            {
                currentForwardSpeed += forwardAcceleration * Time.fixedDeltaTime;
                currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, 0f, maxForwardSpeed);
            }
            else
            {
                currentForwardSpeed -= deceleration * Time.fixedDeltaTime;
                if (currentForwardSpeed < 0f) currentForwardSpeed = 0f;
            }
            targetTurnDirection = 0f;
        }
        else
        {
            forwardHoldTimer = 0f; 
            if (pressingLeft && !pressingRight)
            {
                targetTurnDirection = -1f;
                currentForwardSpeed *= 0.95f; 
            }
            else if (pressingRight && !pressingLeft)
            {
                targetTurnDirection = 1f;
                currentForwardSpeed *= 0.95f;
            }
            else
            {
                targetTurnDirection = 0f;
                currentForwardSpeed -= deceleration * Time.fixedDeltaTime;
                if (currentForwardSpeed < 0f) currentForwardSpeed = 0f;
            }
        }

        float targetTurnSpeed = targetTurnDirection * turnSpeed;
        currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, targetTurnSpeed, turnLerpSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, currentTurnSpeed * Time.fixedDeltaTime, 0f));

        Vector3 forwardVelocity = transform.forward * currentForwardSpeed;
        Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right) * driftFactor;
        rb.velocity = new Vector3(forwardVelocity.x + lateralVelocity.x, rb.velocity.y, forwardVelocity.z + lateralVelocity.z);

        if (startXReset && cameraRigidbody != null)
        {
            xResetTimer += Time.fixedDeltaTime;
            if (xResetTimer >= xResetDelay)
            {
                Vector3 camEuler = cameraRigidbody.rotation.eulerAngles;
                float x = camEuler.x;
                if (x > 180f) x -= 360f;
                float newX = Mathf.MoveTowards(x, 0f, xResetSpeed * Time.fixedDeltaTime);
                cameraRigidbody.rotation = Quaternion.Euler(newX, camEuler.y, camEuler.z);
            }
        }
    }

    void Update()
    {
        if (cameraTransform != null && controlEnabled)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            cameraTransform.parent.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!controlEnabled) return;

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            controlEnabled = false;

            Vector3 wheelchairVelocity = rb.velocity;
            rb.velocity = Vector3.zero;
            currentForwardSpeed = 0f;
            currentTurnSpeed = 0f;

            if (cameraRigidbody != null)
            {
                cameraRigidbody.isKinematic = false;

                Vector3 flingForce = transform.forward * wheelchairVelocity.magnitude * collisionForceMultiplier * flingMultiplier;
                flingForce += Vector3.up * Mathf.Abs(wheelchairVelocity.magnitude) * 0.5f;
                cameraRigidbody.AddForce(flingForce, ForceMode.VelocityChange);

                Vector3 randomTorque = new Vector3(
                    Random.Range(-rotationTorque, rotationTorque),
                    Random.Range(-rotationTorque, rotationTorque),
                    Random.Range(-rotationTorque, rotationTorque)
                );
                cameraRigidbody.AddTorque(randomTorque, ForceMode.VelocityChange);

                xResetTimer = 0f;
                startXReset = true;
            }
        }
    }
}
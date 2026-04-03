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
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Collider cameraCollider;

    private Rigidbody rb;
    private float currentForwardSpeed = 0f;
    private float targetTurnDirection = 0f; 
    private float currentTurnSpeed = 0f;
    private float forwardHoldTimer = 0f;
    private bool controlEnabled = true;

    private float xResetTimer = 0f;
    private bool startXReset = false;
    private bool isCameraFlying = false;
    private float cameraRestTimer = 0f;
    private const float CAMERA_REST_THRESHOLD = 0.1f;
    private bool isSmoothingX = false;
    private float targetXRotation = 0f;
    private float smoothXSpeed = 0f;
    private Vector3 resetStartPosition;
    private Vector3 resetTargetPosition;
    private float resetPositionTimer = 0f;
    private float resetPositionDuration = 0.5f;
    private bool isMovingPosition = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        if (cameraRigidbody != null)
        {
            cameraRigidbody.isKinematic = true;
            cameraCollider = cameraRigidbody.GetComponent<Collider>();
            if (cameraCollider != null)
                cameraCollider.enabled = false;
        }

        if (cameraRigidbody != null && cameraRigidbody.transform.IsChildOf(transform))
        {
            cameraTransform = cameraRigidbody.transform;
            originalCameraParent = cameraTransform.parent;
            originalCameraPosition = cameraTransform.localPosition;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw = cameraTransform.eulerAngles.y;
            pitch = cameraTransform.localEulerAngles.x;
        }
    }

    void FixedUpdate()
    {
        if (!controlEnabled) 
        {
            if (isCameraFlying && cameraRigidbody != null)
            {
                if (cameraRigidbody.velocity.magnitude < CAMERA_REST_THRESHOLD && 
                    cameraRigidbody.angularVelocity.magnitude < CAMERA_REST_THRESHOLD)
                {
                    cameraRestTimer += Time.fixedDeltaTime;
                    
                    if (cameraRestTimer >= 0.5f)
                    {
                        StartCameraReset();
                        isCameraFlying = false;
                        cameraRestTimer = 0f;
                    }
                }
                else
                {
                    cameraRestTimer = 0f;
                }
            }
            
            if (isMovingPosition && cameraRigidbody != null)
            {
                resetPositionTimer += Time.fixedDeltaTime;
                float t = resetPositionTimer / resetPositionDuration;
                
                cameraRigidbody.MovePosition(Vector3.Lerp(resetStartPosition, resetTargetPosition, t));
                
                if (t >= 1f)
                {
                    isMovingPosition = false;
                    isSmoothingX = true;
                }
            }
            
            if (isSmoothingX && cameraRigidbody != null)
            {
                Vector3 currentEuler = cameraRigidbody.rotation.eulerAngles;
                float currentX = currentEuler.x;
                if (currentX > 180f) currentX -= 360f;
                
                float newX = Mathf.SmoothDamp(currentX, targetXRotation, ref smoothXSpeed, 0.5f);
                cameraRigidbody.MoveRotation(Quaternion.Euler(newX, currentEuler.y, currentEuler.z));
                
                if (Mathf.Abs(newX - targetXRotation) < 0.01f)
                {
                    isSmoothingX = false;
                    cameraRigidbody.MoveRotation(Quaternion.Euler(targetXRotation, currentEuler.y, currentEuler.z));
                    cameraRigidbody.isKinematic = true;
                    if (cameraCollider != null)
                        cameraCollider.enabled = false;
                    startXReset = false;
                    xResetTimer = 0f;
                    Invoke("EnableControls", 0.5f);
                }
            }
            
            return;
        }

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

        if (startXReset && cameraRigidbody != null && !isSmoothingX)
        {
            xResetTimer += Time.fixedDeltaTime;
            if (xResetTimer >= xResetDelay)
            {
                Vector3 camEuler = cameraRigidbody.rotation.eulerAngles;
                float x = camEuler.x;
                if (x > 180f) x -= 360f;
                float newX = Mathf.MoveTowards(x, 0f, xResetSpeed * Time.fixedDeltaTime);
                cameraRigidbody.MoveRotation(Quaternion.Euler(newX, camEuler.y, camEuler.z));
            }
        }
    }

    void Update()
    {
        if (cameraTransform != null && controlEnabled && !isCameraFlying && !isSmoothingX && !isMovingPosition)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!controlEnabled) return;

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            controlEnabled = false;
            rb.isKinematic = true;
            isCameraFlying = true;
            cameraRestTimer = 0f;

            currentForwardSpeed = 0f;
            currentTurnSpeed = 0f;

            if (cameraRigidbody != null)
            {
                if (cameraTransform != null && originalCameraParent != null)
                {
                    cameraTransform.parent = null;
                }

                cameraRigidbody.isKinematic = false;
                if (cameraCollider != null)
                    cameraCollider.enabled = true;
                cameraRigidbody.velocity = Vector3.zero;

                Vector3 flingForce = transform.forward * maxForwardSpeed * collisionForceMultiplier * flingMultiplier;
                flingForce += Vector3.up * maxForwardSpeed * 0.5f;
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

    private void StartCameraReset()
    {
        if (cameraRigidbody != null)
        {
            Vector3 currentEuler = cameraRigidbody.rotation.eulerAngles;
            float currentX = currentEuler.x;
            if (currentX > 180f) currentX -= 360f;
            
            resetStartPosition = cameraRigidbody.position;
            resetTargetPosition = cameraRigidbody.position + Vector3.up * 1f;
            resetPositionTimer = 0f;
            isMovingPosition = true;
            
            targetXRotation = 0f;
            smoothXSpeed = 0f;
            
            if (cameraTransform != null)
            {
                pitch = 0f;
                yaw = currentEuler.y;
            }
            
            cameraRigidbody.velocity = Vector3.zero;
            cameraRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void EnableControls()
    {
        controlEnabled = true;
        rb.isKinematic = false;
    }
}
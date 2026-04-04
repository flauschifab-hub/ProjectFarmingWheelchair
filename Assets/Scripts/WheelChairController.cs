using UnityEngine;
using TMPro;

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
    public Transform forwardDirectionReference;

    [Header("Braking")]
    public float brakeDeceleration = 8f;
    public float reverseSpeed = -3f;
    public float reverseAcceleration = 5f;

    [Header("Dual Push Stacking")]
    public float dualPushSpeedMultiplier = 2f;
    public float dualPushAccelerationMultiplier = 3f;

    [Header("Particle Effects")]
    public ParticleSystem speedParticleSystem;
    public float speedThreshold = 8f;
    public float particleFadeSpeed = 2f;
    private float currentParticleEmissionRate = 0f;
    private float originalEmissionRate = 0f;

    [Header("Wheel Rotation")]
    public Transform leftWheel;
    public Transform rightWheel;
    public float wheelRotationSpeed = 360f;
    public float reverseRotationMultiplier = -1f;

    [Header("Collision")]
    public Rigidbody cameraRigidbody; 
    public float collisionForceMultiplier = 0.5f; 
    public float flingMultiplier = 2f;           
    public float rotationTorque = 5f;
    public GameObject legs;
    public GameObject legsHiddenAlternate; // New GameObject to enable when legs are hidden

    [Header("Camera Look")]
    public float lookSensitivity = 2f;
    private Transform cameraTransform;
    private float pitch = 0f;
    private float yaw = 0f;

    [Header("Camera Reset")]
    public float cameraResetSpeed = 5f;
    private bool isResettingCamera = false;
    private Quaternion targetCameraRotation;

    [Header("FOV Effects")]
    public Camera playerCamera;
    public float baseFOV = 60f;
    public float maxFOV = 80f;
    public float fovChangeSpeed = 5f;
    private float targetFOV;

    [Header("Sway Effects")]
    public float swayAmount = 2f;
    public float swaySpeed = 3f;
    private float swayAngle = 0f;

    [Header("UI")]
    public TextMeshProUGUI resetPromptText;
    public float promptFadeSpeed = 3f;
    public float promptDelayTime = 2f;
    private float promptDelayTimer = 0f;
    private bool showPromptDelayed = false;

    private Rigidbody rb;
    private float currentForwardSpeed = 0f;
    private float currentTurnSpeed = 0f;
    private float forwardHoldTimer = 0f;
    private bool controlEnabled = true;
    private bool cameraThrown = false;
    private float wheelRotationAccumulator = 0f;
    private Collider wheelchairCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        wheelchairCollider = GetComponent<Collider>();

        if (cameraRigidbody != null)
            cameraRigidbody.isKinematic = true;

        if (cameraRigidbody != null)
        {
            cameraTransform = cameraRigidbody.transform;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerCamera == null && cameraTransform != null)
            playerCamera = cameraTransform.GetComponent<Camera>();
        
        if (playerCamera != null)
        {
            baseFOV = playerCamera.fieldOfView;
            targetFOV = baseFOV;
        }

        if (resetPromptText != null)
        {
            resetPromptText.text = "Press R to Reset Camera";
            resetPromptText.alpha = 0f;
        }

        if (speedParticleSystem != null)
        {
            originalEmissionRate = speedParticleSystem.emission.rateOverTime.constant;
            var emission = speedParticleSystem.emission;
            emission.rateOverTime = 0f;
        }

        if (legs != null)
            legs.SetActive(true);
        
        if (legsHiddenAlternate != null)
            legsHiddenAlternate.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!controlEnabled) return;

        bool pressingLeft = Input.GetKey(KeyCode.A);
        bool pressingRight = Input.GetKey(KeyCode.D);
        bool pressingBoth = pressingLeft && pressingRight;
        bool pressingOnlyOne = (pressingLeft || pressingRight) && !pressingBoth;
        bool pressingBrake = Input.GetKey(KeyCode.S);

        Vector3 forwardDir = forwardDirectionReference != null ? forwardDirectionReference.forward : transform.forward;

        if (pressingBrake)
        {
            forwardHoldTimer = 0f;
            currentTurnSpeed = 0f;
            
            if (currentForwardSpeed > 0)
            {
                currentForwardSpeed -= brakeDeceleration * Time.fixedDeltaTime;
                if (currentForwardSpeed < 0) currentForwardSpeed = 0;
            }
            else if (currentForwardSpeed <= 0 && pressingBrake)
            {
                currentForwardSpeed -= reverseAcceleration * Time.fixedDeltaTime;
                currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, reverseSpeed, 0);
            }
        }
        else if (pressingBoth)
        {
            forwardHoldTimer += Time.fixedDeltaTime;
            
            float currentAcceleration = forwardAcceleration * dualPushAccelerationMultiplier;
            
            if (forwardHoldTimer <= maxForwardHoldTime)
            {
                currentForwardSpeed += currentAcceleration * Time.fixedDeltaTime;
            }
            else
            {
                currentForwardSpeed -= deceleration * Time.fixedDeltaTime;
                if (currentForwardSpeed < 0f) currentForwardSpeed = 0f;
            }
            currentTurnSpeed = 0f;
        }
        else if (pressingOnlyOne)
        {
            forwardHoldTimer = 0f;
            
            if (pressingLeft)
            {
                currentTurnSpeed = -turnSpeed;
            }
            else if (pressingRight)
            {
                currentTurnSpeed = turnSpeed;
            }
            
            currentForwardSpeed -= deceleration * Time.fixedDeltaTime;
            if (currentForwardSpeed < 0f) currentForwardSpeed = 0f;
        }
        else
        {
            forwardHoldTimer = 0f;
            currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, 0f, turnLerpSpeed * Time.fixedDeltaTime);
            if (currentForwardSpeed > 0)
            {
                currentForwardSpeed -= deceleration * Time.fixedDeltaTime;
                if (currentForwardSpeed < 0f) currentForwardSpeed = 0f;
            }
        }

        float wheelRotationThisFrame = currentForwardSpeed * wheelRotationSpeed * Time.fixedDeltaTime;
        if (currentForwardSpeed < 0)
        {
            wheelRotationThisFrame *= reverseRotationMultiplier;
        }
        
        wheelRotationAccumulator += wheelRotationThisFrame;
        
        if (leftWheel != null)
        {
            float turnInfluence = 0f;
            if (currentTurnSpeed < 0) turnInfluence = -15f;
            if (currentTurnSpeed > 0) turnInfluence = 15f;
            
            leftWheel.localRotation = Quaternion.Euler(0, turnInfluence, wheelRotationAccumulator);
        }
        
        if (rightWheel != null)
        {
            float turnInfluence = 0f;
            if (currentTurnSpeed < 0) turnInfluence = -15f;
            if (currentTurnSpeed > 0) turnInfluence = 15f;
            
            rightWheel.localRotation = Quaternion.Euler(0, turnInfluence, -wheelRotationAccumulator);
        }

        if (Mathf.Abs(currentTurnSpeed) > 0.01f && forwardDirectionReference != null)
        {
            transform.RotateAround(forwardDirectionReference.position, Vector3.up, currentTurnSpeed * Time.fixedDeltaTime);
        }

        Vector3 forwardVelocity = forwardDir * currentForwardSpeed;
        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized;
        Vector3 lateralVelocity = rightDir * Vector3.Dot(rb.velocity, rightDir) * driftFactor;
        rb.velocity = new Vector3(forwardVelocity.x + lateralVelocity.x, rb.velocity.y, forwardVelocity.z + lateralVelocity.z);

        float speedNormalized = Mathf.Clamp01(Mathf.Abs(currentForwardSpeed) / maxForwardSpeed);
        
        if (pressingLeft || pressingRight || pressingBrake)
        {
            targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedNormalized);
        }
        else
        {
            targetFOV = Mathf.Lerp(targetFOV, baseFOV, fovChangeSpeed * Time.fixedDeltaTime * 2f);
        }

        if (speedParticleSystem != null)
        {
            float currentSpeed = Mathf.Abs(currentForwardSpeed);
            
            if (currentSpeed >= speedThreshold)
            {
                float speedFactor = (currentSpeed - speedThreshold) / (maxForwardSpeed * dualPushSpeedMultiplier - speedThreshold);
                speedFactor = Mathf.Clamp01(speedFactor);
                
                float targetRate = originalEmissionRate * speedFactor;
                currentParticleEmissionRate = Mathf.Lerp(currentParticleEmissionRate, targetRate, particleFadeSpeed * Time.fixedDeltaTime);
            }
            else
            {
                currentParticleEmissionRate = Mathf.Lerp(currentParticleEmissionRate, 0f, particleFadeSpeed * Time.fixedDeltaTime);
            }
            
            var emission = speedParticleSystem.emission;
            emission.rateOverTime = currentParticleEmissionRate;
            
            if (currentParticleEmissionRate > 0.01f && !speedParticleSystem.isPlaying)
            {
                speedParticleSystem.Play();
            }
            else if (currentParticleEmissionRate <= 0.01f && speedParticleSystem.isPlaying)
            {
                speedParticleSystem.Stop();
            }
        }
    }

    void Update()
    {
        if (cameraTransform != null && !isResettingCamera)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
            
            if (!cameraThrown)
            {
                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, -90f, 90f);
                
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                cameraTransform.parent.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            else
            {
                cameraTransform.Rotate(Vector3.up, mouseX, Space.World);
                cameraTransform.Rotate(Vector3.right, -mouseY, Space.Self);
            }
        }

        if (cameraThrown && cameraRigidbody != null && Input.GetKeyDown(KeyCode.R) && !isResettingCamera)
        {
            targetCameraRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
            isResettingCamera = true;
            showPromptDelayed = false;
            promptDelayTimer = 0f;
        }

        if (isResettingCamera && cameraTransform != null)
        {
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetCameraRotation, cameraResetSpeed * Time.deltaTime);
            
            if (Quaternion.Angle(cameraTransform.rotation, targetCameraRotation) < 0.1f)
            {
                cameraTransform.rotation = targetCameraRotation;
                isResettingCamera = false;
            }
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
        }

        bool isTurning = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        float currentSpeedNormalized = Mathf.Clamp01(Mathf.Abs(currentForwardSpeed) / maxForwardSpeed);
        
        if (isTurning && currentSpeedNormalized > 0.1f && currentForwardSpeed > 0)
        {
            float turnDirection = 0f;
            if (Input.GetKey(KeyCode.A)) turnDirection = -1f;
            if (Input.GetKey(KeyCode.D)) turnDirection = 1f;
            
            swayAngle = Mathf.Sin(Time.time * swaySpeed * currentSpeedNormalized) * swayAmount * currentSpeedNormalized * turnDirection;
            
            if (cameraTransform != null && !cameraThrown)
            {
                Vector3 localPos = cameraTransform.localPosition;
                localPos.x = swayAngle * 0.02f;
                cameraTransform.localPosition = localPos;
            }
        }
        else
        {
            if (cameraTransform != null && !cameraThrown)
            {
                Vector3 localPos = cameraTransform.localPosition;
                localPos.x = Mathf.Lerp(localPos.x, 0f, Time.deltaTime * 5f);
                cameraTransform.localPosition = localPos;
            }
        }

        if (resetPromptText != null)
        {
            if (cameraThrown && !isResettingCamera && !showPromptDelayed)
            {
                promptDelayTimer += Time.deltaTime;
                if (promptDelayTimer >= promptDelayTime)
                {
                    showPromptDelayed = true;
                }
            }
            else if (!cameraThrown || isResettingCamera)
            {
                promptDelayTimer = 0f;
                showPromptDelayed = false;
            }

            float targetAlpha = (cameraThrown && !isResettingCamera && showPromptDelayed) ? 1f : 0f;
            resetPromptText.alpha = Mathf.Lerp(resetPromptText.alpha, targetAlpha, promptFadeSpeed * Time.deltaTime);
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
                cameraTransform.parent = null;
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

                cameraThrown = true;
                showPromptDelayed = false;
                promptDelayTimer = 0f;

                if (legs != null)
                    legs.SetActive(false);
                
                if (legsHiddenAlternate != null)
                    legsHiddenAlternate.SetActive(true);

                if (wheelchairCollider != null)
                    Physics.IgnoreCollision(cameraRigidbody.GetComponent<Collider>(), wheelchairCollider, true);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (cameraThrown && cameraRigidbody != null && wheelchairCollider != null)
        {
            Collider cameraCollider = cameraRigidbody.GetComponent<Collider>();
            if (cameraCollider != null && collision.collider == wheelchairCollider)
            {
                Vector3 pushDirection = (cameraRigidbody.position - transform.position).normalized;
                pushDirection.y = 0.5f;
                cameraRigidbody.AddForce(pushDirection * 5f, ForceMode.Impulse);
            }
        }
    }

    public void ReenableControl()
    {
        controlEnabled = true;
        cameraThrown = false;
        pitch = 0f;
        yaw = cameraTransform.parent.eulerAngles.y;
        
        currentForwardSpeed = 0f;
        currentTurnSpeed = 0f;
        forwardHoldTimer = 0f;
        
        if (playerCamera != null)
            targetFOV = baseFOV;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (resetPromptText != null)
        {
            resetPromptText.alpha = 0f;
            showPromptDelayed = false;
            promptDelayTimer = 0f;
        }

        if (speedParticleSystem != null)
        {
            var emission = speedParticleSystem.emission;
            emission.rateOverTime = 0f;
            currentParticleEmissionRate = 0f;
            speedParticleSystem.Stop();
        }

        if (legs != null)
            legs.SetActive(true);
        
        if (legsHiddenAlternate != null)
            legsHiddenAlternate.SetActive(false);

        if (cameraRigidbody != null && wheelchairCollider != null)
        {
            Collider cameraCollider = cameraRigidbody.GetComponent<Collider>();
            if (cameraCollider != null)
                Physics.IgnoreCollision(cameraCollider, wheelchairCollider, false);
        }
    }
}
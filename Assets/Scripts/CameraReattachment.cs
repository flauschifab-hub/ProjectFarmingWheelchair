using UnityEngine;

public class CameraReattachment : MonoBehaviour
{
    [Header("References")]
    public GameObject cameraObject; 
    public Transform cameraPos; 
    public WheelChairController wheelchairController;
    public LayerMask groundLayer;
    
    [Header("Settings")]
    public KeyCode reattachKey = KeyCode.E;
    public float checkRadius = 0.5f; 
    public bool requireGroundContact = true;
    
    private Rigidbody cameraRigidbody;
    
    void Start()
    {
        if (cameraObject != null)
            cameraRigidbody = cameraObject.GetComponent<Rigidbody>();
        
        if (wheelchairController == null && cameraPos != null)
        {
            wheelchairController = cameraPos.GetComponentInParent<WheelChairController>();
        }
    }
    
    void Update()
    {
        if (cameraObject != null && cameraPos != null && 
            cameraObject.transform.parent != cameraPos)
        {
            bool isOnGround = !requireGroundContact || IsCameraTouchingGround();
            
            if (Input.GetKeyDown(reattachKey) && isOnGround)
            {
                ReattachCamera();
            }
        }
    }
    
    bool IsCameraTouchingGround()
    {
        if (cameraObject == null) return false;
        
        Collider[] hitColliders = Physics.OverlapSphere(cameraObject.transform.position, checkRadius, groundLayer);
        
        bool isGroundedByVelocity = false;
        if (cameraRigidbody != null && !cameraRigidbody.isKinematic)
        {
            isGroundedByVelocity = Mathf.Abs(cameraRigidbody.velocity.y) < 0.1f;
        }
        
        return hitColliders.Length > 0 || (isGroundedByVelocity && CheckGroundBelowCamera());
    }
    
    bool CheckGroundBelowCamera()
    {
        if (cameraObject == null) return false;
        
        RaycastHit hit;
        float rayDistance = 0.6f; 
        
        if (Physics.Raycast(cameraObject.transform.position, Vector3.down, out hit, rayDistance, groundLayer))
        {
            return true;
        }
        
        return false;
    }
    
    void ReattachCamera()
    {
        if (cameraRigidbody != null)
        {
            cameraRigidbody.isKinematic = true;
            cameraRigidbody.velocity = Vector3.zero;
            cameraRigidbody.angularVelocity = Vector3.zero;
        }
        
        cameraObject.transform.SetParent(cameraPos);
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localRotation = Quaternion.identity;
        
        if (wheelchairController != null)
        {
            wheelchairController.ReenableControl();
        }     
    }
        void OnDrawGizmosSelected()
    {
        if (cameraObject != null && requireGroundContact)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cameraObject.transform.position, checkRadius);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(cameraObject.transform.position, Vector3.down * 0.6f);
        }
    }
}
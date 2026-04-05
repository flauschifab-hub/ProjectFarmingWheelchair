using UnityEngine;
using TMPro;
using System.Collections;

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
    
    [Header("UI Text")]
    public TextMeshProUGUI reattachText;
    public float fadeDuration = 0.5f;
    public string promptMessage = "Press E to reattach camera";
    
    private Rigidbody cameraRigidbody;
    private Coroutine currentFadeRoutine;
    private bool wasInRange = false;
    
    void Start()
    {
        if (cameraObject != null)
            cameraRigidbody = cameraObject.GetComponent<Rigidbody>();
        
        if (wheelchairController == null && cameraPos != null)
        {
            wheelchairController = cameraPos.GetComponentInParent<WheelChairController>();
        }
        
        if (reattachText != null)
        {
            reattachText.text = promptMessage;
            Color color = reattachText.color;
            color.a = 0f;
            reattachText.color = color;
        }
    }
    
    void Update()
    {
        if (cameraObject != null && cameraPos != null)
        {
            bool isDetached = cameraObject.transform.parent != cameraPos;
            bool canReattach = false;
            
            if (isDetached)
            {
                bool isOnGround = !requireGroundContact || IsCameraTouchingGround();
                canReattach = isOnGround;
                
                if (canReattach && Input.GetKeyDown(reattachKey))
                {
                    ReattachCamera();
                }
            }
            
            UpdateTextVisibility(isDetached && canReattach);
        }
        else
        {
            UpdateTextVisibility(false);
        }
    }
    
    void UpdateTextVisibility(bool show)
    {
        if (reattachText == null) return;
        
        if (show && !wasInRange)
        {
            if (currentFadeRoutine != null)
                StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = StartCoroutine(FadeText(0f, 1f));
            wasInRange = true;
        }
        else if (!show && wasInRange)
        {
            if (currentFadeRoutine != null)
                StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = StartCoroutine(FadeText(1f, 0f));
            wasInRange = false;
        }
    }
    
    IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        if (reattachText == null) yield break;
        
        Color color = reattachText.color;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            reattachText.color = color;
            yield return null;
        }
        
        color.a = endAlpha;
        reattachText.color = color;
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
        
        UpdateTextVisibility(false);
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
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelToggler : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button button;
    [SerializeField] private float fadeDuration = 0.2f;
    
    private CanvasGroup canvasGroup;
    private Coroutine activeCoroutine;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();
        
        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();
            
            panel.SetActive(false);
            canvasGroup.alpha = 0f;
        }
        
        if (button != null && panel != null)
            button.onClick.AddListener(TogglePanel);
        else
            Debug.LogWarning("Panel or Button not assigned!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && panel != null && panel.activeSelf)
        {
            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);
            
            activeCoroutine = StartCoroutine(FadeOutPanel());
        }
    }

    private void TogglePanel()
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);
        
        activeCoroutine = StartCoroutine(FadePanel());
    }

    private IEnumerator FadeOutPanel()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        panel.SetActive(false);
        activeCoroutine = null;
    }

    private IEnumerator FadePanel()
    {
        if (panel.activeSelf)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
            canvasGroup.alpha = 0f;
            
            float elapsed = 0f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        activeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(TogglePanel);
    }
}
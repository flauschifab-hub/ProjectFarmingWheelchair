using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReadyManager : MonoBehaviour
{
    [Header("Script to Enable")]
    public MonoBehaviour scriptToEnable;
    
    [Header("UI Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public float displayDuration = 2f;
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip clipAt10ms;
    public AudioClip clipAt2s;
    public AudioClip clipAt3s;
    public AudioClip clipAt4s;
    public AudioClip clipAt5s;
    public AudioClip loopMusic;
    public float musicFadeInDuration = 1.5f;
    
    private float originalVolume;
    
    void Start()
    {
        originalVolume = audioSource.volume;
        audioSource.volume = originalVolume;
        
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(true);
        }
        
        StartCoroutine(TimedSequence());
    }
    
    IEnumerator TimedSequence()
    {
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeInDuration));
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeCanvas(0f, 1f, fadeOutDuration));
            
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.gameObject.SetActive(false);
            }
        }
        
        yield return new WaitForSeconds(0.01f);
        if (audioSource != null && clipAt10ms != null)
        {
            audioSource.PlayOneShot(clipAt10ms);
        }
        
        yield return new WaitForSeconds(1.99f);
        if (audioSource != null && clipAt2s != null)
        {
            audioSource.PlayOneShot(clipAt2s);
        }
        
        yield return new WaitForSeconds(1.0f);
        if (audioSource != null && clipAt3s != null)
        {
            audioSource.PlayOneShot(clipAt3s);
        }
        
        yield return new WaitForSeconds(1.0f);
        if (audioSource != null && clipAt4s != null)
        {
            audioSource.PlayOneShot(clipAt4s);
        }
        
        yield return new WaitForSeconds(1.0f);
        if (audioSource != null && clipAt5s != null)
        {
            audioSource.PlayOneShot(clipAt5s);
        }
        
        if (scriptToEnable != null)
        {
            scriptToEnable.enabled = true;
        }
        
        if (audioSource != null && loopMusic != null)
        {
            audioSource.clip = loopMusic;
            audioSource.loop = true;
            audioSource.Play();
            
            float elapsedTime = 0f;
            while (elapsedTime < musicFadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, originalVolume, elapsedTime / musicFadeInDuration);
                yield return null;
            }
            
            audioSource.volume = originalVolume;
        }
    }
    
    IEnumerator FadeCanvas(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = endAlpha;
    }
}
using System.Collections;
using UnityEngine;

public class ReadyManager : MonoBehaviour
{
    [Header("Script to Enable")]
    public MonoBehaviour scriptToEnable;
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip clipAt10ms;
    public AudioClip clipAt2s;
    public AudioClip clipAt3s;
    public AudioClip clipAt4s;
    public AudioClip clipAt5s;
    public AudioClip loopMusic;
    public float fadeInDuration = 1.5f;
    
    private float startTime;
    private float originalVolume;
    
    void Start()
    {
        startTime = Time.time;
        originalVolume = audioSource.volume;
        audioSource.volume = 0f;
        StartCoroutine(TimedSequence());
    }
    
    IEnumerator TimedSequence()
    {
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
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, originalVolume, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            audioSource.volume = originalVolume;
        }
    }
}
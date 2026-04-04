using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage; 
    [SerializeField] private float fadeDuration = 1f;

    [Header("Button Feedback")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;

    void Start()
    {
        if (playClickSound)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && clickSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public void LoadGameScene()
    {
        if (playClickSound && audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image not assigned. Bro you forgot the black screen.");
            yield break;
        }

        float timer = 0f;
        Color c = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Scene name is empty. Something went very wrong.");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Scoreboard : MonoBehaviour
{
    [Header("Display Texts")]
    [SerializeField] private TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] scoreTexts = new TextMeshProUGUI[5];
    
    [Header("Scoreboard Panel")]
    [SerializeField] private GameObject scoreboardPanel;
    
    [Header("New Highscore Panel")]
    [SerializeField] private GameObject highscorePanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI currentScoreDisplay;
    [SerializeField] private Button submitButton;
    
    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeOutDuration = 1.5f;
    
    [Header("Wheelchair Reference")]
    [SerializeField] private WheelChairController wheelchairController;
    
    private List<ScoreEntry> scores = new List<ScoreEntry>();
    private string savePath;
    private int pendingScore;
    private int currentGameScore;
    private float originalVolume;
    private Coroutine fadeOutCoroutine;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        LoadScores();
        
        if (highscorePanel != null)
            highscorePanel.SetActive(false);
        if (scoreboardPanel != null)
            scoreboardPanel.SetActive(false);
        
        if (nameInputField != null)
        {
            nameInputField.characterLimit = 3;
            nameInputField.onValueChanged.AddListener(OnInputValueChanged);
        }
        
        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitNewHighscore);
        
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        if (audioSource != null)
            originalVolume = audioSource.volume;
    }

    void Start()
    {
        UpdateScoreboardDisplay();
    }

    private void OnInputValueChanged(string value)
    {
        if (nameInputField != null && value.Length > 3)
            nameInputField.text = value.Substring(0, 3);
    }

    public void ShowScoreboard()
    {
        UpdateScoreboardDisplay();
        scoreboardPanel.SetActive(true);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            if (fadeOutCoroutine != null)
                StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = StartCoroutine(FadeOutMusic());
        }
        
        if (wheelchairController != null)
        {
            wheelchairController.enabled = false;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        int playerScore = GetCurrentGameScore();
        
        if (IsNewHighscore(playerScore) && playerScore > 0)
        {
            pendingScore = playerScore;
            if (currentScoreDisplay != null)
                currentScoreDisplay.text = $"YOUR SCORE: {playerScore}";
            
            highscorePanel.SetActive(true);
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    private IEnumerator FadeOutMusic()
    {
        float elapsedTime = 0f;
        float startVolume = audioSource.volume;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }
        
        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.volume = originalVolume;
    }

    public void HideScoreboard()
    {
        scoreboardPanel.SetActive(false);
        highscorePanel.SetActive(false);
        
        if (wheelchairController != null)
        {
            wheelchairController.enabled = true;
            wheelchairController.ReenableControl();
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SubmitNewHighscore()
    {
        string input = nameInputField.text.Trim().ToUpper();
        
        if (string.IsNullOrEmpty(input))
            input = "AAA";
        
        if (input.Length < 3)
            input = input.PadRight(3, 'X');
        if (input.Length > 3)
            input = input.Substring(0, 3);
        
        AddScore(input, pendingScore);
        
        nameInputField.text = "";
        highscorePanel.SetActive(false);
        UpdateScoreboardDisplay();
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SetCurrentGameScore(int score)
    {
        currentGameScore = score;
    }

    private int GetCurrentGameScore()
    {
        return currentGameScore;
    }

    public void AddPoints(int points)
    {
        currentGameScore += points;
        SetCurrentGameScore(currentGameScore);
    }

    public void CheckAndAddScore(int score)
    {
        pendingScore = score;
        
        if (IsNewHighscore(score) && score > 0)
        {
            if (currentScoreDisplay != null)
                currentScoreDisplay.text = $"YOUR SCORE: {score}";
            
            highscorePanel.SetActive(true);
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    private bool IsNewHighscore(int score)
    {
        if (scores.Count < 5) return score > 0;
        return score > scores.Last().score;
    }

    private void AddScore(string threeLetterName, int score)
    {
        ScoreEntry newEntry = new ScoreEntry(threeLetterName, score);
        scores.Add(newEntry);
        scores = scores.OrderByDescending(s => s.score).Take(5).ToList();
        SaveScores();
        UpdateScoreboardDisplay();
    }

    private void LoadScores()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ScoreboardData data = JsonUtility.FromJson<ScoreboardData>(json);
            if (data != null && data.scores != null)
                scores = data.scores.ToList();
            else
                scores = new List<ScoreEntry>();
        }
        else
        {
            scores = new List<ScoreEntry>();
        }
        
        while (scores.Count < 5)
            scores.Add(new ScoreEntry("---", 0));
    }

    private void SaveScores()
    {
        ScoreboardData data = new ScoreboardData();
        data.scores = scores.ToArray();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    private void UpdateScoreboardDisplay()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < scores.Count)
            {
                if (nameTexts[i] != null)
                    nameTexts[i].text = scores[i].name;
                if (scoreTexts[i] != null)
                    scoreTexts[i].text = scores[i].score.ToString();
            }
            else
            {
                if (nameTexts[i] != null)
                    nameTexts[i].text = "---";
                if (scoreTexts[i] != null)
                    scoreTexts[i].text = "0";
            }
        }
    }

    public List<ScoreEntry> GetTopScores()
    {
        return new List<ScoreEntry>(scores);
    }

    public void ResetScores()
    {
        scores.Clear();
        for (int i = 0; i < 5; i++)
            scores.Add(new ScoreEntry("---", 0));
        SaveScores();
        UpdateScoreboardDisplay();
    }

    public void RestartGame()
    {
        currentGameScore = 0;
        Time.timeScale = 1f;
        HideScoreboard();
    }
}

[System.Serializable]
public class ScoreEntry
{
    public string name;
    public int score;

    public ScoreEntry(string name, int score)
    {
        this.name = name;
        this.score = score;
    }
}

[System.Serializable]
public class ScoreboardData
{
    public ScoreEntry[] scores;
}
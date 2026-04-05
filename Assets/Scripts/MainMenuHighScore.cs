using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class MainMenuHighScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private string highScorePrefix = "HIGH SCORE: ";
    [SerializeField] private string noScoreText = "NO SCORE YET";
    
    private string savePath;
    private List<ScoreEntry> scores = new List<ScoreEntry>();
    private int currentHighScore = 0;
    
    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        LoadScores();
    }
    
    void Start()
    {
        UpdateHighScoreDisplay();
    }
    
    void OnEnable()
    {
        LoadScores();
        UpdateHighScoreDisplay();
    }
    
    private void LoadScores()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                ScoreboardData data = JsonUtility.FromJson<ScoreboardData>(json);
                
                if (data != null && data.scores != null && data.scores.Length > 0)
                {
                    scores = data.scores.ToList();
                    
                    if (scores.Count > 0)
                    {
                        scores = scores.OrderByDescending(s => s.score).ToList();
                        currentHighScore = scores[0].score;
                    }
                    else
                    {
                        currentHighScore = 0;
                    }
                }
                else
                {
                    InitializeEmptyScores();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading high scores: {e.Message}");
                InitializeEmptyScores();
            }
        }
        else
        {
            InitializeEmptyScores();
        }
    }
    
    private void InitializeEmptyScores()
    {
        scores = new List<ScoreEntry>();
        for (int i = 0; i < 5; i++)
            scores.Add(new ScoreEntry("---", 0));
        currentHighScore = 0;
    }
    
    private void UpdateHighScoreDisplay()
    {
        if (highScoreText == null)
            return;
        
        if (currentHighScore > 0)
        {
            highScoreText.text = $"{highScorePrefix}{currentHighScore}";
        }
        else
        {
            highScoreText.text = noScoreText;
        }
    }
    
    public void RefreshHighScore()
    {
        LoadScores();
        UpdateHighScoreDisplay();
    }
    
    public int GetCurrentHighScore()
    {
        return currentHighScore;
    }
    
    public bool WouldBeNewHighScore(int score)
    {
        return score > currentHighScore;
    }
    
    public void OnReturnToMainMenu()
    {
        RefreshHighScore();
    }
}
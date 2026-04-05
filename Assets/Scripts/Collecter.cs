using UnityEngine;
using TMPro;

public class Collecter : MonoBehaviour
{
    public TextMeshProUGUI WeedText;
    [SerializeField] Timer timer;
    [SerializeField] Scoreboard scoreboard;

    private int package = 0;

    void Start()
    {
        if (scoreboard != null)
        {
            scoreboard.SetCurrentGameScore(0);
        }
        UpdateScoreText();
    }

    public void AddWeed()
    {
        Debug.Log("Weed wurde geaddet");
        package++;
        UpdateScoreText();
        
        if (scoreboard != null)
        {
            scoreboard.AddPoints(1);
        }
        
        timer.AddTime(30f);
    }

    public void UpdateScoreText()
    {
        if (WeedText != null)
        {
            WeedText.text = "Score: " + package;
        }
    }

  //  private void OnTriggerEnter(Collider other)
   // {
        //if (other.CompareTag("Weed"))
        //{
         //   AddWeed();
         //   Destroy(other.gameObject);
       // }
  //  }
    
    public int GetCurrentScore()
    {
        return package;
    }
    
    public void ResetScore()
    {
        package = 0;
        UpdateScoreText();
        if (scoreboard != null)
        {
            scoreboard.SetCurrentGameScore(0);
        }
    }
}
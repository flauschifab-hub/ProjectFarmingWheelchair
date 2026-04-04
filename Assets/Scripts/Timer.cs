using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] float remainingTime;
    private bool isRunning = true;

    public void AddTime(float seconds)
    {
        remainingTime += seconds;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(isRunning && remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            if(remainingTime < 0)
            {
                remainingTime = 0;
                isRunning = false;
            }

            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else if (remainingTime <= 0)
        {
            TimerText.text = "00:00";
        }
    }
}

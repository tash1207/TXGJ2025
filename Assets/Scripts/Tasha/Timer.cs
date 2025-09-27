using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;
    [SerializeField] float initialDuration = 10f;

    float timeLeft = 0f;
    bool timerOn = false;

    void Start()
    {
        ResetTimer();
        StartTimer();
    }

    void Update()
    {
        if (timerOn)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                UpdateTimerUI();

            }
            else
            {
                timeLeft = 0;
                timerOn = false;
            }
        }
    }

    void UpdateTimerUI()
    {
        // Since we round the float down, add 1 so the timer UI matches the real countdown.
        float currentTime = timeLeft + 1;
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StartTimer()
    {
        timerOn = true;
    }

    public void PauseTimer()
    {
        timerOn = false;
    }

    public void ResetTimer()
    {
        timeLeft = initialDuration;
    }

    public float GetTimeRemaining()
    {
        return timeLeft;
    }
}

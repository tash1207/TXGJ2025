using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;
    [SerializeField] float initialDuration = 30f;

    public static Action OnTimeRunOut;
    public static Action<float> OnSubtractTime;

    float timeLeft = 0f;
    bool timerOn = false;

    bool timeRanOut = false;

    void Start()
    {
        ResetTimer();
        StartTimer();
    }

    void OnEnable()
    {
        OnSubtractTime += SubtractTime;
        WinGame.OnGameWon += PauseTimer;
    }

    void OnDisable()
    {
        OnSubtractTime -= SubtractTime;
        WinGame.OnGameWon -= PauseTimer;
    }

    void Update()
    {
        if (timerOn && !timeRanOut)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                UpdateTimerUI();

            }
            else
            {
                timerOn = false;
                timeRanOut = true;
                timeLeft = 0;
                OnTimeRunOut();
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
        timeRanOut = false;
    }

    public float GetTimeRemaining()
    {
        return timeLeft;
    }

    public void AddTime(float timeToAddSeconds)
    {
        timeLeft += timeToAddSeconds;
        UpdateTimerUI();
    }

    public void SubtractTime(float timeToSubtractSeconds)
    {
        timeLeft -= timeToSubtractSeconds;
        UpdateTimerUI();
    }
}

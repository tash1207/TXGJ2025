using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPopup : MonoBehaviour
{
    [SerializeField] GameObject gameOverPopup;
    [SerializeField] GameObject gameWinPopup;

    [SerializeField] AudioClip gameOverMusic;
    [SerializeField] AudioClip gameWinMusic;

    const int MainScreen = 0;
    const int Level1 = 1;

    bool gameLost = false;

    AudioSource audioSource;

    void Start()
    {
        BackgroundMusic backgroundMusic = FindFirstObjectByType<BackgroundMusic>();
        audioSource = backgroundMusic.GetAudioSource();
    }

    void OnEnable()
    {
        Timer.OnTimeRunOut += ShowGameOverAfterDelay;
        WinGame.OnGameWon += ShowGameWin;
    }

    void OnDisable()
    {
        Timer.OnTimeRunOut -= ShowGameOverAfterDelay;
        WinGame.OnGameWon -= ShowGameWin;
    }

    public void ShowGameOverAfterDelay()
    {
        StartCoroutine(ShowGameOverAfterDelayCoroutine());
    }

    IEnumerator ShowGameOverAfterDelayCoroutine()
    {
        gameLost = true;
        audioSource.Stop();
        audioSource.clip = gameOverMusic;
        audioSource.loop = false;
        audioSource.Play();
        // Allow death animation to play.
        yield return new WaitForSeconds(1.5f);
        gameOverPopup.SetActive(true);
    }

    public void ShowGameWin()
    {
        if (!gameLost)
        {
            gameWinPopup.SetActive(true);
            audioSource.Stop();
            audioSource.clip = gameWinMusic;
            audioSource.Play();
        }
    }

    public void ExitGame()
    {
        SceneManager.LoadScene(MainScreen);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(Level1);
    }
}

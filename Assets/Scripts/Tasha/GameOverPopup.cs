using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPopup : MonoBehaviour
{
    [SerializeField] GameObject gameOverPopup;
    [SerializeField] GameObject gameWinPopup;

    const int MainScreen = 0;
    const int Level1 = 1;

    bool gameLost = false;

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
        // Allow death animation to play.
        yield return new WaitForSeconds(1.5f);
        gameOverPopup.SetActive(true);
    }

    public void ShowGameWin()
    {
        if (!gameLost)
        {
            gameWinPopup.SetActive(true);   
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

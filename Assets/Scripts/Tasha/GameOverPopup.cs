using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPopup : MonoBehaviour
{
    [SerializeField] GameObject popup;

    const int MainScreen = 0;
    const int Level1 = 1;

    void OnEnable()
    {
        Timer.OnTimeRunOut += ShowGameOverAfterDelay;
    }

    void OnDisable()
    {
        Timer.OnTimeRunOut -= ShowGameOverAfterDelay;
    }

    public void ShowGameOverAfterDelay()
    {
        StartCoroutine(ShowGameOverAfterDelayCoroutine());
    }

    IEnumerator ShowGameOverAfterDelayCoroutine()
    {
        // Allow death animation to play.
        yield return new WaitForSeconds(1.5f);
        popup.SetActive(true);
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

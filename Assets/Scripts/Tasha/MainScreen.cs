using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour
{
    const int Level1 = 1;

    public void StartGame()
    {
        SceneManager.LoadScene(Level1);
    }
}

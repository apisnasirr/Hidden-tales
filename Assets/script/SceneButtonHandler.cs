using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonHandler : MonoBehaviour
{
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToBengkel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Bengkel");
    }

    public void GoToKedaiRuncit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("KedaiRuncit");
    }
}
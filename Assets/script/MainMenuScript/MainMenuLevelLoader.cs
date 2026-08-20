using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLevelLoader : MonoBehaviour
{
    public void GoToBandar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Bandar");
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
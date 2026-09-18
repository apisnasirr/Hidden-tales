using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    public void ClickPlayButton()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonClick();
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
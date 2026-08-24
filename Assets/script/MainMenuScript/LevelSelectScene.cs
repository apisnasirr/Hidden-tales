using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu"; 
    [SerializeField] private string bandarSceneName = "Bandar";
    [SerializeField] private string bengkelSceneName = "bengkel";
    [SerializeField] private string runcitSceneName = "Kedai Runcit";

    public void BackToMainMenu()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(mainMenuSceneName);
    }

    public void LoadBandar()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(bandarSceneName);
    }

    public void LoadBengkel()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(bengkelSceneName);
    }

    public void LoadRuncit()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(runcitSceneName);
    }

    private void LoadSceneWithLoading(string sceneName)
    {
        Time.timeScale = 1f;

        
        if (LoadingScreenManager.Instance != null)
            LoadingScreenManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    private void PlayButtonSFX()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick(); 
    }
}


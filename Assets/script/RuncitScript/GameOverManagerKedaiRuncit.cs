using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManagerKedaiRuncit : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private bool hasGameOver = false;

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (hasGameOver)
            return;

        hasGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (BGMManager.Instance != null)
            BGMManager.Instance.PauseBGM();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayDefeat();

        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        PlayButtonSFX();
        LoadSceneWithLoading(mainMenuSceneName);
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
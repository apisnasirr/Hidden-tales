using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderSafe : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private bool playButtonClickSFX = true;

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneLoaderSafe: sceneName kosong.");
            return;
        }

        PrepareBeforeSceneLoad();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadBandar()
    {
        PrepareBeforeSceneLoad();
        SceneManager.LoadScene("Bandar", LoadSceneMode.Single);
    }

    public void LoadBengkel()
    {
        PrepareBeforeSceneLoad();
        SceneManager.LoadScene("Bengkel", LoadSceneMode.Single);
    }

    public void LoadRuncit()
    {
        PrepareBeforeSceneLoad();
        SceneManager.LoadScene("Runcit", LoadSceneMode.Single);
    }

    public void ReloadCurrentScene()
    {
        PrepareBeforeSceneLoad();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    private void PrepareBeforeSceneLoad()
    {
        // Sangat penting
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (playButtonClickSFX && SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }
}
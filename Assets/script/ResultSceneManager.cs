using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultSceneManager : MonoBehaviour
{
    [Header("Star Sprites")]
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] private Sprite starEmptySprite;

    [Header("Bandar Stars UI")]
    [SerializeField] private Image[] bandarStars = new Image[3];

    [Header("Bengkel Stars UI")]
    [SerializeField] private Image[] bengkelStars = new Image[3];

    [Header("Kedai Runcit Stars UI")]
    [SerializeField] private Image[] runcitStars = new Image[3];

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private void Start()
    {
        LoadAndDisplayStars();
    }

    private void LoadAndDisplayStars()
    {
        // Get the saved stars from PlayerPrefs (Defaults to 0 if not played yet)
        int bandarScore = PlayerPrefs.GetInt("BandarStars", 0);
        int bengkelScore = PlayerPrefs.GetInt("BengkelStars", 0);
        int runcitScore = PlayerPrefs.GetInt("RuncitStars", 0);

        // Update the UI Images
        UpdateStarUI(bandarStars, bandarScore);
        UpdateStarUI(bengkelStars, bengkelScore);
        UpdateStarUI(runcitStars, runcitScore);
    }

    private void UpdateStarUI(Image[] starImages, int score)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                // If the loop index is less than the score, fill the star. Otherwise, empty it.
                starImages[i].sprite = (i < score) ? starFilledSprite : starEmptySprite;
            }
        }
    }

    public void BackToMainMenu()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonClick();
        
        Time.timeScale = 1f;
        
        if (LoadingScreenManager.Instance != null)
            LoadingScreenManager.Instance.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }
}
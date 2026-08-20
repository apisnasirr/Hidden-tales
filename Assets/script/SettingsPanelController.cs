using UnityEngine;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();

        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
    }
}
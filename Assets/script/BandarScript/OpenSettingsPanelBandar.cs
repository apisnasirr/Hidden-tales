using UnityEngine;

public class OpenSettingsPanelBandar : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("OpenSettingsPanelBandar: SettingsPanel belum assign!");
            return;
        }

        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();

        Time.timeScale = 0f;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }

    public void ClosePanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }
}
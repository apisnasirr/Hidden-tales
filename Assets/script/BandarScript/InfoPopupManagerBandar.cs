using UnityEngine;

public class InfoPopupManagerBandar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private CanvasGroup infoCanvasGroup;

    [Header("Instruction")]
    [SerializeField] private InstructionPanelBandar instructionPanelManager;

    public bool IsInfoPopupOpen => infoPanel != null && infoPanel.activeSelf;
    public bool IgnoreNextWrongClick { get; private set; }

    private void Awake()
    {
        if (infoPanel != null && infoCanvasGroup == null)
            infoCanvasGroup = infoPanel.GetComponent<CanvasGroup>();

        if (infoPanel != null && infoCanvasGroup == null)
            infoCanvasGroup = infoPanel.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.alpha = 1f;
            infoCanvasGroup.interactable = true;
            infoCanvasGroup.blocksRaycasts = true;
        }

        Time.timeScale = 0f;
    }

    public void CloseInfoPopup()
    {
        IgnoreNextWrongClick = true;

        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.alpha = 0f;
            infoCanvasGroup.interactable = false;
            infoCanvasGroup.blocksRaycasts = false;
        }

        if (infoPanel != null)
            infoPanel.SetActive(false);

        Time.timeScale = 1f;

        if (instructionPanelManager != null)
            instructionPanelManager.ShowInstructionNow();
    }

    public void ConsumeIgnoreNextWrongClick()
    {
        IgnoreNextWrongClick = false;
    }
}
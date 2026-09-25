using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpLimitUIBengkel : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private ManagerHiddenObjectBengkel manager;
    [SerializeField] private GameTimerManager timerManager; // Handles the Freeze Time

    [Header("Shop Item IDs")]
    [SerializeField] private string focusHintItemId = "focus_hint";
    [SerializeField] private string magnetHintItemId = "magnet_hint";
    [SerializeField] private string freezeTimeItemId = "freeze_time";

    [Header("Focus Hint UI")]
    [SerializeField] private TMP_Text focusHintCountText;
    [SerializeField] private Button focusHintButton;
    [SerializeField] private Image focusHintIcon;

    [Header("Magnet UI")]
    [SerializeField] private TMP_Text magnetCountText;
    [SerializeField] private Button magnetButton;
    [SerializeField] private Image magnetIcon;

    [Header("Freeze Time UI")]
    [SerializeField] private TMP_Text freezeTimeCountText;
    [SerializeField] private Button freezeTimeButton;
    [SerializeField] private Image freezeTimeIcon;

    [Header("Settings")]
    [SerializeField] private float freezeDuration = 10f; // 10 seconds of frozen time
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;

    private void OnEnable()
    {
        CurrencyManager.OnItemAmountChanged += HandleItemAmountChanged;
    }

    private void OnDisable()
    {
        CurrencyManager.OnItemAmountChanged -= HandleItemAmountChanged;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void HandleItemAmountChanged(string itemId, int newAmount)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (CurrencyManager.Instance == null) return;

        int focusCount = CurrencyManager.Instance.GetItemAmount(focusHintItemId);
        int magnetCount = CurrencyManager.Instance.GetItemAmount(magnetHintItemId);
        int freezeCount = CurrencyManager.Instance.GetItemAmount(freezeTimeItemId);

        RefreshOne(focusCount, focusHintCountText, focusHintButton, focusHintIcon);
        RefreshOne(magnetCount, magnetCountText, magnetButton, magnetIcon);
        RefreshOne(freezeCount, freezeTimeCountText, freezeTimeButton, freezeTimeIcon);
    }

    public void OnClickFocusHint()
    {
        if (CurrencyManager.Instance == null || manager == null) return;

        int currentAmount = CurrencyManager.Instance.GetItemAmount(focusHintItemId);
        if (currentAmount <= 0) return;

        bool success = manager.UseFocusHint();
        if (!success) return;

        CurrencyManager.Instance.UseItem(focusHintItemId, 1);
        RefreshUI();
    }

    public void OnClickMagnet()
    {
        if (CurrencyManager.Instance == null || manager == null) return;

        int currentAmount = CurrencyManager.Instance.GetItemAmount(magnetHintItemId);
        if (currentAmount <= 0) return;

        bool success = manager.UseMagnetHint(); 
        if (!success) return;

        CurrencyManager.Instance.UseItem(magnetHintItemId, 1);
        RefreshUI();
    }

    public void OnClickFreezeTime()
    {
        if (CurrencyManager.Instance == null || timerManager == null) return;

        int currentAmount = CurrencyManager.Instance.GetItemAmount(freezeTimeItemId);
        if (currentAmount <= 0) return;

        bool success = timerManager.ApplyFreezeTime(freezeDuration);
        if (!success) return;

        CurrencyManager.Instance.UseItem(freezeTimeItemId, 1);
        RefreshUI();
    }

    private void RefreshOne(int remaining, TMP_Text countText, Button button, Image icon)
    {
        bool canUse = remaining > 0;

        if (countText != null) countText.text = remaining.ToString();
        if (button != null) button.interactable = canUse;
        if (icon != null) icon.color = canUse ? activeColor : disabledColor;
        if (countText != null) countText.color = canUse ? activeColor : disabledColor;
    }
}
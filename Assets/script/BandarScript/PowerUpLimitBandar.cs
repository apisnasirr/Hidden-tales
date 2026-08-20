using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpLimitUIBandar : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private ManagerHiddenObjectBandar manager;

    [Header("Shop Item IDs")]
    [SerializeField] private string focusHintItemId = "focus_hint";
    [SerializeField] private string zoomHintItemId = "zoom_hint";
    [SerializeField] private string magnetItemId = "magnet";

    [Header("Focus Hint UI")]
    [SerializeField] private TMP_Text focusHintCountText;
    [SerializeField] private Button focusHintButton;
    [SerializeField] private Image focusHintIcon;

    [Header("Zoom Hint UI")]
    [SerializeField] private TMP_Text zoomHintCountText;
    [SerializeField] private Button zoomHintButton;
    [SerializeField] private Image zoomHintIcon;

    [Header("Magnet UI")]
    [SerializeField] private TMP_Text magnetCountText;
    [SerializeField] private Button magnetButton;
    [SerializeField] private Image magnetIcon;

    [Header("Color")]
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
        int zoomCount = CurrencyManager.Instance.GetItemAmount(zoomHintItemId);
        int magnetCount = CurrencyManager.Instance.GetItemAmount(magnetItemId);

        RefreshOne(focusCount, focusHintCountText, focusHintButton, focusHintIcon);
        RefreshOne(zoomCount, zoomHintCountText, zoomHintButton, zoomHintIcon);
        RefreshOne(magnetCount, magnetCountText, magnetButton, magnetIcon);
    }

    public void OnClickFocusHint()
    {
        if (CurrencyManager.Instance == null || manager == null) return;

        int currentAmount = CurrencyManager.Instance.GetItemAmount(focusHintItemId);
        if (currentAmount <= 0)
        {
            RefreshUI();
            return;
        }

        bool success = manager.UseFocusHint();
        if (!success) return;

        CurrencyManager.Instance.UseItem(focusHintItemId, 1);
        RefreshUI();
    }

    public void OnClickZoomHint()
    {
        if (CurrencyManager.Instance == null || manager == null) return;

        int currentAmount = CurrencyManager.Instance.GetItemAmount(zoomHintItemId);
        if (currentAmount <= 0)
        {
            RefreshUI();
            return;
        }

        bool success = manager.UseZoomHint();
        if (!success) return;

        CurrencyManager.Instance.UseItem(zoomHintItemId, 1);
        RefreshUI();
    }

    public void OnClickMagnet()
    {
        if (CurrencyManager.Instance == null || manager == null) return;

        int currentAmount = CurrencyManager.Instance.GetItemAmount(magnetItemId);
        if (currentAmount <= 0)
        {
            RefreshUI();
            return;
        }

        bool success = manager.UseMagnetPower();
        if (!success) return;

        CurrencyManager.Instance.UseItem(magnetItemId, 1);
        RefreshUI();
    }

    private void RefreshOne(int remaining, TMP_Text countText, Button button, Image icon)
    {
        bool canUse = remaining > 0;

        if (countText != null)
            countText.text = remaining.ToString();

        if (button != null)
            button.interactable = canUse;

        if (icon != null)
            icon.color = canUse ? activeColor : disabledColor;

        if (countText != null)
            countText.color = canUse ? activeColor : disabledColor;
    }
}
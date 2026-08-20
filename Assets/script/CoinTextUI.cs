using UnityEngine;
using TMPro;

public class CoinTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    [Header("Text Format")]
    [SerializeField] private string prefix = "Coins: ";

    private void OnEnable()
    {
        CurrencyManager.OnCoinsChanged += UpdateCoinText;
        RefreshNow();
    }

    private void OnDisable()
    {
        CurrencyManager.OnCoinsChanged -= UpdateCoinText;
    }

    private void Start()
    {
        RefreshNow();
    }

    private void RefreshNow()
    {
        if (coinText == null)
            return;

        if (CurrencyManager.Instance == null)
        {
            coinText.text = prefix + "0";
            return;
        }

        coinText.text = prefix + CurrencyManager.Instance.GetCoins();
    }

    private void UpdateCoinText(int amount)
    {
        if (coinText == null)
            return;

        coinText.text = prefix + amount;
    }
}
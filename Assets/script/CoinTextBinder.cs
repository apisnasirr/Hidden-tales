using TMPro;
using UnityEngine;

public class CoinTextBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    private void Awake()
    {
        if (coinsText == null)
            coinsText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        RegisterText();
    }

    private void Start()
    {
        RegisterText();
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null && coinsText != null)
            CurrencyManager.Instance.ClearCoinsTextReference(coinsText);
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null && coinsText != null)
            CurrencyManager.Instance.ClearCoinsTextReference(coinsText);
    }

    private void RegisterText()
    {
        if (CurrencyManager.Instance != null && coinsText != null)
            CurrencyManager.Instance.SetCoinsText(coinsText);
    }
}
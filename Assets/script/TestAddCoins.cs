using UnityEngine;

public class TestAddCoins : MonoBehaviour
{
    private void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.SetCoins(0);
    }
}
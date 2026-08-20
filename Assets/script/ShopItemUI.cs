using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private Button buyButton;

    [Header("Item Info")]
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private int price = 5;
    [SerializeField] private int amount = 1;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text itemNameText;

    private bool isBuying = false;

    private void Start()
    {
        if (priceText != null)
            priceText.text = price.ToString();

        if (itemNameText != null)
            itemNameText.text = itemName;
    }

    public void BuyItem()
    {
        if (isBuying)
        {
            Debug.LogWarning("[ShopItemUI] Buy diblock sebab dipanggil dua kali.");
            return;
        }

        isBuying = true;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();

        if (shopManager == null)
        {
            Debug.LogError("[ShopItemUI] ShopManager belum assign pada " + gameObject.name);
            isBuying = false;
            return;
        }

        Debug.Log("[ShopItemUI] BUY CLICK -> " + itemId + " | price: " + price + " | amount: " + amount);

        bool success = shopManager.TryBuyItem(itemId, itemName, price, amount);

        if (!success)
            Debug.LogWarning("[ShopItemUI] Buy gagal untuk item: " + itemId);
        else
            Debug.Log("[ShopItemUI] Buy berjaya untuk item: " + itemId);

        isBuying = false;
    }
}
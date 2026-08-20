using UnityEngine;

public class ShopBuyButton : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;

    [Header("Item Info")]
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private int price = 5;
    [SerializeField] private int amount = 1;

    public void BuyItem()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();

        if (shopManager == null)
        {
            Debug.LogError("ShopManager belum assign pada " + gameObject.name);
            return;
        }

        Debug.Log("BUTTON BUY CLICK -> " + itemId);
        shopManager.TryBuyItem(itemId, itemName, price, amount);
    }
}
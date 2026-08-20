using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItemData
    {
        public string itemId;
        public string itemName;
        public int price = 1;
        public int amount = 1;
    }

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button closeShopButton;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Shop Items")]
    [SerializeField] private ShopItemData[] shopItems;

    [Header("Settings")]
    [SerializeField] private bool hideShopOnStart = true;
    [SerializeField] private bool bringShopToFront = true;
    [SerializeField] private bool debugMode = true;

    private bool isProcessingPurchase = false;

    public bool IsShopOpen => shopPanel != null && shopPanel.activeInHierarchy;

    // Compatibility untuk script lama
    public bool IgnoreNextWrongClick { get; private set; }

    private void Awake()
    {
        RegisterButtons();
    }

    private void OnEnable()
    {
        CurrencyManager.OnCoinsChanged += HandleCoinsChanged;
        RegisterButtons();
    }

    private void OnDisable()
    {
        CurrencyManager.OnCoinsChanged -= HandleCoinsChanged;
    }

    private void Start()
    {
        RegisterButtons();

        if (shopPanel != null && hideShopOnStart)
            shopPanel.SetActive(false);

        RefreshCoinsUI();
        ClearFeedback();

        if (debugMode)
        {
            Debug.Log("[ShopManager] Start aktif pada object: " + gameObject.name);

            if (shopPanel == null)
                Debug.LogError("[ShopManager] Shop Panel belum assign!");

            if (openShopButton == null)
                Debug.LogWarning("[ShopManager] Open Shop Button belum assign.");

            if (closeShopButton == null)
                Debug.LogWarning("[ShopManager] Close Shop Button belum assign.");

            if (shopItems == null || shopItems.Length == 0)
                Debug.LogWarning("[ShopManager] Shop Items masih kosong. Isi dekat Inspector.");
        }
    }

    private void RegisterButtons()
    {
        if (openShopButton != null)
        {
            openShopButton.onClick.RemoveListener(OpenShop);
            openShopButton.onClick.AddListener(OpenShop);
        }

        if (closeShopButton != null)
        {
            closeShopButton.onClick.RemoveListener(CloseShop);
            closeShopButton.onClick.AddListener(CloseShop);
        }
    }

    public void OpenShop()
    {
        if (debugMode)
            Debug.Log("[ShopManager] OpenShop dipanggil.");

        IgnoreNextWrongClick = true;

        if (shopPanel == null)
        {
            Debug.LogError("[ShopManager] Tak boleh buka shop sebab shopPanel belum assign.");
            return;
        }

        ActivateParents(shopPanel.transform);

        shopPanel.SetActive(true);

        CanvasGroup canvasGroup = shopPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (bringShopToFront)
            shopPanel.transform.SetAsLastSibling();

        RefreshCoinsUI();
        ClearFeedback();

        CancelInvoke(nameof(ClearIgnoreNextWrongClick));
        Invoke(nameof(ClearIgnoreNextWrongClick), 0.1f);

        if (debugMode)
            Debug.Log("[ShopManager] ShopPanel status sekarang: " + shopPanel.activeInHierarchy);
    }

    public void CloseShop()
    {
        if (debugMode)
            Debug.Log("[ShopManager] CloseShop dipanggil.");

        IgnoreNextWrongClick = true;

        if (shopPanel != null)
            shopPanel.SetActive(false);

        ClearFeedback();

        CancelInvoke(nameof(ClearIgnoreNextWrongClick));
        Invoke(nameof(ClearIgnoreNextWrongClick), 0.1f);
    }

    private void ActivateParents(Transform target)
    {
        if (target == null)
            return;

        Transform current = target.parent;

        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                if (debugMode)
                    Debug.LogWarning("[ShopManager] Parent inactive dijumpai dan diaktifkan: " + current.name);

                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    private void ClearIgnoreNextWrongClick()
    {
        IgnoreNextWrongClick = false;
    }

    public void ConsumeIgnoreNextWrongClick()
    {
        IgnoreNextWrongClick = false;
    }

    public void BuyItemByIndex(int index)
    {
        if (shopItems == null || shopItems.Length == 0)
        {
            ShowFeedback("Shop item belum setup.");
            Debug.LogWarning("[ShopManager] BuyItemByIndex gagal sebab shopItems kosong.");
            return;
        }

        if (index < 0 || index >= shopItems.Length)
        {
            ShowFeedback("Item tak jumpa.");
            Debug.LogWarning("[ShopManager] BuyItemByIndex gagal. Index tak valid: " + index);
            return;
        }

        ShopItemData item = shopItems[index];

        if (item == null)
        {
            ShowFeedback("Item kosong.");
            Debug.LogWarning("[ShopManager] Item index " + index + " kosong.");
            return;
        }

        TryBuyItem(item.itemId, item.itemName, item.price, item.amount);
    }

    // Optional shortcut kalau lebih senang pilih function tanpa isi int
    public void BuyItem0()
    {
        BuyItemByIndex(0);
    }

    public void BuyItem1()
    {
        BuyItemByIndex(1);
    }

    public void BuyItem2()
    {
        BuyItemByIndex(2);
    }

    public void BuyItem3()
    {
        BuyItemByIndex(3);
    }

    public void BuyItem4()
    {
        BuyItemByIndex(4);
    }

    public void BuyItem5()
    {
        BuyItemByIndex(5);
    }

    public bool TryBuyItem(string itemId, string itemName, int price, int amount)
    {
        if (isProcessingPurchase)
        {
            Debug.LogWarning("[ShopManager] Purchase diblock sebab function dipanggil dua kali.");
            return false;
        }

        isProcessingPurchase = true;
        IgnoreNextWrongClick = true;

        if (CurrencyManager.Instance == null)
        {
            ShowFeedback("CurrencyManager tak jumpa.");
            Debug.LogError("[ShopManager] CurrencyManager.Instance null.");
            isProcessingPurchase = false;
            return false;
        }

        if (string.IsNullOrEmpty(itemId))
        {
            ShowFeedback("Item ID kosong.");
            Debug.LogWarning("[ShopManager] Item ID kosong.");
            isProcessingPurchase = false;
            return false;
        }

        if (price < 0 || amount <= 0)
        {
            ShowFeedback("Harga atau jumlah item tak valid.");
            Debug.LogWarning("[ShopManager] Harga atau amount tak valid. Price: " + price + " | Amount: " + amount);
            isProcessingPurchase = false;
            return false;
        }

        int currentCoins = CurrencyManager.Instance.GetCoins();

        Debug.Log("[ShopManager] TRY BUY -> " + itemId +
                  " | Item Name: " + itemName +
                  " | Coins: " + currentCoins +
                  " | Price: " + price +
                  " | Amount: " + amount);

        bool success = CurrencyManager.Instance.SpendCoins(price);

        if (!success)
        {
            ShowFeedback("Coin tak cukup.");

            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayNotEnoughCoin();

            Debug.LogWarning("[ShopManager] SHOP FAIL -> coins tak cukup | Coins: " +
                             CurrencyManager.Instance.GetCoins() + " | Price: " + price);

            RefreshCoinsUI();
            isProcessingPurchase = false;

            CancelInvoke(nameof(ClearIgnoreNextWrongClick));
            Invoke(nameof(ClearIgnoreNextWrongClick), 0.1f);

            return false;
        }

        CurrencyManager.Instance.AddItem(itemId, amount);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayCoinUse();

        ShowFeedback("Berjaya beli " + itemName + " x" + amount);
        RefreshCoinsUI();

        Debug.Log("[ShopManager] SHOP SUCCESS -> item: " + itemId +
                  " | total item: " + CurrencyManager.Instance.GetItemAmount(itemId) +
                  " | baki coins: " + CurrencyManager.Instance.GetCoins());

        isProcessingPurchase = false;

        CancelInvoke(nameof(ClearIgnoreNextWrongClick));
        Invoke(nameof(ClearIgnoreNextWrongClick), 0.1f);

        return true;
    }

    private void HandleCoinsChanged(int newCoins)
    {
        SetCoinsText(newCoins);
    }

    private void RefreshCoinsUI()
    {
        if (CurrencyManager.Instance == null)
        {
            SetCoinsText(0);
            return;
        }

        SetCoinsText(CurrencyManager.Instance.GetCoins());
    }

    private void SetCoinsText(int coins)
    {
        if (coinsText == null)
            return;

        coinsText.text = "Coins: " + coins;
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }
}
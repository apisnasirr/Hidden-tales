using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public static event Action<int> OnCoinsChanged;
    public static event Action<string, int> OnItemAmountChanged;

    [Header("Debug Start Value")]
    [SerializeField] private int startCoins = 0;

    [Header("Coin Text Format")]
    [SerializeField] private string coinsPrefix = "Coins: ";

    private static bool isInitialized = false;
    private static int currentCoins = 0;
    private static readonly Dictionary<string, int> itemInventory = new Dictionary<string, int>();

    private readonly List<TMP_Text> coinsTexts = new List<TMP_Text>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("CurrencyManager duplicate dibuang: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!isInitialized)
        {
            currentCoins = Mathf.Max(0, startCoins);
            itemInventory.Clear();
            isInitialized = true;
        }

        Debug.Log("CurrencyManager aktif | InstanceID: " + GetInstanceID() + " | Coins: " + currentCoins);
    }

    private void Start()
    {
        RefreshCoinsUI();
        OnCoinsChanged?.Invoke(currentCoins);

        foreach (var pair in itemInventory)
            OnItemAmountChanged?.Invoke(pair.Key, pair.Value);
    }

    public int GetCoins()
    {
        return currentCoins;
    }

    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Max(0, amount);

        RefreshCoinsUI();
        OnCoinsChanged?.Invoke(currentCoins);

        Debug.Log("SetCoins -> " + currentCoins);
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;

        if (currentCoins < 0)
            currentCoins = 0;

        RefreshCoinsUI();
        OnCoinsChanged?.Invoke(currentCoins);

        Debug.Log("AddCoins -> jumlah baru: " + currentCoins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("SpendCoins gagal | amount tak valid: " + amount);
            return false;
        }

        Debug.Log("SpendCoins cuba | Coins: " + currentCoins + " | Price: " + amount);

        if (currentCoins < amount)
        {
            Debug.LogWarning("SpendCoins gagal | Coins tak cukup | Coins: " + currentCoins + " | Price: " + amount);
            return false;
        }

        currentCoins -= amount;

        RefreshCoinsUI();
        OnCoinsChanged?.Invoke(currentCoins);

        Debug.Log("SpendCoins berjaya | Baki: " + currentCoins);
        return true;
    }

    public void AddItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("AddItem gagal | itemId kosong");
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("AddItem gagal | amount tak valid: " + amount);
            return;
        }

        if (!itemInventory.ContainsKey(itemId))
            itemInventory[itemId] = 0;

        itemInventory[itemId] += amount;

        OnItemAmountChanged?.Invoke(itemId, itemInventory[itemId]);

        Debug.Log("AddItem -> " + itemId + " | jumlah: " + itemInventory[itemId]);
    }

    public bool UseItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return false;

        int currentAmount = GetItemAmount(itemId);

        if (currentAmount < amount)
            return false;

        itemInventory[itemId] = currentAmount - amount;

        OnItemAmountChanged?.Invoke(itemId, itemInventory[itemId]);

        Debug.Log("UseItem -> " + itemId + " | baki: " + itemInventory[itemId]);
        return true;
    }

    public int GetItemAmount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return 0;

        if (itemInventory.TryGetValue(itemId, out int amount))
            return amount;

        return 0;
    }

    public void SetCoinsText(TMP_Text newCoinsText)
    {
        if (newCoinsText == null)
            return;

        if (!coinsTexts.Contains(newCoinsText))
            coinsTexts.Add(newCoinsText);

        RefreshCoinsUI();

        Debug.Log("Coins text didaftarkan: " + newCoinsText.name);
    }

    public void ClearCoinsTextReference(TMP_Text textToClear)
    {
        if (textToClear == null)
            return;

        if (coinsTexts.Contains(textToClear))
            coinsTexts.Remove(textToClear);
    }

    private void RefreshCoinsUI()
    {
        for (int i = coinsTexts.Count - 1; i >= 0; i--)
        {
            if (coinsTexts[i] == null)
            {
                coinsTexts.RemoveAt(i);
                continue;
            }

            coinsTexts[i].text = coinsPrefix + currentCoins;
        }
    }

    public void DebugResetAllData()
    {
        currentCoins = 0;
        itemInventory.Clear();

        RefreshCoinsUI();
        OnCoinsChanged?.Invoke(currentCoins);
    }
}
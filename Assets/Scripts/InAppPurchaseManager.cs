using UnityEngine;
using System;
using System.Collections.Generic;

public class InAppPurchaseManager : MonoBehaviour
{
    public static InAppPurchaseManager Instance { get; private set; }

    [System.Serializable]
    public class IAPProduct
    {
        public string productId;
        public string name;
        public float price;
        public string description;
        public bool isSubscription;
    }

    public IAPProduct[] availableProducts;
    
    private bool isInitialized;
    
    public event Action<string> OnPurchaseComplete;
    public event Action<string> OnPurchaseFailed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // TODO: Initialize your IAP SDK here
        // Example: Unity IAP initialization
        isInitialized = true;
    }

    public void PurchaseProduct(string productId)
    {
        if (!isInitialized)
        {
            OnPurchaseFailed?.Invoke("IAP not initialized");
            return;
        }

        // TODO: Implement actual purchase through your IAP SDK
        // For now, we'll simulate a purchase
        SimulatePurchase(productId);
    }

    private void SimulatePurchase(string productId)
    {
        // In real implementation, this would be handled by the IAP SDK
        bool success = UnityEngine.Random.value > 0.3f;
        
        if (success)
        {
            ProcessPurchase(productId);
        }
        else
        {
            OnPurchaseFailed?.Invoke("Purchase failed");
        }
    }

    private void ProcessPurchase(string productId)
    {
        switch (productId)
        {
            case "remove_ads":
                PlayerPrefs.SetInt("AdsRemoved", 1);
                break;
            case "unlock_all":
                PlayerPrefs.SetInt("AllUnlocked", 1);
                break;
            case "coins_pack_1":
                AddCoins(1000);
                break;
            // Add more products as needed
        }
        
        PlayerPrefs.Save();
        OnPurchaseComplete?.Invoke(productId);
    }

    private void AddCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        PlayerPrefs.SetInt("Coins", currentCoins + amount);
    }
}
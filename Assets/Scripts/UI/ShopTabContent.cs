using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class ShopTabContent : TabContent
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemId;
        public string displayName;
        public int price;
        public Sprite icon;
        public Button purchaseButton;
        public TextMeshProUGUI priceText;
        public Image itemImage;
    }

    [FormerlySerializedAs("coinsText")]
    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI _coinsText;
    [FormerlySerializedAs("gemsText")] [SerializeField] private TextMeshProUGUI _gemsText;
    [FormerlySerializedAs("addCoinsButton")] [SerializeField] private Button _addCoinsButton;
    [FormerlySerializedAs("addGemsButton")] [SerializeField] private Button _addGemsButton;

    [FormerlySerializedAs("shopItems")]
    [Header("Shop Items")]
    [SerializeField] private ShopItem[] _shopItems;
    [FormerlySerializedAs("itemsContainer")] [SerializeField] private Transform _itemsContainer;
    [FormerlySerializedAs("itemPrefab")] [SerializeField] private GameObject _itemPrefab;

    [FormerlySerializedAs("categoryButtons")]
    [Header("Categories")]
    [SerializeField] private Button[] _categoryButtons;
    private int _currentCategory = 0;

    private void Start()
    {
        InitializeShop();
        SetupListeners();
    }

    private void InitializeShop()
    {
        UpdateCurrencyDisplay();
        
        foreach (var item in _shopItems)
        {
            SetupShopItem(item);
        }

        SelectCategory(0);
    }

    private void SetupListeners()
    {
        _addCoinsButton.onClick.AddListener(OnAddCoinsPressed);
        _addGemsButton.onClick.AddListener(OnAddGemsPressed);

        for (int i = 0; i < _categoryButtons.Length; i++)
        {
            int index = i;
            _categoryButtons[i].onClick.AddListener(() => SelectCategory(index));
        }
    }

    public override void RefreshContent()
    {
        UpdateCurrencyDisplay();
        UpdateItemAvailability();
    }

    private void UpdateCurrencyDisplay()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        int gems = PlayerPrefs.GetInt("Gems", 0);
        
        _coinsText.text = coins.ToString("N0");
        _gemsText.text = gems.ToString("N0");
    }

    private void SetupShopItem(ShopItem item)
    {
        item.priceText.text = item.price.ToString();
        item.itemImage.sprite = item.icon;
        item.purchaseButton.onClick.AddListener(() => OnItemPurchase(item));
    }

    private void UpdateItemAvailability()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        int gems = PlayerPrefs.GetInt("Gems", 0);

        foreach (var item in _shopItems)
        {
            bool canAfford = coins >= item.price; // Adjust based on currency type
            item.purchaseButton.interactable = canAfford;
        }
    }

    private void OnItemPurchase(ShopItem item)
    {
        // TODO: Implement purchase logic
        Debug.Log($"Attempting to purchase {item.displayName}");
    }

    private void SelectCategory(int index)
    {
        _currentCategory = index;
        // Update category selection visual state
        for (int i = 0; i < _categoryButtons.Length; i++)
        {
            _categoryButtons[i].interactable = i != index;
        }
        // TODO: Filter items based on category
    }

    private void OnAddCoinsPressed()
    {
        // TODO: Implement IAP or reward video for coins
        Debug.Log("Opening coins purchase dialog");
    }

    private void OnAddGemsPressed()
    {
        // TODO: Implement IAP for gems
        Debug.Log("Opening gems purchase dialog");
    }
}
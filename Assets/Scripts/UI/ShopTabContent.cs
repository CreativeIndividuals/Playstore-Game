using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShopTabContent : TabContent
{
    [System.Serializable]
    public class ShopCategory
    {
        public string categoryName;
        public Button categoryButton;
        public GameObject categoryPanel;
        public Transform itemContainer;
        public Sprite categoryIcon;
        public string description;
        public bool isUnlocked = true;
    }

    [System.Serializable]
    public class ShopItem
    {
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        public int price;
        public bool isPurchased;
        public ItemType itemType;
        public int value; // For consumables or powerups
        public bool isUnlocked = true;
        public int unlockLevel = 1;
        [TextArea(3, 5)]
        public string unlockRequirement;
    }

    public enum ItemType
    {
        Consumable,
        Powerup,
        Cosmetic,
        Currency,
        Character,
        Special
    }

    [Header("Shop UI")]
    [SerializeField] private List<ShopCategory> categories = new List<ShopCategory>();
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Text currencyText;
    [SerializeField] private GameObject noItemsMessage;
    
    [Header("Item Details")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private Image detailsIcon;
    [SerializeField] private Text detailsName;
    [SerializeField] private Text detailsDescription;
    [SerializeField] private Text detailsPrice;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Text purchaseButtonText;
    [SerializeField] private GameObject purchaseConfirmationPanel;
    [SerializeField] private Text confirmationText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Sale Settings")]
    [SerializeField] private float saleDiscount = 0.2f;
    [SerializeField] private Color saleColor = Color.red;
    [SerializeField] private GameObject saleBadgePrefab;
    [SerializeField] private float saleDuration = 3600f; // Duration in seconds
    [SerializeField] private Text saleTimerText;

    [Header("Effects")]
    [SerializeField] private ParticleSystem purchaseEffect;
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private GameObject coinSpawnEffect;
    
    [Header("Animation")]
    [SerializeField] private float itemAppearDelay = 0.1f;
    [SerializeField] private AnimationCurve itemAppearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private ShopCategory currentCategory;
    private Dictionary<string, List<ShopItem>> categoryItems;
    private ShopItem selectedItem;
    private List<ShopItem> saleItems = new List<ShopItem>();
    private float saleTimeRemaining;
    private AudioSource audioSource;
    private bool isProcessingPurchase;

    protected override void Awake()
    {
        base.Awake();
        audioSource = gameObject.AddComponent<AudioSource>();
        categoryItems = new Dictionary<string, List<ShopItem>>();
    }

    protected override IEnumerator LoadContent()
    {
        ShowLoading();

        // Initialize dictionary and load saved data
        yield return LoadShopData();

        // Set up UI elements
        SetupUIElements();

        // Generate initial sales
        GenerateRandomSales();

        // Select first category by default
        if (categories.Count > 0)
        {
            yield return SelectCategory(categories[0]);
        }

        HideLoading();
    }

    private IEnumerator LoadShopData()
    {
        // Initialize categories
        foreach (var category in categories)
        {
            categoryItems[category.categoryName] = new List<ShopItem>();
            
            // Load category unlock status
            category.isUnlocked = PlayerPrefs.GetInt($"ShopCategory_{category.categoryName}_Unlocked", 
                category.isUnlocked ? 1 : 0) == 1;
        }

        // Simulate loading shop data (replace with actual data loading)
        yield return new WaitForSeconds(0.5f);

        // Load saved purchase status for items
        foreach (var category in categoryItems)
        {
            foreach (var item in category.Value)
            {
                item.isPurchased = PlayerPrefs.GetInt($"ShopItem_{item.itemId}_Purchased", 0) == 1;
                
                if (item.itemType == ItemType.Consumable)
                {
                    item.value = PlayerPrefs.GetInt($"ShopItem_{item.itemId}_Count", 0);
                }
            }
        }
    }

    private void SetupUIElements()
    {
        // Set up category buttons
        foreach (var category in categories)
        {
            category.categoryButton.onClick.AddListener(() => StartCoroutine(SelectCategory(category)));
            
            // Set category icon if available
            var iconImage = category.categoryButton.GetComponentInChildren<Image>();
            if (iconImage && category.categoryIcon)
            {
                iconImage.sprite = category.categoryIcon;
            }
        }

        // Set up purchase buttons
        purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        confirmButton.onClick.AddListener(ConfirmPurchase);
        cancelButton.onClick.AddListener(CancelPurchase);

        // Hide panels initially
        detailsPanel.SetActive(false);
        purchaseConfirmationPanel.SetActive(false);
        noItemsMessage.SetActive(false);
    }

    private IEnumerator SelectCategory(ShopCategory category)
    {
        if (!category.isUnlocked)
        {
            PlaySound(errorSound);
            yield break;
        }

        PlaySound(selectSound);

        if (currentCategory != null)
            currentCategory.categoryPanel.SetActive(false);

        currentCategory = category;
        currentCategory.categoryPanel.SetActive(true);

        // Clear existing items
        foreach (Transform child in category.itemContainer)
            Destroy(child.gameObject);

        // Check if category has items
        if (categoryItems[category.categoryName].Count == 0)
        {
            noItemsMessage.SetActive(true);
            yield break;
        }

        noItemsMessage.SetActive(false);

        // Populate items with animation
        float delay = 0;
        foreach (var item in categoryItems[category.categoryName])
        {
            yield return new WaitForSeconds(itemAppearDelay);
            CreateShopItemUI(item, category.itemContainer, delay);
            delay += itemAppearDelay;
        }

        // Hide details panel when switching categories
        detailsPanel.SetActive(false);
        purchaseConfirmationPanel.SetActive(false);
    }

    private void CreateShopItemUI(ShopItem item, Transform container, float delay)
    {
        GameObject itemUI = Instantiate(shopItemPrefab, container);
        SetupShopItemUI(itemUI, item);
        
        // Add animation
        StartCoroutine(AnimateItemAppearance(itemUI, delay));
    }

    private IEnumerator AnimateItemAppearance(GameObject itemUI, float delay)
    {
        // Initial state
        itemUI.transform.localScale = Vector3.zero;
        CanvasGroup canvasGroup = itemUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = itemUI.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        yield return new WaitForSeconds(delay);

        float elapsed = 0;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = itemAppearCurve.Evaluate(elapsed / duration);
            
            itemUI.transform.localScale = Vector3.one * progress;
            canvasGroup.alpha = progress;
            
            yield return null;
        }

        itemUI.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1;
    }

    private void SetupShopItemUI(GameObject itemUI, ShopItem item)
    {
        // Set basic item info
        itemUI.GetComponentInChildren<Text>().text = item.itemName;
        var iconImage = itemUI.GetComponentInChildren<Image>();
        if (iconImage) iconImage.sprite = item.icon;

        // Set up price display
        SetupPriceDisplay(itemUI, item);

        // Set up item count (for consumables)
        SetupItemCount(itemUI, item);

        // Set up lock status
        SetupLockStatus(itemUI, item);

        // Add click handler
        Button button = itemUI.GetComponent<Button>();
        button.onClick.AddListener(() => ShowItemDetails(item));

        // Set up sale badge if applicable
        if (saleItems.Contains(item))
        {
            AddSaleBadge(itemUI);
        }
    }

    private void SetupPriceDisplay(GameObject itemUI, ShopItem item)
    {
        var priceText = itemUI.transform.Find("PriceText")?.GetComponent<Text>();
        if (priceText == null) return;

        if (saleItems.Contains(item))
        {
            int discountedPrice = CalculateDiscountedPrice(item);
            priceText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(saleColor)}>" +
                            $"<s>{item.price}</s> {discountedPrice}</color>";
        }
        else
        {
            priceText.text = item.price.ToString();
        }
    }

    private void SetupItemCount(GameObject itemUI, ShopItem item)
    {
        var countText = itemUI.transform.Find("CountText")?.GetComponent<Text>();
        if (countText != null && item.itemType == ItemType.Consumable)
        {
            countText.text = item.value > 0 ? $"x{item.value}" : "";
        }
    }

    private void SetupLockStatus(GameObject itemUI, ShopItem item)
    {
        var lockIcon = itemUI.transform.Find("LockIcon")?.gameObject;
        if (lockIcon != null)
        {
            lockIcon.SetActive(!item.isUnlocked);
        }

        // Dim the item if it's locked
        var itemImage = itemUI.GetComponent<Image>();
        if (itemImage != null)
        {
            itemImage.color = item.isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    private void AddSaleBadge(GameObject itemUI)
    {
        if (saleBadgePrefab != null)
        {
            var badge = Instantiate(saleBadgePrefab, itemUI.transform);
            badge.transform.SetAsLastSibling();
        }
    }

    private void ShowItemDetails(ShopItem item)
    {
        if (!item.isUnlocked)
        {
            ShowUnlockRequirements(item);
            return;
        }

        PlaySound(selectSound);
        selectedItem = item;
        detailsPanel.SetActive(true);
        purchaseConfirmationPanel.SetActive(false);
        
        detailsIcon.sprite = item.icon;
        detailsName.text = item.itemName;
        detailsDescription.text = item.description;
        
        UpdatePriceDisplay();
        UpdatePurchaseButton();
    }

    private void ShowUnlockRequirements(ShopItem item)
    {
        detailsPanel.SetActive(true);
        detailsName.text = "Locked";
        detailsDescription.text = item.unlockRequirement;
        purchaseButton.interactable = false;
        purchaseButtonText.text = $"Unlock at Level {item.unlockLevel}";
    }

    private void UpdatePriceDisplay()
    {
        if (selectedItem == null) return;

        if (saleItems.Contains(selectedItem))
        {
            int discountedPrice = CalculateDiscountedPrice(selectedItem);
            detailsPrice.text = $"<color=#{ColorUtility.ToHtmlStringRGB(saleColor)}>" +
                               $"<s>{selectedItem.price}</s> {discountedPrice}</color>";
        }
        else
        {
            detailsPrice.text = selectedItem.price.ToString();
        }
    }

    private void UpdatePurchaseButton()
    {
        if (selectedItem == null) return;

        bool canPurchase = !selectedItem.isPurchased || selectedItem.itemType == ItemType.Consumable;
        purchaseButton.interactable = canPurchase;
        
        if (selectedItem.isPurchased)
        {
            purchaseButtonText.text = selectedItem.itemType == ItemType.Consumable ? "BUY MORE" : "OWNED";
        }
        else
        {
            int price = CalculateDiscountedPrice(selectedItem);
            purchaseButtonText.text = $"BUY {price}";
        }
    }

    private void OnPurchaseButtonClicked()
    {
        if (selectedItem == null || isProcessingPurchase) return;

        int price = CalculateDiscountedPrice(selectedItem);
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        
        if (currentCoins >= price)
        {
            ShowPurchaseConfirmation();
        }
        else
        {
            ShowInsufficientFundsMessage();
        }
    }

    private void ShowPurchaseConfirmation()
    {
        purchaseConfirmationPanel.SetActive(true);
        confirmationText.text = $"Are you sure you want to buy {selectedItem.itemName}?";
    }

    private void ShowInsufficientFundsMessage()
    {
        PlaySound(errorSound);
        // Implement your insufficient funds UI here
        Debug.Log("Not enough coins!");
    }

    private void CancelPurchase()
    {
        PlaySound(selectSound);
        purchaseConfirmationPanel.SetActive(false);
    }

    private void ConfirmPurchase()
    {
        if (isProcessingPurchase) return;
        StartCoroutine(ProcessPurchase());
    }

    private IEnumerator ProcessPurchase()
    {
        isProcessingPurchase = true;
        
        int price = CalculateDiscountedPrice(selectedItem);
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        
        // Process the purchase
        currentCoins -= price;
        PlayerPrefs.SetInt("Coins", currentCoins);

        // Handle item acquisition
        switch (selectedItem.itemType)
        {
            case ItemType.Consumable:
                selectedItem.value++;
                PlayerPrefs.SetInt($"Item_{selectedItem.itemId}_Count", selectedItem.value);
                break;
            
            case ItemType.Powerup:
                selectedItem.isPurchased = true;
                PlayerPrefs.SetInt($"Powerup_{selectedItem.itemId}_Level", 1);
                break;
            
            default:
                selectedItem.isPurchased = true;
                break;
        }

        // Save purchase status
        if (selectedItem.isPurchased)
        {
            PlayerPrefs.SetInt($"ShopItem_{selectedItem.itemId}_Purchased", 1);
        }

        // Play effects
        PlayPurchaseEffects();

        // Update UI
        UpdateCurrencyDisplay();
                yield return new WaitForSeconds(0.5f); // Wait for effects to play

        // Hide confirmation panel
        purchaseConfirmationPanel.SetActive(false);

        // Refresh UI
        ShowItemDetails(selectedItem);
        yield return SelectCategory(currentCategory);

        isProcessingPurchase = false;
    }

    private void PlayPurchaseEffects()
    {
        // Play sound effect
        PlaySound(purchaseSound);

        // Play particle effect if available
        if (purchaseEffect != null)
        {
            purchaseEffect.Play();
        }

        // Spawn coin effect if available
        if (coinSpawnEffect != null)
        {
            Instantiate(coinSpawnEffect, transform.position, Quaternion.identity);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void GenerateRandomSales()
    {
        saleItems.Clear();
        saleTimeRemaining = saleDuration;

        foreach (var category in categoryItems.Values)
        {
            foreach (var item in category)
            {
                if (Random.value < 0.2f && !item.isPurchased) // 20% chance for unpurchased items
                {
                    saleItems.Add(item);
                }
            }
        }
    }

    private int CalculateDiscountedPrice(ShopItem item)
    {
        return saleItems.Contains(item) 
            ? Mathf.RoundToInt(item.price * (1 - saleDiscount))
            : item.price;
    }

    private void UpdateCurrencyDisplay()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        currencyText.text = $"{coins:N0}";
    }

    private void Update()
    {
        if (saleItems.Count > 0)
        {
            UpdateSaleTimer();
        }
    }

    private void UpdateSaleTimer()
    {
        if (saleTimeRemaining > 0)
        {
            saleTimeRemaining -= Time.deltaTime;
            if (saleTimerText != null)
            {
                int hours = Mathf.FloorToInt(saleTimeRemaining / 3600);
                int minutes = Mathf.FloorToInt((saleTimeRemaining % 3600) / 60);
                int seconds = Mathf.FloorToInt(saleTimeRemaining % 60);
                saleTimerText.text = $"Sale ends in: {hours:00}:{minutes:00}:{seconds:00}";
            }
        }
        else
        {
            // Sale ended
            saleItems.Clear();
            if (saleTimerText != null)
            {
                saleTimerText.text = "No active sales";
            }
            
            // Refresh current category to update prices
            if (currentCategory != null)
            {
                StartCoroutine(SelectCategory(currentCategory));
            }
        }
    }

    protected override IEnumerator RefreshContent()
    {
        UpdateCurrencyDisplay();
        
        // Refresh current category if one is selected
        if (currentCategory != null)
        {
            yield return SelectCategory(currentCategory);
        }
    }

    private void OnEnable()
    {
        // Refresh sales when tab becomes active
        if (isInitialized)
        {
            GenerateRandomSales();
            if (currentCategory != null)
            {
                StartCoroutine(SelectCategory(currentCategory));
            }
        }
    }

    private void OnDisable()
    {
        // Clear selected item and hide panels
        selectedItem = null;
        detailsPanel.SetActive(false);
        purchaseConfirmationPanel.SetActive(false);
    }

    protected override void OnDestroy()
    {
        // Clean up button listeners
        foreach (var category in categories)
        {
            if (category.categoryButton != null)
            {
                category.categoryButton.onClick.RemoveAllListeners();
            }
        }

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
        }
    }

    // Helper method to save all shop data
    private void SaveShopData()
    {
        foreach (var category in categoryItems)
        {
            foreach (var item in category.Value)
            {
                PlayerPrefs.SetInt($"ShopItem_{item.itemId}_Purchased", item.isPurchased ? 1 : 0);
                if (item.itemType == ItemType.Consumable)
                {
                    PlayerPrefs.SetInt($"ShopItem_{item.itemId}_Count", item.value);
                }
            }
        }
        PlayerPrefs.Save();
    }
}
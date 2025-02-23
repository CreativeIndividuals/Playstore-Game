using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharactersTabContent : TabContent
{
    [System.Serializable]
    public class Character
    {
        public string id;
        public string characterName;
        public Sprite portrait;
        public Sprite fullBodyImage;
        public int unlockCost;
        public bool isDefault;
        [HideInInspector] public bool isUnlocked;
        public string[] abilities;
        public int powerLevel;
    }

    [Header("Character Display")]
    [SerializeField] private Image _selectedCharacterImage;
    [SerializeField] private TextMeshProUGUI _characterNameText;
    [SerializeField] private TextMeshProUGUI _powerLevelText;
    [SerializeField] private TextMeshProUGUI _abilitiesText;
    
    [Header("Character List")]
    [SerializeField] private Transform _charactersGrid;
    [SerializeField] private GameObject _characterButtonPrefab;
    [SerializeField] private Character[] _availableCharacters;

    [Header("Actions")]
    [SerializeField] private Button _selectButton;
    [SerializeField] private Button _unlockButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private TextMeshProUGUI _unlockCostText;
    [SerializeField] private TextMeshProUGUI _currentCoinsText;

    private Character _selectedCharacter;
    private List<GameObject> _characterButtons = new List<GameObject>();

    // Events
    public System.Action<Character> onCharacterSelected;
    public System.Action<Character> onCharacterUnlocked;
    public System.Action<Character> onCharacterUpgraded;

    void Start()
    {
        InitializeCharacters();
        SetupListeners();
        UpdateCoinsDisplay();
    }

    private void InitializeCharacters()
    {
        ClearExistingButtons();
        CreateCharacterButtons();
        SelectDefaultCharacter();
    }

    private void ClearExistingButtons()
    {
        foreach (var button in _characterButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        _characterButtons.Clear();
    }

    private void CreateCharacterButtons()
    {
        foreach (var character in _availableCharacters)
        {
            character.isUnlocked = character.isDefault || 
                                 PlayerPrefs.GetInt($"Character_{character.id}_Unlocked", 0) == 1;
            
            GameObject buttonObj = Instantiate(_characterButtonPrefab, _charactersGrid);
            SetupCharacterButton(buttonObj, character);
            _characterButtons.Add(buttonObj);
        }
    }

    private void SetupCharacterButton(GameObject buttonObj, Character character)
    {
        Button button = buttonObj.GetComponent<Button>();
        Image portrait = buttonObj.GetComponentInChildren<Image>();
        
        portrait.sprite = character.portrait;
        button.onClick.AddListener(() => SelectCharacter(character));
        
        // Setup lock icon
        Transform lockIcon = buttonObj.transform.Find("LockIcon");
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(!character.isUnlocked);
        }

        // Setup level or power indicator if exists
        TextMeshProUGUI levelText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (levelText != null)
        {
            int level = PlayerPrefs.GetInt($"Character_{character.id}_Level", 1);
            levelText.text = $"Lvl {level}";
        }
    }

    private void SelectDefaultCharacter()
    {
        string savedCharacterId = PlayerPrefs.GetString("SelectedCharacter", "");
        Character defaultCharacter = null;

        // Try to select previously selected character
        if (!string.IsNullOrEmpty(savedCharacterId))
        {
            defaultCharacter = System.Array.Find(_availableCharacters, 
                c => c.id == savedCharacterId && c.isUnlocked);
        }

        // If no previous character or it's locked, select first unlocked character
        if (defaultCharacter == null)
        {
            defaultCharacter = System.Array.Find(_availableCharacters, c => c.isUnlocked);
        }

        if (defaultCharacter != null)
        {
            SelectCharacter(defaultCharacter);
        }
    }

    private void SetupListeners()
    {
        _selectButton?.onClick.AddListener(OnSelectPressed);
        _unlockButton?.onClick.AddListener(OnUnlockPressed);
        _upgradeButton?.onClick.AddListener(OnUpgradePressed);
    }

    public override void RefreshContent()
    {
        UpdateCharacterDisplay();
        UpdateButtonStates();
        UpdateCoinsDisplay();
    }

    private void SelectCharacter(Character character)
    {
        _selectedCharacter = character;
        UpdateCharacterDisplay();
        UpdateButtonStates();
    }

    private void UpdateCharacterDisplay()
    {
        if (_selectedCharacter == null) return;

        _selectedCharacterImage.sprite = _selectedCharacter.fullBodyImage;
        _characterNameText.text = _selectedCharacter.characterName;
        
        int characterLevel = PlayerPrefs.GetInt($"Character_{_selectedCharacter.id}_Level", 1);
        _powerLevelText.text = $"Power Level: {_selectedCharacter.powerLevel * characterLevel}";
        
        string abilities = string.Join("\n• ", _selectedCharacter.abilities);
        _abilitiesText.text = $"Abilities:\n• {abilities}";
    }

    private void UpdateButtonStates()
    {
        if (_selectedCharacter == null) return;

        bool isUnlocked = _selectedCharacter.isUnlocked;
        bool isSelected = PlayerPrefs.GetString("SelectedCharacter", "") == _selectedCharacter.id;

        _selectButton.gameObject.SetActive(isUnlocked);
        _selectButton.interactable = !isSelected;
        _unlockButton.gameObject.SetActive(!isUnlocked);
        _upgradeButton.gameObject.SetActive(isUnlocked);

        if (!isUnlocked)
        {
            _unlockCostText.text = $"{_selectedCharacter.unlockCost} Coins";
        }

        // Update upgrade button state
        if (isUnlocked)
        {
            int currentLevel = PlayerPrefs.GetInt($"Character_{_selectedCharacter.id}_Level", 1);
            int upgradeCost = CalculateUpgradeCost(currentLevel);
            _upgradeButton.interactable = PlayerPrefs.GetInt("Coins", 0) >= upgradeCost;
        }
    }

    private void UpdateCoinsDisplay()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        _currentCoinsText.text = $"Coins: {coins}";
    }

    private void OnSelectPressed()
    {
        if (_selectedCharacter == null || !_selectedCharacter.isUnlocked) return;
        
        PlayerPrefs.SetString("SelectedCharacter", _selectedCharacter.id);
        UpdateButtonStates();
        onCharacterSelected?.Invoke(_selectedCharacter);
        
        // Optional: Play selection animation/sound
        PlaySelectionEffect();
    }

    private void OnUnlockPressed()
    {
        if (_selectedCharacter == null || _selectedCharacter.isUnlocked) return;

        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        if (currentCoins >= _selectedCharacter.unlockCost)
        {
            // Deduct coins and unlock character
            PlayerPrefs.SetInt("Coins", currentCoins - _selectedCharacter.unlockCost);
            PlayerPrefs.SetInt($"Character_{_selectedCharacter.id}_Unlocked", 1);
            _selectedCharacter.isUnlocked = true;
            
            // Update UI
            RefreshContent();
            onCharacterUnlocked?.Invoke(_selectedCharacter);
            
            // Play unlock effect
            PlayUnlockEffect();
        }
        else
        {
            ShowInsufficientCoinsMessage();
        }
    }

    private void OnUpgradePressed()
    {
        if (_selectedCharacter == null || !_selectedCharacter.isUnlocked) return;
        
        int currentLevel = PlayerPrefs.GetInt($"Character_{_selectedCharacter.id}_Level", 1);
        int upgradeCost = CalculateUpgradeCost(currentLevel);
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);

        if (currentCoins >= upgradeCost)
        {
            // Process upgrade
            PlayerPrefs.SetInt("Coins", currentCoins - upgradeCost);
            PlayerPrefs.SetInt($"Character_{_selectedCharacter.id}_Level", currentLevel + 1);
            
            // Update UI
            RefreshContent();
            onCharacterUpgraded?.Invoke(_selectedCharacter);
            
            // Play upgrade effect
            PlayUpgradeEffect();
        }
        else
        {
            ShowInsufficientCoinsMessage();
        }
    }

    private int CalculateUpgradeCost(int currentLevel)
    {
        // Example: Cost increases exponentially with level
        return 100 * currentLevel * currentLevel;
    }

    private void PlaySelectionEffect()
    {
        // TODO: Implement selection animation/sound
        Debug.Log($"Selected character: {_selectedCharacter.characterName}");
    }

    private void PlayUnlockEffect()
    {
        // TODO: Implement unlock animation/sound
        Debug.Log($"Unlocked character: {_selectedCharacter.characterName}");
    }

    private void PlayUpgradeEffect()
    {
        // TODO: Implement upgrade animation/sound
        Debug.Log($"Upgraded character: {_selectedCharacter.characterName}");
    }

    private void ShowInsufficientCoinsMessage()
    {
        // TODO: Show UI message for insufficient coins
        Debug.Log("Not enough coins!");
    }

    private void OnDestroy()
    {
        // Clean up listeners
        foreach (var button in _characterButtons)
        {
            if (button != null)
            {
                Button buttonComponent = button.GetComponent<Button>();
                buttonComponent.onClick.RemoveAllListeners();
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CharactersTabContent : TabContent
{
    [System.Serializable]
    public class Character
    {
        public string id;
        public string characterName;
        public Sprite icon;
        public string description;
        public bool isUnlocked;
        public int unlockCost;
        public RuntimeAnimatorController animator;
        public GameObject characterPrefab;
    }

    [Header("Character Display")]
    [SerializeField] private Transform characterGrid;
    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private GameObject characterPreviewArea;
    
    [Header("Character Details")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Text characterDescriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button unlockButton;
    [SerializeField] private Text unlockCostText;
    
    [Header("Characters")]
    [SerializeField] private List<Character> characters = new List<Character>();

    private Character selectedCharacter;
    private string currentCharacterId;
    private GameObject previewCharacter;

    protected override IEnumerator LoadContent()
    {
        // Load current character selection
        currentCharacterId = PlayerPrefs.GetString("SelectedCharacter", characters[0].id);

        // Set up buttons
        selectButton.onClick.AddListener(OnSelectButtonClicked);
        unlockButton.onClick.AddListener(OnUnlockButtonClicked);

        // Create character cards
        CreateCharacterCards();

        // Select current character
        SelectCharacter(characters.Find(c => c.id == currentCharacterId));

        yield break;
    }

    private void CreateCharacterCards()
    {
        // Clear existing cards
        foreach (Transform child in characterGrid)
            Destroy(child.gameObject);

        // Create new cards
        foreach (var character in characters)
        {
            GameObject card = Instantiate(characterCardPrefab, characterGrid);
            SetupCharacterCard(card, character);
        }
    }

    private void SetupCharacterCard(GameObject card, Character character)
    {
        // Set up card visuals
        card.GetComponentInChildren<Image>().sprite = character.icon;
        card.GetComponentInChildren<Text>().text = character.characterName;

        // Set up lock indicator
        var lockIcon = card.transform.Find("LockIcon")?.gameObject;
        if (lockIcon != null)
            lockIcon.SetActive(!character.isUnlocked);

        // Add click handler
        Button button = card.GetComponent<Button>();
        button.onClick.AddListener(() => SelectCharacter(character));

        // Visual feedback for selected character
        if (character.id == currentCharacterId)
        {
            card.transform.localScale *= 1.1f;
        }
    }

    private void SelectCharacter(Character character)
    {
        selectedCharacter = character;

        // Update UI
        characterIcon.sprite = character.icon;
        characterNameText.text = character.characterName;
        characterDescriptionText.text = character.description;

        // Update buttons
        selectButton.gameObject.SetActive(character.isUnlocked);
        unlockButton.gameObject.SetActive(!character.isUnlocked);
        
        if (!character.isUnlocked)
        {
            unlockCostText.text = $"Unlock: {character.unlockCost}";
        }

        // Update preview
        UpdateCharacterPreview();
    }

    private void UpdateCharacterPreview()
    {
        // Clear existing preview
        if (previewCharacter != null)
            Destroy(previewCharacter);

        // Create new preview
        if (selectedCharacter.characterPrefab != null)
        {
            previewCharacter = Instantiate(selectedCharacter.characterPrefab, characterPreviewArea.transform);
            var animator = previewCharacter.GetComponent<Animator>();
            if (animator && selectedCharacter.animator)
            {
                animator.runtimeAnimatorController = selectedCharacter.animator;
                animator.Play("Idle");
            }
        }
    }

    private void OnSelectButtonClicked()
    {
        if (selectedCharacter != null && selectedCharacter.isUnlocked)
        {
            currentCharacterId = selectedCharacter.id;
            PlayerPrefs.SetString("SelectedCharacter", currentCharacterId);
            
            // Refresh character cards to update selection visual
            CreateCharacterCards();
        }
    }

    private void OnUnlockButtonClicked()
    {
        if (selectedCharacter == null || selectedCharacter.isUnlocked)
            return;

        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        if (currentCoins >= selectedCharacter.unlockCost)
        {
            // Process unlock
            currentCoins -= selectedCharacter.unlockCost;
            PlayerPrefs.SetInt("Coins", currentCoins);
            selectedCharacter.isUnlocked = true;

            // Save unlocked status
            PlayerPrefs.SetInt($"Character_{selectedCharacter.id}_Unlocked", 1);

            // Refresh UI
            SelectCharacter(selectedCharacter);
            CreateCharacterCards();
        }
        else
        {
            // Show "not enough coins" message
            Debug.Log("Not enough coins to unlock character!");
        }

        void OnDestroy()
        {
            if (previewCharacter != null)
                Destroy(previewCharacter);
        }
    }
}
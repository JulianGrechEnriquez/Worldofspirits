using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WorldOfSpirits.Progression.Upgrades
{
    public sealed class UpgradeCardView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Image border;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private TMP_Text levelText;

        private UpgradeSelectionManager manager;
        private int offerIndex;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(Choose);
        }

        public void Bind(UpgradeSelectionManager owner, UpgradeCardDefinition card, int index, int currentLevel)
        {
            manager = owner;
            offerIndex = index;
            if (button != null) button.interactable = true;
            if (titleText != null) titleText.text = card.CardName;
            if (descriptionText != null) descriptionText.text = card.Description;
            if (rarityText != null) rarityText.text = card.Rarity.ToString();
            if (levelText != null) levelText.text = $"Level {currentLevel + 1}/{card.MaximumLevel}";
            if (icon != null) { icon.sprite = card.Icon; icon.enabled = card.Icon != null; }
            if (border != null) border.color = RarityColor(card.Rarity);
            gameObject.SetActive(true);
        }

        public void BindDisplay(UpgradeCardDefinition card, int currentLevel)
        {
            manager = null;
            offerIndex = -1;
            if (button != null) button.interactable = false;
            if (titleText != null) titleText.text = card.CardName;
            if (descriptionText != null) descriptionText.text = card.Description;
            if (rarityText != null) rarityText.text = card.Rarity.ToString();
            if (levelText != null) levelText.text = $"Level {currentLevel}/{card.MaximumLevel}";
            if (icon != null) { icon.sprite = card.Icon; icon.enabled = card.Icon != null; }
            if (border != null) border.color = RarityColor(card.Rarity);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            manager = null;
            gameObject.SetActive(false);
        }

        private void Choose()
        {
            if (manager != null) manager.Choose(offerIndex);
        }

        public static Color RarityColor(UpgradeRarity rarity)
        {
            switch (rarity)
            {
                case UpgradeRarity.Uncommon: return new Color(0.25f, 0.85f, 0.35f);
                case UpgradeRarity.Rare: return new Color(0.25f, 0.5f, 1f);
                case UpgradeRarity.Epic: return new Color(0.7f, 0.25f, 1f);
                case UpgradeRarity.Legendary: return new Color(1f, 0.65f, 0.08f);
                default: return new Color(0.82f, 0.82f, 0.82f);
            }
        }
    }
}

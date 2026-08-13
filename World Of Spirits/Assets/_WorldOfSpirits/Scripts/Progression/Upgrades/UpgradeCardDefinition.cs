using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.Progression.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade Card", menuName = "World of Spirits/Upgrades/Card")]
    public sealed class UpgradeCardDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string cardId;
        [SerializeField] private string cardName;
        [TextArea(2, 5), SerializeField] private string description;
        [TextArea(1, 3), SerializeField] private string flavorText;
        [SerializeField] private Sprite icon;
        [Tooltip("Optional full card illustration. Leave empty to show only the icon.")]
        [SerializeField] private Sprite cardArtwork;
        [SerializeField] private string suggestedIconTheme;

        [Header("Availability")]
        [SerializeField] private UpgradeCategory category;
        [SerializeField] private UpgradeRarity rarity;
        [Min(1), SerializeField] private int maximumLevel = 1;
        [Min(0f), SerializeField] private float baseWeight = 100f;
        [SerializeField] private bool repeatableAfterMaximum;

        [Header("Target (optional)")]
        [SerializeField] private SpiritDefinition targetSpirit;
        [Min(-1), SerializeField] private int abilityIndex = -1;
        [Tooltip("Prefab granted by a Spirit Contract card.")]
        [SerializeField] private GameObject spiritPrefab;

        [Header("Effects")]
        [SerializeField] private List<UpgradeModifier> modifiers = new List<UpgradeModifier>();

        public string Id => string.IsNullOrWhiteSpace(cardId) ? name : cardId;
        public string CardName => string.IsNullOrWhiteSpace(cardName) ? name : cardName;
        public string Description => description;
        public string FlavorText => flavorText;
        public Sprite Icon => icon;
        public Sprite CardArtwork => cardArtwork;
        public string SuggestedIconTheme => suggestedIconTheme;
        public UpgradeCategory Category => category;
        public UpgradeRarity Rarity => rarity;
        public int MaximumLevel => Mathf.Max(1, maximumLevel);
        public float BaseWeight => Mathf.Max(0f, baseWeight);
        public bool RepeatableAfterMaximum => repeatableAfterMaximum;
        public SpiritDefinition TargetSpirit => targetSpirit;
        public int AbilityIndex => abilityIndex;
        public GameObject SpiritPrefab => spiritPrefab;
        public IReadOnlyList<UpgradeModifier> Modifiers => modifiers;

        private void OnValidate()
        {
            maximumLevel = Mathf.Max(1, maximumLevel);
            baseWeight = Mathf.Max(0f, baseWeight);
            abilityIndex = Mathf.Max(-1, abilityIndex);
            if (string.IsNullOrWhiteSpace(cardId)) cardId = name.Trim().ToLowerInvariant().Replace(' ', '_');
        }
    }
}

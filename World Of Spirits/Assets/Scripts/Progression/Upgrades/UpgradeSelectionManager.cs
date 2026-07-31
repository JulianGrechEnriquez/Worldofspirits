using System;
using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.Progression.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class UpgradeSelectionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UpgradeCatalog catalog;
        [SerializeField] private PlayerLevelSystem levelSystem;
        [SerializeField] private UpgradeRuntimeStats runtimeStats;
        [SerializeField] private SpiritManager spiritManager;

        [Header("Choice Rules")]
        [SerializeField, Range(1, 5)] private int choicesPerLevel = 3;
        [SerializeField, Min(1)] private int previousAbilityInvestmentToUnlockNext = 2;
        [SerializeField, Min(1)] private int rarePityAfterLevels = 5;
        [SerializeField, Min(1)] private int epicPityAfterLevels = 12;
        [Tooltip("Makes an already-started ability path more likely to return.")]
        [SerializeField, Min(1f)] private float focusedPathWeight = 1.75f;

        private readonly List<UpgradeCardDefinition> candidates = new List<UpgradeCardDefinition>(128);
        private readonly List<float> candidateWeights = new List<float>(128);
        private readonly List<UpgradeCardDefinition> offers = new List<UpgradeCardDefinition>(5);
        private int pendingSelections;
        private int levelsWithoutRare;
        private int levelsWithoutEpic;
        private bool selectionOpen;
        private float previousTimeScale = 1f;

        public IReadOnlyList<UpgradeCardDefinition> CurrentOffers => offers;
        public bool IsSelectionOpen => selectionOpen;
        public event Action<IReadOnlyList<UpgradeCardDefinition>> ChoicesReady;
        public event Action<UpgradeCardDefinition> CardChosen;

        private void Awake()
        {
            if (levelSystem == null) levelSystem = GetComponent<PlayerLevelSystem>();
            if (runtimeStats == null) runtimeStats = GetComponent<UpgradeRuntimeStats>();
            if (spiritManager == null) spiritManager = GetComponent<SpiritManager>();
        }

        private void OnEnable()
        {
            if (levelSystem != null) levelSystem.LevelGained += OnLevelGained;
        }

        private void OnDisable()
        {
            if (levelSystem != null) levelSystem.LevelGained -= OnLevelGained;
            if (selectionOpen) RestoreTime();
        }

        private void OnLevelGained(int newLevel)
        {
            pendingSelections++;
            if (!selectionOpen) CreateChoices(newLevel);
        }

        public bool Choose(int offerIndex)
        {
            if (!selectionOpen || offerIndex < 0 || offerIndex >= offers.Count || runtimeStats == null) return false;
            UpgradeCardDefinition selected = offers[offerIndex];
            if (!runtimeStats.TryApply(selected)) return false;

            pendingSelections = Mathf.Max(0, pendingSelections - 1);
            UpdatePity(selected.Rarity);

            if (pendingSelections > 0)
                CreateChoices(levelSystem != null ? levelSystem.Level : 1);
            else
            {
                selectionOpen = false;
                offers.Clear();
                RestoreTime();
            }
            CardChosen?.Invoke(selected);
            return true;
        }

        [ContextMenu("Debug Open Upgrade Choice")]
        public void DebugOpenChoice()
        {
            if (selectionOpen) return;
            pendingSelections++;
            CreateChoices(levelSystem != null ? levelSystem.Level : 1);
        }

        private void CreateChoices(int playerLevel)
        {
            candidates.Clear();
            candidateWeights.Clear();
            offers.Clear();
            if (catalog == null || runtimeStats == null) return;

            IReadOnlyList<UpgradeCardDefinition> allCards = catalog.Cards;
            for (int i = 0; i < allCards.Count; i++)
            {
                UpgradeCardDefinition card = allCards[i];
                if (!IsEligible(card, playerLevel)) continue;
                candidates.Add(card);
                candidateWeights.Add(CalculateWeight(card));
            }

            int count = Mathf.Min(choicesPerLevel, candidates.Count);
            for (int i = 0; i < count; i++) PickOneWithoutReplacement();
            if (offers.Count == 0) return;

            selectionOpen = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            ChoicesReady?.Invoke(offers);
        }

        private bool IsEligible(UpgradeCardDefinition card, int playerLevel)
        {
            if (card == null || card.BaseWeight <= 0f || playerLevel < card.MinimumPlayerLevel) return false;
            int level = runtimeStats.GetCardLevel(card.Id);
            if (level >= card.MaximumLevel && !card.RepeatableAfterMaximum) return false;

            IReadOnlyList<UpgradeRequirement> requirements = card.Requirements;
            for (int i = 0; i < requirements.Count; i++)
                if (runtimeStats.GetCardLevel(requirements[i].cardId) < requirements[i].requiredLevel) return false;

            if (card.Category == UpgradeCategory.SpiritContract)
                return spiritManager != null && spiritManager.HasOpenSpiritSlot &&
                    card.TargetSpirit != null && !spiritManager.OwnsSpirit(card.TargetSpirit);

            if (card.Category != UpgradeCategory.Weapon && card.Category != UpgradeCategory.SpiritAbility) return true;
            SpiritMember spirit = spiritManager != null ? spiritManager.FindSpirit(card.TargetSpirit) : null;
            if (spirit == null) return false;

            if (card.Category == UpgradeCategory.Weapon)
                return spirit.Definition != null && spirit.Progression.WeaponLevel <
                    (spirit.Definition.RuntimeWeapon != null ? spirit.Definition.RuntimeWeapon.MaxLevel : spirit.Definition.Weapon.MaxLevel);

            int abilityIndex = card.AbilityIndex;
            int abilityCount = spirit.Definition.RuntimeAbilities.Count > 0 ?
                spirit.Definition.RuntimeAbilities.Count : spirit.Definition.Abilities.Count;
            if (abilityIndex < 0 || abilityIndex >= abilityCount) return false;
            if (abilityIndex > 0 && spirit.Progression.GetAbilityLevel(abilityIndex) == 0 &&
                spirit.Progression.GetAbilityLevel(abilityIndex - 1) < previousAbilityInvestmentToUnlockNext) return false;
            int max = spirit.Definition.RuntimeAbilities.Count > 0 ?
                spirit.Definition.RuntimeAbilities[abilityIndex].MaxLevel : spirit.Definition.Abilities[abilityIndex].MaxLevel;
            return spirit.Progression.GetAbilityLevel(abilityIndex) < max;
        }

        private float CalculateWeight(UpgradeCardDefinition card)
        {
            float weight = card.BaseWeight * RarityWeight(card.Rarity);
            if (levelsWithoutRare >= rarePityAfterLevels && (int)card.Rarity >= (int)UpgradeRarity.Rare) weight *= 3f;
            if (levelsWithoutEpic >= epicPityAfterLevels && (int)card.Rarity >= (int)UpgradeRarity.Epic) weight *= 4f;
            if ((card.Category == UpgradeCategory.SpiritAbility || card.Category == UpgradeCategory.Weapon) &&
                runtimeStats.GetCardLevel(card.Id) > 0) weight *= focusedPathWeight;
            weight *= 1f + runtimeStats.GetFlat(UpgradeStat.Luck) * 0.01f * (int)card.Rarity;
            return Mathf.Max(0.001f, weight);
        }

        private static float RarityWeight(UpgradeRarity rarity)
        {
            switch (rarity)
            {
                case UpgradeRarity.Uncommon: return 0.35f;
                case UpgradeRarity.Rare: return 0.12f;
                case UpgradeRarity.Epic: return 0.035f;
                case UpgradeRarity.Legendary: return 0.006f;
                default: return 1f;
            }
        }

        private void PickOneWithoutReplacement()
        {
            float total = 0f;
            for (int i = 0; i < candidateWeights.Count; i++) total += candidateWeights[i];
            float roll = UnityEngine.Random.value * total;
            int chosen = candidateWeights.Count - 1;
            for (int i = 0; i < candidateWeights.Count; i++)
            {
                roll -= candidateWeights[i];
                if (roll <= 0f) { chosen = i; break; }
            }
            offers.Add(candidates[chosen]);
            candidates.RemoveAt(chosen);
            candidateWeights.RemoveAt(chosen);
        }

        private void UpdatePity(UpgradeRarity rarity)
        {
            levelsWithoutRare = (int)rarity >= (int)UpgradeRarity.Rare ? 0 : levelsWithoutRare + 1;
            levelsWithoutEpic = (int)rarity >= (int)UpgradeRarity.Epic ? 0 : levelsWithoutEpic + 1;
        }

        private void RestoreTime()
        {
            Time.timeScale = previousTimeScale;
            selectionOpen = false;
        }

        private void OnValidate()
        {
            choicesPerLevel = Mathf.Clamp(choicesPerLevel, 1, 5);
            previousAbilityInvestmentToUnlockNext = Mathf.Max(1, previousAbilityInvestmentToUnlockNext);
            rarePityAfterLevels = Mathf.Max(1, rarePityAfterLevels);
            epicPityAfterLevels = Mathf.Max(1, epicPityAfterLevels);
            focusedPathWeight = Mathf.Max(1f, focusedPathWeight);
        }
    }
}

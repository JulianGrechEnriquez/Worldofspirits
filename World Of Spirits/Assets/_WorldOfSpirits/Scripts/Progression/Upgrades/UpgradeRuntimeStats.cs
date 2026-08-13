using System;
using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Player;
using WorldOfSpirits.Spirits;
using WorldOfSpirits.Progression;

namespace WorldOfSpirits.Progression.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class UpgradeRuntimeStats : MonoBehaviour, IUpgradeable
    {
        private readonly Dictionary<string, int> cardLevels = new Dictionary<string, int>(64);
        private readonly float[] additive = new float[(int)UpgradeStat.Count];
        private readonly float[] multiplicative = new float[(int)UpgradeStat.Count];
        private PlayerCharacter player;
        private SpiritManager spiritManager;
        private float nextRegenerationTime;

        public event Action<UpgradeCardDefinition, int> UpgradeApplied;
        public IReadOnlyDictionary<string, int> CardLevels => cardLevels;

        private void Awake()
        {
            player = GetComponent<PlayerCharacter>();
            spiritManager = GetComponent<SpiritManager>();
        }

        private void Update()
        {
            float regeneration = GetFlat(UpgradeStat.HealthRegeneration);
            if (regeneration <= 0f || player == null || Time.time < nextRegenerationTime) return;
            nextRegenerationTime = Time.time + 1f;
            player.Heal(regeneration * GetMultiplier(UpgradeStat.HealingPower));
        }

        public int GetCardLevel(string cardId) =>
            !string.IsNullOrEmpty(cardId) && cardLevels.TryGetValue(cardId, out int level) ? level : 0;

        public float GetFlat(UpgradeStat stat) => additive[(int)stat];
        public float GetMultiplier(UpgradeStat stat) =>
            Mathf.Max(0f, 1f + additive[(int)stat] + multiplicative[(int)stat]);

        public bool TryApply(UpgradeCardDefinition card)
        {
            if (card == null) return false;
            int oldLevel = GetCardLevel(card.Id);
            if (oldLevel >= card.MaximumLevel && !card.RepeatableAfterMaximum) return false;

            bool applied = ApplySpecialCard(card);
            if (!applied && card.Category == UpgradeCategory.SpiritContract) return false;

            IReadOnlyList<UpgradeModifier> modifiers = card.Modifiers;
            for (int i = 0; i < modifiers.Count; i++)
            {
                UpgradeModifier modifier = modifiers[i];
                if (modifier.operation == ModifierOperation.Add)
                    additive[(int)modifier.stat] += modifier.valuePerLevel;
                else
                    multiplicative[(int)modifier.stat] += modifier.valuePerLevel;

                if (modifier.stat == UpgradeStat.MaxHealth && player != null)
                    player.IncreaseMaximumHealth(modifier.valuePerLevel);

                if (modifier.stat == UpgradeStat.SpiritCapacity && spiritManager != null)
                    spiritManager.SetBonusSpiritSlots(
                        Mathf.Max(0, Mathf.RoundToInt(GetFlat(UpgradeStat.SpiritCapacity))));
            }

            int newLevel = oldLevel + 1;
            cardLevels[card.Id] = newLevel;
            UpgradeApplied?.Invoke(card, newLevel);
            return true;
        }

        private bool ApplySpecialCard(UpgradeCardDefinition card)
        {
            if (spiritManager == null) return card.Category == UpgradeCategory.Player ||
                card.Category == UpgradeCategory.Legendary || card.Category == UpgradeCategory.Evolution;

            switch (card.Category)
            {
                case UpgradeCategory.SpiritContract:
                {
                    bool added = spiritManager.TryAddSpirit(card.SpiritPrefab);
                    if (added) SpiritUnlockProgress.Unlock(card.TargetSpirit);
                    return added;
                }
                case UpgradeCategory.Weapon:
                {
                    SpiritMember spirit = spiritManager.FindSpirit(card.TargetSpirit);
                    return spirit != null && spirit.TryLevelWeapon();
                }
                case UpgradeCategory.SpiritAbility:
                {
                    SpiritMember spirit = spiritManager.FindSpirit(card.TargetSpirit);
                    return spirit != null && spirit.TryLevelAbility(card.AbilityIndex);
                }
                default:
                    return true;
            }
        }

        public bool RollDodge()
        {
            float chance = Mathf.Clamp01(GetFlat(UpgradeStat.DodgeChance));
            return chance > 0f && UnityEngine.Random.value < chance;
        }

        public int GetProjectileCount(int baseCount) =>
            Mathf.Max(1, baseCount + Mathf.RoundToInt(GetFlat(UpgradeStat.MultiShot)));

        public int GetMeleeStrikeCount(int baseCount) =>
            Mathf.Max(1, baseCount + Mathf.RoundToInt(GetFlat(UpgradeStat.MeleeEcho)));

        public bool PrimarySpiritAbilitiesEnabled =>
            GetFlat(UpgradeStat.PrimarySpiritAbility) > 0f;

        public float ScaleDuration(float baseDuration) =>
            baseDuration * GetMultiplier(UpgradeStat.Duration);

        public float ScaleArea(float baseArea) =>
            baseArea * GetMultiplier(UpgradeStat.AreaSize);

        public float ScaleForce(float baseForce) =>
            baseForce * GetMultiplier(UpgradeStat.Knockback);

        public float ScaleHealing(float baseHealing) =>
            baseHealing * GetMultiplier(UpgradeStat.HealingPower);

        public float ScaleShield(float baseShield) =>
            baseShield * GetMultiplier(UpgradeStat.ShieldGeneration);

        public float ScaleWeaponDamage(float baseDamage, IDamageable target = null)
        {
            float result = baseDamage * GetMultiplier(UpgradeStat.AttackDamage) *
                GetMultiplier(UpgradeStat.ElementalDamage);
            return ScaleDamageAgainstTarget(RollCritical(result), target);
        }

        public float ResolveDamage(DamageContext context, IDamageable target)
        {
            float result = context.BaseDamage;
            if (context.IsWeaponDamage || context.IsSpiritDamage)
                result *= GetMultiplier(UpgradeStat.AttackDamage);
            if (context.IsSpiritDamage)
                result *= GetMultiplier(UpgradeStat.SpiritDamage);
            if (context.Element != DamageElement.Physical)
                result *= GetMultiplier(UpgradeStat.ElementalDamage);
            if (context.CanCritical)
                result = RollCritical(result);
            return ScaleDamageAgainstTarget(result, target);
        }

        public float ScaleSpiritDamage(float baseDamage, IDamageable target = null)
        {
            float result = baseDamage * GetMultiplier(UpgradeStat.AttackDamage) *
                GetMultiplier(UpgradeStat.SpiritDamage) *
                GetMultiplier(UpgradeStat.ElementalDamage);
            return ScaleDamageAgainstTarget(RollCritical(result), target);
        }

        public float ScaleDamageAgainstTarget(float damage, IDamageable target)
        {
            if (target == null || damage <= 0f) return damage;

            float result = damage;
            if (target is IEnemyClassification enemy && enemy.IsElite)
                result *= GetMultiplier(UpgradeStat.EliteDamage);

            if (target is LivingEntity living && living.MaxHealth > 0f &&
                living.CurrentHealth / living.MaxHealth <= GetFlat(UpgradeStat.ExecuteThreshold))
            {
                result = Mathf.Max(result, living.CurrentHealth + living.CurrentShield);
            }

            return result;
        }

        private float RollCritical(float damage)
        {
            if (UnityEngine.Random.value >= Mathf.Clamp01(GetFlat(UpgradeStat.CriticalChance))) return damage;
            return damage * Mathf.Max(1.5f, 1.5f + GetFlat(UpgradeStat.CriticalDamage));
        }
    }
}

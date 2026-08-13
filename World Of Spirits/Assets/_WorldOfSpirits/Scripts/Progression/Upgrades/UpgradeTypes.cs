using System;
using UnityEngine;

namespace WorldOfSpirits.Progression.Upgrades
{
    public enum UpgradeCategory { Player, Weapon, SpiritAbility, SpiritContract, Evolution, Legendary }
    public enum UpgradeRarity { Common, Uncommon, Rare, Epic, Legendary }

    // Array-backed at runtime: keep new values before Count.
    public enum UpgradeStat
    {
        MaxHealth, AttackDamage, AttackSpeed, CriticalChance, CriticalDamage,
        MovementSpeed, CooldownReduction, PickupRadius, ElementalDamage, Luck,
        ExperienceGain, GoldGain, HealthRegeneration, DodgeChance, ShieldGeneration,
        HealingPower, SpiritDamage, AreaSize, Knockback, ProjectileSpeed,
        EliteDamage, ExecuteThreshold, ProjectileSize, Pierce, Ricochet,
        Homing, MultiShot, Duration, Armor, MeleeEcho, PrimarySpiritAbility,
        SpiritCapacity, Count
    }

    public enum ModifierOperation { Add, Multiply }

    [Serializable]
    public struct UpgradeModifier
    {
        [Tooltip("Runtime statistic affected by this card.")]
        public UpgradeStat stat;
        [Tooltip("Add adds a flat value. Multiply adds a percentage: 0.1 = +10%.")]
        public ModifierOperation operation;
        [Tooltip("Applied once for every card level obtained.")]
        public float valuePerLevel;
    }

    [Serializable]
    public struct UpgradeRequirement
    {
        [Tooltip("Stable ID of a prerequisite card.")]
        public string cardId;
        [Min(1)] public int requiredLevel;
    }
}

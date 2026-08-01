using System;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Combat
{
    public enum DamageElement
    {
        Physical,
        Fire,
        Ice,
        Earth,
        Lightning,
        Poison,
        Water,
        Wind,
        Holy
    }

    [Serializable]
    public readonly struct DamageContext
    {
        public DamageContext(
            float baseDamage,
            Transform source = null,
            DamageElement element = DamageElement.Physical,
            bool canCritical = false,
            bool isStatusDamage = false,
            bool isWeaponDamage = false,
            bool isSpiritDamage = false)
        {
            BaseDamage = Mathf.Max(0f, baseDamage);
            Source = source;
            Element = element;
            CanCritical = canCritical;
            IsStatusDamage = isStatusDamage;
            IsWeaponDamage = isWeaponDamage;
            IsSpiritDamage = isSpiritDamage;
        }

        public float BaseDamage { get; }
        public Transform Source { get; }
        public DamageElement Element { get; }
        public bool CanCritical { get; }
        public bool IsStatusDamage { get; }
        public bool IsWeaponDamage { get; }
        public bool IsSpiritDamage { get; }

        public DamageContext WithBaseDamage(float damage) => new DamageContext(
            damage, Source, Element, CanCritical, IsStatusDamage, IsWeaponDamage, IsSpiritDamage);

        public DamageContext AsStatus(float damage, DamageElement statusElement) => new DamageContext(
            damage, Source, statusElement, false, true, IsWeaponDamage, IsSpiritDamage);

        public static DamageContext Weapon(float damage, Transform source, DamageElement element) =>
            new DamageContext(damage, source, element, true, false, true, false);

        public static DamageContext Spirit(float damage, Transform source, DamageElement element) =>
            new DamageContext(damage, source, element, true, false, false, true);
    }

    public static class DamageElementUtility
    {
        public static DamageElement FromSpiritName(string spiritName)
        {
            if (string.IsNullOrWhiteSpace(spiritName)) return DamageElement.Physical;
            string value = spiritName.ToLowerInvariant();
            if (value.Contains("fire")) return DamageElement.Fire;
            if (value.Contains("ice")) return DamageElement.Ice;
            if (value.Contains("earth")) return DamageElement.Earth;
            if (value.Contains("lightning")) return DamageElement.Lightning;
            if (value.Contains("poison")) return DamageElement.Poison;
            if (value.Contains("water")) return DamageElement.Water;
            if (value.Contains("wind")) return DamageElement.Wind;
            if (value.Contains("holy")) return DamageElement.Holy;
            return DamageElement.Physical;
        }

        public static DamageElement FromStatus(CombatStatus status)
        {
            return status switch
            {
                CombatStatus.Burn => DamageElement.Fire,
                CombatStatus.Poison => DamageElement.Poison,
                CombatStatus.Freeze => DamageElement.Ice,
                _ => DamageElement.Physical
            };
        }
    }

    public static class DamageResolver
    {
        public static float Calculate(DamageContext context, IDamageable target)
        {
            UpgradeRuntimeStats stats = context.Source != null
                ? context.Source.GetComponentInParent<UpgradeRuntimeStats>()
                : null;
            return stats != null
                ? stats.ResolveDamage(context, target)
                : context.BaseDamage;
        }
    }
}

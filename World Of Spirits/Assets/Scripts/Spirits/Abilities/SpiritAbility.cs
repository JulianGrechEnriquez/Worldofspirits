using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    public abstract class SpiritAbility : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField, Min(0)] private int abilityIndex;
        [SerializeField, Min(0.05f)] private float cooldown = 1f;
        [SerializeField] private bool primarySpiritOnly;
        [SerializeField] private bool castWhileMoving = true;
        [SerializeField] private bool castWhileStandingStill;

        private float nextCastTime;
        protected UpgradeRuntimeStats UpgradeStats { get; private set; }
        protected SpiritAbilityContext LatestContext { get; private set; }
        protected bool HasContext { get; private set; }

        public int AbilityIndex => abilityIndex;
        protected SpiritMember OwnerSpirit { get; private set; }
        protected int CurrentLevel => OwnerSpirit != null ? Mathf.Max(1, OwnerSpirit.Progression.GetAbilityLevel(abilityIndex)) : 1;

        protected virtual void Awake()
        {
            OwnerSpirit = GetComponentInParent<SpiritMember>();
            UpgradeStats = GetComponentInParent<UpgradeRuntimeStats>();
        }

        public void TickAbility(SpiritAbilityContext context)
        {
            LatestContext = context;
            HasContext = true;

            if (OwnerSpirit != null && !OwnerSpirit.Progression.IsAbilityUnlocked(abilityIndex))
            {
                return;
            }

            if (!isActiveAndEnabled || Time.time < nextCastTime || primarySpiritOnly && !context.IsPrimary)
            {
                return;
            }

            // Support spirits continue casting in either movement state. Movement
            // restrictions only control the currently selected primary spirit.
            bool primaryAbilityMastered = context.PrimaryWeaponAndAbilitiesEnabled;
            bool movementStateAllowed = !context.IsPrimary || primaryAbilityMastered ||
                (context.PlayerIsMoving ? castWhileMoving : castWhileStandingStill);
            if (!movementStateAllowed || !CanCast(context))
            {
                return;
            }

            Cast(context);
            nextCastTime = Time.time + GetCooldown();
        }

        protected virtual float GetCooldown() => ScaleCooldown(cooldown);
        protected bool IsAbilityUnlocked => OwnerSpirit == null ||
            OwnerSpirit.Progression.IsAbilityUnlocked(abilityIndex);
        protected float ScaleCooldown(float value) => value /
            (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.CooldownReduction) : 1f);
        protected DamageElement DamageElement => DamageElementUtility.FromSpiritName(
            OwnerSpirit != null && OwnerSpirit.Definition != null
                ? OwnerSpirit.Definition.SpiritName
                : string.Empty);
        protected DamageContext CreateSpiritDamage(float baseDamage) => DamageContext.Spirit(
            baseDamage,
            OwnerSpirit != null ? OwnerSpirit.transform : transform,
            DamageElement);
        protected virtual bool CanCast(SpiritAbilityContext context) => true;
        protected abstract void Cast(SpiritAbilityContext context);
    }

    public readonly struct SpiritAbilityContext
    {
        public SpiritAbilityContext(
            Transform player,
            bool playerIsMoving,
            bool isPrimary,
            bool primaryWeaponAndAbilitiesEnabled = false)
        {
            Player = player;
            PlayerIsMoving = playerIsMoving;
            IsPrimary = isPrimary;
            PrimaryWeaponAndAbilitiesEnabled = primaryWeaponAndAbilitiesEnabled;
        }

        public Transform Player { get; }
        public bool PlayerIsMoving { get; }
        public bool IsPrimary { get; }
        public bool PrimaryWeaponAndAbilitiesEnabled { get; }
    }
}

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
        private float scheduledCastSpeed = 1f;
        protected UpgradeRuntimeStats UpgradeStats { get; private set; }
        protected SpiritAbilityContext LatestContext { get; private set; }
        protected bool HasContext { get; private set; }
        private Transform creatureDamageOwner;

        public void BindCreatureCaster(Transform owner)
        {
            creatureDamageOwner = owner;
            UpgradeStats = owner != null ? owner.GetComponent<UpgradeRuntimeStats>() : null;
        }

        public int AbilityIndex => abilityIndex;
        protected SpiritMember OwnerSpirit { get; private set; }
        public int EffectiveLevel => OwnerSpirit != null ? OwnerSpirit.GetEffectiveAbilityLevel(abilityIndex) : 1;
        protected int CurrentLevel => Mathf.Max(1, EffectiveLevel);

        protected virtual void Awake()
        {
            OwnerSpirit = GetComponentInParent<SpiritMember>();
            UpgradeStats = GetComponentInParent<UpgradeRuntimeStats>();
        }

        public void TickAbility(SpiritAbilityContext context)
        {
            LatestContext = context;
            HasContext = true;

            // Preserve cooldown progress when the stationary cast-speed bonus switches.
            float castSpeed = UpgradeStats != null ? UpgradeStats.CompanionCastSpeedMultiplier : 1f;
            if (!Mathf.Approximately(castSpeed, scheduledCastSpeed) && nextCastTime > Time.time)
                nextCastTime = Time.time + (nextCastTime - Time.time) * scheduledCastSpeed / castSpeed;
            scheduledCastSpeed = castSpeed;

            bool stateAllowed = (UpgradeStats == null || UpgradeStats.Character != UpgradeCharacter.SpiritWarrior) &&
                IsAbilityUnlocked && isActiveAndEnabled &&
                (!primarySpiritOnly || context.IsPrimary) && IsMovementStateAllowed(context);
            OnActivationStateChanged(stateAllowed);
            if (!stateAllowed) return;

            if (Time.time < nextCastTime)
            {
                return;
            }

            if (!CanCast(context))
            {
                return;
            }

            Cast(context);
            nextCastTime = Time.time + GetCooldown();
        }

        protected virtual float GetCooldown() => ScaleCooldown(cooldown);
        // Support spirits and the mastery upgrade bypass primary movement restrictions.
        protected bool IsMovementStateAllowed(SpiritAbilityContext context) =>
            (UpgradeStats == null || UpgradeStats.Character != UpgradeCharacter.SpiritWarrior) &&
            (context.CompanionsOnly || !context.IsPrimary || context.PrimaryWeaponAndAbilitiesEnabled ||
             (context.PlayerIsMoving ? castWhileMoving : castWhileStandingStill));
        protected virtual void OnActivationStateChanged(bool allowed) { }
        protected bool IsAbilityUnlocked => OwnerSpirit == null ||
            OwnerSpirit.Progression.IsAbilityUnlocked(abilityIndex);
        protected float ScaleCooldown(float value) => value /
            (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.CooldownReduction) *
                UpgradeStats.CompanionCastSpeedMultiplier : 1f);
        protected DamageElement DamageElement => DamageElementUtility.FromSpiritName(
            OwnerSpirit != null && OwnerSpirit.Definition != null
                ? OwnerSpirit.Definition.SpiritName
                : string.Empty);
        protected DamageContext CreateSpiritDamage(float baseDamage) => DamageContext.Spirit(
            baseDamage,
            creatureDamageOwner != null ? creatureDamageOwner : OwnerSpirit != null ? OwnerSpirit.transform : transform,
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
            bool primaryWeaponAndAbilitiesEnabled = false,
            bool companionsOnly = false)
        {
            Player = player;
            PlayerIsMoving = playerIsMoving;
            IsPrimary = isPrimary;
            PrimaryWeaponAndAbilitiesEnabled = primaryWeaponAndAbilitiesEnabled;
            CompanionsOnly = companionsOnly;
        }

        public Transform Player { get; }
        public bool PlayerIsMoving { get; }
        public bool IsPrimary { get; }
        public bool PrimaryWeaponAndAbilitiesEnabled { get; }
        public bool CompanionsOnly { get; }
    }
}

using System;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Player
{
    public sealed class PlayerCharacter : LivingEntity
    {
        [Header("Contact Protection")]
        [SerializeField, Min(0f)] private float globalContactInvulnerability = 0.2f;

        private Rigidbody2D body;
        private float nextContactDamageTime;
        private UpgradeRuntimeStats upgradeStats;
        private SpiritWarriorController warrior;

        public override Faction Faction => global::WorldOfSpirits.Combat.Faction.Player;
        public event Action<float, float> PlayerHealthChanged;
        public event Action PlayerDied;

        protected override void Awake()
        {
            base.Awake();
            body = GetComponent<Rigidbody2D>();
            upgradeStats = GetComponent<UpgradeRuntimeStats>();
            warrior = GetComponent<SpiritWarriorController>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            HealthChanged += HandleHealthChanged;
            Died += HandlePlayerDied;
        }

        protected override void OnDisable()
        {
            HealthChanged -= HandleHealthChanged;
            Died -= HandlePlayerDied;
            base.OnDisable();
        }

        public override void TakeDamage(float amount)
        {
            TakeDamage(new DamageContext(amount));
        }

        public override void TakeDamage(DamageContext context)
        {
            if (upgradeStats != null && upgradeStats.RollDodge()) return;
            float armor = upgradeStats != null ? upgradeStats.GetFlat(UpgradeStat.Armor) : 0f;
            float amount = DamageResolver.Calculate(context, this) *
                (100f / (100f + Mathf.Max(0f, armor)));
            if (warrior != null) amount = warrior.ReduceIncomingDamage(amount, context);
            ApplyResolvedDamage(amount, context);
        }

        private void HandleHealthChanged(float current, float maximum) =>
            PlayerHealthChanged?.Invoke(current, maximum);

        private void HandlePlayerDied() => PlayerDied?.Invoke();

        public bool TryTakeContactDamage(
            float damage,
            Vector2 incomingDirection,
            float knockback,
            Transform source = null)
        {
            if (!IsAlive || Time.time < nextContactDamageTime)
            {
                return false;
            }

            TakeDamage(new DamageContext(damage, source));
            nextContactDamageTime = Time.time + globalContactInvulnerability;

            if (body != null && knockback > 0f)
            {
                body.AddForce(incomingDirection * knockback, ForceMode2D.Impulse);
            }

            return true;
        }

        public override void OnSpawnedFromPool(GameObject prefab)
        {
            base.OnSpawnedFromPool(prefab);
            nextContactDamageTime = 0f;
        }
    }
}

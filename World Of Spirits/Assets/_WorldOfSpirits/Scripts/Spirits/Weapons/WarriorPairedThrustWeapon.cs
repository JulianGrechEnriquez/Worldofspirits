using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Crowd;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    // Uses exactly the Ice Gauntlets' extend / spatial hit / return implementation.
    public sealed class WarriorPairedThrustWeapon : ThrustMeleeWeaponBase
    {
        [SerializeField, Min(0f)] private float knockback;
        [SerializeField, Min(0f)] private float burnDamagePerTick;
        private UpgradeRuntimeStats stats;
        protected override WeaponExecutionType ExpectedExecutionType => WeaponExecutionType.PunchingMelee;

        protected override void Awake()
        {
            base.Awake();
            stats = GetComponentInParent<UpgradeRuntimeStats>();
        }

        protected override void Update()
        {
            if (Time.timeScale <= 0f) return;
            base.Update();
        }

        protected override void OnTargetHit(IDamageable target, int weaponLevel)
        {
            if (target == null || !target.IsAlive) return;
            if (burnDamagePerTick > 0f && target is IStatusEffectReceiver receiver)
            {
                float damage = burnDamagePerTick * (1f + 0.2f * Mathf.Max(0, weaponLevel - 1));
                receiver.ApplyStatus(CombatStatus.Burn, stats != null ? stats.ScaleDuration(1.5f) : 1.5f,
                    damage, DamageContext.Weapon(damage, transform.parent, DamageElement.Fire));
            }
            if (knockback <= 0f) return;
            Vector2 direction = target.Transform.position - transform.parent.position;
            float strength = stats != null ? stats.ScaleForce(knockback) : knockback;
            var crowd = target.Transform.GetComponent<EnemyCrowdAgent>();
            if (crowd != null) crowd.ApplyExternalAcceleration(direction.normalized * strength, 1f);
            else
            {
                var body = target.Transform.GetComponent<Rigidbody2D>();
                if (body != null && body.bodyType == RigidbodyType2D.Dynamic)
                    body.AddForce(direction.normalized * strength, ForceMode2D.Impulse);
            }
        }
    }
}

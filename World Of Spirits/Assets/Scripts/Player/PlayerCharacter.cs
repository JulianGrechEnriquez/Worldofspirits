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

        public override Faction Faction => global::WorldOfSpirits.Combat.Faction.Player;

        protected override void Awake()
        {
            base.Awake();
            body = GetComponent<Rigidbody2D>();
            upgradeStats = GetComponent<UpgradeRuntimeStats>();
        }

        public override void TakeDamage(float amount)
        {
            if (upgradeStats != null && upgradeStats.RollDodge()) return;
            float armor = upgradeStats != null ? upgradeStats.GetFlat(UpgradeStat.Armor) : 0f;
            base.TakeDamage(amount * (100f / (100f + Mathf.Max(0f, armor))));
        }

        public bool TryTakeContactDamage(
            float damage,
            Vector2 incomingDirection,
            float knockback)
        {
            if (!IsAlive || Time.time < nextContactDamageTime)
            {
                return false;
            }

            TakeDamage(damage);
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

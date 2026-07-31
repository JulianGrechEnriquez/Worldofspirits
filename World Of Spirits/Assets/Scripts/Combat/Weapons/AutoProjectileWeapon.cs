using UnityEngine;
using WorldOfSpirits.Spirits;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Combat
{
    public class AutoProjectileWeapon : SpiritWeaponAttack
    {
        [Header("Projectile")]
        [SerializeField] private ProjectileBase projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 10f;
        [SerializeField, Min(0f)] private float damageIncreasePerWeaponLevel = 0.2f;

        [Header("Automatic Fire")]
        [SerializeField, Min(0.05f)] private float attackCooldown = 0.75f;
        [SerializeField, Min(0.1f)] private float targetingRange = 12f;
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("Debug")]
        [SerializeField] private bool logTargeting;
        [SerializeField] private bool drawTargetingRange = true;

        private LivingEntity owner;
        private SpiritMember spiritOwner;
        private Transform firePointOverride;
        private UpgradeRuntimeStats upgradeStats;

        private Transform ActiveFirePoint => firePointOverride != null ? firePointOverride : firePoint;

        private void Awake()
        {
            owner = GetComponentInParent<LivingEntity>();
            spiritOwner = GetComponentInParent<SpiritMember>();
            upgradeStats = GetComponentInParent<UpgradeRuntimeStats>();
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Start()
        {
            if (owner == null)
            {
                Debug.LogError($"[{name}] AutoProjectileWeapon needs a LivingEntity on this object or a parent.", this);
            }

            if (projectilePrefab == null)
            {
                Debug.LogError($"[{name}] No projectile prefab is assigned.", this);
            }
        }

        protected override void PerformAttack()
        {
            IDamageable target = FindClosestTarget();
            if (target != null)
            {
                if (logTargeting)
                {
                    Debug.Log($"[{name}] Targeting {target.Transform.name}.", this);
                }

                FireAt(target.Transform.position);
            }
        }

        protected override bool CanAttack()
        {
            return owner != null && owner.IsAlive;
        }

        protected override float AttackCooldown => attackCooldown /
            (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.AttackSpeed) : 1f);

        private IDamageable FindClosestTarget()
        {
            return CombatTargeting.FindClosest(
                transform.position,
                targetingRange,
                owner.Faction,
                targetLayers.value);
        }

        private void FireAt(Vector3 targetPosition)
        {
            if (projectilePrefab == null)
            {
                return;
            }

            Transform spawnPoint = ActiveFirePoint;
            Vector2 direction = targetPosition - spawnPoint.position;
            ProjectileBase projectile = ProjectilePool.Spawn(projectilePrefab, spawnPoint.position, Quaternion.identity);
            int weaponLevel = spiritOwner != null ? spiritOwner.Progression.WeaponLevel : 1;
            float scaledDamage = damage * (1f + damageIncreasePerWeaponLevel * Mathf.Max(0, weaponLevel - 1));
            if (upgradeStats != null) scaledDamage = upgradeStats.ScaleWeaponDamage(scaledDamage);
            float speed = projectileSpeed * (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSpeed) : 1f);
            projectile.Launch(direction, speed, scaledDamage, owner.Faction);
        }

        public void SetFirePointOverride(Transform overridePoint)
        {
            firePointOverride = overridePoint;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawTargetingRange)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, targetingRange);
        }
    }
}

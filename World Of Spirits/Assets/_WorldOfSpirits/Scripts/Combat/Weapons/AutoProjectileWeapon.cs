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
            int weaponLevel = spiritOwner != null ? spiritOwner.Progression.WeaponLevel : 1;
            float scaledDamage = damage * (1f + damageIncreasePerWeaponLevel * Mathf.Max(0, weaponLevel - 1));
            DamageContext damageContext = DamageContext.Weapon(
                scaledDamage,
                owner != null ? owner.transform : transform,
                DamageElementUtility.FromSpiritName(
                    spiritOwner != null && spiritOwner.Definition != null
                        ? spiritOwner.Definition.SpiritName
                        : string.Empty));
            float speed = projectileSpeed * (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSpeed) : 1f);
            int projectileCount = upgradeStats != null ? upgradeStats.GetProjectileCount(1) : 1;
            for (int i = 0; i < projectileCount; i++)
            {
                Vector2 shotDirection = SpreadDirection(direction, i, projectileCount, 12f);
                ProjectileBase projectile = ProjectilePool.Spawn(
                    projectilePrefab, spawnPoint.position, Quaternion.identity);
                projectile.ConfigureUpgradeModifiers(upgradeStats);
                projectile.ConfigureDamageContext(damageContext);
                projectile.Launch(shotDirection, speed, scaledDamage, owner.Faction);
            }
        }

        private static Vector2 SpreadDirection(Vector2 center, int index, int count, float spread)
        {
            if (count <= 1) return center.normalized;
            float centerAngle = Mathf.Atan2(center.y, center.x) * Mathf.Rad2Deg;
            float angle = centerAngle - spread * 0.5f + spread * index / (count - 1);
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
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

using UnityEngine;
using WorldOfSpirits.Spirits;

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

        private Transform ActiveFirePoint => firePointOverride != null ? firePointOverride : firePoint;

        private void Awake()
        {
            owner = GetComponentInParent<LivingEntity>();
            spiritOwner = GetComponentInParent<SpiritMember>();
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

        protected override float AttackCooldown => attackCooldown;

        private IDamageable FindClosestTarget()
        {
            LivingEntity[] candidates = FindObjectsByType<LivingEntity>(FindObjectsSortMode.None);
            IDamageable closest = null;
            float closestDistance = float.PositiveInfinity;

            foreach (LivingEntity candidate in candidates)
            {
                bool layerAllowed = (targetLayers.value & (1 << candidate.gameObject.layer)) != 0;
                if (!candidate.IsAlive || candidate.Faction == owner.Faction || !layerAllowed)
                {
                    continue;
                }

                float distance = (candidate.Transform.position - transform.position).sqrMagnitude;
                if (distance <= targetingRange * targetingRange && distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private void FireAt(Vector3 targetPosition)
        {
            if (projectilePrefab == null)
            {
                return;
            }

            Transform spawnPoint = ActiveFirePoint;
            Vector2 direction = targetPosition - spawnPoint.position;
            ProjectileBase projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            int weaponLevel = spiritOwner != null ? spiritOwner.Progression.WeaponLevel : 1;
            float scaledDamage = damage * (1f + damageIncreasePerWeaponLevel * Mathf.Max(0, weaponLevel - 1));
            projectile.Launch(direction, projectileSpeed, scaledDamage, owner.Faction);
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

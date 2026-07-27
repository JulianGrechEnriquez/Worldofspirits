using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public class ConfigurableProjectile : ProjectileBase
    {
        [Header("Projectile Behaviour")]
        [SerializeField, Min(0)] private int pierceCount;
        [SerializeField, Min(0)] private int bounceCount;
        [SerializeField, Min(0.1f)] private float bounceRange = 5f;
        [SerializeField, Min(0f)] private float explosionRadius;
        [SerializeField, Min(0f)] private float growthPerSecond;
        [SerializeField] private bool appliesStatus;
        [SerializeField] private CombatStatus status;
        [SerializeField, Min(0f)] private float statusDuration = 2f;
        [SerializeField, Min(0f)] private float statusStrength = 2f;

        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private readonly List<IDamageable> targetBuffer = new List<IDamageable>(64);
        private int remainingPierces;
        private int remainingBounces;
        private Vector3 initialScale;

        protected override void Awake()
        {
            base.Awake();
            initialScale = transform.localScale;
        }

        public override void Launch(Vector2 direction, float projectileSpeed, float damage, Faction ownerFaction)
        {
            hitTargets.Clear();
            remainingPierces = pierceCount;
            remainingBounces = bounceCount;
            transform.localScale = initialScale;
            base.Launch(direction, projectileSpeed, damage, ownerFaction);
        }

        public void Configure(int newPierceCount, float newExplosionRadius, float newGrowthPerSecond,
            bool shouldApplyStatus, CombatStatus newStatus, float newStatusDuration, float newStatusStrength,
            int newBounceCount = 0, float newBounceRange = 5f)
        {
            pierceCount = Mathf.Max(0, newPierceCount);
            bounceCount = Mathf.Max(0, newBounceCount);
            bounceRange = Mathf.Max(0.1f, newBounceRange);
            explosionRadius = Mathf.Max(0f, newExplosionRadius);
            growthPerSecond = Mathf.Max(0f, newGrowthPerSecond);
            appliesStatus = shouldApplyStatus;
            status = newStatus;
            statusDuration = Mathf.Max(0f, newStatusDuration);
            statusStrength = Mathf.Max(0f, newStatusStrength);
        }

        protected override void ResetPooledConfiguration(ProjectileBase prefab)
        {
            if (prefab is not ConfigurableProjectile configurablePrefab)
            {
                return;
            }

            pierceCount = configurablePrefab.pierceCount;
            bounceCount = configurablePrefab.bounceCount;
            bounceRange = configurablePrefab.bounceRange;
            explosionRadius = configurablePrefab.explosionRadius;
            growthPerSecond = configurablePrefab.growthPerSecond;
            appliesStatus = configurablePrefab.appliesStatus;
            status = configurablePrefab.status;
            statusDuration = configurablePrefab.statusDuration;
            statusStrength = configurablePrefab.statusStrength;
        }

        protected override void Update()
        {
            base.Update();
            if (growthPerSecond > 0f)
            {
                transform.localScale += Vector3.one * (growthPerSecond * Time.deltaTime);
            }
        }

        protected override void OnHit(IDamageable target)
        {
            int id = target.Transform.gameObject.GetInstanceID();
            if (!hitTargets.Add(id))
            {
                return;
            }

            if (explosionRadius > 0f)
            {
                CombatTargeting.FindAllNonAlloc(
                    transform.position, explosionRadius, OwnerFaction, targetBuffer);
                foreach (IDamageable nearbyTarget in targetBuffer)
                {
                    nearbyTarget.TakeDamage(Damage);
                }
            }
            else
            {
                target.TakeDamage(Damage);
            }

            if (appliesStatus && target is IStatusEffectReceiver receiver)
            {
                receiver.ApplyStatus(status, statusDuration, statusStrength);
            }

            if (remainingBounces > 0)
            {
                IDamageable nextTarget = FindClosestUnhitTarget(target.Transform.position);
                if (nextTarget != null)
                {
                    remainingBounces--;
                    Redirect(nextTarget.Transform.position - transform.position);
                    return;
                }
            }

            if (remainingPierces-- <= 0)
            {
                Despawn();
            }
        }

        private IDamageable FindClosestUnhitTarget(Vector3 position)
        {
            IDamageable closest = null;
            float closestDistance = bounceRange * bounceRange;
            CombatTargeting.FindAllNonAlloc(position, bounceRange, OwnerFaction, targetBuffer);
            foreach (IDamageable candidate in targetBuffer)
            {
                if (candidate == null || hitTargets.Contains(candidate.Transform.gameObject.GetInstanceID()))
                {
                    continue;
                }

                float distance = (candidate.Transform.position - position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            return closest;
        }
    }
}

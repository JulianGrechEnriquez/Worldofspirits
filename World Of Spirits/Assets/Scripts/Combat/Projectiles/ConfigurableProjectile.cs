using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public class ConfigurableProjectile : ProjectileBase
    {
        [Header("Projectile Behaviour")]
        [SerializeField, Min(0)] private int pierceCount;
        [SerializeField, Min(0f)] private float explosionRadius;
        [SerializeField, Min(0f)] private float growthPerSecond;
        [SerializeField] private bool appliesStatus;
        [SerializeField] private CombatStatus status;
        [SerializeField, Min(0f)] private float statusDuration = 2f;
        [SerializeField, Min(0f)] private float statusStrength = 2f;

        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private int remainingPierces;

        public override void Launch(Vector2 direction, float projectileSpeed, float damage, Faction ownerFaction)
        {
            remainingPierces = pierceCount;
            base.Launch(direction, projectileSpeed, damage, ownerFaction);
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
                foreach (IDamageable nearbyTarget in CombatTargeting.FindAll(transform.position, explosionRadius, OwnerFaction))
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

            if (remainingPierces-- <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

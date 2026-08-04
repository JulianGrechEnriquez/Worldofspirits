using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    /// <summary>
    /// A piercing wave that carries every enemy it touches and bursts when its
    /// travel time ends. Direct contact and the final burst deal separate hits.
    /// </summary>
    public sealed class TsunamiProjectile : ProjectileBase
    {
        [Header("Tsunami Behaviour")]
        [SerializeField, Min(0.1f)] private float explosionRadius = 1.5f;
        [SerializeField, Min(0f)] private float explosionDamageMultiplier = 1f;

        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private readonly List<IDamageable> draggedTargets = new List<IDamageable>(16);
        private readonly List<IDamageable> explosionTargets = new List<IDamageable>(64);
        private Vector2 previousPosition;

        public override void Launch(Vector2 direction, float speed, float damage, Faction ownerFaction)
        {
            hitTargets.Clear();
            draggedTargets.Clear();
            previousPosition = Body.position;
            base.Launch(direction, speed, damage, ownerFaction);
        }

        protected override void ResetPooledConfiguration(ProjectileBase prefab)
        {
            hitTargets.Clear();
            draggedTargets.Clear();
            explosionTargets.Clear();
            if (prefab is TsunamiProjectile tsunamiPrefab)
            {
                explosionRadius = tsunamiPrefab.explosionRadius;
                explosionDamageMultiplier = tsunamiPrefab.explosionDamageMultiplier;
            }
        }

        private void FixedUpdate()
        {
            Vector2 currentPosition = Body.position;
            Vector2 movement = currentPosition - previousPosition;
            previousPosition = currentPosition;
            if (movement.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            for (int i = draggedTargets.Count - 1; i >= 0; i--)
            {
                IDamageable target = draggedTargets[i];
                if (target == null || !target.IsAlive || target.Transform == null)
                {
                    draggedTargets.RemoveAt(i);
                    continue;
                }

                Rigidbody2D targetBody = target.Transform.GetComponent<Rigidbody2D>();
                if (targetBody != null)
                {
                    targetBody.position += movement;
                }
                else
                {
                    target.Transform.position += (Vector3)movement;
                }
            }
        }

        protected override void OnHit(IDamageable target)
        {
            int targetId = target.Transform.gameObject.GetInstanceID();
            if (!hitTargets.Add(targetId))
            {
                return;
            }

            target.TakeDamage(DamageSourceContext);
            if (target.IsAlive)
            {
                draggedTargets.Add(target);
            }
        }

        protected override void OnLifetimeExpired()
        {
            float radius = explosionRadius * UpgradeAreaMultiplier;
            if (radius > 0f && explosionDamageMultiplier > 0f)
            {
                CombatTargeting.FindAllNonAlloc(
                    transform.position, radius, OwnerFaction, explosionTargets);
                DamageContext explosionDamage = DamageSourceContext.WithBaseDamage(
                    DamageSourceContext.BaseDamage * explosionDamageMultiplier);
                foreach (IDamageable target in explosionTargets)
                {
                    if (target != null && target.IsAlive)
                    {
                        target.TakeDamage(explosionDamage);
                    }
                }
            }

            draggedTargets.Clear();
            Despawn();
        }
    }
}

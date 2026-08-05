using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public class DamageProjectile : ProjectileBase
    {
        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private readonly List<IDamageable> targetBuffer = new List<IDamageable>(32);
        private int remainingPierces;
        private int remainingRicochets;

        public override void Launch(Vector2 direction, float speed, float damage, Faction ownerFaction)
        {
            hitTargets.Clear();
            remainingPierces = UpgradePierceCount;
            remainingRicochets = UpgradeRicochetCount;
            base.Launch(direction, speed, damage, ownerFaction);
        }

        protected override void OnHit(IDamageable target)
        {
            int id = target.Transform.gameObject.GetInstanceID();
            if (!hitTargets.Add(id)) return;

            target.TakeDamage(DamageSourceContext);
            if (remainingRicochets > 0)
            {
                IDamageable next = FindClosestUnhit(target.Transform.position, 6f);
                if (next != null)
                {
                    remainingRicochets--;
                    Redirect(next.Transform.position - transform.position);
                    return;
                }
            }

            if (remainingPierces-- <= 0) Despawn();
        }

        private IDamageable FindClosestUnhit(Vector3 position, float range)
        {
            IDamageable closest = null;
            float closestDistance = range * range;
            CombatTargeting.FindAllNonAlloc(position, range, OwnerFaction, targetBuffer);
            foreach (IDamageable candidate in targetBuffer)
            {
                if (candidate == null || hitTargets.Contains(candidate.Transform.gameObject.GetInstanceID())) continue;
                float distance = (candidate.Transform.position - position).sqrMagnitude;
                if (distance >= closestDistance) continue;
                closest = candidate;
                closestDistance = distance;
            }
            return closest;
        }
    }
}

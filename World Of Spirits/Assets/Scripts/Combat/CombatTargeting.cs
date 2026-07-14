using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public static class CombatTargeting
    {
        public static IDamageable FindClosest(Vector3 origin, float range, Faction enemyOf)
        {
            IDamageable closest = null;
            float closestDistance = range * range;
            foreach (LivingEntity candidate in Object.FindObjectsByType<LivingEntity>(FindObjectsSortMode.None))
            {
                float distance = (candidate.Transform.position - origin).sqrMagnitude;
                if (candidate.IsAlive && candidate.Faction != enemyOf && distance <= closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        public static List<IDamageable> FindAll(Vector3 origin, float range, Faction enemyOf)
        {
            List<IDamageable> targets = new List<IDamageable>();
            foreach (LivingEntity candidate in Object.FindObjectsByType<LivingEntity>(FindObjectsSortMode.None))
            {
                if (candidate.IsAlive && candidate.Faction != enemyOf &&
                    (candidate.Transform.position - origin).sqrMagnitude <= range * range)
                {
                    targets.Add(candidate);
                }
            }

            return targets;
        }
    }
}

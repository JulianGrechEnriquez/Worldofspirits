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
            IReadOnlyList<LivingEntity> candidates = LivingEntity.ActiveEntities;
            for (int i = 0; i < candidates.Count; i++)
            {
                LivingEntity candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

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
            FindAllNonAlloc(origin, range, enemyOf, targets);
            return targets;
        }

        public static void FindAllNonAlloc(
            Vector3 origin, float range, Faction enemyOf, List<IDamageable> results)
        {
            results.Clear();
            float rangeSquared = range * range;
            IReadOnlyList<LivingEntity> candidates = LivingEntity.ActiveEntities;
            for (int i = 0; i < candidates.Count; i++)
            {
                LivingEntity candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.IsAlive && candidate.Faction != enemyOf &&
                    (candidate.Transform.position - origin).sqrMagnitude <= rangeSquared)
                {
                    results.Add(candidate);
                }
            }
        }
    }
}

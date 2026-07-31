using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public static class CombatTargeting
    {
        private static readonly List<LivingEntity> queryBuffer =
            new List<LivingEntity>(64);

        public static IDamageable FindClosest(
            Vector3 origin, float range, Faction enemyOf, int layerMask = ~0)
        {
            IDamageable closest = null;
            float closestDistance = range * range;
            CombatSimulationManager.Instance.Query(origin, range, queryBuffer);
            for (int i = 0; i < queryBuffer.Count; i++)
            {
                LivingEntity candidate = queryBuffer[i];

                float distance = (candidate.Transform.position - origin).sqrMagnitude;
                bool layerAllowed = (layerMask & (1 << candidate.gameObject.layer)) != 0;
                if (layerAllowed && candidate.IsAlive &&
                    candidate.Faction != enemyOf && distance <= closestDistance)
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
            CombatSimulationManager.Instance.Query(origin, range, queryBuffer);
            for (int i = 0; i < queryBuffer.Count; i++)
            {
                LivingEntity candidate = queryBuffer[i];

                if (candidate.IsAlive && candidate.Faction != enemyOf &&
                    (candidate.Transform.position - origin).sqrMagnitude <= rangeSquared)
                {
                    results.Add(candidate);
                }
            }
        }
    }
}

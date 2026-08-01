using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    public class ChainLightningAbility : SpiritAbility
    {
        private readonly List<IDamageable> targetBuffer = new List<IDamageable>(64);
        [SerializeField] private IntegerLevelScaling jumps = new IntegerLevelScaling();
        [SerializeField] private LevelScaling damage = new LevelScaling();
        [SerializeField] private LevelScaling jumpRange = new LevelScaling();
        [SerializeField] private LineRenderer lightningPrefab;
        [SerializeField, Min(0.01f)] private float visualDuration = 0.15f;

        protected override bool CanCast(SpiritAbilityContext context)
        {
            return CombatTargeting.FindClosest(transform.position, jumpRange.Evaluate(CurrentLevel), Faction.Player) != null;
        }

        protected override void Cast(SpiritAbilityContext context)
        {
            HashSet<Transform> hit = new HashSet<Transform>();
            Vector3 currentPosition = transform.position;
            for (int i = 0; i < Mathf.Max(1, jumps.Evaluate(CurrentLevel)); i++)
            {
                IDamageable target = FindNext(currentPosition, hit);
                if (target == null) break;
                target.TakeDamage(CreateSpiritDamage(damage.Evaluate(CurrentLevel)));
                DrawArc(currentPosition, target.Transform.position);
                hit.Add(target.Transform);
                currentPosition = target.Transform.position;
            }
        }

        private IDamageable FindNext(Vector3 position, HashSet<Transform> hit)
        {
            IDamageable closest = null;
            float range = jumpRange.Evaluate(CurrentLevel) *
                (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            float distance = range * range;
            CombatTargeting.FindAllNonAlloc(
                position, range, Faction.Player, targetBuffer);
            foreach (IDamageable candidate in targetBuffer)
            {
                float candidateDistance = (candidate.Transform.position - position).sqrMagnitude;
                if (!hit.Contains(candidate.Transform) && candidateDistance <= distance)
                {
                    closest = candidate;
                    distance = candidateDistance;
                }
            }
            return closest;
        }

        private void DrawArc(Vector3 start, Vector3 end)
        {
            if (lightningPrefab == null) return;
            LineRenderer line = SceneObjectPool.Spawn(
                lightningPrefab, Vector3.zero, Quaternion.identity, PoolCategory.Effects);
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            SceneObjectPool.ReleaseAfter(line.gameObject, visualDuration);
        }
    }
}

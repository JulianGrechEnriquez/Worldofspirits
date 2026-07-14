using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public enum EffectSpawnPosition
    {
        AtSpirit,
        AtPlayer,
        AtClosestEnemy,
        RandomAroundPlayer
    }

    public class SpawnEffectAbility : SpiritAbility
    {
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private EffectSpawnPosition spawnPosition;
        [SerializeField] private IntegerLevelScaling spawnCount = new IntegerLevelScaling();
        [SerializeField] private LevelScaling spawnRadius = new LevelScaling();
        [SerializeField, Min(0.1f)] private float targetingRange = 15f;

        protected override bool CanCast(SpiritAbilityContext context) => effectPrefab != null;

        protected override void Cast(SpiritAbilityContext context)
        {
            int count = Mathf.Max(1, spawnCount.Evaluate(CurrentLevel));
            for (int i = 0; i < count; i++)
            {
                Instantiate(effectPrefab, ResolvePosition(context), Quaternion.identity);
            }
        }

        private Vector3 ResolvePosition(SpiritAbilityContext context)
        {
            Vector3 playerPosition = context.Player != null ? context.Player.position : transform.position;
            if (spawnPosition == EffectSpawnPosition.AtPlayer) return playerPosition;
            if (spawnPosition == EffectSpawnPosition.AtClosestEnemy)
            {
                IDamageable target = CombatTargeting.FindClosest(transform.position, targetingRange, Faction.Player);
                return target != null ? target.Transform.position : transform.position;
            }
            if (spawnPosition == EffectSpawnPosition.RandomAroundPlayer)
            {
                return playerPosition + (Vector3)(Random.insideUnitCircle * spawnRadius.Evaluate(CurrentLevel));
            }
            return transform.position;
        }
    }
}

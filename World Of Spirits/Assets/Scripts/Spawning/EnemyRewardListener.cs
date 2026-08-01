using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Enemies;
using WorldOfSpirits.Progression;

namespace WorldOfSpirits.Spawning
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyPool))]
    public sealed class EnemyRewardListener : MonoBehaviour
    {
        private EnemyPool enemyPool;

        private void Awake()
        {
            enemyPool = GetComponent<EnemyPool>();
        }

        private void OnEnable()
        {
            if (enemyPool != null) enemyPool.EnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            if (enemyPool != null) enemyPool.EnemyKilled -= HandleEnemyKilled;
        }

        private static void HandleEnemyKilled(EnemyBase enemy)
        {
            if (enemy is not IRewardSource reward || reward.ExperienceReward <= 0f) return;
            ExperienceOrbService.Spawn(enemy.transform.position, reward.ExperienceReward);
        }
    }
}

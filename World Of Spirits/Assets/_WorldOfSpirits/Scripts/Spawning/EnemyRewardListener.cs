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
        [SerializeField, Min(0.01f)] private float spiritDustPerExperience = 0.25f;
        private EnemyPool enemyPool;
        private SpiritDustWallet spiritDustWallet;

        private void Awake()
        {
            enemyPool = GetComponent<EnemyPool>();
            spiritDustWallet = FindFirstObjectByType<SpiritDustWallet>();
        }

        private void OnEnable()
        {
            if (enemyPool != null) enemyPool.EnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            if (enemyPool != null) enemyPool.EnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            if (enemy is not IRewardSource reward || reward.ExperienceReward <= 0f) return;
            ExperienceOrbService.Spawn(enemy.transform.position, reward.ExperienceReward);
            if (spiritDustWallet == null) spiritDustWallet = FindFirstObjectByType<SpiritDustWallet>();
            spiritDustWallet?.Add(Mathf.Max(1, Mathf.RoundToInt(reward.ExperienceReward * spiritDustPerExperience)));
        }
    }
}

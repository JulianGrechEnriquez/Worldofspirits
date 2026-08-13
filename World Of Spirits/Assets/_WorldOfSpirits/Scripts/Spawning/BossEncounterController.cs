using UnityEngine;
using UnityEngine.Events;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Spawning
{
    /// <summary>Starts one configured boss at a run time and forwards its result to UI/scene flow.</summary>
    [DisallowMultipleComponent]
    public sealed class BossEncounterController : MonoBehaviour
    {
        [SerializeField] private SpawnDirector spawnDirector;
        [SerializeField] private EnemySpawnData boss;
        [SerializeField, Min(1f)] private float triggerAtSeconds = 600f;
        [SerializeField] private UnityEvent<EnemyBase> bossStarted;
        [SerializeField] private UnityEvent<EnemyBase> bossDefeated;

        private bool triggered;

        private void Awake()
        {
            if (spawnDirector == null) spawnDirector = FindFirstObjectByType<SpawnDirector>();
            if (spawnDirector == null)
            {
                Debug.LogError("Boss Encounter Controller requires a SpawnDirector.", this);
                enabled = false;
                return;
            }
            spawnDirector.BossStarted += HandleStarted;
            spawnDirector.BossDefeated += HandleDefeated;
        }

        private void OnDestroy()
        {
            if (spawnDirector == null) return;
            spawnDirector.BossStarted -= HandleStarted;
            spawnDirector.BossDefeated -= HandleDefeated;
        }

        private void Update()
        {
            if (!triggered && spawnDirector.ElapsedRunTime >= triggerAtSeconds)
            {
                triggered = spawnDirector.StartBossEvent(boss);
            }
        }

        private void HandleStarted(EnemyBase enemy) => bossStarted?.Invoke(enemy);
        private void HandleDefeated(EnemyBase enemy) => bossDefeated?.Invoke(enemy);
    }
}

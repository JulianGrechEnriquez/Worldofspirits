using System;
using System.Collections;
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
        [SerializeField, Min(0f)] private float warningDuration = 5f;
        [SerializeField] private UnityEvent<EnemyBase> bossStarted;
        [SerializeField] private UnityEvent<EnemyBase> bossDefeated;

        private bool triggered;
        private bool countdownActive;

        public event Action<float> BossCountdownStarted;

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
            if (!triggered && !countdownActive && spawnDirector.ElapsedRunTime >= triggerAtSeconds)
            {
                BeginBossCountdown();
            }
        }

        /// <summary>Pauses normal spawning, shows the warning period, then starts the boss.</summary>
        public bool BeginBossCountdown()
        {
            if (triggered || countdownActive || spawnDirector == null)
                return false;

            countdownActive = true;
            spawnDirector.PauseSpawning();
            spawnDirector.ClearNormalEnemies();
            BossCountdownStarted?.Invoke(warningDuration);
            StartCoroutine(BossCountdownRoutine());
            return true;
        }

        private IEnumerator BossCountdownRoutine()
        {
            if (warningDuration > 0f)
                yield return new WaitForSecondsRealtime(warningDuration);

            countdownActive = false;
            if (!StartBossEncounter())
                spawnDirector.ResumeSpawning();
        }

        /// <summary>
        /// Starts the configured encounter through the same path used by the run timer.
        /// Returns false when it has already started or configuration is incomplete.
        /// </summary>
        public bool StartBossEncounter()
        {
            if (triggered || spawnDirector == null)
            {
                return false;
            }

            triggered = spawnDirector.StartBossEvent(boss);
            return triggered;
        }

        private void HandleStarted(EnemyBase enemy) => bossStarted?.Invoke(enemy);
        private void HandleDefeated(EnemyBase enemy) => bossDefeated?.Invoke(enemy);
    }
}

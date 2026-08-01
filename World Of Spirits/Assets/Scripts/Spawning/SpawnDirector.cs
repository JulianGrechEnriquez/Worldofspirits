using UnityEngine;
using WorldOfSpirits.Crowd;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Spawning
{
    /// <summary>
    /// The only high-level system permitted to request enemy spawns.
    /// It converts a time-scaled budget into pooled enemies selected from the
    /// active biome using weighted random selection.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpawnDirector : MonoBehaviour
    {
        [Header("Required References")]
        [Tooltip("Player used as the center of the spawning ring.")]
        [SerializeField] private Transform player;

        [Tooltip("Gameplay camera used to reject visible spawn positions.")]
        [SerializeField] private Camera gameplayCamera;

        [Tooltip("High-level pool that creates and tracks active enemies.")]
        [SerializeField] private EnemyPool enemyPool;

        [Tooltip("Crowd simulation used to reject positions already occupied by enemies.")]
        [SerializeField] private CrowdSimulationManager crowdSimulation;

        [Tooltip("Enemy roster used by normal budget spawning.")]
        [SerializeField] private BiomeSpawnData activeBiome;

        [Header("Population")]
        [Tooltip("Normal spawning stops while this many enemies are alive.")]
        [SerializeField, Min(1)] private int maximumAliveEnemies = 250;

        [Tooltip("Maximum enemies a single budget cycle may create. This prevents frame spikes.")]
        [SerializeField, Min(1)] private int maximumSpawnsPerCycle = 20;

        [Header("Difficulty Over Time")]
        [Tooltip("Spawn budget granted each cycle. The horizontal axis is elapsed run time in minutes.")]
        [SerializeField] private AnimationCurve budgetByMinute =
            new AnimationCurve(
                new Keyframe(0f, 4f),
                new Keyframe(5f, 12f),
                new Keyframe(10f, 24f));

        [Tooltip("Seconds between budget cycles. The horizontal axis is elapsed run time in minutes.")]
        [SerializeField] private AnimationCurve intervalByMinute =
            new AnimationCurve(
                new Keyframe(0f, 2f),
                new Keyframe(5f, 1.25f),
                new Keyframe(10f, 0.65f));

        [Tooltip("Chance that a cycle selects elite entries. The horizontal axis is elapsed run time in minutes.")]
        [SerializeField] private AnimationCurve eliteChanceByMinute =
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(5f, 0.05f),
                new Keyframe(10f, 0.15f));

        [Header("Spawn Ring")]
        [Tooltip("Closest permitted spawn distance from the player.")]
        [SerializeField, Min(0.1f)] private float minimumSpawnRadius = 10f;

        [Tooltip("Farthest permitted spawn distance from the player.")]
        [SerializeField, Min(0.1f)] private float maximumSpawnRadius = 18f;

        [Tooltip("Extra viewport margin used to guarantee enemies begin off camera.")]
        [SerializeField, Min(0f)] private float viewportMargin = 0.05f;

        [Tooltip("Radius checked against obstacles at a candidate spawn position.")]
        [SerializeField, Min(0.01f)] private float clearanceRadius = 0.4f;

        [Tooltip("Minimum distance maintained between newly spawned enemies.")]
        [SerializeField, Min(0f)] private float minimumEnemySpacing = 1f;

        [Tooltip("Number of candidate positions tested before abandoning one spawn.")]
        [SerializeField, Min(1)] private int positionAttempts = 12;

        [Header("Valid Locations")]
        [Tooltip("Layers representing valid walkable ground. Leave empty to accept any unobstructed position.")]
        [SerializeField] private LayerMask validGroundLayers;

        [Tooltip("Layers that make a spawn position invalid, such as walls and props.")]
        [SerializeField] private LayerMask obstacleLayers;

        [Header("Runtime State")]
        [Tooltip("Begin continuous normal spawning when the scene starts.")]
        [SerializeField] private bool startActive = true;

        [Tooltip("Resume normal spawning automatically after a spawned boss dies.")]
        [SerializeField] private bool resumeAfterBoss = true;

        private float elapsedRunTime;
        private float nextSpawnTime;
        private bool spawningPaused;
        private bool bossEventActive;
        private EnemyBase activeBoss;

        public float ElapsedRunTime => elapsedRunTime;
        public bool IsSpawningPaused => spawningPaused;
        public bool IsBossEventActive => bossEventActive;
        public BiomeSpawnData ActiveBiome => activeBiome;
        public event System.Action<EnemyBase> BossStarted;
        public event System.Action<EnemyBase> BossDefeated;

        private void Awake()
        {
            spawningPaused = !startActive;

            if (enemyPool != null && activeBiome != null)
            {
                enemyPool.Preload(activeBiome);
            }
        }

        private void OnEnable()
        {
            nextSpawnTime = Time.time;
        }

        private void OnDisable()
        {
            UnsubscribeFromBoss();
        }

        private void Update()
        {
            elapsedRunTime += Time.deltaTime;

            if (spawningPaused || bossEventActive || !CanSpawnNormally())
            {
                return;
            }

            if (Time.time < nextSpawnTime)
            {
                return;
            }

            float elapsedMinutes = elapsedRunTime / 60f;
            float interval = Mathf.Max(0.05f, intervalByMinute.Evaluate(elapsedMinutes));
            nextSpawnTime = Time.time + interval;
            SpendBudget(Mathf.Max(0, Mathf.FloorToInt(budgetByMinute.Evaluate(elapsedMinutes))));
        }

        public void PauseSpawning()
        {
            spawningPaused = true;
        }

        public void ResumeSpawning()
        {
            if (bossEventActive)
            {
                return;
            }

            spawningPaused = false;
            nextSpawnTime = Time.time;
        }

        public void StopSpawning()
        {
            spawningPaused = true;
        }

        /// <summary>
        /// Changes the normal enemy roster and prepares its pools.
        /// Existing enemies are not removed.
        /// </summary>
        public void SetActiveBiome(BiomeSpawnData biome)
        {
            if (biome == null || biome == activeBiome)
            {
                return;
            }

            activeBiome = biome;
            enemyPool.Preload(activeBiome);
        }

        /// <summary>
        /// Stops normal spawning and creates one boss outside the camera.
        /// The boss must be marked as a boss in EnemySpawnData.
        /// </summary>
        public bool StartBossEvent(EnemySpawnData bossData)
        {
            if (bossEventActive || bossData == null || !bossData.IsBoss ||
                enemyPool == null || player == null)
            {
                return false;
            }

            if (!TryGetSpawnPosition(out Vector3 position))
            {
                Debug.LogWarning("Boss event could not find a valid off-camera spawn position.", this);
                return false;
            }

            bossEventActive = true;
            spawningPaused = true;
            activeBoss = enemyPool.Spawn(bossData, position, Quaternion.identity);
            if (activeBoss == null)
            {
                bossEventActive = false;
                return false;
            }

            activeBoss.Died += HandleBossDied;
            BossStarted?.Invoke(activeBoss);
            return true;
        }

        private void SpendBudget(int budget)
        {
            if (budget <= 0)
            {
                return;
            }

            int spawnedThisCycle = 0;
            bool requestElite = Random.value <
                Mathf.Clamp01(eliteChanceByMinute.Evaluate(elapsedRunTime / 60f));

            while (budget > 0 &&
                   spawnedThisCycle < maximumSpawnsPerCycle &&
                   enemyPool.AliveCount < maximumAliveEnemies)
            {
                EnemySpawnData selected = SelectWeightedEnemy(budget, requestElite);
                if (selected == null && requestElite)
                {
                    // An elite roll should not waste the complete cycle when
                    // no eligible elite fits the remaining budget.
                    requestElite = false;
                    selected = SelectWeightedEnemy(budget, false);
                }

                if (selected == null || !TryGetSpawnPosition(out Vector3 position))
                {
                    break;
                }

                EnemyBase spawned = enemyPool.Spawn(selected, position, Quaternion.identity);
                if (spawned == null)
                {
                    break;
                }

                budget -= selected.SpawnCost;
                spawnedThisCycle++;
            }
        }

        private EnemySpawnData SelectWeightedEnemy(int remainingBudget, bool eliteOnly)
        {
            if (activeBiome == null)
            {
                return null;
            }

            float totalWeight = 0f;
            var entries = activeBiome.Enemies;
            for (int i = 0; i < entries.Count; i++)
            {
                EnemySpawnData data = entries[i]?.Enemy;
                if (IsValidNormalCandidate(data, remainingBudget, eliteOnly))
                {
                    totalWeight += data.SpawnWeight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float selection = Random.value * totalWeight;
            for (int i = 0; i < entries.Count; i++)
            {
                EnemySpawnData data = entries[i]?.Enemy;
                if (!IsValidNormalCandidate(data, remainingBudget, eliteOnly))
                {
                    continue;
                }

                selection -= data.SpawnWeight;
                if (selection <= 0f)
                {
                    return data;
                }
            }

            return null;
        }

        private bool IsValidNormalCandidate(
            EnemySpawnData data,
            int remainingBudget,
            bool eliteOnly)
        {
            return data != null &&
                   !data.IsBoss &&
                   data.IsElite == eliteOnly &&
                   data.SpawnCost <= remainingBudget &&
                   data.IsAvailable(activeBiome.BiomeId);
        }

        private bool TryGetSpawnPosition(out Vector3 position)
        {
            Vector3 playerPosition = player.position;
            for (int attempt = 0; attempt < positionAttempts; attempt++)
            {
                Vector2 direction = Random.insideUnitCircle;
                if (direction.sqrMagnitude < 0.001f)
                {
                    direction = Vector2.right;
                }
                else
                {
                    direction.Normalize();
                }

                float distance = Random.Range(minimumSpawnRadius, maximumSpawnRadius);
                Vector3 candidate = playerPosition + (Vector3)(direction * distance);

                if (IsVisible(candidate) ||
                    IsBlocked(candidate) ||
                    IsOccupiedByEnemy(candidate) ||
                    !IsValidGround(candidate))
                {
                    continue;
                }

                position = candidate;
                return true;
            }

            position = default;
            return false;
        }

        private bool IsVisible(Vector3 worldPosition)
        {
            if (gameplayCamera == null)
            {
                return false;
            }

            Vector3 viewport = gameplayCamera.WorldToViewportPoint(worldPosition);
            return viewport.z > 0f &&
                   viewport.x >= -viewportMargin &&
                   viewport.x <= 1f + viewportMargin &&
                   viewport.y >= -viewportMargin &&
                   viewport.y <= 1f + viewportMargin;
        }

        private bool IsBlocked(Vector3 worldPosition)
        {
            return obstacleLayers.value != 0 &&
                   Physics2D.OverlapCircle(worldPosition, clearanceRadius, obstacleLayers) != null;
        }

        private bool IsValidGround(Vector3 worldPosition)
        {
            return validGroundLayers.value == 0 ||
                   Physics2D.OverlapPoint(worldPosition, validGroundLayers) != null;
        }

        private bool IsOccupiedByEnemy(Vector3 worldPosition)
        {
            if (crowdSimulation == null || minimumEnemySpacing <= 0f)
            {
                return false;
            }

            return crowdSimulation.HasAgentWithin(worldPosition, minimumEnemySpacing);
        }

        private bool CanSpawnNormally()
        {
            return player != null &&
                   enemyPool != null &&
                   activeBiome != null &&
                   enemyPool.AliveCount < maximumAliveEnemies;
        }

        private void HandleBossDied()
        {
            EnemyBase defeatedBoss = activeBoss;
            UnsubscribeFromBoss();
            bossEventActive = false;
            BossDefeated?.Invoke(defeatedBoss);

            if (resumeAfterBoss)
            {
                spawningPaused = false;
                nextSpawnTime = Time.time;
            }
        }

        private void UnsubscribeFromBoss()
        {
            if (activeBoss != null)
            {
                activeBoss.Died -= HandleBossDied;
                activeBoss = null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maximumAliveEnemies = Mathf.Max(1, maximumAliveEnemies);
            maximumSpawnsPerCycle = Mathf.Max(1, maximumSpawnsPerCycle);
            minimumSpawnRadius = Mathf.Max(0.1f, minimumSpawnRadius);
            maximumSpawnRadius = Mathf.Max(minimumSpawnRadius, maximumSpawnRadius);
            viewportMargin = Mathf.Max(0f, viewportMargin);
            clearanceRadius = Mathf.Max(0.01f, clearanceRadius);
            minimumEnemySpacing = Mathf.Max(0f, minimumEnemySpacing);
            positionAttempts = Mathf.Max(1, positionAttempts);
        }
#endif
    }
}

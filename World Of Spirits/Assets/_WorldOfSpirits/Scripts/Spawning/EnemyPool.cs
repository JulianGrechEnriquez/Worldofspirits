using System;
using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Core;
using WorldOfSpirits.Crowd;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Spawning
{
    /// <summary>
    /// High-level enemy pool used by SpawnDirector.
    ///
    /// SceneObjectPool owns the reusable instances. This class adds biome
    /// preloading, alive-enemy accounting, death tracking, and spatial-system
    /// registration without creating a second competing pool.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyPool : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("High-performance crowd simulation used by every pooled enemy.")]
        [SerializeField] private CrowdSimulationManager crowdSimulation;

        [Header("Preloading")]
        [Tooltip("Optional biome to preload when this component starts.")]
        [SerializeField] private BiomeSpawnData initialBiome;

        [Tooltip("Preload the initial biome during Awake instead of waiting for SpawnDirector.")]
        [SerializeField] private bool preloadOnAwake = true;

        private readonly HashSet<EnemyBase> activeEnemies =
            new HashSet<EnemyBase>();
        private readonly HashSet<EnemyBase> trackedInstances =
            new HashSet<EnemyBase>();
        private readonly Dictionary<EnemyBase, Action> deathCallbacks =
            new Dictionary<EnemyBase, Action>(256);
        private readonly Dictionary<EnemyBase, EnemyCrowdAgent> crowdAgents =
            new Dictionary<EnemyBase, EnemyCrowdAgent>(256);
        private readonly HashSet<BiomeSpawnData> preloadedBiomes =
            new HashSet<BiomeSpawnData>();
        private readonly List<EnemyBase> despawnBuffer = new List<EnemyBase>(256);
        private int aliveEliteCount;

        public int AliveCount => activeEnemies.Count;
        public int AliveEliteCount => aliveEliteCount;
        public event Action<EnemyBase> EnemyKilled;

        private void Awake()
        {
            if (preloadOnAwake && initialBiome != null)
            {
                Preload(initialBiome);
            }
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<EnemyBase, Action> pair in deathCallbacks)
            {
                if (pair.Key != null)
                {
                    pair.Key.Died -= pair.Value;
                }
            }

            deathCallbacks.Clear();
            crowdAgents.Clear();
            trackedInstances.Clear();
            activeEnemies.Clear();
            aliveEliteCount = 0;
        }

        /// <summary>
        /// Creates the configured number of inactive instances for a biome.
        /// Calling this again for the same asset does nothing.
        /// </summary>
        public void Preload(BiomeSpawnData biome)
        {
            if (biome == null || !preloadedBiomes.Add(biome))
            {
                return;
            }

            IReadOnlyList<BiomeEnemyEntry> entries = biome.Enemies;
            for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                BiomeEnemyEntry entry = entries[entryIndex];
                EnemySpawnData spawnData = entry?.Enemy;
                if (spawnData == null || spawnData.EnemyPrefab == null)
                {
                    continue;
                }

                for (int instanceIndex = 0; instanceIndex < entry.PreloadCount; instanceIndex++)
                {
                    EnemyBase enemy = SpawnLowLevel(
                        spawnData.EnemyPrefab,
                        Vector3.zero,
                        Quaternion.identity);
                    if (enemy == null)
                    {
                        break;
                    }

                    TrackInstance(enemy);
                    SceneObjectPool.ReleaseOrDestroy(enemy.gameObject);
                }
            }
        }

        /// <summary>
        /// Gets an enemy from the reusable scene pool, expanding automatically
        /// when no inactive instance is available.
        /// </summary>
        public EnemyBase Spawn(
            EnemySpawnData spawnData,
            Vector3 position,
            Quaternion rotation)
        {
            if (spawnData == null || spawnData.EnemyPrefab == null)
            {
                Debug.LogError("EnemyPool cannot spawn an empty EnemySpawnData.", this);
                return null;
            }

            EnemyBase enemy = SpawnLowLevel(spawnData.EnemyPrefab, position, rotation);
            if (enemy == null)
            {
                return null;
            }

            enemy.ConfigureClassification(spawnData.IsElite, spawnData.IsBoss);
            if (enemy is BossEnemyBase boss && spawnData.BossData != null)
            {
                boss.Initialize(spawnData.BossData);
            }

            TrackInstance(enemy);
            if (activeEnemies.Add(enemy))
            {
                if (spawnData.IsElite) aliveEliteCount++;
                if (crowdAgents.TryGetValue(enemy, out EnemyCrowdAgent crowdAgent) &&
                    crowdAgent != null && crowdSimulation != null)
                {
                    crowdAgent.ResetRuntimeState();
                    crowdSimulation.Register(crowdAgent);
                }
            }

            return enemy;
        }

        /// <summary>
        /// Returns an enemy early, for example when clearing normal enemies
        /// before a boss event.
        /// </summary>
        public void Despawn(EnemyBase enemy)
        {
            if (enemy == null)
            {
                return;
            }

            DeactivateTracking(enemy);
            SceneObjectPool.ReleaseOrDestroy(enemy.gameObject);
        }

        /// <summary>Returns all active normal enemies to their pools while preserving bosses.</summary>
        public void DespawnAllNonBosses()
        {
            despawnBuffer.Clear();
            foreach (EnemyBase enemy in activeEnemies)
            {
                if (enemy != null && !enemy.IsBoss)
                    despawnBuffer.Add(enemy);
            }

            for (int i = 0; i < despawnBuffer.Count; i++)
                Despawn(despawnBuffer[i]);
            despawnBuffer.Clear();
        }

        private static EnemyBase SpawnLowLevel(
            EnemyBase prefab,
            Vector3 position,
            Quaternion rotation)
        {
            return global::WorldOfSpirits.Enemies.EnemyPool.Spawn(
                prefab,
                position,
                rotation);
        }

        private void TrackInstance(EnemyBase enemy)
        {
            if (enemy == null || !trackedInstances.Add(enemy))
            {
                return;
            }

            // One closure is allocated per created pooled instance, not per
            // spawn and never inside Update/FixedUpdate.
            Action callback = () => HandleEnemyDeath(enemy);
            deathCallbacks.Add(enemy, callback);
            enemy.Died += callback;

            if (enemy.TryGetComponent(out EnemyCrowdAgent crowdAgent))
            {
                crowdAgents.Add(enemy, crowdAgent);
            }
        }

        private void HandleEnemyDeath(EnemyBase enemy)
        {
            // LivingEntity releases itself to SceneObjectPool immediately
            // after raising Died. This callback only updates high-level state.
            EnemyKilled?.Invoke(enemy);
            DeactivateTracking(enemy);
        }

        private void DeactivateTracking(EnemyBase enemy)
        {
            if (!activeEnemies.Remove(enemy))
            {
                return;
            }

            if (enemy.IsElite) aliveEliteCount = Mathf.Max(0, aliveEliteCount - 1);

            if (crowdAgents.TryGetValue(enemy, out EnemyCrowdAgent crowdAgent) &&
                crowdAgent != null && crowdSimulation != null)
            {
                crowdSimulation.Unregister(crowdAgent);
            }
        }
    }
}

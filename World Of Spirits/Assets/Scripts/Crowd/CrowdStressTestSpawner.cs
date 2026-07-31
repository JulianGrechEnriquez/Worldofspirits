using UnityEngine;
using WorldOfSpirits.Spawning;

namespace WorldOfSpirits.Crowd
{
    /// <summary>
    /// Development-only helper for repeatable crowd performance tests.
    /// Keep disabled in production scenes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CrowdStressTestSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private EnemySpawnData enemy;
        [SerializeField] private Transform center;

        [Header("Test")]
        [SerializeField, Min(1)] private int targetCount = 300;
        [SerializeField, Min(1)] private int spawnsPerFrame = 25;
        [SerializeField, Min(1f)] private float radius = 25f;
        [SerializeField] private bool spawnOnStart;

        private int spawnedCount;
        private bool running;

        private void Start()
        {
            running = spawnOnStart;
        }

        private void Update()
        {
            if (!running || enemyPool == null || enemy == null || center == null)
            {
                return;
            }

            int count = Mathf.Min(spawnsPerFrame, targetCount - spawnedCount);
            for (int i = 0; i < count; i++)
            {
                float angle = (spawnedCount * 137.508f) * Mathf.Deg2Rad;
                float normalized = Mathf.Sqrt((spawnedCount + 0.5f) / targetCount);
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) *
                    (normalized * radius);
                if (enemyPool.Spawn(
                        enemy,
                        center.position + (Vector3)offset,
                        Quaternion.identity) != null)
                {
                    spawnedCount++;
                }
            }

            if (spawnedCount >= targetCount)
            {
                running = false;
            }
        }

        [ContextMenu("Begin Stress Test")]
        public void Begin()
        {
            spawnedCount = 0;
            running = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            targetCount = Mathf.Max(1, targetCount);
            spawnsPerFrame = Mathf.Max(1, spawnsPerFrame);
            radius = Mathf.Max(1f, radius);
        }
#endif
    }
}

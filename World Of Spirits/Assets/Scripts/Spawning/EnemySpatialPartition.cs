using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Spawning
{
    /// <summary>
    /// Spatial hash for active enemies.
    ///
    /// Systems such as local separation, targeting, and area damage can query
    /// nearby grid cells instead of scanning every living enemy. The grid does
    /// not replace Unity's physics broad phase; it prevents custom gameplay
    /// systems from creating their own O(n²) enemy checks.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemySpatialPartition : MonoBehaviour
    {
        [Header("Partition Settings")]
        [Tooltip("World-space width and height of each square partition. Use a value near the largest common query radius.")]
        [SerializeField, Min(0.5f)] private float cellSize = 4f;

        [Tooltip("Maximum registered enemies whose partition membership is refreshed per frame. Zero refreshes every enemy.")]
        [SerializeField, Min(0)] private int positionUpdatesPerFrame = 64;

        [Header("Diagnostics")]
        [Tooltip("Draw occupied partitions when this component is selected.")]
        [SerializeField] private bool drawOccupiedCells;

        private readonly Dictionary<long, List<EnemyBase>> cells =
            new Dictionary<long, List<EnemyBase>>(128);
        private readonly Dictionary<EnemyBase, long> enemyCells =
            new Dictionary<EnemyBase, long>(256);
        private readonly Dictionary<EnemyBase, int> enemyIndices =
            new Dictionary<EnemyBase, int>(256);
        private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>(256);

        private float inverseCellSize;
        private int nextUpdateIndex;

        public int Count => activeEnemies.Count;
        public float CellSize => cellSize;

        private void Awake()
        {
            inverseCellSize = 1f / cellSize;
        }

        private void Update()
        {
            int count = activeEnemies.Count;
            if (count == 0)
            {
                nextUpdateIndex = 0;
                return;
            }

            int updateCount = positionUpdatesPerFrame <= 0
                ? count
                : Mathf.Min(positionUpdatesPerFrame, count);

            for (int i = 0; i < updateCount; i++)
            {
                if (nextUpdateIndex >= activeEnemies.Count)
                {
                    nextUpdateIndex = 0;
                }

                EnemyBase enemy = activeEnemies[nextUpdateIndex++];
                if (enemy != null && enemy.isActiveAndEnabled)
                {
                    Refresh(enemy);
                }
            }
        }

        /// <summary>
        /// Adds a spawned enemy to the grid. EnemyPool will call this once when
        /// an instance becomes active.
        /// </summary>
        public void Register(EnemyBase enemy)
        {
            if (enemy == null || enemyCells.ContainsKey(enemy))
            {
                return;
            }

            long key = PositionToKey(enemy.transform.position);
            AddToCell(key, enemy);
            enemyCells.Add(enemy, key);
            enemyIndices.Add(enemy, activeEnemies.Count);
            activeEnemies.Add(enemy);
        }

        /// <summary>
        /// Removes a pooled or disabled enemy in O(1) average time.
        /// </summary>
        public void Unregister(EnemyBase enemy)
        {
            if (enemy == null || !enemyCells.TryGetValue(enemy, out long key))
            {
                return;
            }

            RemoveFromCell(key, enemy);
            enemyCells.Remove(enemy);

            int removedIndex = enemyIndices[enemy];
            int lastIndex = activeEnemies.Count - 1;
            EnemyBase lastEnemy = activeEnemies[lastIndex];
            activeEnemies[removedIndex] = lastEnemy;
            activeEnemies.RemoveAt(lastIndex);
            enemyIndices.Remove(enemy);

            if (removedIndex < activeEnemies.Count)
            {
                enemyIndices[lastEnemy] = removedIndex;
            }

            if (nextUpdateIndex > activeEnemies.Count)
            {
                nextUpdateIndex = 0;
            }
        }

        /// <summary>
        /// Immediately updates an enemy's partition. This is useful after a
        /// teleport or spawn; ordinary movement is refreshed incrementally.
        /// </summary>
        public void Refresh(EnemyBase enemy)
        {
            if (enemy == null || !enemyCells.TryGetValue(enemy, out long previousKey))
            {
                return;
            }

            long currentKey = PositionToKey(enemy.transform.position);
            if (currentKey == previousKey)
            {
                return;
            }

            RemoveFromCell(previousKey, enemy);
            AddToCell(currentKey, enemy);
            enemyCells[enemy] = currentKey;
        }

        /// <summary>
        /// Appends active enemies inside radius to a caller-owned list.
        /// No garbage is generated when the result list has enough capacity.
        /// </summary>
        /// <returns>Number of enemies appended to results.</returns>
        public int QueryRadius(Vector2 center, float radius, List<EnemyBase> results)
        {
            if (results == null)
            {
                Debug.LogError("EnemySpatialPartition requires a caller-owned result list.", this);
                return 0;
            }

            float safeRadius = Mathf.Max(0f, radius);
            float radiusSquared = safeRadius * safeRadius;
            int minimumX = WorldToCell(center.x - safeRadius);
            int maximumX = WorldToCell(center.x + safeRadius);
            int minimumY = WorldToCell(center.y - safeRadius);
            int maximumY = WorldToCell(center.y + safeRadius);
            int initialCount = results.Count;

            for (int y = minimumY; y <= maximumY; y++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    if (!cells.TryGetValue(CellToKey(x, y), out List<EnemyBase> occupants))
                    {
                        continue;
                    }

                    for (int i = 0; i < occupants.Count; i++)
                    {
                        EnemyBase enemy = occupants[i];
                        if (enemy == null || !enemy.isActiveAndEnabled || !enemy.IsAlive)
                        {
                            continue;
                        }

                        Vector2 offset = (Vector2)enemy.transform.position - center;
                        if (offset.sqrMagnitude <= radiusSquared)
                        {
                            results.Add(enemy);
                        }
                    }
                }
            }

            return results.Count - initialCount;
        }

        private void AddToCell(long key, EnemyBase enemy)
        {
            if (!cells.TryGetValue(key, out List<EnemyBase> occupants))
            {
                occupants = new List<EnemyBase>(8);
                cells.Add(key, occupants);
            }

            occupants.Add(enemy);
        }

        private void RemoveFromCell(long key, EnemyBase enemy)
        {
            if (!cells.TryGetValue(key, out List<EnemyBase> occupants))
            {
                return;
            }

            int index = occupants.IndexOf(enemy);
            if (index >= 0)
            {
                int lastIndex = occupants.Count - 1;
                occupants[index] = occupants[lastIndex];
                occupants.RemoveAt(lastIndex);
            }

            if (occupants.Count == 0)
            {
                cells.Remove(key);
            }
        }

        private long PositionToKey(Vector3 position)
        {
            return CellToKey(WorldToCell(position.x), WorldToCell(position.y));
        }

        private int WorldToCell(float coordinate)
        {
            return Mathf.FloorToInt(coordinate * inverseCellSize);
        }

        private static long CellToKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            cellSize = Mathf.Max(0.5f, cellSize);
            inverseCellSize = 1f / cellSize;
            positionUpdatesPerFrame = Mathf.Max(0, positionUpdatesPerFrame);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawOccupiedCells || cells.Count == 0)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            foreach (long key in cells.Keys)
            {
                int x = (int)(key >> 32);
                int y = (int)key;
                Vector3 center = new Vector3(
                    (x + 0.5f) * cellSize,
                    (y + 0.5f) * cellSize,
                    0f);
                Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0f));
            }
        }
#endif
    }
}

using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    /// <summary>
    /// Centralizes spatial refreshes and status ticks so hundreds of entities
    /// do not each need an Update callback.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class CombatSimulationManager : MonoBehaviour
    {
        private static readonly ProfilerMarker SpatialMarker =
            new ProfilerMarker("WorldOfSpirits.Combat.SpatialUpdate");
        private static readonly ProfilerMarker StatusMarker =
            new ProfilerMarker("WorldOfSpirits.Combat.StatusUpdate");
        private static CombatSimulationManager instance;

        [Header("Spatial Targeting")]
        [SerializeField, Min(0.5f)] private float cellSize = 4f;
        [SerializeField, Min(32)] private int expectedEntityCapacity = 1024;
        [SerializeField, Min(1)] private int positionUpdatesPerFrame = 256;

        [Header("Status Effects")]
        [SerializeField, Min(1)] private int statusUpdatesPerFrame = 64;

        private readonly List<LivingEntity> entities = new List<LivingEntity>(1024);
        private readonly Dictionary<LivingEntity, int> indices =
            new Dictionary<LivingEntity, int>(1024);
        private CombatSpatialIndex2D spatialIndex;
        private int nextPositionIndex;
        private int nextStatusIndex;

        public static CombatSimulationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject root = new GameObject("Combat Simulation Manager");
                    instance = root.AddComponent<CombatSimulationManager>();
                }
                return instance;
            }
        }

        public static bool TryGetExisting(out CombatSimulationManager manager)
        {
            manager = instance;
            return manager != null;
        }

        public int EntityCount => entities.Count;
        public int ActiveCellCount => spatialIndex != null ? spatialIndex.ActiveCellCount : 0;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            spatialIndex = new CombatSpatialIndex2D(cellSize, expectedEntityCapacity);
        }

        private void Update()
        {
            int count = entities.Count;
            if (count == 0)
            {
                nextPositionIndex = nextStatusIndex = 0;
                return;
            }

            using (SpatialMarker.Auto())
            {
                int updates = Mathf.Min(positionUpdatesPerFrame, count);
                for (int i = 0; i < updates; i++)
                {
                    if (nextPositionIndex >= count) nextPositionIndex = 0;
                    LivingEntity entity = entities[nextPositionIndex++];
                    if (entity != null && entity.isActiveAndEnabled)
                    {
                        spatialIndex.UpdatePosition(entity, entity.Transform.position);
                    }
                }
            }

            using (StatusMarker.Auto())
            {
                int updates = Mathf.Min(statusUpdatesPerFrame, count);
                float now = Time.time;
                for (int i = 0; i < updates; i++)
                {
                    if (nextStatusIndex >= count) nextStatusIndex = 0;
                    LivingEntity entity = entities[nextStatusIndex++];
                    if (entity != null && entity.isActiveAndEnabled)
                    {
                        entity.TickStatusEffects(now);
                    }
                }
            }
        }

        public void Register(LivingEntity entity)
        {
            if (entity == null || indices.ContainsKey(entity)) return;
            indices.Add(entity, entities.Count);
            entities.Add(entity);
            spatialIndex.Add(entity, entity.Transform.position);
        }

        public void Unregister(LivingEntity entity)
        {
            if (entity == null || !indices.TryGetValue(entity, out int removedIndex)) return;
            int lastIndex = entities.Count - 1;
            LivingEntity last = entities[lastIndex];
            entities[removedIndex] = last;
            entities.RemoveAt(lastIndex);
            indices.Remove(entity);
            spatialIndex.Remove(entity);
            if (removedIndex < entities.Count) indices[last] = removedIndex;
            if (nextPositionIndex > entities.Count) nextPositionIndex = 0;
            if (nextStatusIndex > entities.Count) nextStatusIndex = 0;
        }

        public void Query(Vector2 center, float radius, List<LivingEntity> results)
        {
            results.Clear();
            spatialIndex.Query(center, radius, results);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            cellSize = Mathf.Max(0.5f, cellSize);
            expectedEntityCapacity = Mathf.Max(32, expectedEntityCapacity);
            positionUpdatesPerFrame = Mathf.Max(1, positionUpdatesPerFrame);
            statusUpdatesPerFrame = Mathf.Max(1, statusUpdatesPerFrame);
        }
#endif
    }
}

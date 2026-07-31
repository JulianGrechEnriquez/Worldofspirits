using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    /// <summary>Allocation-conscious spatial hash for combat targets.</summary>
    internal sealed class CombatSpatialIndex2D
    {
        private struct Entry
        {
            public long Cell;
            public Vector2 Position;
        }

        private readonly Dictionary<long, List<LivingEntity>> cells;
        private readonly Dictionary<LivingEntity, Entry> entries;
        private readonly Stack<List<LivingEntity>> recycledCells;
        private readonly float inverseCellSize;

        public CombatSpatialIndex2D(float cellSize, int capacity)
        {
            inverseCellSize = 1f / Mathf.Max(0.5f, cellSize);
            entries = new Dictionary<LivingEntity, Entry>(capacity);
            cells = new Dictionary<long, List<LivingEntity>>(Mathf.Max(32, capacity / 4));
            recycledCells = new Stack<List<LivingEntity>>(64);
        }

        public int ActiveCellCount => cells.Count;

        public void Add(LivingEntity entity, Vector2 position)
        {
            if (entity == null || entries.ContainsKey(entity)) return;
            long cell = PositionToCell(position);
            AddToCell(cell, entity);
            entries.Add(entity, new Entry { Cell = cell, Position = position });
        }

        public void Remove(LivingEntity entity)
        {
            if (entity == null || !entries.TryGetValue(entity, out Entry entry)) return;
            RemoveFromCell(entry.Cell, entity);
            entries.Remove(entity);
        }

        public void UpdatePosition(LivingEntity entity, Vector2 position)
        {
            if (entity == null || !entries.TryGetValue(entity, out Entry entry)) return;
            long newCell = PositionToCell(position);
            if (newCell != entry.Cell)
            {
                RemoveFromCell(entry.Cell, entity);
                AddToCell(newCell, entity);
                entry.Cell = newCell;
            }

            entry.Position = position;
            entries[entity] = entry;
        }

        public void Query(Vector2 center, float radius, List<LivingEntity> results)
        {
            float safeRadius = Mathf.Max(0f, radius);
            float radiusSquared = safeRadius * safeRadius;
            int minX = WorldToCell(center.x - safeRadius);
            int maxX = WorldToCell(center.x + safeRadius);
            int minY = WorldToCell(center.y - safeRadius);
            int maxY = WorldToCell(center.y + safeRadius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!cells.TryGetValue(CellKey(x, y), out List<LivingEntity> occupants)) continue;
                    for (int i = 0; i < occupants.Count; i++)
                    {
                        LivingEntity entity = occupants[i];
                        if (entity == null || !entity.isActiveAndEnabled || !entity.IsAlive) continue;
                        if (((Vector2)entity.Transform.position - center).sqrMagnitude <= radiusSquared)
                        {
                            results.Add(entity);
                        }
                    }
                }
            }
        }

        private void AddToCell(long key, LivingEntity entity)
        {
            if (!cells.TryGetValue(key, out List<LivingEntity> occupants))
            {
                occupants = recycledCells.Count > 0 ? recycledCells.Pop() : new List<LivingEntity>(8);
                cells.Add(key, occupants);
            }
            occupants.Add(entity);
        }

        private void RemoveFromCell(long key, LivingEntity entity)
        {
            if (!cells.TryGetValue(key, out List<LivingEntity> occupants)) return;
            int index = occupants.IndexOf(entity);
            if (index >= 0)
            {
                int last = occupants.Count - 1;
                occupants[index] = occupants[last];
                occupants.RemoveAt(last);
            }
            if (occupants.Count == 0)
            {
                cells.Remove(key);
                recycledCells.Push(occupants);
            }
        }

        private long PositionToCell(Vector2 position) =>
            CellKey(WorldToCell(position.x), WorldToCell(position.y));

        private int WorldToCell(float value) => Mathf.FloorToInt(value * inverseCellSize);
        private static long CellKey(int x, int y) => ((long)x << 32) ^ (uint)y;
    }
}

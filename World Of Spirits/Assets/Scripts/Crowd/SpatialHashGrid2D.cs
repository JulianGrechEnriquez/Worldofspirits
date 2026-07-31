using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Crowd
{
    /// <summary>
    /// Allocation-conscious spatial hash for a large collection of moving 2D
    /// objects. Callers identify objects using stable integer handles.
    ///
    /// This class deliberately has no MonoBehaviour or enemy dependencies.
    /// CrowdSimulationManager owns one instance and controls when it is updated.
    /// </summary>
    public sealed class SpatialHashGrid2D
    {
        private struct Entry
        {
            public Entry(long cellKey, Vector2 position)
            {
                CellKey = cellKey;
                Position = position;
            }

            public long CellKey;
            public Vector2 Position;
        }

        private readonly Dictionary<long, List<int>> cells;
        private readonly Dictionary<int, Entry> entries;
        private readonly Stack<List<int>> recycledCellLists;

        private float cellSize;
        private float inverseCellSize;

        public SpatialHashGrid2D(
            float cellSize,
            int initialEntryCapacity = 512,
            int initialCellCapacity = 128)
        {
            this.cellSize = Mathf.Max(0.1f, cellSize);
            inverseCellSize = 1f / this.cellSize;

            int entryCapacity = Mathf.Max(1, initialEntryCapacity);
            int cellCapacity = Mathf.Max(1, initialCellCapacity);
            entries = new Dictionary<int, Entry>(entryCapacity);
            cells = new Dictionary<long, List<int>>(cellCapacity);
            recycledCellLists = new Stack<List<int>>(cellCapacity);
        }

        public int Count => entries.Count;
        public int ActiveCellCount => cells.Count;
        public float CellSize => cellSize;

        /// <summary>
        /// Changes cell size and rebuilds the grid without changing handles.
        /// This is intended for configuration changes, not frequent gameplay use.
        /// </summary>
        public void SetCellSize(float newCellSize)
        {
            newCellSize = Mathf.Max(0.1f, newCellSize);
            if (Mathf.Approximately(cellSize, newCellSize))
            {
                return;
            }

            cellSize = newCellSize;
            inverseCellSize = 1f / cellSize;
            Rebuild();
        }

        /// <summary>
        /// Registers a new handle. Returns false if the handle already exists.
        /// </summary>
        public bool Add(int handle, Vector2 position)
        {
            if (entries.ContainsKey(handle))
            {
                return false;
            }

            long key = PositionToKey(position);
            AddToCell(key, handle);
            entries.Add(handle, new Entry(key, position));
            return true;
        }

        /// <summary>
        /// Removes a handle using swap-back removal inside its cell.
        /// </summary>
        public bool Remove(int handle)
        {
            if (!entries.TryGetValue(handle, out Entry entry))
            {
                return false;
            }

            RemoveFromCell(entry.CellKey, handle);
            entries.Remove(handle);
            return true;
        }

        /// <summary>
        /// Updates a handle's cached position and changes cell only when needed.
        /// </summary>
        public bool UpdatePosition(int handle, Vector2 position)
        {
            if (!entries.TryGetValue(handle, out Entry entry))
            {
                return false;
            }

            long newKey = PositionToKey(position);
            if (newKey != entry.CellKey)
            {
                RemoveFromCell(entry.CellKey, handle);
                AddToCell(newKey, handle);
                entry.CellKey = newKey;
            }

            entry.Position = position;
            entries[handle] = entry;
            return true;
        }

        public bool TryGetPosition(int handle, out Vector2 position)
        {
            if (entries.TryGetValue(handle, out Entry entry))
            {
                position = entry.Position;
                return true;
            }

            position = default;
            return false;
        }

        /// <summary>
        /// Appends handles inside the exact radius to a caller-owned list.
        /// The caller decides whether to clear the list before calling.
        /// No garbage is generated when the list has enough capacity.
        /// </summary>
        public int QueryRadius(Vector2 center, float radius, List<int> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            float safeRadius = Mathf.Max(0f, radius);
            float radiusSquared = safeRadius * safeRadius;
            int minimumX = WorldToCell(center.x - safeRadius);
            int maximumX = WorldToCell(center.x + safeRadius);
            int minimumY = WorldToCell(center.y - safeRadius);
            int maximumY = WorldToCell(center.y + safeRadius);
            int initialResultCount = results.Count;

            for (int y = minimumY; y <= maximumY; y++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    if (!cells.TryGetValue(CellToKey(x, y), out List<int> occupants))
                    {
                        continue;
                    }

                    for (int i = 0; i < occupants.Count; i++)
                    {
                        int handle = occupants[i];
                        Entry entry = entries[handle];
                        Vector2 offset = entry.Position - center;
                        if (offset.sqrMagnitude <= radiusSquared)
                        {
                            results.Add(handle);
                        }
                    }
                }
            }

            return results.Count - initialResultCount;
        }

        /// <summary>
        /// Copies occupied cell keys for debug rendering without exposing the
        /// internal dictionary or allocating an enumerator collection.
        /// </summary>
        public void CopyOccupiedCellKeys(List<long> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (long key in cells.Keys)
            {
                results.Add(key);
            }
        }

        public Vector2 GetCellCenter(long key)
        {
            DecodeCellKey(key, out int x, out int y);
            return new Vector2(
                (x + 0.5f) * cellSize,
                (y + 0.5f) * cellSize);
        }

        public void Clear()
        {
            foreach (List<int> occupants in cells.Values)
            {
                occupants.Clear();
                recycledCellLists.Push(occupants);
            }

            cells.Clear();
            entries.Clear();
        }

        public static void DecodeCellKey(long key, out int x, out int y)
        {
            x = (int)(key >> 32);
            y = (int)key;
        }

        private void Rebuild()
        {
            foreach (List<int> occupants in cells.Values)
            {
                occupants.Clear();
                recycledCellLists.Push(occupants);
            }

            cells.Clear();

            // Updating a value during Dictionary enumeration is unsafe, so
            // copy handles into a reusable temporary array only during this
            // rare configuration operation.
            int[] handles = new int[entries.Count];
            entries.Keys.CopyTo(handles, 0);
            for (int i = 0; i < handles.Length; i++)
            {
                int handle = handles[i];
                Entry entry = entries[handle];
                entry.CellKey = PositionToKey(entry.Position);
                entries[handle] = entry;
                AddToCell(entry.CellKey, handle);
            }
        }

        private void AddToCell(long key, int handle)
        {
            if (!cells.TryGetValue(key, out List<int> occupants))
            {
                occupants = recycledCellLists.Count > 0
                    ? recycledCellLists.Pop()
                    : new List<int>(8);
                cells.Add(key, occupants);
            }

            occupants.Add(handle);
        }

        private void RemoveFromCell(long key, int handle)
        {
            if (!cells.TryGetValue(key, out List<int> occupants))
            {
                return;
            }

            int index = occupants.IndexOf(handle);
            if (index >= 0)
            {
                int lastIndex = occupants.Count - 1;
                occupants[index] = occupants[lastIndex];
                occupants.RemoveAt(lastIndex);
            }

            if (occupants.Count == 0)
            {
                cells.Remove(key);
                recycledCellLists.Push(occupants);
            }
        }

        private long PositionToKey(Vector2 position)
        {
            return CellToKey(
                WorldToCell(position.x),
                WorldToCell(position.y));
        }

        private int WorldToCell(float coordinate)
        {
            return Mathf.FloorToInt(coordinate * inverseCellSize);
        }

        private static long CellToKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }
    }
}

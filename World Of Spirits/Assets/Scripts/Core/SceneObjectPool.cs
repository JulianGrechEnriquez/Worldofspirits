using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Core
{
    public enum PoolCategory
    {
        Projectiles,
        FloorEffects,
        Enemies,
        CombatUI,
        Effects
    }

    public interface IScenePoolable
    {
        void OnSpawnedFromPool(GameObject prefab);
        void OnReturnedToPool();
    }

    public sealed class PooledSceneObject : MonoBehaviour
    {
        internal GameObject Prefab;
        internal PoolCategory Category;
        internal Transform StorageParent;
        internal int SpawnVersion;
        internal string OriginalName;
    }

    public sealed class SceneObjectPool : MonoBehaviour
    {
        private sealed class Bucket
        {
            public readonly Stack<GameObject> Available = new Stack<GameObject>();
            public Transform Parent;
            public int CreatedCount;
        }

        private static SceneObjectPool instance;
        private readonly Dictionary<GameObject, Bucket> buckets = new Dictionary<GameObject, Bucket>();
        private readonly Dictionary<PoolCategory, Transform> categoryParents =
            new Dictionary<PoolCategory, Transform>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePoolHierarchy()
        {
            SceneObjectPool manager = GetOrCreate();
            manager.GetCategory(PoolCategory.Projectiles);
            manager.GetCategory(PoolCategory.FloorEffects);
            manager.GetCategory(PoolCategory.Enemies);
            manager.GetCategory(PoolCategory.CombatUI);
            manager.GetCategory(PoolCategory.Effects);
        }

        public static GameObject Spawn(
            GameObject prefab, Vector3 position, Quaternion rotation, PoolCategory category,
            Transform activeParent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            SceneObjectPool manager = GetOrCreate();
            Bucket bucket = manager.GetBucket(prefab, category);
            GameObject spawned = null;
            while (bucket.Available.Count > 0 && spawned == null)
            {
                spawned = bucket.Available.Pop();
            }

            if (spawned == null)
            {
                spawned = Instantiate(prefab, bucket.Parent);
                spawned.SetActive(false);
                bucket.CreatedCount++;
                string displayName = prefab.TryGetComponent(out PooledSceneObject prefabPoolData) &&
                    !string.IsNullOrWhiteSpace(prefabPoolData.OriginalName)
                    ? prefabPoolData.OriginalName
                    : prefab.name;
                spawned.name = $"{displayName} [Pooled {bucket.CreatedCount:000}]";
            }

            PooledSceneObject pooled = spawned.GetComponent<PooledSceneObject>();
            if (pooled == null)
            {
                pooled = spawned.AddComponent<PooledSceneObject>();
            }

            pooled.Prefab = prefab;
            pooled.Category = category;
            pooled.StorageParent = bucket.Parent;
            pooled.OriginalName = prefab.TryGetComponent(out PooledSceneObject sourcePoolData) &&
                !string.IsNullOrWhiteSpace(sourcePoolData.OriginalName)
                ? sourcePoolData.OriginalName
                : prefab.name;
            pooled.SpawnVersion++;
            spawned.transform.SetParent(activeParent != null ? activeParent : bucket.Parent, false);
            spawned.transform.SetPositionAndRotation(position, rotation);
            NotifySpawned(spawned, prefab);
            spawned.SetActive(true);
            return spawned;
        }

        public static T Spawn<T>(
            T prefab, Vector3 position, Quaternion rotation, PoolCategory category,
            Transform activeParent = null) where T : Component
        {
            GameObject spawned = Spawn(prefab.gameObject, position, rotation, category, activeParent);
            return spawned != null ? spawned.GetComponent<T>() : null;
        }

        public static void AdoptExisting(GameObject spawned, PoolCategory category)
        {
            if (spawned == null || spawned.TryGetComponent(out PooledSceneObject _))
            {
                return;
            }

            SceneObjectPool manager = GetOrCreate();
            Bucket bucket = manager.GetBucket(spawned, category);
            bucket.CreatedCount++;
            PooledSceneObject pooled = spawned.AddComponent<PooledSceneObject>();
            pooled.Prefab = spawned;
            pooled.Category = category;
            pooled.StorageParent = bucket.Parent;
            pooled.SpawnVersion = 1;
            pooled.OriginalName = spawned.name;
            spawned.name = $"{pooled.OriginalName} [Pooled {bucket.CreatedCount:000}]";
            spawned.transform.SetParent(bucket.Parent, true);
        }

        public static bool TryRelease(GameObject spawned)
        {
            if (spawned == null || !spawned.TryGetComponent(out PooledSceneObject pooled) ||
                pooled.Prefab == null)
            {
                return false;
            }
            if (!spawned.activeSelf)
            {
                return true;
            }

            SceneObjectPool manager = GetOrCreate();
            NotifyReturned(spawned);
            spawned.SetActive(false);
            spawned.transform.SetParent(pooled.StorageParent, false);
            if (!manager.buckets.TryGetValue(pooled.Prefab, out Bucket bucket))
            {
                bucket = manager.GetBucket(pooled.Prefab, pooled.Category);
            }

            bucket.Available.Push(spawned);
            return true;
        }

        public static void ReleaseOrDestroy(GameObject spawned)
        {
            if (!TryRelease(spawned))
            {
                Destroy(spawned);
            }
        }

        public static void ReleaseAfter(GameObject spawned, float delay)
        {
            if (spawned == null)
            {
                return;
            }

            PooledSceneObject pooled = spawned.GetComponent<PooledSceneObject>();
            if (pooled == null)
            {
                Destroy(spawned, delay);
                return;
            }

            GetOrCreate().StartCoroutine(
                ReleaseAfterRoutine(spawned, pooled, pooled.SpawnVersion, Mathf.Max(0f, delay)));
        }

        public static Transform GetCategoryParent(PoolCategory category)
        {
            return GetOrCreate().GetCategory(category);
        }

        private static SceneObjectPool GetOrCreate()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject root = GameObject.Find("=== Runtime Pools ===");
            if (root == null)
            {
                root = new GameObject("=== Runtime Pools ===");
            }

            instance = root.GetComponent<SceneObjectPool>();
            if (instance == null)
            {
                instance = root.AddComponent<SceneObjectPool>();
            }

            return instance;
        }

        private Bucket GetBucket(GameObject prefab, PoolCategory category)
        {
            if (buckets.TryGetValue(prefab, out Bucket bucket))
            {
                return bucket;
            }

            Transform categoryParent = GetCategory(category);
            GameObject bucketObject = new GameObject($"{prefab.name} Pool");
            bucketObject.transform.SetParent(categoryParent, false);
            bucket = new Bucket { Parent = bucketObject.transform };
            buckets.Add(prefab, bucket);
            return bucket;
        }

        private Transform GetCategory(PoolCategory category)
        {
            if (categoryParents.TryGetValue(category, out Transform parent) && parent != null)
            {
                return parent;
            }

            string categoryName = category switch
            {
                PoolCategory.Projectiles => "01 - Projectiles",
                PoolCategory.FloorEffects => "02 - Floor Effects",
                PoolCategory.Enemies => "03 - Enemies",
                PoolCategory.CombatUI => "04 - Combat UI",
                _ => "05 - Other Effects"
            };
            Transform existing = transform.Find(categoryName);
            if (existing == null)
            {
                GameObject categoryObject = new GameObject(categoryName);
                existing = categoryObject.transform;
                existing.SetParent(transform, false);
            }

            categoryParents[category] = existing;
            return existing;
        }

        private static IEnumerator ReleaseAfterRoutine(
            GameObject spawned, PooledSceneObject pooled, int version, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (spawned != null && spawned.activeSelf && pooled.SpawnVersion == version)
            {
                TryRelease(spawned);
            }
        }

        private static void NotifySpawned(GameObject spawned, GameObject prefab)
        {
            MonoBehaviour[] behaviours = spawned.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IScenePoolable poolable)
                {
                    poolable.OnSpawnedFromPool(prefab);
                }
            }
        }

        private static void NotifyReturned(GameObject spawned)
        {
            MonoBehaviour[] behaviours = spawned.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IScenePoolable poolable)
                {
                    poolable.OnReturnedToPool();
                }
            }
        }
    }
}

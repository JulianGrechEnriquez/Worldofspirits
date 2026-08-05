using UnityEngine;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerLevelSystem), typeof(UpgradeRuntimeStats))]
    public sealed class ExperienceOrbService : MonoBehaviour
    {
        [SerializeField] private ExperienceOrb orbPrefab;
        [SerializeField, Min(0)] private int prewarmCount = 64;
        [SerializeField, Min(0.1f)] private float attractionRadius = 4f;
        [SerializeField, Min(0.05f)] private float collectionRadius = 0.45f;

        private static ExperienceOrbService instance;
        private PlayerLevelSystem levelSystem;
        private UpgradeRuntimeStats runtimeStats;

        private void Awake()
        {
            instance = this;
            levelSystem = GetComponent<PlayerLevelSystem>();
            runtimeStats = GetComponent<UpgradeRuntimeStats>();
        }

        private void Start()
        {
            if (orbPrefab != null) SceneObjectPool.Preload(orbPrefab.gameObject, prewarmCount, PoolCategory.Pickups);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void Spawn(Vector3 position, float experience)
        {
            if (experience <= 0f) return;
            if (instance == null) instance = FindFirstObjectByType<ExperienceOrbService>();
            if (instance == null || instance.orbPrefab == null) return;

            ExperienceOrb orb = SceneObjectPool.Spawn(
                instance.orbPrefab, position, Quaternion.identity, PoolCategory.Pickups);
            if (orb != null)
            {
                orb.Configure(experience, instance.transform, instance.levelSystem, instance.runtimeStats,
                    instance.attractionRadius, instance.collectionRadius);
            }
        }
    }
}

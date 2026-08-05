using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    public class OrbitingProjectileAbility : SpiritAbility
    {
        [Tooltip("Assign a prefab from the Project window. The orbiting objects are spawned automatically at runtime.")]
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private IntegerLevelScaling orbCount = new IntegerLevelScaling();
        [SerializeField] private LevelScaling radius = new LevelScaling();
        [SerializeField] private LevelScaling rotationSpeed = new LevelScaling();
        [Tooltip("When enabled, the orbiting objects appear immediately when this ability component becomes active.")]
        [SerializeField] private bool spawnOnEnable = true;

        private readonly List<Transform> orbs = new List<Transform>();
        private float angle;
        private bool hasStarted;

        private void Start()
        {
            hasStarted = true;
            if (spawnOnEnable && orbPrefab != null)
            {
                EnsureOrbCount(GetOrbCount());
            }
        }

        private void OnEnable()
        {
            if (hasStarted && spawnOnEnable && orbPrefab != null)
            {
                EnsureOrbCount(GetOrbCount());
            }
        }

        private void OnDisable()
        {
            DestroyAllOrbs();
        }

        protected override void Cast(SpiritAbilityContext context)
        {
            EnsureOrbCount(GetOrbCount());
        }

        private void Update()
        {
            if (spawnOnEnable && orbPrefab != null)
            {
                // This also adds or removes objects immediately after an ability level changes.
                EnsureOrbCount(GetOrbCount());
            }

            if (orbs.Count == 0)
            {
                return;
            }

            angle = Mathf.Repeat(angle + rotationSpeed.Evaluate(CurrentLevel) * Time.deltaTime, 360f);
            float orbitRadius = radius.Evaluate(CurrentLevel) *
                (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            for (int i = 0; i < orbs.Count; i++)
            {
                if (orbs[i] == null) continue;
                float radians = (angle + 360f * i / orbs.Count) * Mathf.Deg2Rad;
                orbs[i].position = transform.position + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians)) * orbitRadius;
            }
        }

        protected override bool CanCast(SpiritAbilityContext context) => orbPrefab != null;

        private int GetOrbCount()
        {
            int count = Mathf.Max(1, orbCount.Evaluate(CurrentLevel));
            return UpgradeStats != null ? UpgradeStats.GetProjectileCount(count) : count;
        }

        private void EnsureOrbCount(int desiredCount)
        {
            orbs.RemoveAll(orb => orb == null);
            while (orbs.Count < desiredCount)
            {
                Transform orb = SceneObjectPool.Spawn(
                    orbPrefab, transform.position, Quaternion.identity,
                    PoolCategory.Effects, transform).transform;
                PersistentDamageZone zone = orb.GetComponent<PersistentDamageZone>();
                if (zone != null)
                {
                    zone.SetOwner(transform);
                    zone.ConfigureUpgradeModifiers(UpgradeStats);
                    zone.ConfigureDamageSource(CreateSpiritDamage(0f));
                }
                orbs.Add(orb);
            }
            while (orbs.Count > desiredCount)
            {
                Transform extra = orbs[orbs.Count - 1];
                orbs.RemoveAt(orbs.Count - 1);
                SceneObjectPool.ReleaseOrDestroy(extra.gameObject);
            }
        }

        private void DestroyAllOrbs()
        {
            foreach (Transform orb in orbs)
            {
                if (orb != null)
                {
                    SceneObjectPool.ReleaseOrDestroy(orb.gameObject);
                }
            }

            orbs.Clear();
        }
    }
}

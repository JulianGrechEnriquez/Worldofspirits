using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;

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
                EnsureOrbCount(Mathf.Max(1, orbCount.Evaluate(CurrentLevel)));
            }
        }

        private void OnEnable()
        {
            if (hasStarted && spawnOnEnable && orbPrefab != null)
            {
                EnsureOrbCount(Mathf.Max(1, orbCount.Evaluate(CurrentLevel)));
            }
        }

        private void OnDisable()
        {
            DestroyAllOrbs();
        }

        protected override void Cast(SpiritAbilityContext context)
        {
            EnsureOrbCount(Mathf.Max(1, orbCount.Evaluate(CurrentLevel)));
        }

        private void Update()
        {
            if (spawnOnEnable && orbPrefab != null)
            {
                // This also adds or removes objects immediately after an ability level changes.
                EnsureOrbCount(Mathf.Max(1, orbCount.Evaluate(CurrentLevel)));
            }

            if (orbs.Count == 0)
            {
                return;
            }

            angle = Mathf.Repeat(angle + rotationSpeed.Evaluate(CurrentLevel) * Time.deltaTime, 360f);
            float orbitRadius = radius.Evaluate(CurrentLevel);
            for (int i = 0; i < orbs.Count; i++)
            {
                if (orbs[i] == null) continue;
                float radians = (angle + 360f * i / orbs.Count) * Mathf.Deg2Rad;
                orbs[i].position = transform.position + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians)) * orbitRadius;
            }
        }

        protected override bool CanCast(SpiritAbilityContext context) => orbPrefab != null;

        private void EnsureOrbCount(int desiredCount)
        {
            orbs.RemoveAll(orb => orb == null);
            while (orbs.Count < desiredCount)
            {
                orbs.Add(Instantiate(orbPrefab, transform.position, Quaternion.identity, transform).transform);
            }
            while (orbs.Count > desiredCount)
            {
                Transform extra = orbs[orbs.Count - 1];
                orbs.RemoveAt(orbs.Count - 1);
                Destroy(extra.gameObject);
            }
        }

        private void DestroyAllOrbs()
        {
            foreach (Transform orb in orbs)
            {
                if (orb != null)
                {
                    Destroy(orb.gameObject);
                }
            }

            orbs.Clear();
        }
    }
}

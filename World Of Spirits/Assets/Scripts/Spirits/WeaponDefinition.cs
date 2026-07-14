using System;
using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public enum WeaponExecutionType { Projectile, OrbitingMelee }

    [Serializable]
    public class WeaponLevelData
    {
        [Min(1)] public int level = 1;
        [TextArea(1, 3)] public string upgradeDescription;
        public ProjectileBase projectilePrefab;
        public GameObject weaponPrefab;
        [Min(0f)] public float damage = 10f;
        [Min(0.05f)] public float attackCooldown = 0.75f;
        [Min(0.1f)] public float targetingRange = 12f;
        [Min(0.1f)] public float projectileSpeed = 12f;
        public bool homeOnEnemies;
        [Min(0f)] public float homingStrength = 6f;
        [Min(0.1f)] public float homingRange = 10f;
        [Min(0.1f)] public float orbitRadius = 1.4f;
        public float orbitSpeed = 180f;
    }

    [CreateAssetMenu(fileName = "Weapon Definition", menuName = "World of Spirits/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string weaponName;
        [TextArea(2, 4)] [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private WeaponExecutionType executionType;
        [SerializeField] private List<WeaponLevelData> levels = new List<WeaponLevelData>();

        public string WeaponName => weaponName;
        public string Description => description;
        public Sprite Icon => icon;
        public WeaponExecutionType ExecutionType => executionType;
        public int MaxLevel => Mathf.Max(1, levels.Count);
        public WeaponLevelData GetLevel(int requestedLevel) => levels.Count == 0
            ? null : levels[Mathf.Clamp(requestedLevel - 1, 0, levels.Count - 1)];
    }
}

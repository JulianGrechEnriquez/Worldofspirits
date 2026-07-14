using System;
using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public enum AbilityExecutionType
    {
        Projectile,
        Area,
        SpawnEffect,
        Orbiting,
        Chain,
        Self
    }

    public enum AbilityTargetingMode
    {
        ClosestEnemy,
        RandomEnemy,
        StrongestEnemy,
        LowestHealthEnemy,
        PlayerFacing,
        AroundPlayer,
        RandomPositionNearPlayer,
        Self
    }

    public enum AbilityEffectType
    {
        Damage,
        ApplyStatus,
        Pull,
        Knockback,
        Heal,
        Shield,
        GrantRevive,
        SpawnEffect
    }

    [Serializable]
    public class AbilityEffectData
    {
        public AbilityEffectType effectType;
        [Min(0f)] public float value;
        [Min(0f)] public float duration;
        public CombatStatus status;
        public GameObject effectPrefab;
    }

    [Serializable]
    public class AbilityProjectileData
    {
        public ProjectileBase projectilePrefab;
        [Min(1)] public int count = 1;
        [Range(0f, 360f)] public float spreadAngle;
        public ProjectileSpreadMode spreadMode;
        [Min(0.1f)] public float speed = 10f;
        [Min(0f)] public float damage = 10f;
        public bool homeOnEnemies;
        [Min(0f)] public float homingStrength = 5f;
        [Min(0.1f)] public float homingRange = 8f;
        [Min(0)] public int pierceCount;
        [Min(0f)] public float explosionRadius;
        [Min(0f)] public float growthPerSecond;
        public bool appliesStatus;
        public CombatStatus status;
        [Min(0f)] public float statusDuration = 2f;
        [Min(0f)] public float statusStrength = 2f;
    }

    [Serializable]
    public class AbilityLevelData
    {
        [Min(1)] public int level = 1;
        [TextArea(1, 3)] public string upgradeDescription;
        [Min(0.05f)] public float cooldown = 1f;
        [Min(0.1f)] public float targetingRange = 15f;
        public AbilityProjectileData projectile = new AbilityProjectileData();
        public GameObject spawnedEffectPrefab;
        [Min(1)] public int spawnCount = 1;
        [Min(0f)] public float areaRadius = 3f;
        [Min(0f)] public float orbitRadius = 1.5f;
        public float orbitSpeed = 120f;
        [Min(1)] public int chainCount = 1;
        [Min(0.1f)] public float chainRange = 5f;
        public List<AbilityEffectData> effects = new List<AbilityEffectData>();
    }

    [CreateAssetMenu(fileName = "Ability Definition", menuName = "World of Spirits/Ability Definition")]
    public class AbilityDefinition : ScriptableObject
    {
        [SerializeField] private string abilityName;
        [TextArea(2, 4)] [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private AbilityExecutionType executionType;
        [SerializeField] private AbilityTargetingMode targetingMode;
        [SerializeField] private List<AbilityLevelData> levels = new List<AbilityLevelData>();

        public string AbilityName => abilityName;
        public string Description => description;
        public Sprite Icon => icon;
        public AbilityExecutionType ExecutionType => executionType;
        public AbilityTargetingMode TargetingMode => targetingMode;
        public IReadOnlyList<AbilityLevelData> Levels => levels;
        public int MaxLevel => Mathf.Max(1, levels.Count);

        public AbilityLevelData GetLevel(int requestedLevel)
        {
            if (levels.Count == 0) return null;
            return levels[Mathf.Clamp(requestedLevel - 1, 0, levels.Count - 1)];
        }

        public void Configure(string name, string abilityDescription, AbilityExecutionType execution,
            AbilityTargetingMode targeting, params AbilityLevelData[] levelData)
        {
            abilityName = name;
            description = abilityDescription;
            executionType = execution;
            targetingMode = targeting;
            levels = new List<AbilityLevelData>(levelData);
        }
    }
}

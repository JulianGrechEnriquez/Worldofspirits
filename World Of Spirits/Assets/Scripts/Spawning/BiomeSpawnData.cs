using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Spawning
{
    /// <summary>
    /// One enemy entry in a biome roster, including how many instances should
    /// be created before gameplay begins.
    /// </summary>
    [Serializable]
    public sealed class BiomeEnemyEntry
    {
        [Tooltip("Spawn rules and prefab for this enemy.")]
        [SerializeField] private EnemySpawnData enemy;

        [Tooltip("Number of pooled instances prepared when this biome is loaded.")]
        [SerializeField, Min(0)] private int preloadCount = 8;

        public EnemySpawnData Enemy => enemy;
        public int PreloadCount => preloadCount;
    }

    /// <summary>
    /// Data-driven enemy roster for one stage or biome.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Biome Spawn Data",
        menuName = "World of Spirits/Spawning/Biome Spawn Data")]
    public sealed class BiomeSpawnData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Human-readable biome name shown in tools and UI.")]
        [SerializeField] private string displayName = "Burning Plains";

        [Tooltip("Stable identifier shared by this biome and its EnemySpawnData assets.")]
        [SerializeField] private string biomeId = "burning-plains";

        [Header("Enemy Roster")]
        [Tooltip("Enemies available in this biome and their initial pool sizes.")]
        [SerializeField] private List<BiomeEnemyEntry> enemies =
            new List<BiomeEnemyEntry>();

        public string DisplayName => displayName;
        public string BiomeId => biomeId;
        public IReadOnlyList<BiomeEnemyEntry> Enemies => enemies;

#if UNITY_EDITOR
        private void OnValidate()
        {
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName.Trim();
            biomeId = string.IsNullOrWhiteSpace(biomeId)
                ? "burning-plains"
                : biomeId.Trim().ToLowerInvariant();

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemySpawnData spawnData = enemies[i]?.Enemy;
                if (spawnData != null &&
                    !string.Equals(
                        spawnData.BiomeId,
                        biomeId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning(
                        $"{spawnData.name} uses biome ID '{spawnData.BiomeId}', " +
                        $"but {name} uses '{biomeId}'. It will not be eligible for this biome.",
                        this);
                }
            }
        }
#endif
    }
}

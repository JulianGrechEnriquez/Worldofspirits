using UnityEngine;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Spawning
{
    /// <summary>
    /// Immutable authoring data used by the spawning systems to decide when,
    /// where, and how often an enemy may be spawned.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Enemy Spawn Data",
        menuName = "World of Spirits/Spawning/Enemy Spawn Data")]
    public sealed class EnemySpawnData : ScriptableObject
    {
        [Header("Enemy")]
        [Tooltip("Prefab containing an EnemyBase component. The pool reuses instances of this prefab.")]
        [SerializeField] private EnemyBase enemyPrefab;

        [Header("Budget and Selection")]
        [Tooltip("Amount removed from the director's spawn budget whenever this enemy is spawned.")]
        [SerializeField, Min(1)] private int spawnCost = 1;

        [Tooltip("Relative probability of selecting this enemy among all currently valid entries.")]
        [SerializeField, Min(0f)] private float spawnWeight = 1f;

        [Header("Availability")]
        [Tooltip("Stable biome identifier, such as burning-plains. Matching is case-insensitive.")]
        [SerializeField] private string biomeId = "burning-plains";

        [Header("Classification")]
        [Tooltip("Whether this entry represents an elite enemy.")]
        [SerializeField] private bool elite;

        [Tooltip("Whether this entry represents a boss. Bosses are excluded from normal budget spawning.")]
        [SerializeField] private bool boss;

        public EnemyBase EnemyPrefab => enemyPrefab;
        public int SpawnCost => spawnCost;
        public float SpawnWeight => spawnWeight;
        public string BiomeId => biomeId;
        public bool IsElite => elite;
        public bool IsBoss => boss;

        /// <summary>
        /// Returns true when this entry is eligible for the active biome.
        /// Budget, elite chance, and boss-event rules are intentionally handled by the
        /// director because they depend on the current spawning context.
        /// </summary>
        public bool IsAvailable(string activeBiomeId)
        {
            if (enemyPrefab == null || spawnWeight <= 0f)
            {
                return false;
            }

            return string.Equals(
                biomeId,
                activeBiomeId,
                System.StringComparison.OrdinalIgnoreCase);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            spawnCost = Mathf.Max(1, spawnCost);
            spawnWeight = Mathf.Max(0f, spawnWeight);
            biomeId = string.IsNullOrWhiteSpace(biomeId)
                ? "burning-plains"
                : biomeId.Trim().ToLowerInvariant();
        }
#endif
    }
}

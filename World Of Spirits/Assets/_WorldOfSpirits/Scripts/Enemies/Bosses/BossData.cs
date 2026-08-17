using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    [CreateAssetMenu(fileName = "Boss Data", menuName = "World of Spirits/Bosses/Boss Data")]
    public sealed class BossData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string bossName = "FIRE PHOENIX";
        [SerializeField] private Sprite bossIcon;

        [Header("Core Stats")]
        [SerializeField, Min(1f)] private float maximumHealth = 1000f;
        [SerializeField, Min(0f)] private float movementSpeed = 3f;
        [SerializeField, Min(0f)] private float contactDamage = 20f;
        [SerializeField, Min(0f)] private float attackCooldown = 1f;

        [Header("Prefab")]
        [SerializeField] private BossEnemyBase bossPrefab;

        public string BossName => string.IsNullOrWhiteSpace(bossName) ? name : bossName;
        public Sprite BossIcon => bossIcon;
        public float MaximumHealth => maximumHealth;
        public float MovementSpeed => movementSpeed;
        public float ContactDamage => contactDamage;
        public float AttackCooldown => attackCooldown;
        public BossEnemyBase BossPrefab => bossPrefab;
    }
}

using UnityEngine;
using UnityEngine.Serialization;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public enum ProjectilePattern
    {
        AimedFan,
        Radial,
        ForwardAndBackward,
        FourDirections
    }

    public enum ProjectileSpreadMode
    {
        EvenlySpaced,
        Random
    }

    public class ProjectilePatternAbility : SpiritAbility
    {
        [SerializeField] private ProjectileBase projectilePrefab;
        [SerializeField] private ProjectilePattern pattern = ProjectilePattern.AimedFan;
        [SerializeField] private IntegerLevelScaling projectileCount = new IntegerLevelScaling();
        [SerializeField] private LevelScaling damage = new LevelScaling();
        [SerializeField] private LevelScaling speed = new LevelScaling();
        [Header("Spread Control")]
        [Tooltip("Total width of the volley in degrees. 0 fires straight; 45 is a narrow fan; 180 is a half-circle.")]
        [FormerlySerializedAs("fanAngle")]
        [SerializeField, Range(0f, 360f)] private float spreadAngle = 45f;
        [Tooltip("Evenly Spaced creates a predictable fan. Random gives every projectile a random angle inside the spread.")]
        [SerializeField] private ProjectileSpreadMode spreadMode = ProjectileSpreadMode.EvenlySpaced;

        [Header("Homing Control")]
        [Tooltip("Makes every projectile in this ability track the closest enemy after it is fired.")]
        [SerializeField] private bool homeOnEnemies;
        [Tooltip("How quickly projectiles turn toward enemies. Try 3 for gentle tracking or 10 for strong tracking.")]
        [SerializeField, Min(0f)] private float homingStrength = 5f;
        [Tooltip("How far a projectile can search for an enemy.")]
        [SerializeField, Min(0.1f)] private float homingRange = 8f;

        [Header("Targeting")]
        [SerializeField, Min(0.1f)] private float targetingRange = 15f;

        protected override bool CanCast(SpiritAbilityContext context) => projectilePrefab != null;

        protected override void Cast(SpiritAbilityContext context)
        {
            int count = Mathf.Max(1, projectileCount.Evaluate(CurrentLevel));
            Vector2 forward = FindDirection(context);
            switch (pattern)
            {
                case ProjectilePattern.Radial:
                    SpawnArc(Vector2.right, count, 360f, true);
                    break;
                case ProjectilePattern.ForwardAndBackward:
                    Spawn(forward);
                    Spawn(-forward);
                    break;
                case ProjectilePattern.FourDirections:
                    Spawn(Vector2.up); Spawn(Vector2.down); Spawn(Vector2.left); Spawn(Vector2.right);
                    break;
                default:
                    SpawnArc(forward, count, spreadAngle, false);
                    break;
            }
        }

        private Vector2 FindDirection(SpiritAbilityContext context)
        {
            IDamageable target = CombatTargeting.FindClosest(transform.position, targetingRange, Faction.Player);
            if (target != null)
            {
                return (target.Transform.position - transform.position).normalized;
            }

            return context.Player != null ? (context.Player.right).normalized : Vector2.right;
        }

        private void SpawnArc(Vector2 centerDirection, int count, float arc, bool fullCircle)
        {
            float centerAngle = Mathf.Atan2(centerDirection.y, centerDirection.x) * Mathf.Rad2Deg;
            float step = fullCircle ? arc / count : count > 1 ? arc / (count - 1) : 0f;
            float start = fullCircle ? centerAngle : centerAngle - arc * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float angle = !fullCircle && spreadMode == ProjectileSpreadMode.Random
                    ? centerAngle + Random.Range(-arc * 0.5f, arc * 0.5f)
                    : start + step * i;
                float radians = angle * Mathf.Deg2Rad;
                Spawn(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)));
            }
        }

        private void Spawn(Vector2 direction)
        {
            ProjectileBase projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.ConfigureHoming(homeOnEnemies, homingStrength, homingRange);
            projectile.Launch(direction, speed.Evaluate(CurrentLevel), damage.Evaluate(CurrentLevel), Faction.Player);
        }
    }
}

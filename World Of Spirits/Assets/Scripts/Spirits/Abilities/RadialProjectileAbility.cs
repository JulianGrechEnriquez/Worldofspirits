using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public class RadialProjectileAbility : SpiritAbility
    {
        [SerializeField] private ProjectileBase projectilePrefab;
        [SerializeField, Min(1)] private int projectileCount = 4;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 8f;

        protected override bool CanCast(SpiritAbilityContext context)
        {
            return projectilePrefab != null;
        }

        protected override void Cast(SpiritAbilityContext context)
        {
            float angleStep = 360f / projectileCount;
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                ProjectileBase projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                projectile.Launch(direction, projectileSpeed, damage, Faction.Player);
            }
        }
    }
}

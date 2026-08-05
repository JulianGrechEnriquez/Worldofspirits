using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Progression.Upgrades;

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
            int count = UpgradeStats != null ? UpgradeStats.GetProjectileCount(projectileCount) : projectileCount;
            float angleStep = 360f / count;
            float speed = projectileSpeed *
                (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.ProjectileSpeed) : 1f);
            DamageContext damageContext = CreateSpiritDamage(damage);
            for (int i = 0; i < count; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                ProjectileBase projectile = ProjectilePool.Spawn(
                    projectilePrefab, transform.position, Quaternion.identity);
                projectile.ConfigureUpgradeModifiers(UpgradeStats);
                projectile.ConfigureDamageContext(damageContext);
                projectile.Launch(direction, speed, damageContext.BaseDamage, Faction.Player);
            }
        }
    }
}

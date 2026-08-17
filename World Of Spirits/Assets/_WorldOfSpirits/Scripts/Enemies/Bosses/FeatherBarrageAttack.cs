using System.Collections;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Enemies
{
    public sealed class FeatherBarrageAttack : BossAttackBase
    {
        [SerializeField] private ProjectileBase featherProjectile;
        [SerializeField, Min(1)] private int projectileCount = 7;
        [SerializeField, Range(0f, 360f)] private float spreadDegrees = 70f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 11f;
        [SerializeField, Min(0f)] private float damage = 12f;
        [SerializeField, Min(0f)] private float warningDelay = 0.6f;

        public override IEnumerator Execute(BossContext context)
        {
            context.Movement.TakeAttackControl();
            yield return Wait(warningDelay);
            Vector2 center = (context.Target.position - transform.position).normalized;
            for (int i = 0; i < projectileCount; i++)
            {
                float t = projectileCount == 1 ? 0.5f : i / (float)(projectileCount - 1);
                Vector2 direction = Quaternion.Euler(0f, 0f,
                    Mathf.Lerp(-spreadDegrees * 0.5f, spreadDegrees * 0.5f, t)) * center;
                if (featherProjectile == null) continue;
                ProjectileBase projectile = ProjectilePool.Spawn(featherProjectile, transform.position, Quaternion.identity);
                projectile.ConfigureDamageContext(new DamageContext(damage, transform, DamageElement.Fire));
                projectile.Launch(direction, projectileSpeed, damage, Faction.Enemy);
            }
            context.Movement.ReleaseAttackControl();
        }
    }
}

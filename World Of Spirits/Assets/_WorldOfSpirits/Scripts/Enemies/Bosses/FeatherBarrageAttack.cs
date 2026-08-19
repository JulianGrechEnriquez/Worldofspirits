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
        [SerializeField, Min(0f)] private float delayBetweenWaves = 0.18f;
        [SerializeField, Range(0f, 90f)] private float waveRotation = 12f;

        public override IEnumerator Execute(BossContext context)
        {
            context.Movement.TakeAttackControl();
            yield return Wait(warningDelay * Mathf.Max(0.65f, 1f - context.Boss.CurrentPhase * 0.12f));
            int waves = 1 + context.Boss.CurrentPhase;
            for (int wave = 0; wave < waves; wave++)
            {
                Vector2 center = (context.Target.position - transform.position).normalized;
                float rotation = (wave - (waves - 1) * 0.5f) * waveRotation;
                for (int i = 0; i < projectileCount + context.Boss.CurrentPhase; i++)
                {
                    int count = projectileCount + context.Boss.CurrentPhase;
                    float t = count == 1 ? 0.5f : i / (float)(count - 1);
                    Vector2 direction = Quaternion.Euler(0f, 0f,
                        Mathf.Lerp(-spreadDegrees * 0.5f, spreadDegrees * 0.5f, t) + rotation) * center;
                    if (featherProjectile == null) continue;
                    ProjectileBase projectile = ProjectilePool.Spawn(featherProjectile, transform.position, Quaternion.identity);
                    projectile.ConfigureDamageContext(new DamageContext(damage, transform, DamageElement.Fire));
                    projectile.Launch(direction, projectileSpeed + context.Boss.CurrentPhase, damage, Faction.Enemy);
                }
                if (wave + 1 < waves) yield return Wait(delayBetweenWaves);
            }
            context.Movement.ReleaseAttackControl();
        }
    }
}

using UnityEngine;
using WorldOfSpirits.Crowd;

namespace WorldOfSpirits.Combat
{
    public sealed class SpiritBolt : ProjectileBase
    {
        [SerializeField, Min(0f)] private float knockback = 2f;

        protected override void OnHit(IDamageable target)
        {
            Vector2 direction = Body.linearVelocity.normalized;
            target.TakeDamage(DamageSourceContext);
            if (target.IsAlive && target.Transform.TryGetComponent(out EnemyCrowdAgent crowd))
                crowd.ApplyExternalAcceleration(direction * knockback, 1f);
            else if (target.IsAlive && target.Transform.TryGetComponent(out Rigidbody2D body))
                body.AddForce(direction * knockback, ForceMode2D.Impulse);
            Despawn();
        }
    }
}

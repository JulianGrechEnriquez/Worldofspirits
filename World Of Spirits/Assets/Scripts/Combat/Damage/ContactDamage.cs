using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public class ContactDamage : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.05f)] private float hitCooldown = 0.75f;

        private LivingEntity owner;
        private float nextHitTime;

        private void Awake()
        {
            owner = GetComponentInParent<LivingEntity>();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDamage(collision.collider);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void TryDamage(Collider2D other)
        {
            if (owner == null || !owner.IsAlive || Time.time < nextHitTime)
            {
                return;
            }

            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || target.Faction == owner.Faction)
            {
                return;
            }

            // Child weapon and effect colliders can live below a character in
            // the hierarchy. They must not extend that character's contact
            // hurtbox (for example, an Ice Gauntlet punching away from the
            // player). Only colliders on the damageable's body count.
            if (other.transform != target.Transform)
            {
                return;
            }

            target.TakeDamage(new DamageContext(
                damage,
                owner.transform,
                DamageElement.Physical,
                false));
            nextHitTime = Time.time + hitCooldown;
        }
    }
}

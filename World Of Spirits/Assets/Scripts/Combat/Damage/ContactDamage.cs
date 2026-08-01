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

            target.TakeDamage(new DamageContext(
                damage,
                owner.transform,
                DamageElement.Physical,
                false));
            nextHitTime = Time.time + hitCooldown;
        }
    }
}

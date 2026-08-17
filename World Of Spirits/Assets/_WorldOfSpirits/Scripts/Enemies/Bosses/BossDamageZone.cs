using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Enemies
{
    [DisallowMultipleComponent]
    public sealed class BossDamageZone : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damage = 15f;
        [SerializeField, Min(0.05f)] private float hitInterval = 0.5f;
        [SerializeField, Min(0.05f)] private float lifetime = 3f;
        [SerializeField] private Vector2 movementVelocity;
        [SerializeField] private DamageElement element = DamageElement.Fire;

        private Transform source;
        private float expiresAt;
        private float nextHitAt;

        public void Activate(Transform damageSource, float configuredDamage, float configuredLifetime, Vector2 velocity)
        {
            source = damageSource;
            damage = Mathf.Max(0f, configuredDamage);
            lifetime = Mathf.Max(0.05f, configuredLifetime);
            movementVelocity = velocity;
            expiresAt = Time.time + lifetime;
            nextHitAt = 0f;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.position += (Vector3)(movementVelocity * Time.deltaTime);
            if (Time.time >= expiresAt) gameObject.SetActive(false);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextHitAt) return;
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || target.Faction == Faction.Enemy) return;
            target.TakeDamage(new DamageContext(damage, source, element));
            nextHitAt = Time.time + hitInterval;
        }
    }
}

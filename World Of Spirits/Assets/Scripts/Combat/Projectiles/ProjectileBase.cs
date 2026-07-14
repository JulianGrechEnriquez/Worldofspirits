using UnityEngine;

namespace WorldOfSpirits.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 5f;
        [Tooltip("Rotation correction for sprites that do not face right by default.")]
        [SerializeField] private float rotationOffset;

        [Header("Debug")]
        [SerializeField] private bool logProjectileEvents;
        [SerializeField] private bool drawVelocity = true;

        protected Rigidbody2D Body { get; private set; }
        protected float Damage { get; private set; }
        protected Faction OwnerFaction { get; private set; }

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            GetComponent<Collider2D>().isTrigger = true;
        }

        public virtual void Launch(Vector2 direction, float speed, float damage, Faction ownerFaction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                Debug.LogWarning($"[{name}] Cannot launch with a zero direction.", this);
                return;
            }

            Damage = damage;
            OwnerFaction = ownerFaction;
            Vector2 normalizedDirection = direction.normalized;
            float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
            Body.linearVelocity = normalizedDirection * speed;

            if (logProjectileEvents)
            {
                Debug.Log($"[{name}] Launched by {ownerFaction}: speed={speed:0.##}, damage={damage:0.##}", this);
            }

            Destroy(gameObject, lifetime);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || target.Faction == OwnerFaction)
            {
                return;
            }

            if (logProjectileEvents)
            {
                Debug.Log($"[{name}] Hit {target.Transform.name} for {Damage:0.##} damage.", this);
            }

            OnHit(target);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!drawVelocity || Body == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)Body.linearVelocity * 0.25f);
        }

        protected abstract void OnHit(IDamageable target);
    }
}

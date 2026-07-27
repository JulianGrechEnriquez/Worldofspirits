using UnityEngine;

namespace WorldOfSpirits.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 5f;
        [Tooltip("Rotation correction for sprites that do not face right by default.")]
        [SerializeField] private float rotationOffset;

        [Header("Homing")]
        [Tooltip("When enabled, the projectile turns toward the nearest enemy while travelling.")]
        [SerializeField] private bool homeOnEnemies;
        [Tooltip("How quickly the projectile turns. Try 3 for gentle tracking or 10 for strong tracking.")]
        [SerializeField, Min(0f)] private float homingStrength = 5f;
        [Tooltip("Maximum distance at which this projectile can acquire an enemy.")]
        [SerializeField, Min(0.1f)] private float homingRange = 8f;
        [SerializeField, Min(0.02f)] private float homingTargetRefreshInterval = 0.15f;

        [Header("Debug")]
        [SerializeField] private bool logProjectileEvents;
        [SerializeField] private bool drawVelocity = true;

        protected Rigidbody2D Body { get; private set; }
        protected float Damage { get; private set; }
        protected Faction OwnerFaction { get; private set; }
        private float launchSpeed;
        private float despawnTime;
        private float nextHomingTargetRefresh;
        private IDamageable homingTarget;
        private ProjectileBase poolPrefab;

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
            launchSpeed = speed;
            despawnTime = Time.time + lifetime;
            nextHomingTargetRefresh = Time.time;
            homingTarget = null;
            Vector2 normalizedDirection = direction.normalized;

            FaceDirection(normalizedDirection);
            Body.linearVelocity = normalizedDirection * speed;

            if (logProjectileEvents)
            {
                Debug.Log($"[{name}] Launched by {ownerFaction}: speed={speed:0.##}, damage={damage:0.##}", this);
            }

        }

        internal void AssignPool(ProjectileBase prefab)
        {
            poolPrefab = prefab;
            homeOnEnemies = prefab.homeOnEnemies;
            homingStrength = prefab.homingStrength;
            homingRange = prefab.homingRange;
            homingTargetRefreshInterval = prefab.homingTargetRefreshInterval;
            ResetPooledConfiguration(prefab);
        }

        protected virtual void ResetPooledConfiguration(ProjectileBase prefab) { }

        protected void Despawn()
        {
            Body.linearVelocity = Vector2.zero;
            homingTarget = null;
            if (poolPrefab != null)
            {
                ProjectilePool.Release(this, poolPrefab);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ConfigureHoming(bool enabled, float strength, float range)
        {
            homeOnEnemies = enabled;
            homingStrength = Mathf.Max(0f, strength);
            homingRange = Mathf.Max(0.1f, range);
        }

        protected void Redirect(Vector2 direction)
        {
            if (Body == null || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Vector2 normalizedDirection = direction.normalized;
            Body.linearVelocity = normalizedDirection * launchSpeed;
            FaceDirection(normalizedDirection);
        }

        protected virtual void Update()
        {
            if (Time.time >= despawnTime)
            {
                Despawn();
                return;
            }

            if (!homeOnEnemies || homingStrength <= 0f || Body == null)
            {
                return;
            }

            if (Time.time >= nextHomingTargetRefresh)
            {
                homingTarget = CombatTargeting.FindClosest(transform.position, homingRange, OwnerFaction);
                nextHomingTargetRefresh = Time.time + homingTargetRefreshInterval;
            }

            if (homingTarget == null || !homingTarget.IsAlive ||
                (homingTarget.Transform.position - transform.position).sqrMagnitude > homingRange * homingRange)
            {
                return;
            }

            Vector2 desiredVelocity = (homingTarget.Transform.position - transform.position).normalized * launchSpeed;
            Body.linearVelocity = Vector2.Lerp(Body.linearVelocity, desiredVelocity,
                Mathf.Clamp01(homingStrength * Time.deltaTime));
            FaceDirection(Body.linearVelocity);
        }

        private void FaceDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
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

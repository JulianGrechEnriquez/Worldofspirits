using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

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
        private readonly HashSet<int> homingIgnoredTargets = new HashSet<int>();
        private ProjectileBase poolPrefab;
        private Vector3 authoredScale;
        private float lifetimeMultiplier = 1f;
        private float projectileScaleMultiplier = 1f;
        private float castLifetimeMultiplier = 1f;
        private float castScaleMultiplier = 1f;
        private DamageContext damageContext;
        private bool hasConfiguredDamageContext;
        protected UpgradeRuntimeStats UpgradeStats { get; private set; }
        protected int UpgradePierceCount { get; private set; }
        protected int UpgradeRicochetCount { get; private set; }
        protected float UpgradeDurationMultiplier { get; private set; } = 1f;
        protected float UpgradeAreaMultiplier { get; private set; } = 1f;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            GetComponent<Collider2D>().isTrigger = true;
            authoredScale = transform.localScale;
        }

        public virtual void Launch(Vector2 direction, float speed, float damage, Faction ownerFaction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                Debug.LogWarning($"[{name}] Cannot launch with a zero direction.", this);
                return;
            }

            Damage = damage;
            if (!hasConfiguredDamageContext) damageContext = new DamageContext(damage);
            OwnerFaction = ownerFaction;
            launchSpeed = speed;
            despawnTime = Time.time + lifetime * lifetimeMultiplier * castLifetimeMultiplier;
            nextHomingTargetRefresh = Time.time;
            homingTarget = null;
            homingIgnoredTargets.Clear();
            transform.localScale = authoredScale * projectileScaleMultiplier * castScaleMultiplier;
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
            authoredScale = prefab.transform.localScale;
            transform.localScale = authoredScale;
            UpgradeStats = null;
            UpgradePierceCount = 0;
            UpgradeRicochetCount = 0;
            UpgradeDurationMultiplier = 1f;
            UpgradeAreaMultiplier = 1f;
            homingIgnoredTargets.Clear();
            lifetimeMultiplier = 1f;
            projectileScaleMultiplier = 1f;
            castLifetimeMultiplier = 1f;
            castScaleMultiplier = 1f;
            damageContext = default;
            hasConfiguredDamageContext = false;
            ResetPooledConfiguration(prefab);
        }

        protected virtual void ResetPooledConfiguration(ProjectileBase prefab) { }

        protected void Despawn()
        {
            Body.linearVelocity = Vector2.zero;
            homingTarget = null;
            homingIgnoredTargets.Clear();
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

        public void ConfigureUpgradeModifiers(UpgradeRuntimeStats stats)
        {
            UpgradeStats = stats;
            if (stats == null) return;

            UpgradePierceCount = Mathf.Max(0, Mathf.RoundToInt(stats.GetFlat(UpgradeStat.Pierce)));
            UpgradeRicochetCount = Mathf.Max(0, Mathf.RoundToInt(stats.GetFlat(UpgradeStat.Ricochet)));
            UpgradeDurationMultiplier = stats.GetMultiplier(UpgradeStat.Duration);
            UpgradeAreaMultiplier = stats.GetMultiplier(UpgradeStat.AreaSize);
            lifetimeMultiplier = UpgradeDurationMultiplier;
            projectileScaleMultiplier = stats.GetMultiplier(UpgradeStat.ProjectileSize);
            transform.localScale = authoredScale * projectileScaleMultiplier;

            if (homeOnEnemies)
                homingStrength *= stats.GetMultiplier(UpgradeStat.Homing);
        }

        public void ConfigureDamageContext(DamageContext context)
        {
            damageContext = context;
            hasConfiguredDamageContext = true;
        }

        public void ConfigureCastModifiers(float sizeMultiplier, float durationMultiplier)
        {
            castScaleMultiplier = Mathf.Max(0.1f, sizeMultiplier);
            castLifetimeMultiplier = Mathf.Max(0.1f, durationMultiplier);
            transform.localScale = authoredScale * projectileScaleMultiplier * castScaleMultiplier;
        }

        protected DamageContext DamageSourceContext => damageContext;
        protected float GetDamageAgainst(IDamageable target) =>
            DamageResolver.Calculate(damageContext, target);

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
                OnLifetimeExpired();
                return;
            }

            if (!homeOnEnemies || homingStrength <= 0f || Body == null)
            {
                return;
            }

            if (Time.time >= nextHomingTargetRefresh)
            {
                homingTarget = CombatTargeting.FindClosest(
                    transform.position,
                    homingRange,
                    OwnerFaction,
                    ~0,
                    homingIgnoredTargets);
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

        protected virtual void OnLifetimeExpired() => Despawn();

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

            // A piercing homing projectile must not turn back into an enemy it
            // already passed through. Force an immediate search for another target.
            homingIgnoredTargets.Add(target.Transform.gameObject.GetInstanceID());
            if (homingTarget == target ||
                (homingTarget != null && homingTarget.Transform == target.Transform))
            {
                homingTarget = null;
            }
            nextHomingTargetRefresh = Time.time;

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

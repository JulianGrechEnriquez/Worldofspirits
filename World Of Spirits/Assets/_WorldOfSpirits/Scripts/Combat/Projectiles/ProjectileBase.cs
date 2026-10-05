using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 5f;
        [Header("Triggered Effects")]
        [Tooltip("Optional actions on hit, lifetime expiry or normal despawn. Upgrade requirements are evaluated using the firing player.")]
        [SerializeField] private List<ProjectileTrigger> triggeredEffects = new List<ProjectileTrigger>();
        private readonly HashSet<int> firedTriggers = new HashSet<int>();
        private readonly List<IDamageable> triggerTargets = new List<IDamageable>();
        private int triggerWeaponLevel = 1;
        private int triggerDepth;
        private bool shotActive;
        private bool expiryTriggered;
        private Vector2 shotDirection;
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
            firedTriggers.Clear();
            shotActive = true;
            expiryTriggered = false;
            shotDirection = direction.normalized;
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
            shotActive = false;
            expiryTriggered = false;
            firedTriggers.Clear();
            triggerWeaponLevel = 1;
            triggerDepth = 0;
            triggeredEffects = prefab.triggeredEffects;
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
            if (!shotActive) return;
            EmitTriggers(ProjectileTriggerEvent.Despawn);
            shotActive = false;
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

        public void ConfigureTriggerContext(int weaponLevel, int generation = 0)
        {
            triggerWeaponLevel = Mathf.Max(1, weaponLevel);
            triggerDepth = Mathf.Max(0, generation);
        }

        private void EmitTriggers(ProjectileTriggerEvent triggerEvent)
        {
            if (!shotActive || triggeredEffects == null || triggerDepth >= 3) return;
            Vector2 forward = Body != null && Body.linearVelocity.sqrMagnitude > 0.001f
                ? Body.linearVelocity.normalized : shotDirection;
            for (int i = 0; i < triggeredEffects.Count; i++)
            {
                ProjectileTrigger entry = triggeredEffects[i];
                if (entry == null || entry.when != triggerEvent ||
                    (entry.oncePerShot && firedTriggers.Contains(i)) ||
                    !entry.IsUnlocked(triggerWeaponLevel, UpgradeStats)) continue;
                firedTriggers.Add(i);
                DamageContext secondaryDamage = DamageSourceContext.WithBaseDamage(Damage * Mathf.Max(0f, entry.damageMultiplier));
                switch (entry.action)
                {
                    case ProjectileTriggerAction.AreaDamage:
                        CombatTargeting.FindAllNonAlloc(transform.position,
                            Mathf.Max(0.05f, entry.areaRadius) * UpgradeAreaMultiplier, OwnerFaction, triggerTargets);
                        foreach (IDamageable target in triggerTargets) target.TakeDamage(secondaryDamage);
                        break;
                    case ProjectileTriggerAction.SpawnEffect:
                        if (entry.effectPrefab == null || entry.effectPrefab.GetComponent<ProjectileBase>() != null) break;
                        GameObject effect = WorldOfSpirits.Core.SceneObjectPool.Spawn(entry.effectPrefab,
                            transform.position, Quaternion.Euler(0f, 0f, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg),
                            WorldOfSpirits.Core.PoolCategory.Effects);
                        if (effect.TryGetComponent(out WorldOfSpirits.Spirits.PersistentDamageZone zone))
                        {
                            zone.SetOwner(secondaryDamage.Source);
                            zone.ConfigureUpgradeModifiers(UpgradeStats);
                            zone.ConfigureTriggeredDamage(secondaryDamage);
                            zone.SetReusable(true);
                        }
                        WorldOfSpirits.Core.SceneObjectPool.ReleaseAfter(effect,
                            Mathf.Max(0.05f, entry.effectDuration) * UpgradeDurationMultiplier);
                        break;
                    case ProjectileTriggerAction.SpawnProjectiles:
                        if (entry.projectilePrefab == null) break;
                        int count = Mathf.Clamp(entry.projectileCount, 1, 16);
                        float center = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
                        for (int n = 0; n < count; n++)
                        {
                            float angle = center + (count == 1 ? 0f : entry.spreadAngle >= 360f
                                ? 360f * n / count : -entry.spreadAngle * 0.5f + entry.spreadAngle * n / (count - 1));
                            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                            ProjectileBase child = ProjectilePool.Spawn(entry.projectilePrefab, transform.position, Quaternion.identity);
                            child.ConfigureUpgradeModifiers(UpgradeStats);
                            child.ConfigureDamageContext(secondaryDamage);
                            child.ConfigureTriggerContext(triggerWeaponLevel, triggerDepth + 1);
                            child.Launch(direction, Mathf.Max(0.1f, entry.projectileSpeed), secondaryDamage.BaseDamage, OwnerFaction);
                        }
                        break;
                }
            }
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
            if (!shotActive) return;
            if (Time.time >= despawnTime)
            {
                if (!expiryTriggered)
                {
                    expiryTriggered = true;
                    EmitTriggers(ProjectileTriggerEvent.LifetimeExpired);
                }
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

        protected virtual void LateUpdate()
        {
            if (Body != null && Body.linearVelocity.sqrMagnitude > Mathf.Epsilon)
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
            float facingAngle = angle + rotationOffset;
            Body.rotation = facingAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, facingAngle);
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
            bool firstHit = homingIgnoredTargets.Add(target.Transform.gameObject.GetInstanceID());
            if (firstHit) EmitTriggers(ProjectileTriggerEvent.EnemyHit);
            if (homingTarget == target ||
                (homingTarget != null && homingTarget.Transform == target.Transform))
            {
                homingTarget = null;
            }
            nextHomingTargetRefresh = Time.time;

            OnHit(target);
        }

        protected virtual void OnDisable() => shotActive = false;

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

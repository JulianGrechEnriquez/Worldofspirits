using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Crowd;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    [RequireComponent(typeof(Collider2D))]
    public class PersistentDamageZone : MonoBehaviour, IScenePoolable
    {
        private sealed class Occupant
        {
            public IDamageable Target;
            public Rigidbody2D Body;
            public float NextHitTime;
            public int ColliderCount;
        }

        [SerializeField, Min(0f)] private float damagePerTick = 5f;
        [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;
        [SerializeField, Min(0.05f)] private float duration = 3f;
        [SerializeField] private bool destroyAfterDuration = true;
        [SerializeField] private Faction ownerFaction = Faction.Player;
        [SerializeField, Min(0f)] private float pullForce;
        [SerializeField] private bool followOwner;
        [SerializeField, Tooltip("Grow only a CircleCollider2D, keeping the artwork at its authored size.")]
        private bool resizeColliderOnly;

        private readonly Dictionary<int, Occupant> occupants = new Dictionary<int, Occupant>();
        private readonly List<int> occupantsToRemove = new List<int>();
        private Transform owner;
        private float disableTime;
        private UpgradeRuntimeStats upgradeStats;
        private Vector3 authoredScale;
        private float authoredDamagePerTick;
        private float authoredDuration;
        private float authoredPullForce;
        private DamageContext damageSource;
        private float castPullMultiplier = 1f;
        private float authoredColliderRadius;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            authoredScale = transform.localScale;
            authoredDamagePerTick = damagePerTick;
            authoredDuration = duration;
            authoredPullForce = pullForce;
            CircleCollider2D circle = GetComponent<CircleCollider2D>();
            authoredColliderRadius = circle != null ? circle.radius : 0f;
        }

        private void OnEnable()
        {
            disableTime = Time.time + duration;
        }

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
            upgradeStats = newOwner != null
                ? newOwner.GetComponentInParent<UpgradeRuntimeStats>()
                : null;
        }

        public void ConfigureUpgradeModifiers(UpgradeRuntimeStats stats)
        {
            upgradeStats = stats;
            damagePerTick = authoredDamagePerTick;
            duration = stats != null ? stats.ScaleDuration(authoredDuration) : authoredDuration;
            pullForce = stats != null ? stats.ScaleForce(authoredPullForce) : authoredPullForce;
            if (!resizeColliderOnly)
                transform.localScale = authoredScale *
                    (stats != null ? stats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            disableTime = Time.time + duration;
        }

        public void ConfigureDamageSource(DamageContext context)
        {
            damageSource = context;
        }

        public void ConfigureCast(float radius, float pullMultiplier = 1f)
        {
            float areaMultiplier = upgradeStats != null
                ? upgradeStats.GetMultiplier(UpgradeStat.AreaSize)
                : 1f;
            if (resizeColliderOnly && TryGetComponent(out CircleCollider2D circle))
            {
                transform.localScale = authoredScale;
                circle.radius = Mathf.Max(0.1f, radius) * areaMultiplier;
                castPullMultiplier = Mathf.Max(0f, pullMultiplier);
                RegisterOverlappingTargets(circle);
                return;
            }

            float colliderRadius = GetComponent<CircleCollider2D>()?.radius ?? 1f;
            float diameter = Mathf.Max(0.1f, colliderRadius * 2f);
            transform.localScale = authoredScale * (Mathf.Max(0.1f, radius) * 2f / diameter) *
                areaMultiplier;
            castPullMultiplier = Mathf.Max(0f, pullMultiplier);
        }

        private void RegisterOverlappingTargets(Collider2D zoneCollider)
        {
            ContactFilter2D filter = ContactFilter2D.noFilter;
            Collider2D[] overlaps = new Collider2D[64];
            int count = zoneCollider.Overlap(filter, overlaps);
            for (int i = 0; i < count; i++)
            {
                if (overlaps[i] != null)
                    OnTriggerEnter2D(overlaps[i]);
            }
        }

        public void SetReusable(bool reusable)
        {
            destroyAfterDuration = !reusable;
        }

        private void Update()
        {
            if (destroyAfterDuration && Time.time >= disableTime)
            {
                SceneObjectPool.ReleaseOrDestroy(gameObject);
                return;
            }

            if (followOwner && owner != null)
            {
                transform.position = owner.position;
            }

            occupantsToRemove.Clear();
            foreach (KeyValuePair<int, Occupant> pair in occupants)
            {
                Occupant occupant = pair.Value;
                if (occupant.Target == null || !occupant.Target.IsAlive)
                {
                    occupantsToRemove.Add(pair.Key);
                    continue;
                }

                if (damagePerTick > 0f && Time.time >= occupant.NextHitTime)
                {
                    occupant.Target.TakeDamage(damageSource.WithBaseDamage(damagePerTick));
                    occupant.NextHitTime = Time.time + tickInterval;
                }
            }

            for (int i = 0; i < occupantsToRemove.Count; i++)
            {
                occupants.Remove(occupantsToRemove[i]);
            }
        }

        private void FixedUpdate()
        {
            if (pullForce <= 0f)
            {
                return;
            }

            foreach (Occupant occupant in occupants.Values)
            {
                if (occupant.Body != null && occupant.Target != null && occupant.Target.IsAlive)
                {
                    Vector2 direction = (transform.position - occupant.Target.Transform.position).normalized;
                    float force = pullForce * castPullMultiplier;
                    EnemyCrowdAgent crowdAgent = occupant.Target.Transform.GetComponentInParent<EnemyCrowdAgent>();
                    if (crowdAgent != null)
                        crowdAgent.ApplyExternalAcceleration(direction * force, Time.fixedDeltaTime);
                    else
                        occupant.Body.AddForce(direction * force, ForceMode2D.Force);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || target.Faction == ownerFaction)
            {
                return;
            }

            int id = target.Transform.gameObject.GetInstanceID();
            if (occupants.TryGetValue(id, out Occupant existing))
            {
                existing.ColliderCount++;
                return;
            }

            occupants.Add(id, new Occupant
            {
                Target = target,
                Body = target.Transform.GetComponent<Rigidbody2D>(),
                NextHitTime = Time.time,
                ColliderCount = 1
            });
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null)
            {
                return;
            }

            int id = target.Transform.gameObject.GetInstanceID();
            if (occupants.TryGetValue(id, out Occupant occupant) && --occupant.ColliderCount <= 0)
            {
                occupants.Remove(id);
            }
        }

        private void OnDisable()
        {
            occupants.Clear();
        }

        public void OnSpawnedFromPool(GameObject prefab)
        {
            PersistentDamageZone prefabZone = prefab.GetComponent<PersistentDamageZone>();
            if (prefabZone != null)
            {
                damagePerTick = prefabZone.damagePerTick;
                tickInterval = prefabZone.tickInterval;
                duration = prefabZone.duration;
                destroyAfterDuration = prefabZone.destroyAfterDuration;
                ownerFaction = prefabZone.ownerFaction;
                pullForce = prefabZone.pullForce;
                followOwner = prefabZone.followOwner;
                resizeColliderOnly = prefabZone.resizeColliderOnly;
                authoredScale = prefabZone.transform.localScale;
                authoredDamagePerTick = prefabZone.damagePerTick;
                authoredDuration = prefabZone.duration;
                authoredPullForce = prefabZone.pullForce;
                authoredColliderRadius = prefabZone.authoredColliderRadius > 0f
                    ? prefabZone.authoredColliderRadius
                    : (prefabZone.TryGetComponent(out CircleCollider2D prefabCircle)
                        ? prefabCircle.radius
                        : 0f);
            }

            owner = null;
            upgradeStats = null;
            castPullMultiplier = 1f;
            damageSource = new DamageContext(damagePerTick);
            transform.localScale = authoredScale;
            if (authoredColliderRadius > 0f && TryGetComponent(out CircleCollider2D circle))
                circle.radius = authoredColliderRadius;
            disableTime = Time.time + duration;
        }

        public void OnReturnedToPool()
        {
            occupants.Clear();
            owner = null;
        }
    }
}

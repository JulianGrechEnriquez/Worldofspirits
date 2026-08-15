using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Crowd;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    /// <summary>
    /// Persistent damage zone that can damage and pull enemies.
    ///
    /// Pull styles:
    /// Direct     - Pulls enemies directly toward the centre.
    /// Quicksand  - Slow, heavy pull that becomes stronger near the centre.
    /// Tornado    - Pulls enemies inward while spiralling around the centre.
    /// Whirlpool  - Strong rotational pull with inward movement.
    /// BlackHole  - Very strong gravitational pull that increases near the centre.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PersistentDamageZone : MonoBehaviour, IScenePoolable
    {
        public enum PullStyle
        {
            Direct,
            Quicksand,
            Tornado,
            Whirlpool,
            BlackHole
        }

        private sealed class Occupant
        {
            public IDamageable Target;
            public Rigidbody2D Body;
            public float NextHitTime;
            public int ColliderCount;
        }

        [Header("Damage")]

        [SerializeField, Min(0f)]
        private float damagePerTick = 5f;

        [SerializeField, Min(0.05f)]
        private float tickInterval = 0.5f;

        [SerializeField, Min(0.05f)]
        private float duration = 3f;

        [SerializeField]
        private bool destroyAfterDuration = true;

        [SerializeField]
        private Faction ownerFaction = Faction.Player;


        [Header("Pull")]

        [SerializeField]
        private PullStyle pullStyle = PullStyle.Direct;

        [SerializeField, Min(0f)]
        private float pullForce = 0f;

        [SerializeField, Tooltip(
            "Additional rotational force used by Tornado and Whirlpool.")]
        private float swirlForce = 5f;

        [SerializeField, Tooltip(
            "Controls how strongly the pull increases/decreases based on distance.")]
        private float distanceFalloff = 1f;

        [SerializeField, Tooltip(
            "Prevents enemies from being pulled infinitely close to the centre.")]
        private float minimumRadius = 0.25f;

        [SerializeField, Tooltip(
            "Adds a small random movement to Quicksand.")]
        private float quicksandWobble = 0.5f;

        [SerializeField, Tooltip(
            "How much the pull increases when an enemy is close to the centre.")]
        private float blackHoleMultiplier = 3f;


        [Header("Behaviour")]

        [SerializeField]
        private bool followOwner;

        [SerializeField, Tooltip(
            "Grow only a CircleCollider2D, keeping the artwork at its authored size.")]
        private bool resizeColliderOnly;


        private readonly Dictionary<int, Occupant> occupants =
            new Dictionary<int, Occupant>();

        private readonly List<int> occupantsToRemove =
            new List<int>();


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

            CircleCollider2D circle =
                GetComponent<CircleCollider2D>();

            authoredColliderRadius =
                circle != null ? circle.radius : 0f;
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

            duration = stats != null
                ? stats.ScaleDuration(authoredDuration)
                : authoredDuration;

            pullForce = stats != null
                ? stats.ScaleForce(authoredPullForce)
                : authoredPullForce;


            if (!resizeColliderOnly)
            {
                transform.localScale =
                    authoredScale *
                    (stats != null
                        ? stats.GetMultiplier(UpgradeStat.AreaSize)
                        : 1f);
            }

            disableTime = Time.time + duration;
        }


        public void ConfigureDamageSource(DamageContext context)
        {
            damageSource = context;
        }


        public void ConfigureCast(
            float radius,
            float pullMultiplier = 1f)
        {
            float areaMultiplier =
                upgradeStats != null
                    ? upgradeStats.GetMultiplier(
                        UpgradeStat.AreaSize)
                    : 1f;


            if (resizeColliderOnly &&
                TryGetComponent(
                    out CircleCollider2D circle))
            {
                transform.localScale = authoredScale;

                circle.radius =
                    Mathf.Max(0.1f, radius) *
                    areaMultiplier;

                castPullMultiplier =
                    Mathf.Max(0f, pullMultiplier);

                RegisterOverlappingTargets(circle);

                return;
            }


            float colliderRadius =
                GetComponent<CircleCollider2D>()?.radius ?? 1f;

            float diameter =
                Mathf.Max(0.1f, colliderRadius * 2f);


            transform.localScale =
                authoredScale *
                (Mathf.Max(0.1f, radius) * 2f / diameter) *
                areaMultiplier;


            castPullMultiplier =
                Mathf.Max(0f, pullMultiplier);
        }


        private void RegisterOverlappingTargets(
            Collider2D zoneCollider)
        {
            ContactFilter2D filter =
                ContactFilter2D.noFilter;

            Collider2D[] overlaps =
                new Collider2D[64];

            int count =
                zoneCollider.Overlap(
                    filter,
                    overlaps);


            for (int i = 0; i < count; i++)
            {
                if (overlaps[i] != null)
                {
                    OnTriggerEnter2D(overlaps[i]);
                }
            }
        }


        public void SetReusable(bool reusable)
        {
            destroyAfterDuration = !reusable;
        }


        private void Update()
        {
            if (destroyAfterDuration &&
                Time.time >= disableTime)
            {
                SceneObjectPool.ReleaseOrDestroy(gameObject);
                return;
            }


            if (followOwner && owner != null)
            {
                transform.position =
                    owner.position;
            }


            occupantsToRemove.Clear();


            foreach (
                KeyValuePair<int, Occupant> pair
                in occupants)
            {
                Occupant occupant =
                    pair.Value;


                if (occupant.Target == null ||
                    !occupant.Target.IsAlive)
                {
                    occupantsToRemove.Add(
                        pair.Key);

                    continue;
                }


                if (damagePerTick > 0f &&
                    Time.time >= occupant.NextHitTime)
                {
                    occupant.Target.TakeDamage(
                        damageSource.WithBaseDamage(
                            damagePerTick));

                    occupant.NextHitTime =
                        Time.time + tickInterval;
                }
            }


            for (int i = 0;
                 i < occupantsToRemove.Count;
                 i++)
            {
                occupants.Remove(
                    occupantsToRemove[i]);
            }
        }


        private void FixedUpdate()
        {
            if (pullForce <= 0f)
            {
                return;
            }


            foreach (
                Occupant occupant
                in occupants.Values)
            {
                if (occupant.Body == null ||
                    occupant.Target == null ||
                    !occupant.Target.IsAlive)
                {
                    continue;
                }


                Vector2 enemyPosition =
                    occupant.Target.Transform.position;

                Vector2 centre =
                    transform.position;


                Vector2 offset =
                    centre - enemyPosition;

                float distance =
                    offset.magnitude;


                if (distance <= minimumRadius)
                {
                    continue;
                }


                Vector2 radial =
                    offset.normalized;


                Vector2 direction =
                    CalculatePullDirection(
                        radial,
                        distance);


                float force =
                    CalculatePullForce(
                        distance);


                force *= castPullMultiplier;


                ApplyPull(
                    occupant,
                    direction * force);
            }
        }


        /// <summary>
        /// Calculates the direction of the pull depending
        /// on the selected pull style.
        /// </summary>
        private Vector2 CalculatePullDirection(
            Vector2 radial,
            float distance)
        {
            switch (pullStyle)
            {
                case PullStyle.Direct:

                    return radial;


                case PullStyle.Quicksand:

                    return CalculateQuicksandDirection(
                        radial,
                        distance);


                case PullStyle.Tornado:

                    return CalculateTornadoDirection(
                        radial,
                        distance);


                case PullStyle.Whirlpool:

                    return CalculateWhirlpoolDirection(
                        radial,
                        distance);


                case PullStyle.BlackHole:

                    return radial;


                default:

                    return radial;
            }
        }


        /// <summary>
        /// Quicksand keeps enemies moving toward the centre
        /// but adds a small amount of unstable movement.
        /// </summary>
        private Vector2 CalculateQuicksandDirection(
            Vector2 radial,
            float distance)
        {
            Vector2 wobble =
                new Vector2(
                    Mathf.PerlinNoise(
                        Time.time * 2f,
                        distance) - 0.5f,

                    Mathf.PerlinNoise(
                        distance,
                        Time.time * 2f) - 0.5f);


            wobble *= quicksandWobble;


            return (
                radial +
                wobble
            ).normalized;
        }


        /// <summary>
        /// Tornado pulls inward while strongly rotating
        /// around the centre.
        /// </summary>
        private Vector2 CalculateTornadoDirection(
            Vector2 radial,
            float distance)
        {
            Vector2 tangent =
                new Vector2(
                    -radial.y,
                    radial.x);


            float swirl =
                swirlForce;


            // Stronger swirl farther from the centre.
            float distanceFactor =
                Mathf.Clamp01(
                    distance /
                    Mathf.Max(0.01f, minimumRadius * 8f));


            swirl *=
                Mathf.Lerp(
                    0.5f,
                    1.5f,
                    distanceFactor);


            return (
                radial +
                tangent * swirl
            ).normalized;
        }


        /// <summary>
        /// Whirlpool behaves like a rotating vortex,
        /// with a stronger rotational component.
        /// </summary>
        private Vector2 CalculateWhirlpoolDirection(
            Vector2 radial,
            float distance)
        {
            Vector2 tangent =
                new Vector2(
                    -radial.y,
                    radial.x);


            float distanceFactor =
                Mathf.Clamp01(
                    distance /
                    Mathf.Max(0.01f, minimumRadius * 10f));


            float swirl =
                swirlForce *
                Mathf.Lerp(
                    1f,
                    2f,
                    distanceFactor);


            return (
                radial +
                tangent * swirl
            ).normalized;
        }


        /// <summary>
        /// Calculates the actual force based on
        /// the distance from the centre.
        /// </summary>
        private float CalculatePullForce(
            float distance)
        {
            switch (pullStyle)
            {
                case PullStyle.Direct:

                    return pullForce;


                case PullStyle.Quicksand:
                    {
                        // Weak at the edge, stronger toward
                        // the centre.
                        float normalized =
                            Mathf.Clamp01(
                                distanceFalloff /
                                Mathf.Max(
                                    distance,
                                    minimumRadius));

                        return pullForce *
                               Mathf.Lerp(
                                   0.35f,
                                   1f,
                                   normalized);
                    }


                case PullStyle.Tornado:
                    {
                        return pullForce *
                               Mathf.Lerp(
                                   0.75f,
                                   1.25f,
                                   Mathf.Clamp01(
                                       distanceFalloff /
                                       Mathf.Max(
                                           distance,
                                           minimumRadius)));
                    }


                case PullStyle.Whirlpool:
                    {
                        return pullForce *
                               Mathf.Lerp(
                                   0.8f,
                                   1.4f,
                                   Mathf.Clamp01(
                                       distanceFalloff /
                                       Mathf.Max(
                                           distance,
                                           minimumRadius)));
                    }


                case PullStyle.BlackHole:
                    {
                        float gravity =
                            1f +
                            blackHoleMultiplier /
                            Mathf.Max(
                                distance,
                                minimumRadius);

                        return pullForce * gravity;
                    }


                default:

                    return pullForce;
            }
        }


        /// <summary>
        /// Sends the calculated force through EnemyCrowdAgent
        /// when available, otherwise directly through Rigidbody2D.
        /// </summary>
        private void ApplyPull(
            Occupant occupant,
            Vector2 force)
        {
            EnemyCrowdAgent crowdAgent =
                occupant.Target.Transform
                    .GetComponentInParent<EnemyCrowdAgent>();


            if (crowdAgent != null)
            {
                crowdAgent.ApplyExternalAcceleration(
                    force,
                    Time.fixedDeltaTime);
            }
            else
            {
                occupant.Body.AddForce(
                    force,
                    ForceMode2D.Force);
            }
        }


        private void OnTriggerEnter2D(
            Collider2D other)
        {
            IDamageable target =
                other.GetComponentInParent<IDamageable>();


            if (target == null ||
                !target.IsAlive ||
                target.Faction == ownerFaction)
            {
                return;
            }


            int id =
                target.Transform.gameObject
                    .GetInstanceID();


            if (occupants.TryGetValue(
                    id,
                    out Occupant existing))
            {
                existing.ColliderCount++;

                return;
            }


            occupants.Add(
                id,
                new Occupant
                {
                    Target = target,

                    Body =
                        target.Transform
                            .GetComponent<Rigidbody2D>(),

                    NextHitTime =
                        Time.time,

                    ColliderCount = 1
                });
        }


        private void OnTriggerExit2D(
            Collider2D other)
        {
            IDamageable target =
                other.GetComponentInParent<IDamageable>();


            if (target == null)
            {
                return;
            }


            int id =
                target.Transform.gameObject
                    .GetInstanceID();


            if (occupants.TryGetValue(
                    id,
                    out Occupant occupant) &&
                --occupant.ColliderCount <= 0)
            {
                occupants.Remove(id);
            }
        }


        private void OnDisable()
        {
            occupants.Clear();
        }


        public void OnSpawnedFromPool(
            GameObject prefab)
        {
            PersistentDamageZone prefabZone =
                prefab.GetComponent<PersistentDamageZone>();


            if (prefabZone != null)
            {
                damagePerTick =
                    prefabZone.damagePerTick;

                tickInterval =
                    prefabZone.tickInterval;

                duration =
                    prefabZone.duration;

                destroyAfterDuration =
                    prefabZone.destroyAfterDuration;

                ownerFaction =
                    prefabZone.ownerFaction;

                pullStyle =
                    prefabZone.pullStyle;

                pullForce =
                    prefabZone.pullForce;

                swirlForce =
                    prefabZone.swirlForce;

                distanceFalloff =
                    prefabZone.distanceFalloff;

                minimumRadius =
                    prefabZone.minimumRadius;

                quicksandWobble =
                    prefabZone.quicksandWobble;

                blackHoleMultiplier =
                    prefabZone.blackHoleMultiplier;

                followOwner =
                    prefabZone.followOwner;

                resizeColliderOnly =
                    prefabZone.resizeColliderOnly;


                authoredScale =
                    prefabZone.transform.localScale;

                authoredDamagePerTick =
                    prefabZone.damagePerTick;

                authoredDuration =
                    prefabZone.duration;

                authoredPullForce =
                    prefabZone.pullForce;


                authoredColliderRadius =
                    prefabZone.authoredColliderRadius > 0f
                        ? prefabZone.authoredColliderRadius
                        : (
                            prefabZone.TryGetComponent(
                                out CircleCollider2D prefabCircle)
                                ? prefabCircle.radius
                                : 0f
                        );
            }


            owner = null;

            upgradeStats = null;

            castPullMultiplier = 1f;

            damageSource =
                new DamageContext(
                    damagePerTick);


            transform.localScale =
                authoredScale;


            if (authoredColliderRadius > 0f &&
                TryGetComponent(
                    out CircleCollider2D circle))
            {
                circle.radius =
                    authoredColliderRadius;
            }


            disableTime =
                Time.time + duration;
        }


        public void OnReturnedToPool()
        {
            occupants.Clear();

            owner = null;
        }
    }
}
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.UI;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression;

namespace WorldOfSpirits.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class EnemyBase : LivingEntity, IRewardSource, IEnemyClassification
    {
        [Header("Targeting")]
        [SerializeField] private Transform target;
        [Tooltip("How often this enemy recalculates its direction. Its Rigidbody keeps moving between updates.")]
        [SerializeField, Min(0.02f)] private float movementRefreshInterval = 0.06f;

        [Header("Rewards")]
        [SerializeField, Min(0f)] private float experienceReward = 1f;

        [Header("Debug")]
        [SerializeField] private bool drawTargetLine = true;

        protected Rigidbody2D Body { get; private set; }
        protected Transform Target => target;
        public override Faction Faction => global::WorldOfSpirits.Combat.Faction.Enemy;
        public bool IsElite { get; private set; }
        public bool IsBoss { get; private set; }
        public float ExperienceReward => experienceReward;
        private float nextMovementRefresh;
        private bool externalMovement;
        private static Player.PlayerCharacter cachedPlayer;

        public void SetExternalMovement(bool enabled)
        {
            externalMovement = enabled;
            if (enabled && Body != null)
            {
                Body.linearVelocity = Vector2.zero;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            Body.freezeRotation = true;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            nextMovementRefresh = Time.fixedTime +
                (Mathf.Abs(GetInstanceID()) % 4) * movementRefreshInterval * 0.25f;

            if (GetComponent<DamageNumberEmitter>() == null)
            {
                gameObject.AddComponent<DamageNumberEmitter>();
            }

            if (target == null)
            {
                target = GetPlayerTransform();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!externalMovement && IsAlive && target != null)
            {
                if (Time.fixedTime >= nextMovementRefresh)
                {
                    MoveTowardsTarget();
                    nextMovementRefresh = Time.fixedTime + movementRefreshInterval;
                }
            }
            else
            {
                Body.linearVelocity = Vector2.zero;
            }
        }

        protected abstract void MoveTowardsTarget();

        public void ConfigureClassification(bool isElite, bool isBoss)
        {
            IsElite = isElite;
            IsBoss = isBoss;
        }

        public override void OnSpawnedFromPool(GameObject prefab)
        {
            base.OnSpawnedFromPool(prefab);
            Body.linearVelocity = Vector2.zero;
            if (target == null)
            {
                target = GetPlayerTransform();
            }
        }

        private static Transform GetPlayerTransform()
        {
            if (cachedPlayer == null)
            {
                cachedPlayer = FindFirstObjectByType<Player.PlayerCharacter>();
            }
            return cachedPlayer != null ? cachedPlayer.transform : null;
        }

        protected virtual void Start()
        {
            SceneObjectPool.AdoptExisting(gameObject, PoolCategory.Enemies);
        }

        public override void OnReturnedToPool()
        {
            Body.linearVelocity = Vector2.zero;
            base.OnReturnedToPool();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!drawTargetLine || target == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.25f);
        }
    }
}

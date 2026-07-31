using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Player;
using WorldOfSpirits.UI;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression;

namespace WorldOfSpirits.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class BossEnemyBase : LivingEntity
    {
        [Header("Boss Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool faceTarget;
        [SerializeField] private SpriteRenderer bossRenderer;

        [Header("Rewards")]
        [SerializeField, Min(0f)] private float experienceReward = 25f;

        protected Rigidbody2D Body { get; private set; }
        protected Transform Target => target;
        public override Faction Faction => global::WorldOfSpirits.Combat.Faction.Enemy;
        private static PlayerCharacter cachedPlayer;

        protected override void Awake()
        {
            base.Awake();
            Died += AwardExperience;

            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            Body.freezeRotation = true;

            if (GetComponent<DamageNumberEmitter>() == null)
            {
                gameObject.AddComponent<DamageNumberEmitter>();
            }

            if (target == null)
            {
                target = GetPlayerTransform();
            }

            if (bossRenderer == null)
            {
                bossRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (IsAlive && target != null)
            {
                UpdateBoss(target);
                UpdateFacing();
            }
        }

        protected virtual void FixedUpdate()
        {
            // Bosses do not inherit chase movement. They remain stationary unless
            // a derived boss explicitly implements a dash, leap, or teleport.
            Body.linearVelocity = Vector2.zero;
        }

        protected abstract void UpdateBoss(Transform playerTarget);

        private void AwardExperience()
        {
            if (experienceReward > 0f) ExperienceOrbService.Spawn(transform.position, experienceReward);
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
                cachedPlayer = FindFirstObjectByType<PlayerCharacter>();
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

        private void UpdateFacing()
        {
            if (!faceTarget || bossRenderer == null)
            {
                return;
            }

            bossRenderer.flipX = target.position.x < transform.position.x;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (target == null)
            {
                return;
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}

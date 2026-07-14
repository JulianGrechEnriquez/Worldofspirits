using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Player;
using WorldOfSpirits.UI;

namespace WorldOfSpirits.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class BossEnemyBase : LivingEntity
    {
        [Header("Boss Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool faceTarget;
        [SerializeField] private SpriteRenderer bossRenderer;

        protected Rigidbody2D Body { get; private set; }
        protected Transform Target => target;
        public override Faction Faction => global::WorldOfSpirits.Combat.Faction.Enemy;

        protected override void Awake()
        {
            base.Awake();

            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            Body.freezeRotation = true;

            if (GetComponent<DamageNumberEmitter>() == null)
            {
                gameObject.AddComponent<DamageNumberEmitter>();
            }

            if (target == null)
            {
                PlayerCharacter player = FindFirstObjectByType<PlayerCharacter>();
                target = player != null ? player.transform : null;
            }

            if (bossRenderer == null)
            {
                bossRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        protected override void Update()
        {
            base.Update();
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

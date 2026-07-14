using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.UI;

namespace WorldOfSpirits.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class EnemyBase : LivingEntity
    {
        [Header("Targeting")]
        [SerializeField] private Transform target;

        [Header("Debug")]
        [SerializeField] private bool drawTargetLine = true;

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
                Player.PlayerCharacter player = FindFirstObjectByType<Player.PlayerCharacter>();
                target = player != null ? player.transform : null;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (IsAlive && target != null)
            {
                MoveTowardsTarget();
            }
            else
            {
                Body.linearVelocity = Vector2.zero;
            }
        }

        protected abstract void MoveTowardsTarget();

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

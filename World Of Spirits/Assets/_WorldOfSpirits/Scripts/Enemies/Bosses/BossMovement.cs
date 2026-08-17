using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossMovement : MonoBehaviour
    {
        [SerializeField] private BossEnemyBase boss;
        [SerializeField, Min(0f)] private float preferredDistance = 5f;
        [SerializeField, Min(0f)] private float distanceTolerance = 1f;

        private Rigidbody2D body;
        private Transform target;
        private bool attackControl;

        public bool HasAttackControl => attackControl;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (boss == null) boss = GetComponent<BossEnemyBase>();
            if (boss != null) boss.SetExternalMovement(true);
        }

        public void SetTarget(Transform value) => target = value;
        public void TakeAttackControl() { attackControl = true; body.linearVelocity = Vector2.zero; }
        public void SetAttackVelocity(Vector2 velocity) { attackControl = true; body.linearVelocity = velocity; }
        public void ReleaseAttackControl() { attackControl = false; if (body != null) body.linearVelocity = Vector2.zero; }

        private void FixedUpdate()
        {
            if (attackControl || boss == null || !boss.IsAlive || target == null) return;
            Vector2 offset = target.position - transform.position;
            float distance = offset.magnitude;
            float speed = boss.Data != null ? boss.Data.MovementSpeed : boss.MoveSpeed;
            if (distance > preferredDistance + distanceTolerance)
                body.linearVelocity = offset.normalized * speed;
            else if (distance < preferredDistance - distanceTolerance)
                body.linearVelocity = -offset.normalized * speed;
            else
                body.linearVelocity = Vector2.zero;
        }
    }
}

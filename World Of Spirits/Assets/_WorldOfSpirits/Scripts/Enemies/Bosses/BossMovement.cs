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
        [Header("Camera Containment")]
        [SerializeField] private Camera gameplayCamera;
        [SerializeField, Min(0f)] private float cameraEdgePadding = 0.35f;
        [SerializeField] private SpriteRenderer bossRenderer;

        private Rigidbody2D body;
        private Transform target;
        private bool attackControl;

        public bool HasAttackControl => attackControl;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (boss == null) boss = GetComponent<BossEnemyBase>();
            if (bossRenderer == null) bossRenderer = GetComponentInChildren<SpriteRenderer>();
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (boss != null) boss.SetExternalMovement(true);
        }

        public void SetTarget(Transform value) => target = value;
        public void TakeAttackControl() { attackControl = true; body.linearVelocity = Vector2.zero; }
        public void SetAttackVelocity(Vector2 velocity) { attackControl = true; body.linearVelocity = velocity; }
        public void ReleaseAttackControl() { attackControl = false; if (body != null) body.linearVelocity = Vector2.zero; }

        private void FixedUpdate()
        {
            if (boss == null || !boss.IsAlive) return;
            if (!attackControl && target != null)
            {
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

            KeepInsideCamera();
        }

        private void KeepInsideCamera()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (gameplayCamera == null) return;

            float depth = Mathf.Abs(gameplayCamera.transform.position.z - transform.position.z);
            Vector3 bottomLeft = gameplayCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
            Vector3 topRight = gameplayCamera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
            Vector3 extents = bossRenderer != null ? bossRenderer.bounds.extents : Vector3.one * 0.5f;
            float minX = bottomLeft.x + extents.x + cameraEdgePadding;
            float maxX = topRight.x - extents.x - cameraEdgePadding;
            float minY = bottomLeft.y + extents.y + cameraEdgePadding;
            float maxY = topRight.y - extents.y - cameraEdgePadding;

            Vector2 position = body.position;
            Vector2 clamped = new Vector2(
                Mathf.Clamp(position.x, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX)),
                Mathf.Clamp(position.y, Mathf.Min(minY, maxY), Mathf.Max(minY, maxY)));

            if ((clamped - position).sqrMagnitude <= 0.0001f) return;
            body.position = clamped;
            Vector2 velocity = body.linearVelocity;
            if ((position.x < minX && velocity.x < 0f) || (position.x > maxX && velocity.x > 0f))
                velocity.x = 0f;
            if ((position.y < minY && velocity.y < 0f) || (position.y > maxY && velocity.y > 0f))
                velocity.y = 0f;
            body.linearVelocity = velocity;
        }
    }
}

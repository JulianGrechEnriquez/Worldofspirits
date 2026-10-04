using UnityEngine;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Combat
{
    /// <summary>Owns capture state so enemy pooling always restores movement immediately.</summary>
    [DisallowMultipleComponent]
    public sealed class AvalancheCapturedEnemy : MonoBehaviour
    {
        private Rigidbody2D body;
        private EnemyBase enemy;
        private bool wasSimulated;
        private Vector3 localOffset;
        public AvalancheProjectile Owner { get; private set; }

        public bool TryCapture(AvalancheProjectile snowball)
        {
            if (Owner != null || snowball == null) return false;
            body = GetComponent<Rigidbody2D>();
            enemy = GetComponent<EnemyBase>();
            if (body == null || !body.simulated || enemy == null || !enemy.IsAlive) return false;
            Owner = snowball;
            localOffset = Vector3.ClampMagnitude(snowball.transform.InverseTransformPoint(transform.position), 0.25f);
            localOffset.z = 0f;
            wasSimulated = body.simulated;
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            enemy.Died += Release;
            Follow();
            return true;
        }

        public void Follow()
        {
            if (Owner == null || !enemy.IsAlive) { Release(); return; }
            Vector3 position = Owner.transform.TransformPoint(localOffset);
            position.z = transform.position.z;
            transform.position = position;
            body.position = position;
        }

        public void Release()
        {
            if (Owner == null) return;
            Owner = null;
            if (enemy != null) enemy.Died -= Release;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = wasSimulated;
            }
        }

        private void OnDisable() => Release();
    }
}

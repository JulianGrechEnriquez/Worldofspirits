using UnityEngine;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.CameraSystem
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField, Min(0f)] private float smoothTime = 0.12f;

        private Vector3 velocity;

        private void Awake()
        {
            FindPlayerIfNeeded();
            SnapToTarget();
        }

        private void LateUpdate()
        {
            FindPlayerIfNeeded();
            if (target == null) return;

            Vector3 destination = target.position + offset;
            transform.position = smoothTime <= 0f
                ? destination
                : Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);
        }

        private void FindPlayerIfNeeded()
        {
            if (target != null) return;
            PlayerCharacter player = FindFirstObjectByType<PlayerCharacter>();
            if (player != null) target = player.transform;
        }

        private void SnapToTarget()
        {
            if (target == null) return;
            transform.position = target.position + offset;
        }
    }
}

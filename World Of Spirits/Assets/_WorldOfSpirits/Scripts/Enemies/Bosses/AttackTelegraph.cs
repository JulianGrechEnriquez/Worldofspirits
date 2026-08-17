using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AttackTelegraph : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer shapeRenderer;
        [SerializeField] private Color warningColor = new Color(1f, 0f, 0f, 0.35f);

        private void Awake()
        {
            if (shapeRenderer == null) shapeRenderer = GetComponent<SpriteRenderer>();
            shapeRenderer.color = warningColor;
            Hide();
        }

        public void ShowLine(Vector2 origin, Vector2 direction, float length, float width)
        {
            direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            transform.position = origin + direction * length * 0.5f;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            transform.localScale = new Vector3(length, width, 1f);
            shapeRenderer.enabled = true;
        }

        public void ShowCircle(Vector2 center, float radius)
        {
            transform.position = center;
            transform.rotation = Quaternion.identity;
            transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
            shapeRenderer.enabled = true;
        }

        public void ShowCone(Vector2 origin, Vector2 direction, float range, float width)
        {
            ShowLine(origin, direction, range, width);
        }

        public void Hide()
        {
            if (shapeRenderer != null) shapeRenderer.enabled = false;
        }

        private void OnDisable() => Hide();
    }
}

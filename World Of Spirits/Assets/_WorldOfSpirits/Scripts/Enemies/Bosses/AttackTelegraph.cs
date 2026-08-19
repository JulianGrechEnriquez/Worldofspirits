using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AttackTelegraph : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer shapeRenderer;
        [SerializeField] private Color warningColor = new Color(1f, 0.08f, 0.02f, 0.72f);
        [SerializeField] private Color imminentColor = new Color(1f, 0.85f, 0.1f, 0.92f);
        [SerializeField, Min(0)] private int sortingOrder = 25;

        private Vector3 shownScale = Vector3.one;

        private void Awake()
        {
            if (shapeRenderer == null) shapeRenderer = GetComponent<SpriteRenderer>();
            shapeRenderer.color = warningColor;
            shapeRenderer.sortingOrder = sortingOrder;
            Hide();
        }

        public void ShowLine(Vector2 origin, Vector2 direction, float length, float width)
        {
            direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            transform.position = origin + direction * length * 0.5f;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            shownScale = new Vector3(length, width, 1f);
            transform.localScale = shownScale;
            shapeRenderer.enabled = true;
        }

        public void ShowCircle(Vector2 center, float radius)
        {
            transform.position = center;
            transform.rotation = Quaternion.identity;
            shownScale = new Vector3(radius * 2f, radius * 2f, 1f);
            transform.localScale = shownScale;
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

        public void SetWarningProgress(float progress)
        {
            if (shapeRenderer == null) return;

            progress = Mathf.Clamp01(progress);
            float pulseSpeed = Mathf.Lerp(3f, 11f, progress);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI);
            Color color = Color.Lerp(warningColor, imminentColor, progress);
            color.a *= Mathf.Lerp(0.7f, 1f, pulse);
            shapeRenderer.color = color;
            transform.localScale = new Vector3(
                shownScale.x,
                shownScale.y * Mathf.Lerp(0.94f, 1.08f, pulse),
                shownScale.z);
        }

        private void OnDisable() => Hide();
    }
}

using UnityEngine;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.Spirits
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AreaPulseVisual : MonoBehaviour, IScenePoolable
    {
        [SerializeField, Min(0.05f)] private float pulseDuration = 0.5f;
        [SerializeField, Range(0f, 1f)] private float startingScale = 0.35f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private SpriteRenderer spriteRenderer;
        private Color authoredColor;
        private Vector3 targetScale;
        private float startTime;
        private bool configured;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            authoredColor = spriteRenderer.color;
        }

        public void Configure(float radius)
        {
            if (spriteRenderer == null) Awake();
            targetScale = Vector3.one * Mathf.Max(0.1f, radius * 2f);
            transform.localScale = targetScale * startingScale;
            spriteRenderer.color = authoredColor;
            startTime = Time.time;
            configured = true;
        }

        private void Update()
        {
            if (!configured) return;

            float progress = Mathf.Clamp01((Time.time - startTime) / pulseDuration);
            float scale = Mathf.Lerp(startingScale, 1f, scaleCurve.Evaluate(progress));
            transform.localScale = targetScale * scale;
            Color color = authoredColor;
            color.a *= 1f - progress;
            spriteRenderer.color = color;

            if (progress >= 1f)
                SceneObjectPool.ReleaseOrDestroy(gameObject);
        }

        public void OnSpawnedFromPool(GameObject prefab)
        {
            AreaPulseVisual source = prefab.GetComponent<AreaPulseVisual>();
            if (source != null)
            {
                pulseDuration = source.pulseDuration;
                startingScale = source.startingScale;
                scaleCurve = source.scaleCurve;
            }
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            authoredColor = spriteRenderer.color;
            configured = false;
        }

        public void OnReturnedToPool()
        {
            configured = false;
            if (spriteRenderer != null) spriteRenderer.color = authoredColor;
        }
    }
}

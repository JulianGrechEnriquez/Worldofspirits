using UnityEngine;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Progression
{
    [DisallowMultipleComponent]
    public sealed class ExperienceOrb : MonoBehaviour, IScenePoolable
    {
        [SerializeField, Min(0.1f)] private float attractionSpeed = 8f;
        [SerializeField, Min(0.1f)] private float maximumAttractionSpeed = 22f;
        [SerializeField, Min(0f)] private float acceleration = 20f;
        [SerializeField] private Color orbColor = new Color(0.2f, 0.95f, 1f, 1f);

        private static Sprite sharedSprite;
        private Transform player;
        private PlayerLevelSystem levelSystem;
        private UpgradeRuntimeStats runtimeStats;
        private float experience;
        private float baseAttractionRadius;
        private float baseCollectionRadius;
        private float currentSpeed;
        private Vector3 baseScale;

        private void Awake()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetOrCreateSprite();
            renderer.color = orbColor;
            renderer.sortingOrder = 50;
            baseScale = Vector3.one * 0.28f;
            transform.localScale = baseScale;
        }

        public void Configure(float amount, Transform target, PlayerLevelSystem levels,
            UpgradeRuntimeStats stats, float attractionRadius, float collectionRadius)
        {
            experience = Mathf.Max(0f, amount);
            player = target;
            levelSystem = levels;
            runtimeStats = stats;
            baseAttractionRadius = Mathf.Max(0.1f, attractionRadius);
            baseCollectionRadius = Mathf.Max(0.05f, collectionRadius);
            currentSpeed = attractionSpeed;
        }

        private void Update()
        {
            if (player == null || levelSystem == null) return;

            float radiusMultiplier = runtimeStats != null
                ? runtimeStats.GetMultiplier(UpgradeStat.PickupRadius) : 1f;
            float collectionRadius = baseCollectionRadius * radiusMultiplier;
            Vector3 difference = player.position - transform.position;
            float distanceSquared = difference.sqrMagnitude;

            if (distanceSquared <= collectionRadius * collectionRadius)
            {
                levelSystem.AddExperience(experience);
                SceneObjectPool.TryRelease(gameObject);
                return;
            }

            float attractionRadius = baseAttractionRadius * radiusMultiplier;
            if (distanceSquared <= attractionRadius * attractionRadius)
            {
                currentSpeed = Mathf.Min(maximumAttractionSpeed, currentSpeed + acceleration * Time.deltaTime);
                transform.position = Vector3.MoveTowards(
                    transform.position, player.position, currentSpeed * Time.deltaTime);
            }

            float pulse = 1f + Mathf.Sin(Time.time * 6f + GetInstanceID()) * 0.08f;
            transform.localScale = baseScale * pulse;
        }

        public void OnSpawnedFromPool(GameObject prefab)
        {
            experience = 0f;
            player = null;
            levelSystem = null;
            runtimeStats = null;
            currentSpeed = attractionSpeed;
            transform.localScale = baseScale;
        }

        public void OnReturnedToPool()
        {
            experience = 0f;
            player = null;
            levelSystem = null;
            runtimeStats = null;
            transform.localScale = baseScale;
        }

        private static Sprite GetOrCreateSprite()
        {
            if (sharedSprite != null) return sharedSprite;

            const int size = 16;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime XP Orb",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.43f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance + 1f);
                float highlight = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), center + new Vector2(-2f, 2f)) / radius);
                pixels[y * size + x] = new Color(0.45f + highlight * 0.45f, 0.9f, 1f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), Vector2.one * 0.5f, 16f);
            sharedSprite.name = "Runtime XP Orb Sprite";
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }
    }
}

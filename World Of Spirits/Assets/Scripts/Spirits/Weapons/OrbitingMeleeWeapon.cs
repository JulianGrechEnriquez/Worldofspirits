using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public class OrbitingMeleeWeapon : MonoBehaviour
    {
        [Header("Owner")]
        [SerializeField] private SpiritManager spiritManager;
        [SerializeField] private string requiredPrimarySpirit = "Earth";
        [SerializeField] private bool onlyWhileStandingStill = true;

        [Header("Orbit")]
        [SerializeField, Min(0.1f)] private float orbitRadius = 1.4f;
        [SerializeField] private float orbitSpeed = 180f;
        [Tooltip("Adjust this if the hammer artwork's handle does not face inward.")]
        [SerializeField] private float rotationOffset;
        [SerializeField] private float startingAngle;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.05f)] private float hitCooldownPerEnemy = 0.5f;
        [SerializeField] private Faction ownerFaction = Faction.Player;

        private float currentAngle;
        private Renderer[] weaponRenderers;
        private bool isVisible;
        private Collider2D weaponCollider;
        private readonly Dictionary<int, float> nextHitTimes = new Dictionary<int, float>();

        private void Awake()
        {
            if (spiritManager == null)
            {
                spiritManager = FindFirstObjectByType<SpiritManager>();
            }

            currentAngle = startingAngle;
            weaponRenderers = GetComponentsInChildren<Renderer>(true);
            isVisible = true;
            ConfigureCollider();
        }

        private void Update()
        {
            bool correctPrimary = spiritManager != null &&
                spiritManager.IsPrimarySpirit(requiredPrimarySpirit);
            bool movementAllowed = spiritManager != null &&
                (!onlyWhileStandingStill || !spiritManager.PlayerIsMoving);
            bool shouldBeActive = correctPrimary && movementAllowed;

            SetWeaponVisible(shouldBeActive);
            if (!shouldBeActive)
            {
                return;
            }

            currentAngle = Mathf.Repeat(currentAngle + orbitSpeed * Time.deltaTime, 360f);
            float radians = currentAngle * Mathf.Deg2Rad;
            Vector2 outwardDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            transform.position = spiritManager.transform.position + (Vector3)(outwardDirection * orbitRadius);

            // The sprite's local down direction is the handle direction. Aligning
            // local up outward therefore keeps the handle pointed at the player.
            float facingAngle = Mathf.Atan2(outwardDirection.y, outwardDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, facingAngle + rotationOffset);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void TryDamage(Collider2D other)
        {
            if (!isVisible || damage <= 0f)
            {
                return;
            }

            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || target.Faction == ownerFaction)
            {
                return;
            }

            int targetId = target.Transform.gameObject.GetInstanceID();
            if (nextHitTimes.TryGetValue(targetId, out float nextHitTime) && Time.time < nextHitTime)
            {
                return;
            }

            target.TakeDamage(damage);
            nextHitTimes[targetId] = Time.time + hitCooldownPerEnemy;
        }

        private void ConfigureCollider()
        {
            weaponCollider = GetComponent<Collider2D>();
            if (weaponCollider == null)
            {
                BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
                SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    box.size = spriteRenderer.sprite.bounds.size;
                }

                weaponCollider = box;
            }

            weaponCollider.isTrigger = true;
        }

        private void SetWeaponVisible(bool visible)
        {
            if (isVisible == visible)
            {
                return;
            }

            isVisible = visible;
            if (weaponCollider != null)
            {
                weaponCollider.enabled = visible;
            }

            foreach (Renderer weaponRenderer in weaponRenderers)
            {
                weaponRenderer.enabled = visible;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Transform center = spiritManager != null ? spiritManager.transform : null;
            if (center == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center.position, orbitRadius);
            Gizmos.DrawLine(transform.position, center.position);
        }
    }
}

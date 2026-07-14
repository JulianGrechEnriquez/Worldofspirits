using System;
using System.Collections;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public abstract class LivingEntity : MonoBehaviour, IDamageable, IStatusEffectReceiver
    {
        [Header("Core Stats")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        [Header("Debug")]
        [SerializeField] private bool logHealthChanges;

        [Header("Hit Feedback")]
        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.12f;

        private float currentHealth;
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalSpriteColors;
        private Coroutine hitFlashRoutine;
        private float statusEndTime;
        private float statusStrength;
        private CombatStatus activeStatus;
        private float nextStatusDamageTime;

        public abstract Faction Faction { get; }
        public bool IsAlive => currentHealth > 0f;
        public Transform Transform => transform;
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float MoveSpeed => IsMovementDisabled ? 0f : moveSpeed * MovementMultiplier;
        public bool IsMovementDisabled => Time.time < statusEndTime &&
            (activeStatus == CombatStatus.Freeze || activeStatus == CombatStatus.Stun);
        private float MovementMultiplier => Time.time < statusEndTime && activeStatus == CombatStatus.Slow
            ? Mathf.Clamp01(1f - statusStrength) : 1f;

        public event Action<float, float> HealthChanged;
        public event Action<float> Damaged;
        public event Action Died;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            CacheSpriteColors();
        }

        protected virtual void Update()
        {
            if (Time.time >= statusEndTime || Time.time < nextStatusDamageTime)
            {
                return;
            }

            if (activeStatus == CombatStatus.Burn || activeStatus == CombatStatus.Poison || activeStatus == CombatStatus.Bleed)
            {
                TakeDamage(statusStrength);
                nextStatusDamageTime = Time.time + 0.5f;
            }
        }

        public void ApplyStatus(CombatStatus status, float duration, float strength)
        {
            activeStatus = status;
            statusEndTime = Mathf.Max(statusEndTime, Time.time + Mathf.Max(0f, duration));
            statusStrength = Mathf.Max(0f, strength);
            nextStatusDamageTime = Time.time;
        }

        public virtual void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Damaged?.Invoke(amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            PlayHitFlash();

            if (logHealthChanges)
            {
                Debug.Log($"[{name}] Took {amount:0.##} damage. HP: {currentHealth:0.##}/{maxHealth:0.##}", this);
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public virtual void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (logHealthChanges)
            {
                Debug.Log($"[{name}] Healed {amount:0.##}. HP: {currentHealth:0.##}/{maxHealth:0.##}", this);
            }
        }

        protected virtual void Die()
        {
            if (logHealthChanges)
            {
                Debug.Log($"[{name}] Died.", this);
            }

            Died?.Invoke();
            Destroy(gameObject);
        }

        protected virtual void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            moveSpeed = Mathf.Max(0f, moveSpeed);
        }

        private void CacheSpriteColors()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            originalSpriteColors = new Color[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalSpriteColors[i] = spriteRenderers[i].color;
            }
        }

        private void PlayHitFlash()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                CacheSpriteColors();
            }

            if (hitFlashRoutine != null)
            {
                StopCoroutine(hitFlashRoutine);
                RestoreSpriteColors();
            }

            hitFlashRoutine = StartCoroutine(HitFlashCoroutine());
        }

        private IEnumerator HitFlashCoroutine()
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = hitFlashColor;
                }
            }

            yield return new WaitForSeconds(hitFlashDuration);
            RestoreSpriteColors();
            hitFlashRoutine = null;
        }

        private void RestoreSpriteColors()
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = originalSpriteColors[i];
                }
            }
        }
    }
}

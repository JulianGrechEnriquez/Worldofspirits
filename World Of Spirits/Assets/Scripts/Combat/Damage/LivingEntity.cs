using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.Combat
{
    public abstract class LivingEntity : MonoBehaviour, IDamageable, IStatusEffectReceiver, IScenePoolable
    {
        private static readonly List<LivingEntity> activeEntities = new List<LivingEntity>(128);
        public static IReadOnlyList<LivingEntity> ActiveEntities => activeEntities;

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
        private float currentShield;
        private float shieldEndTime;
        private int reviveCharges;

        public abstract Faction Faction { get; }
        public bool IsAlive => currentHealth > 0f;
        public Transform Transform => transform;
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float CurrentShield => Time.time < shieldEndTime ? currentShield : 0f;
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

        protected virtual void OnEnable()
        {
            if (!activeEntities.Contains(this))
            {
                activeEntities.Add(this);
            }
            CombatSimulationManager.Instance.Register(this);
        }

        protected virtual void OnDisable()
        {
            activeEntities.Remove(this);
            if (CombatSimulationManager.TryGetExisting(out CombatSimulationManager simulation))
            {
                simulation.Unregister(this);
            }
        }

        internal void TickStatusEffects(float now)
        {
            if (now >= statusEndTime || now < nextStatusDamageTime)
            {
                return;
            }

            if (activeStatus == CombatStatus.Burn || activeStatus == CombatStatus.Poison || activeStatus == CombatStatus.Bleed)
            {
                TakeDamage(statusStrength);
                nextStatusDamageTime = now + 0.5f;
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

            if (Time.time < shieldEndTime && currentShield > 0f)
            {
                float absorbed = Mathf.Min(currentShield, amount);
                currentShield -= absorbed;
                amount -= absorbed;
            }

            if (amount <= 0f) return;
            if (Time.time < statusEndTime && activeStatus == CombatStatus.ArmorBreak)
                amount *= 1f + statusStrength;

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

        public void AddShield(float amount, float duration)
        {
            if (!IsAlive || amount <= 0f) return;
            currentShield += amount;
            shieldEndTime = Mathf.Max(shieldEndTime, Time.time + Mathf.Max(0.1f, duration));
        }

        public void GrantRevive(int charges = 1)
        {
            reviveCharges += Mathf.Max(0, charges);
        }

        public void IncreaseMaximumHealth(float amount, bool healIncrease = true)
        {
            if (amount <= 0f) return;
            maxHealth += amount;
            if (healIncrease) currentHealth += amount;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        protected virtual void Die()
        {
            if (reviveCharges > 0)
            {
                reviveCharges--;
                currentHealth = Mathf.Max(1f, maxHealth * 0.5f);
                HealthChanged?.Invoke(currentHealth, maxHealth);
                return;
            }

            if (logHealthChanges)
            {
                Debug.Log($"[{name}] Died.", this);
            }

            Died?.Invoke();
            SceneObjectPool.ReleaseOrDestroy(gameObject);
        }

        public virtual void OnSpawnedFromPool(GameObject prefab)
        {
            currentHealth = maxHealth;
            currentShield = 0f;
            shieldEndTime = 0f;
            statusEndTime = 0f;
            statusStrength = 0f;
            nextStatusDamageTime = 0f;
            activeStatus = default;
            if (spriteRenderers == null)
            {
                CacheSpriteColors();
            }
            else
            {
                RestoreSpriteColors();
            }

            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public virtual void OnReturnedToPool()
        {
            if (hitFlashRoutine != null)
            {
                StopCoroutine(hitFlashRoutine);
                hitFlashRoutine = null;
            }

            RestoreSpriteColors();
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

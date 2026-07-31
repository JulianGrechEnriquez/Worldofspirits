using UnityEngine;
using WorldOfSpirits.Combat;
using System.Collections.Generic;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.UI
{
    [RequireComponent(typeof(LivingEntity))]
    public class DamageNumberEmitter : MonoBehaviour
    {
        private static readonly Stack<DamageNumber> pool = new Stack<DamageNumber>(64);
        private static int createdCount;
        private static int activeCount;
        [SerializeField, Min(0)] private int maximumVisibleDamageNumbers = 80;
        [SerializeField, Min(0f)] private float minimumDisplayInterval = 0.06f;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.75f, 0f);
        [SerializeField] private Color damageColor = new Color(1f, 0.25f, 0.15f, 1f);
        [SerializeField, Min(0.1f)] private float duration = 0.8f;
        [SerializeField, Min(0f)] private float riseSpeed = 1.2f;
        [SerializeField, Min(1)] private int fontSize = 5;
        [SerializeField, Min(0f)] private float randomHorizontalOffset = 0.2f;

        private LivingEntity entity;
        private float pendingDamage;
        private float displayPendingAt;
        private bool hasPendingDamage;

        private void Awake()
        {
            entity = GetComponent<LivingEntity>();
        }

        private void OnEnable()
        {
            if (entity == null)
            {
                entity = GetComponent<LivingEntity>();
            }

            entity.Damaged += ShowDamage;
        }

        private void OnDisable()
        {
            if (entity != null)
            {
                entity.Damaged -= ShowDamage;
            }

            pendingDamage = 0f;
            hasPendingDamage = false;
        }

        private void ShowDamage(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            pendingDamage += amount;
            if (!hasPendingDamage)
            {
                hasPendingDamage = true;
                displayPendingAt = Time.time + minimumDisplayInterval;
            }
        }

        private void Update()
        {
            if (!hasPendingDamage || Time.time < displayPendingAt)
            {
                return;
            }

            float amount = pendingDamage;
            pendingDamage = 0f;
            hasPendingDamage = false;

            if (activeCount >= maximumVisibleDamageNumbers)
            {
                return;
            }

            Vector3 randomOffset = Vector3.right * Random.Range(-randomHorizontalOffset, randomHorizontalOffset);
            DamageNumber number = null;
            while (pool.Count > 0 && number == null)
            {
                number = pool.Pop();
            }

            if (number == null)
            {
                GameObject newNumberObject = new GameObject("Damage Number");
                createdCount++;
                newNumberObject.name = $"Damage Number [Pooled {createdCount:000}]";
                newNumberObject.transform.SetParent(
                    SceneObjectPool.GetCategoryParent(PoolCategory.CombatUI), false);
                newNumberObject.AddComponent<TMPro.TextMeshPro>();
                number = newNumberObject.AddComponent<DamageNumber>();
            }

            GameObject numberObject = number.gameObject;
            numberObject.transform.SetParent(
                SceneObjectPool.GetCategoryParent(PoolCategory.CombatUI), false);
            numberObject.transform.position = transform.position + worldOffset + randomOffset;
            numberObject.SetActive(true);
            activeCount++;
            number.Initialize(amount, damageColor, duration, riseSpeed, fontSize, ReturnToPool);
        }

        private static void ReturnToPool(DamageNumber number)
        {
            activeCount = Mathf.Max(0, activeCount - 1);
            number.gameObject.SetActive(false);
            number.transform.SetParent(
                SceneObjectPool.GetCategoryParent(PoolCategory.CombatUI), false);
            pool.Push(number);
        }
    }
}

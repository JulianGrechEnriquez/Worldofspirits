using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.UI
{
    [RequireComponent(typeof(LivingEntity))]
    public class DamageNumberEmitter : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.75f, 0f);
        [SerializeField] private Color damageColor = new Color(1f, 0.25f, 0.15f, 1f);
        [SerializeField, Min(0.1f)] private float duration = 0.8f;
        [SerializeField, Min(0f)] private float riseSpeed = 1.2f;
        [SerializeField, Min(1)] private int fontSize = 5;
        [SerializeField, Min(0f)] private float randomHorizontalOffset = 0.2f;

        private LivingEntity entity;

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
        }

        private void ShowDamage(float amount)
        {
            Vector3 randomOffset = Vector3.right * Random.Range(-randomHorizontalOffset, randomHorizontalOffset);
            GameObject numberObject = new GameObject("Damage Number");
            numberObject.transform.position = transform.position + worldOffset + randomOffset;

            numberObject.AddComponent<TMPro.TextMeshPro>();
            DamageNumber number = numberObject.AddComponent<DamageNumber>();
            number.Initialize(amount, damageColor, duration, riseSpeed, fontSize);
        }
    }
}

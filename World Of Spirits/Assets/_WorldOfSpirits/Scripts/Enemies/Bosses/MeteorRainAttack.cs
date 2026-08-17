using System.Collections;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Enemies
{
    public sealed class MeteorRainAttack : BossAttackBase
    {
        [SerializeField] private AttackTelegraph telegraphPrefab;
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField, Min(1)] private int meteorCount = 5;
        [SerializeField, Min(0f)] private float scatterRadius = 4f;
        [SerializeField, Min(0.1f)] private float impactRadius = 1.25f;
        [SerializeField, Min(0f)] private float warningDuration = 1f;
        [SerializeField, Min(0f)] private float damage = 18f;
        [Header("Falling Visual")]
        [SerializeField, Min(0.1f)] private float fallHeight = 8f;
        [SerializeField] private Vector2 fallDirection = new Vector2(0.25f, -1f);
        [Tooltip("Corrects meteor artwork that does not face right by default.")]
        [SerializeField] private float fallRotationOffset;
        [SerializeField, Min(0f)] private float impactVisualDuration = 0.35f;

        private Vector2[] positions;
        private AttackTelegraph[] warnings;
        private GameObject[] meteorVisuals;
        private Vector3[] meteorStartPositions;

        private void Awake()
        {
            positions = new Vector2[meteorCount];
            warnings = new AttackTelegraph[meteorCount];
            meteorVisuals = new GameObject[meteorCount];
            meteorStartPositions = new Vector3[meteorCount];
        }

        public override IEnumerator Execute(BossContext context)
        {
            context.Movement.TakeAttackControl();
            EnsureCapacity();
            Vector2 lockedCenter = context.Target.position;
            for (int i = 0; i < meteorCount; i++)
            {
                positions[i] = lockedCenter + Random.insideUnitCircle * scatterRadius;
                if (telegraphPrefab != null)
                {
                    warnings[i] = Instantiate(telegraphPrefab, positions[i], Quaternion.identity);
                    warnings[i].ShowCircle(positions[i], impactRadius);
                }

                if (impactEffectPrefab != null)
                {
                    Vector2 direction = fallDirection.sqrMagnitude > 0.001f
                        ? fallDirection.normalized : Vector2.down;
                    meteorStartPositions[i] = positions[i] - direction * fallHeight;
                    float fallAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg +
                        fallRotationOffset;
                    meteorVisuals[i] = Instantiate(
                        impactEffectPrefab,
                        meteorStartPositions[i],
                        Quaternion.Euler(0f, 0f, fallAngle));
                    Animator meteorAnimator = meteorVisuals[i].GetComponent<Animator>();
                    if (meteorAnimator != null)
                    {
                        meteorAnimator.enabled = true;
                        meteorAnimator.Rebind();
                        meteorAnimator.Update(0f);
                    }
                }
            }

            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, warningDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                for (int i = 0; i < meteorCount; i++)
                {
                    if (meteorVisuals[i] != null)
                        meteorVisuals[i].transform.position = Vector3.Lerp(
                            meteorStartPositions[i], positions[i], t);
                }
                yield return null;
            }

            for (int i = 0; i < meteorCount; i++)
            {
                if (warnings[i] != null) Destroy(warnings[i].gameObject);
                if (meteorVisuals[i] != null)
                {
                    meteorVisuals[i].transform.position = positions[i];
                    Destroy(meteorVisuals[i], impactVisualDuration);
                    meteorVisuals[i] = null;
                }
                DamageAt(positions[i], context.Boss);
            }
            context.Movement.ReleaseAttackControl();
        }

        private void DamageAt(Vector2 position, BossEnemyBase boss)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, impactRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                IDamageable target = hits[i].GetComponentInParent<IDamageable>();
                if (target != null && target.IsAlive && target.Faction != Faction.Enemy)
                {
                    target.TakeDamage(new DamageContext(damage, boss.transform, DamageElement.Fire));
                    break;
                }
            }
        }

        private void EnsureCapacity()
        {
            if (positions == null || positions.Length != meteorCount) positions = new Vector2[meteorCount];
            if (warnings == null || warnings.Length != meteorCount) warnings = new AttackTelegraph[meteorCount];
            if (meteorVisuals == null || meteorVisuals.Length != meteorCount)
                meteorVisuals = new GameObject[meteorCount];
            if (meteorStartPositions == null || meteorStartPositions.Length != meteorCount)
                meteorStartPositions = new Vector3[meteorCount];
        }

        public override void Cancel()
        {
            if (warnings == null) return;
            for (int i = 0; i < warnings.Length; i++)
                if (warnings[i] != null) Destroy(warnings[i].gameObject);
            if (meteorVisuals == null) return;
            for (int i = 0; i < meteorVisuals.Length; i++)
            {
                if (meteorVisuals[i] != null) Destroy(meteorVisuals[i]);
                meteorVisuals[i] = null;
            }
        }
    }
}

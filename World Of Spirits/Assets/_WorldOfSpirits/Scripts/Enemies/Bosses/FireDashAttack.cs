using System.Collections;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Enemies
{
    public sealed class FireDashAttack : BossAttackBase
    {
        [SerializeField] private AttackTelegraph telegraph;
        [SerializeField, Min(0.1f)] private float telegraphDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float dashSpeed = 16f;
        [SerializeField, Min(0.1f)] private float dashDistance = 10f;
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.1f)] private float hitRadius = 1f;
        [SerializeField, Min(0.1f)] private float warningWidth = 1.5f;
        [Header("Phase Escalation")]
        [SerializeField] private BossDamageZone fireTrailPrefab;
        [SerializeField, Min(0f)] private float fireTrailDamage = 7f;
        [SerializeField, Min(0.1f)] private float fireTrailLifetime = 2.5f;
        [SerializeField, Min(0.1f)] private float fireTrailSpacing = 0.8f;
        [SerializeField, Min(0f)] private float chainedDashPause = 0.22f;

        private AttackTelegraph activeTelegraph;

        public override IEnumerator Execute(BossContext context)
        {
            context.Movement.TakeAttackControl();
            int dashCount = context.Boss.CurrentPhase >= 3 ? 3 :
                context.Boss.CurrentPhase >= 2 ? 2 : 1;
            for (int dashIndex = 0; dashIndex < dashCount; dashIndex++)
            {
                Vector2 aimPoint = context.Target.position;
                if (dashIndex > 0)
                {
                    Rigidbody2D targetBody = context.Target.GetComponentInParent<Rigidbody2D>();
                    if (targetBody != null) aimPoint += targetBody.linearVelocity * 0.25f;
                }
                Vector2 direction = (aimPoint - (Vector2)transform.position).normalized;
                if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
                yield return TelegraphAndDash(context, direction);
                if (dashIndex + 1 < dashCount) yield return Wait(chainedDashPause);
            }
            context.Movement.ReleaseAttackControl();
        }

        private IEnumerator TelegraphAndDash(BossContext context, Vector2 direction)
        {
            if (telegraph != null)
            {
                activeTelegraph = Instantiate(telegraph, transform.position, Quaternion.identity);
                activeTelegraph.name = $"{name} Dash Warning";
                activeTelegraph.ShowLine(transform.position, direction, dashDistance, warningWidth);
            }

            float warningElapsed = 0f;
            float phaseWarning = telegraphDuration * Mathf.Max(0.62f, 1f - context.Boss.CurrentPhase * 0.16f);
            while (warningElapsed < phaseWarning)
            {
                warningElapsed += Time.deltaTime;
                if (activeTelegraph != null)
                    activeTelegraph.SetWarningProgress(warningElapsed / phaseWarning);
                yield return null;
            }
            ClearTelegraph();
            context.Movement.SetAttackVelocity(direction * (dashSpeed + context.Boss.CurrentPhase * 1.5f));

            float end = Time.time + dashDistance / (dashSpeed + context.Boss.CurrentPhase * 1.5f);
            bool hit = false;
            Vector2 lastTrailPosition = transform.position;
            IDamageable target = context.Target.GetComponentInParent<IDamageable>();
            while (Time.time < end)
            {
                if (fireTrailPrefab != null && context.Boss.CurrentPhase > 0 &&
                    ((Vector2)transform.position - lastTrailPosition).sqrMagnitude >= fireTrailSpacing * fireTrailSpacing)
                {
                    BossDamageZone trail = Instantiate(fireTrailPrefab, transform.position, Quaternion.identity);
                    trail.Activate(context.Boss.transform, fireTrailDamage, fireTrailLifetime, Vector2.zero);
                    Destroy(trail.gameObject, fireTrailLifetime + 0.1f);
                    lastTrailPosition = transform.position;
                }
                if (!hit && target != null && target.IsAlive &&
                    (context.Target.position - transform.position).sqrMagnitude <= hitRadius * hitRadius)
                {
                    target.TakeDamage(new DamageContext(damage, transform, DamageElement.Fire));
                    hit = true;
                }
                yield return new WaitForFixedUpdate();
            }
            context.Movement.SetAttackVelocity(Vector2.zero);
        }

        public override void Cancel()
        {
            ClearTelegraph();
        }

        private void ClearTelegraph()
        {
            if (activeTelegraph == null) return;
            Destroy(activeTelegraph.gameObject);
            activeTelegraph = null;
        }

        private void OnDisable()
        {
            ClearTelegraph();
        }
    }
}

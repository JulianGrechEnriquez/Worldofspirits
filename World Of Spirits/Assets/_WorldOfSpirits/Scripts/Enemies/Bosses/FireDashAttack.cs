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

        public override IEnumerator Execute(BossContext context)
        {
            Vector2 direction = (context.Target.position - transform.position).normalized;
            if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
            context.Movement.TakeAttackControl();
            if (telegraph != null) telegraph.ShowLine(transform.position, direction, dashDistance, warningWidth);
            yield return Wait(telegraphDuration);
            if (telegraph != null) telegraph.Hide();
            context.Movement.SetAttackVelocity(direction * dashSpeed);

            float end = Time.time + dashDistance / dashSpeed;
            bool hit = false;
            IDamageable target = context.Target.GetComponentInParent<IDamageable>();
            while (Time.time < end)
            {
                if (!hit && target != null && target.IsAlive &&
                    (context.Target.position - transform.position).sqrMagnitude <= hitRadius * hitRadius)
                {
                    target.TakeDamage(new DamageContext(damage, transform, DamageElement.Fire));
                    hit = true;
                }
                yield return new WaitForFixedUpdate();
            }
            context.Movement.ReleaseAttackControl();
        }

        public override void Cancel() { if (telegraph != null) telegraph.Hide(); }
    }
}

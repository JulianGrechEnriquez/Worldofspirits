using System.Collections;
using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    public sealed class FlameTornadoAttack : BossAttackBase
    {
        [SerializeField] private AttackTelegraph spawnTelegraph;
        [SerializeField] private BossDamageZone tornadoPrefab;
        [SerializeField, Min(1)] private int tornadoCount = 1;
        [SerializeField, Min(0f)] private float warningDuration = 0.7f;
        [SerializeField, Min(0f)] private float damage = 15f;
        [SerializeField, Min(0.1f)] private float lifetime = 4f;
        [SerializeField, Min(0f)] private float movementSpeed = 3f;
        [SerializeField, Min(0.1f)] private float warningRadius = 1.25f;

        public override IEnumerator Execute(BossContext context)
        {
            context.Movement.TakeAttackControl();
            Vector2 spawnPosition = context.Target.position;
            AttackTelegraph warning = null;
            if (spawnTelegraph != null)
            {
                warning = Instantiate(spawnTelegraph, spawnPosition, Quaternion.identity);
                warning.ShowCircle(spawnPosition, warningRadius);
            }
            yield return Wait(warningDuration * Mathf.Max(0.7f, 1f - context.Boss.CurrentPhase * 0.1f));
            if (warning != null) Destroy(warning.gameObject);

            int activeCount = tornadoCount + context.Boss.CurrentPhase;
            for (int i = 0; i < activeCount; i++)
            {
                if (tornadoPrefab == null) break;
                float angle = activeCount == 1 ? Random.Range(0f, 360f) : i * 360f / activeCount;
                Vector2 offset = Quaternion.Euler(0f, 0f, angle) * Vector2.right * (i == 0 ? 0f : 1.25f);
                Vector2 direction = ((Vector2)context.Target.position - (spawnPosition + offset)).normalized;
                direction = Quaternion.Euler(0f, 0f, Mathf.Lerp(-35f, 35f, activeCount == 1 ? 0.5f : i / (float)(activeCount - 1))) * direction;
                BossDamageZone tornado = Instantiate(tornadoPrefab, spawnPosition + offset, Quaternion.identity);
                tornado.Activate(transform, damage, lifetime, direction * movementSpeed);
                Destroy(tornado.gameObject, lifetime + 0.1f);
            }
            context.Movement.ReleaseAttackControl();
        }

        public override void Cancel() { }
    }
}

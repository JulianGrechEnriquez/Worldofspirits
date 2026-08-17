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
            if (spawnTelegraph != null) spawnTelegraph.ShowCircle(spawnPosition, warningRadius);
            yield return Wait(warningDuration);
            if (spawnTelegraph != null) spawnTelegraph.Hide();

            for (int i = 0; i < tornadoCount; i++)
            {
                if (tornadoPrefab == null) break;
                float angle = tornadoCount == 1 ? Random.Range(0f, 360f) : i * 360f / tornadoCount;
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
                BossDamageZone tornado = Instantiate(tornadoPrefab, spawnPosition, Quaternion.identity);
                tornado.Activate(transform, damage, lifetime, direction * movementSpeed);
                Destroy(tornado.gameObject, lifetime + 0.1f);
            }
            context.Movement.ReleaseAttackControl();
        }

        public override void Cancel() { if (spawnTelegraph != null) spawnTelegraph.Hide(); }
    }
}

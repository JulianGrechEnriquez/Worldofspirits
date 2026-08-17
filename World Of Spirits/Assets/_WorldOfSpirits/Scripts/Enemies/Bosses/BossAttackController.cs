using System.Collections;
using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    [DisallowMultipleComponent]
    public sealed class BossAttackController : MonoBehaviour
    {
        [SerializeField] private BossEnemyBase boss;
        [SerializeField] private BossMovement movement;
        [SerializeField] private BossAttackBase[] attacks;
        [SerializeField, Min(0f)] private float initialDelay = 1.5f;

        private Coroutine loop;
        private int previousAttack = -1;
        private Transform target;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<BossEnemyBase>();
            if (movement == null) movement = GetComponent<BossMovement>();
            if (attacks == null || attacks.Length == 0)
                attacks = GetComponents<BossAttackBase>();
        }

        public void Begin(Transform playerTarget)
        {
            target = playerTarget;
            StopLoop();
            if (boss != null && target != null && attacks.Length > 0)
                loop = StartCoroutine(AttackLoop());
        }

        public void StopLoop()
        {
            if (loop != null) StopCoroutine(loop);
            loop = null;
            for (int i = 0; i < attacks.Length; i++)
                if (attacks[i] != null) attacks[i].Cancel();
            if (movement != null) movement.ReleaseAttackControl();
        }

        private IEnumerator AttackLoop()
        {
            yield return new WaitForSeconds(initialDelay);
            while (boss != null && boss.IsAlive && target != null)
            {
                BossContext context = new BossContext(boss, target, movement);
                int selected = SelectAttack(context);
                if (selected < 0)
                {
                    yield return null;
                    continue;
                }

                BossAttackBase attack = attacks[selected];
                previousAttack = selected;
                yield return attack.Execute(context);
                if (movement != null) movement.ReleaseAttackControl();
                float cooldown = attack.RecoveryDuration +
                    (boss.Data != null ? boss.Data.AttackCooldown : 0f);
                if (cooldown > 0f) yield return new WaitForSeconds(cooldown);
            }
            loop = null;
        }

        private int SelectAttack(BossContext context)
        {
            float total = 0f;
            for (int i = 0; i < attacks.Length; i++)
                if (i != previousAttack && attacks[i] != null && attacks[i].CanExecute(context))
                    total += attacks[i].Weight;

            if (total <= 0f)
            {
                previousAttack = -1;
                for (int i = 0; i < attacks.Length; i++)
                    if (attacks[i] != null && attacks[i].CanExecute(context)) total += attacks[i].Weight;
            }
            if (total <= 0f) return -1;

            float roll = Random.value * total;
            for (int i = 0; i < attacks.Length; i++)
            {
                if (i == previousAttack || attacks[i] == null || !attacks[i].CanExecute(context)) continue;
                roll -= attacks[i].Weight;
                if (roll <= 0f) return i;
            }
            for (int i = 0; i < attacks.Length; i++)
                if (attacks[i] != null && attacks[i].CanExecute(context)) return i;
            return -1;
        }

        private void OnDisable() => StopLoop();
    }
}

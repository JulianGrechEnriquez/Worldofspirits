using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace WorldOfSpirits.Enemies
{
    /// <summary>Fire Phoenix identity and one-time rebirth; attacks are separate components.</summary>
    [RequireComponent(typeof(BossAttackController), typeof(BossMovement))]
    public sealed class FirePhoenixBoss : BossEnemyBase
    {
        [Header("Configuration")]
        [SerializeField] private BossData bossData;
        [SerializeField, Range(0.01f, 1f)] private float rebirthHealthFraction = 0.5f;
        [SerializeField, Min(0.1f)] private float rebirthDelay = 2f;
        [SerializeField] private UnityEvent rebirthStarted;
        [Header("Rebirth Arena Pressure")]
        [SerializeField] private BossDamageZone rebirthFlamePrefab;
        [SerializeField, Min(3)] private int rebirthFlameCount = 12;
        [SerializeField, Min(0.5f)] private float rebirthRingRadius = 4f;
        [SerializeField, Min(0f)] private float rebirthFlameDamage = 6f;
        [SerializeField, Min(0.1f)] private float rebirthFlameLifetime = 3f;

        private BossAttackController attackController;
        private BossMovement movement;
        private bool behaviourStarted;
        private bool rebirthUsed;

        protected override void Awake()
        {
            base.Awake();
            attackController = GetComponent<BossAttackController>();
            movement = GetComponent<BossMovement>();
            if (bossData != null) Initialize(bossData);
        }

        protected override void UpdateBoss(Transform playerTarget)
        {
            if (behaviourStarted) return;
            behaviourStarted = true;
            movement.SetTarget(playerTarget);
            attackController.Begin(playerTarget);
        }

        protected override bool IsPerformingMovementAttack =>
            movement != null && movement.HasAttackControl && Body.linearVelocity.sqrMagnitude > 0.01f;

        protected override bool TryPreventDeath()
        {
            if (rebirthUsed) return false;
            rebirthUsed = true;
            StartCoroutine(RebirthRoutine());
            return true;
        }

        private IEnumerator RebirthRoutine()
        {
            attackController.StopLoop();
            movement.TakeAttackControl();
            SetInvulnerable(true);
            rebirthStarted?.Invoke();
            SpawnRebirthRing();
            yield return new WaitForSeconds(rebirthDelay);
            RestoreHealthFraction(rebirthHealthFraction);
            SetInvulnerable(false);
            movement.ReleaseAttackControl();
            behaviourStarted = false;
        }

        private void SpawnRebirthRing()
        {
            if (rebirthFlamePrefab == null) return;
            for (int i = 0; i < rebirthFlameCount; i++)
            {
                float angle = i * 360f / rebirthFlameCount;
                Vector2 offset = Quaternion.Euler(0f, 0f, angle) * Vector2.right * rebirthRingRadius;
                BossDamageZone flame = Instantiate(
                    rebirthFlamePrefab, (Vector2)transform.position + offset, Quaternion.Euler(0f, 0f, angle));
                flame.Activate(transform, rebirthFlameDamage, rebirthFlameLifetime, Vector2.zero);
                Destroy(flame.gameObject, rebirthFlameLifetime + 0.1f);
            }
        }

        protected override void ResetBossState()
        {
            StopAllCoroutines();
            if (attackController != null) attackController.StopLoop();
            rebirthUsed = false;
            behaviourStarted = false;
            SetInvulnerable(false);
        }
    }
}

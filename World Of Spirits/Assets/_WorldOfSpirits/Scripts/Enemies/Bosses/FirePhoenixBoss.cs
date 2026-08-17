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
            yield return new WaitForSeconds(rebirthDelay);
            RestoreHealthFraction(rebirthHealthFraction);
            SetInvulnerable(false);
            movement.ReleaseAttackControl();
            behaviourStarted = false;
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

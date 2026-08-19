using System;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Enemies
{
    /// <summary>
    /// Shared base for every boss. Bosses are normal EnemyBase instances so
    /// SpawnDirector can pool, classify, and track them like every other enemy.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class BossEnemyBase : EnemyBase, IBoss
    {
        [Header("Boss Phases")]
        [Tooltip("Health fractions at which a new phase begins, in descending order.")]
        [SerializeField] private float[] phaseThresholds = { 0.66f, 0.33f };
        [SerializeField] private bool faceTarget = true;
        [SerializeField] private SpriteRenderer bossRenderer;

        private int currentPhase;
        private BossData runtimeData;
        private bool invulnerable;

        public int CurrentPhase => currentPhase;
        public BossData Data => runtimeData;
        public string BossName => runtimeData != null ? runtimeData.BossName : name;
        public int PhaseCount => phaseThresholds.Length + 1;
        public event Action<int> PhaseStarted;
        public event Action BossDefeated;
        public event Action<BossEnemyBase> BossReady;

        protected override void Awake()
        {
            base.Awake();
            if (bossRenderer == null) bossRenderer = GetComponentInChildren<SpriteRenderer>();
            HealthChanged += CheckPhaseTransition;
            Died += HandleDied;
        }

        public void Initialize(BossData data)
        {
            runtimeData = data;
            if (runtimeData != null)
                ConfigureMaximumHealth(runtimeData.MaximumHealth);
            BossReady?.Invoke(this);
        }

        public override void TakeDamage(DamageContext context)
        {
            if (!invulnerable) base.TakeDamage(context);
        }

        public void SetInvulnerable(bool value) => invulnerable = value;

        public void RestoreHealthFraction(float fraction) =>
            RestoreAfterPreventedDeath(Mathf.Clamp01(fraction));

        protected void OnDestroy()
        {
            HealthChanged -= CheckPhaseTransition;
            Died -= HandleDied;
        }

        protected sealed override void MoveTowardsTarget()
        {
            // A boss moves only when its current attack explicitly assigns velocity.
            if (!IsPerformingMovementAttack) Body.linearVelocity = Vector2.zero;
        }

        protected virtual bool IsPerformingMovementAttack => false;

        protected virtual void Update()
        {
            if (!IsAlive || Target == null) return;
            if (faceTarget && bossRenderer != null)
            {
                bool movingHorizontally = IsPerformingMovementAttack &&
                    Mathf.Abs(Body.linearVelocity.x) > 0.05f;
                bossRenderer.flipX = movingHorizontally
                    ? Body.linearVelocity.x < 0f
                    : Target.position.x < transform.position.x;
            }
            UpdateBoss(Target);
        }

        protected abstract void UpdateBoss(Transform playerTarget);

        public override void OnSpawnedFromPool(GameObject prefab)
        {
            base.OnSpawnedFromPool(prefab);
            currentPhase = 0;
            invulnerable = false;
            Body.linearVelocity = Vector2.zero;
            ResetBossState();
            PhaseStarted?.Invoke(currentPhase);
        }

        public override void OnReturnedToPool()
        {
            Body.linearVelocity = Vector2.zero;
            ResetBossState();
            base.OnReturnedToPool();
        }

        protected virtual void ResetBossState() { }

        private void CheckPhaseTransition(float health, float maxHealth)
        {
            if (maxHealth <= 0f) return;
            float healthFraction = health / maxHealth;
            while (currentPhase < phaseThresholds.Length &&
                   healthFraction <= phaseThresholds[currentPhase])
            {
                currentPhase++;
                PhaseStarted?.Invoke(currentPhase);
                OnPhaseStarted(currentPhase);
            }
        }

        protected virtual void OnPhaseStarted(int phase) { }

        private void HandleDied() => BossDefeated?.Invoke();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (phaseThresholds == null) return;
            float previous = 1f;
            for (int i = 0; i < phaseThresholds.Length; i++)
            {
                phaseThresholds[i] = Mathf.Clamp(phaseThresholds[i], 0.01f, previous - 0.01f);
                previous = phaseThresholds[i];
            }
        }
#endif
    }
}

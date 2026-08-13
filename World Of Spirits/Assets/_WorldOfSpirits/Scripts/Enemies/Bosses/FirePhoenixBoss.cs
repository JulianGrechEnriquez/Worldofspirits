using UnityEngine;
using UnityEngine.Events;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Enemies
{
    /// <summary>Fire Phoenix's reusable telegraph -> attack -> recovery state machine.</summary>
    public sealed class FirePhoenixBoss : BossEnemyBase
    {
        private enum Attack { Dash, FeatherBarrage, FlameTornado, MeteorRain }
        private enum State { Waiting, Telegraphing, Executing, Recovering }

        [Header("Projectile Prefabs")]
        [SerializeField] private ProjectileBase featherProjectile;
        [SerializeField] private ProjectileBase tornadoProjectile;
        [SerializeField] private ProjectileBase meteorProjectile;

        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float initialDelay = 1.5f;
        [SerializeField, Min(0.1f)] private float telegraphDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float recoveryDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float dashDuration = 0.45f;
        [SerializeField, Min(0.1f)] private float dashSpeed = 16f;
        [SerializeField, Min(1)] private int feathersPerBarrage = 7;
        [SerializeField, Min(1f)] private float featherSpreadDegrees = 70f;
        [SerializeField, Min(0.1f)] private float featherSpeed = 11f;
        [SerializeField, Min(0f)] private float featherDamage = 12f;
        [SerializeField, Min(1)] private int meteorsPerRain = 5;
        [SerializeField, Min(0f)] private float meteorScatterRadius = 4f;
        [SerializeField, Min(0.1f)] private float meteorSpeed = 8f;
        [SerializeField, Min(0f)] private float meteorDamage = 18f;

        [Header("Presentation Hooks")]
        [SerializeField] private UnityEvent<string> attackTelegraphed;
        [SerializeField] private UnityEvent<int> phoenixPhaseStarted;
        [SerializeField] private UnityEvent rebirthStarted;

        private State state;
        private Attack currentAttack;
        private Attack previousAttack;
        private float stateEndsAt;
        private Vector2 dashDirection;

        protected override bool IsPerformingMovementAttack =>
            state == State.Executing && currentAttack == Attack.Dash;

        protected override void ResetBossState()
        {
            state = State.Waiting;
            stateEndsAt = Time.time + initialDelay;
            previousAttack = Attack.MeteorRain;
        }

        protected override void OnPhaseStarted(int phase)
        {
            phoenixPhaseStarted?.Invoke(phase);
            if (phase == 2) rebirthStarted?.Invoke();
        }

        protected override void UpdateBoss(Transform playerTarget)
        {
            if (Time.time < stateEndsAt)
            {
                if (IsPerformingMovementAttack) Body.linearVelocity = dashDirection * dashSpeed;
                return;
            }

            switch (state)
            {
                case State.Waiting:
                    BeginAttack(ChooseAttack());
                    break;
                case State.Telegraphing:
                    ExecuteAttack(playerTarget);
                    break;
                case State.Executing:
                    FinishAttack();
                    break;
                case State.Recovering:
                    state = State.Waiting;
                    break;
            }
        }

        private Attack ChooseAttack()
        {
            // Phase 1 teaches dash and feathers; later phases add area denial.
            int available = CurrentPhase == 0 ? 2 : CurrentPhase == 1 ? 3 : 4;
            Attack choice;
            do choice = (Attack)Random.Range(0, available); while (available > 1 && choice == previousAttack);
            return choice;
        }

        private void BeginAttack(Attack attack)
        {
            currentAttack = attack;
            attackTelegraphed?.Invoke(attack.ToString());
            state = State.Telegraphing;
            stateEndsAt = Time.time + telegraphDuration;
        }

        private void ExecuteAttack(Transform playerTarget)
        {
            switch (currentAttack)
            {
                case Attack.Dash:
                    dashDirection = ((Vector2)(playerTarget.position - transform.position)).normalized;
                    if (dashDirection.sqrMagnitude < 0.01f) dashDirection = Vector2.right;
                    stateEndsAt = Time.time + dashDuration;
                    break;
                case Attack.FeatherBarrage:
                    FireFan(featherProjectile, playerTarget.position, feathersPerBarrage, featherSpreadDegrees, featherSpeed, featherDamage);
                    stateEndsAt = Time.time + 0.1f;
                    break;
                case Attack.FlameTornado:
                    LaunchAt(tornadoProjectile, playerTarget.position, featherSpeed * 0.65f, featherDamage * 1.25f);
                    stateEndsAt = Time.time + 0.1f;
                    break;
                case Attack.MeteorRain:
                    for (int i = 0; i < meteorsPerRain; i++)
                        LaunchAt(meteorProjectile, playerTarget.position + (Vector3)(Random.insideUnitCircle * meteorScatterRadius), meteorSpeed, meteorDamage);
                    stateEndsAt = Time.time + 0.1f;
                    break;
            }
            state = State.Executing;
        }

        private void FinishAttack()
        {
            Body.linearVelocity = Vector2.zero;
            previousAttack = currentAttack;
            state = State.Recovering;
            stateEndsAt = Time.time + recoveryDuration;
        }

        private void FireFan(ProjectileBase prefab, Vector3 targetPosition, int count, float spread, float speed, float damage)
        {
            Vector2 center = (targetPosition - transform.position).normalized;
            if (center.sqrMagnitude < 0.01f) center = Vector2.right;
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                Vector2 direction = Quaternion.Euler(0f, 0f, Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t)) * center;
                Launch(prefab, direction, speed, damage);
            }
        }

        private void LaunchAt(ProjectileBase prefab, Vector3 targetPosition, float speed, float damage)
        {
            Launch(prefab, (targetPosition - transform.position).normalized, speed, damage);
        }

        private void Launch(ProjectileBase prefab, Vector2 direction, float speed, float damage)
        {
            if (prefab == null) return;
            ProjectileBase projectile = ProjectilePool.Spawn(prefab, transform.position, Quaternion.identity);
            projectile.ConfigureDamageContext(new DamageContext(damage, transform, DamageElement.Fire));
            projectile.Launch(direction, speed, damage, Faction);
        }
    }
}

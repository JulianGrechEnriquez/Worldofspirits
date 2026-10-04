using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Crowd;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    /// <summary>Pooled rain pursuit or stationary water vortex, queried independently of crowd colliders.</summary>
    public sealed class WaterAreaEffect : MonoBehaviour
    {
        public enum WaterEffectKind { Whirlpool, RainCloud }
        [SerializeField] private WaterEffectKind kind;
        [SerializeField] private Transform visual;
        private readonly List<IDamageable> targets = new List<IDamageable>(32);
        private DamageContext damage;
        private float radius, endTime, nextHitTime, nextTargetTime, speed, pullForce, range;
        private Transform owner;
        private IDamageable target;
        private PooledSceneObject targetPool;
        private int targetVersion;
        private bool configured;
        public WaterEffectKind Kind => kind;

        public void Configure(DamageContext source, AbilityLevelData level, UpgradeRuntimeStats stats,
            Transform player, IDamageable initialTarget)
        {
            damage = source;
            owner = player;
            radius = Mathf.Max(0.1f, level.areaRadius) *
                (stats != null ? stats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            endTime = Time.time + (stats != null ? stats.ScaleDuration(level.activeDuration) : level.activeDuration);
            speed = level.projectile.speed;
            pullForce = stats != null ? stats.ScaleForce(level.projectile.homingStrength) :
                level.projectile.homingStrength;
            range = level.targetingRange;
            nextHitTime = nextTargetTime = Time.time;
            SetTarget(initialTarget);
            configured = true;
            if (visual != null) visual.localScale = Vector3.one * radius;
        }

        private void SetTarget(IDamageable value)
        {
            target = value;
            targetPool = value != null ? value.Transform.GetComponent<PooledSceneObject>() : null;
            targetVersion = targetPool != null ? targetPool.SpawnVersion : 0;
        }

        private bool TargetIsActive => target != null && target.Transform != null &&
            target.Transform.gameObject.activeInHierarchy && target.IsAlive &&
            (targetPool == null || targetPool.SpawnVersion == targetVersion);

        private void Update()
        {
            if (!configured) return;
            if (Time.time >= endTime)
            {
                SceneObjectPool.ReleaseOrDestroy(gameObject);
                return;
            }
            if (kind != WaterEffectKind.RainCloud) return;
            if (!TargetIsActive && Time.time >= nextTargetTime)
            {
                SetTarget(CombatTargeting.FindClosest(owner != null ? owner.position : transform.position,
                    range, Faction.Player));
                nextTargetTime = Time.time + 0.25f;
            }
            if (TargetIsActive)
                transform.position = Vector3.MoveTowards(transform.position, target.Transform.position,
                    speed * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!configured || Time.time >= endTime) return;
            bool hit = damage.BaseDamage > 0f && Time.time >= nextHitTime;
            if (!hit && kind != WaterEffectKind.Whirlpool) return;
            CombatTargeting.FindAllNonAlloc(transform.position, radius, Faction.Player, targets);
            foreach (IDamageable enemy in targets)
            {
                if (hit) enemy.TakeDamage(damage);
                if (kind != WaterEffectKind.Whirlpool || !enemy.IsAlive ||
                    !enemy.Transform.gameObject.activeInHierarchy) continue;
                Vector2 inward = transform.position - enemy.Transform.position;
                if (inward.sqrMagnitude < 0.04f) continue;
                Vector2 radial = inward.normalized;
                Vector2 force = (radial + new Vector2(-radial.y, radial.x) * 0.35f) * pullForce;
                var agent = enemy.Transform.GetComponent<EnemyCrowdAgent>();
                if (agent != null) agent.ApplyExternalAcceleration(force, Time.fixedDeltaTime);
                else if (enemy.Transform.TryGetComponent(out Rigidbody2D body))
                    body.AddForce(force);
            }
            if (hit) nextHitTime = Time.time + 0.5f;
        }

        private void OnDisable()
        {
            configured = false;
            targets.Clear();
            target = null;
            targetPool = null;
            owner = null;
        }
    }
}

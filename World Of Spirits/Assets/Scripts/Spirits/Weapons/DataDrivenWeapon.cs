using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    public class DataDrivenWeapon : SpiritWeaponAttack
    {
        [SerializeField] private WeaponDefinition definition;

        [Header("Orbiting Melee Damage")]
        [SerializeField, Min(0.05f)] private float hitCooldownPerEnemy = 0.45f;
        [SerializeField] private bool drawHitboxes = true;
        [SerializeField] private Color orbitGizmoColor = new Color(1f, 0.8f, 0.15f, 0.8f);
        [SerializeField] private Color hitboxGizmoColor = new Color(1f, 0.15f, 0.1f, 0.9f);

        private readonly List<IDamageable> meleeTargets = new List<IDamageable>(32);
        private readonly Dictionary<int, float> nextMeleeHitTimes = new Dictionary<int, float>(64);
        private SpiritMember spiritOwner;
        private LivingEntity owner;
        private Transform firePointOverride;
        private Transform orbitingWeapon;
        private float orbitAngle;
        private UpgradeRuntimeStats upgradeStats;

        private WeaponLevelData ActiveLevel => definition != null && spiritOwner != null
            ? definition.GetLevel(spiritOwner.Progression.WeaponLevel) : null;

        public WeaponDefinition Definition => definition;

        private void Awake()
        {
            spiritOwner = GetComponentInParent<SpiritMember>();
            owner = GetComponentInParent<LivingEntity>();
            upgradeStats = GetComponentInParent<UpgradeRuntimeStats>();
        }

        protected override float AttackCooldown => ActiveLevel != null
            ? ActiveLevel.attackCooldown / (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.AttackSpeed) : 1f)
            : base.AttackCooldown;

        protected override bool CanAttack()
        {
            return definition != null && ActiveLevel != null && owner != null && owner.IsAlive;
        }

        protected override void PerformAttack()
        {
            if (definition.ExecutionType != WeaponExecutionType.Projectile) return;
            WeaponLevelData level = ActiveLevel;
            if (level.projectilePrefab == null) return;

            Transform origin = firePointOverride != null ? firePointOverride : transform;
            IDamageable target = CombatTargeting.FindClosest(origin.position, level.targetingRange, owner.Faction);
            if (target == null) return;

            Vector2 direction = target.Transform.position - origin.position;
            ProjectileBase projectile = ProjectilePool.Spawn(
                level.projectilePrefab, origin.position, Quaternion.identity);
            projectile.ConfigureHoming(level.homeOnEnemies, level.homingStrength, level.homingRange);
            float speed = level.projectileSpeed * (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSpeed) : 1f);
            float damage = upgradeStats != null ? upgradeStats.ScaleWeaponDamage(level.damage) : level.damage;
            projectile.Launch(direction, speed, damage, owner.Faction);
        }

        protected override void Update()
        {
            if (definition == null || definition.ExecutionType == WeaponExecutionType.Projectile)
            {
                base.Update();
                return;
            }

            WeaponLevelData level = ActiveLevel;
            if (level == null || level.weaponPrefab == null) return;
            if (orbitingWeapon == null)
                orbitingWeapon = SceneObjectPool.Spawn(
                    level.weaponPrefab, transform.position, Quaternion.identity,
                    PoolCategory.Effects).transform;

            orbitAngle = Mathf.Repeat(orbitAngle + level.orbitSpeed * Time.deltaTime, 360f);
            float radians = orbitAngle * Mathf.Deg2Rad;
            Vector2 outward = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            Transform center = firePointOverride != null ? firePointOverride : transform;
            orbitingWeapon.position = center.position + (Vector3)(outward * level.orbitRadius);
            orbitingWeapon.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(outward.y, outward.x) * Mathf.Rad2Deg - 90f);

            if (definition.ExecutionType == WeaponExecutionType.OrbitingMelee)
                DamageAtOrbitingWeapon(level);
        }

        private void DamageAtOrbitingWeapon(WeaponLevelData level)
        {
            float hitRadius = Mathf.Max(0.05f, level.hitRadius);
            CombatTargeting.FindAllNonAlloc(
                orbitingWeapon.position, hitRadius,
                owner != null ? owner.Faction : Faction.Player, meleeTargets);

            int targetLimit = Mathf.Max(1, level.maximumTargets);
            int targetsHit = 0;
            for (int i = 0; i < meleeTargets.Count && targetsHit < targetLimit; i++)
            {
                IDamageable target = meleeTargets[i];
                int id = target.Transform.gameObject.GetInstanceID();
                if (nextMeleeHitTimes.TryGetValue(id, out float nextHit) && Time.time < nextHit) continue;

                float damage = upgradeStats != null
                    ? upgradeStats.ScaleWeaponDamage(level.damage)
                    : level.damage;
                target.TakeDamage(damage);
                nextMeleeHitTimes[id] = Time.time + hitCooldownPerEnemy;
                targetsHit++;
            }
        }

        public void SetFirePointOverride(Transform point) => firePointOverride = point;

        private void OnDisable()
        {
            nextMeleeHitTimes.Clear();
            if (orbitingWeapon != null)
            {
                SceneObjectPool.ReleaseOrDestroy(orbitingWeapon.gameObject);
                orbitingWeapon = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawHitboxes || definition == null ||
                definition.ExecutionType != WeaponExecutionType.OrbitingMelee) return;

            WeaponLevelData level = ActiveLevel ?? definition.GetLevel(1);
            if (level == null) return;

            Transform center = firePointOverride != null ? firePointOverride : transform;
            Gizmos.color = orbitGizmoColor;
            Gizmos.DrawWireSphere(center.position, level.orbitRadius);

            Vector3 hitPosition = orbitingWeapon != null
                ? orbitingWeapon.position
                : center.position + Vector3.right * level.orbitRadius;
            Gizmos.color = hitboxGizmoColor;
            Gizmos.DrawWireSphere(hitPosition, Mathf.Max(0.05f, level.hitRadius));
            Gizmos.DrawLine(center.position, hitPosition);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            hitCooldownPerEnemy = Mathf.Max(0.05f, hitCooldownPerEnemy);
        }
#endif
    }
}

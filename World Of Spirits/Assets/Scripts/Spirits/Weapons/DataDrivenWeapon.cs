using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.Spirits
{
    public class DataDrivenWeapon : SpiritWeaponAttack
    {
        [SerializeField] private WeaponDefinition definition;

        private SpiritMember spiritOwner;
        private LivingEntity owner;
        private Transform firePointOverride;
        private Transform orbitingWeapon;
        private float orbitAngle;

        private WeaponLevelData ActiveLevel => definition != null && spiritOwner != null
            ? definition.GetLevel(spiritOwner.Progression.WeaponLevel) : null;

        public WeaponDefinition Definition => definition;

        private void Awake()
        {
            spiritOwner = GetComponentInParent<SpiritMember>();
            owner = GetComponentInParent<LivingEntity>();
        }

        protected override float AttackCooldown => ActiveLevel != null
            ? ActiveLevel.attackCooldown : base.AttackCooldown;

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
            projectile.Launch(direction, level.projectileSpeed, level.damage, owner.Faction);
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
        }

        public void SetFirePointOverride(Transform point) => firePointOverride = point;

        private void OnDisable()
        {
            if (orbitingWeapon != null)
            {
                SceneObjectPool.ReleaseOrDestroy(orbitingWeapon.gameObject);
                orbitingWeapon = null;
            }
        }
    }
}

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

        [Header("Projectile Visual")]
        [SerializeField] private float projectileVisualRotationOffset = -45f;
        [SerializeField] private bool fireFromVisualPoint;

        [Header("Orbiting Melee Damage")]
        [SerializeField, Min(0.05f)] private float hitCooldownPerEnemy = 0.45f;
        [SerializeField] private bool drawHitboxes = true;
        [SerializeField] private Color orbitGizmoColor = new Color(1f, 0.8f, 0.15f, 0.8f);
        [SerializeField] private Color hitboxGizmoColor = new Color(1f, 0.15f, 0.1f, 0.9f);

        private readonly List<IDamageable> meleeTargets = new List<IDamageable>(32);
        private readonly List<Collider2D> meleeColliderResults = new List<Collider2D>(32);
        private readonly HashSet<int> meleeTargetIds = new HashSet<int>();
        private readonly Dictionary<int, float> nextMeleeHitTimes = new Dictionary<int, float>(64);
        private SpiritMember spiritOwner;
        private LivingEntity owner;
        private Transform firePointOverride;
        private Transform visualPointOverride;
        private Transform orbitingWeapon;
        private Collider2D orbitingWeaponHitbox;
        private Transform projectileWeaponVisual;
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

            Transform origin = fireFromVisualPoint && visualPointOverride != null
                ? visualPointOverride
                : firePointOverride != null ? firePointOverride : transform;
            IDamageable target = CombatTargeting.FindClosest(origin.position, level.targetingRange, owner.Faction);
            if (target == null) return;

            Vector2 direction = target.Transform.position - origin.position;
            int projectileCount = upgradeStats != null ? upgradeStats.GetProjectileCount(1) : 1;
            float speed = level.projectileSpeed * (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSpeed) : 1f);
            DamageContext damage = DamageContext.Weapon(
                level.damage,
                owner != null ? owner.transform : transform,
                DamageElementUtility.FromSpiritName(
                    spiritOwner != null && spiritOwner.Definition != null
                        ? spiritOwner.Definition.SpiritName
                        : string.Empty));
            for (int i = 0; i < projectileCount; i++)
            {
                Vector2 shotDirection = SpreadDirection(direction, i, projectileCount, 12f);
                ProjectileBase projectile = ProjectilePool.Spawn(
                    level.projectilePrefab, origin.position, Quaternion.identity);
                projectile.ConfigureHoming(level.homeOnEnemies, level.homingStrength, level.homingRange);
                projectile.ConfigureUpgradeModifiers(upgradeStats);
                projectile.ConfigureDamageContext(damage);
                projectile.Launch(shotDirection, speed, damage.BaseDamage, owner.Faction);
            }
        }

        protected override void Update()
        {
            if (definition == null || definition.ExecutionType == WeaponExecutionType.Projectile)
            {
                UpdateProjectileVisual();
                base.Update();
                return;
            }

            WeaponLevelData level = ActiveLevel;
            if (level == null || level.weaponPrefab == null) return;
            if (orbitingWeapon == null)
            {
                orbitingWeapon = SceneObjectPool.Spawn(
                    level.weaponPrefab, transform.position, Quaternion.identity,
                    PoolCategory.Effects).transform;
                orbitingWeaponHitbox = orbitingWeapon.GetComponentInChildren<Collider2D>(true);
            }

            float attackSpeed = upgradeStats != null
                ? upgradeStats.GetMultiplier(UpgradeStat.AttackSpeed)
                : 1f;
            orbitAngle = Mathf.Repeat(
                orbitAngle + level.orbitSpeed * attackSpeed * Time.deltaTime,
                360f);
            float radians = orbitAngle * Mathf.Deg2Rad;
            Vector2 outward = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            Transform center = firePointOverride != null ? firePointOverride : transform;
            orbitingWeapon.position = center.position + (Vector3)(outward * level.orbitRadius);
            orbitingWeapon.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(outward.y, outward.x) * Mathf.Rad2Deg - 90f);
            float weaponSize = upgradeStats != null
                ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSize)
                : 1f;
            orbitingWeapon.localScale = level.weaponPrefab.transform.localScale * weaponSize;

            if (definition.ExecutionType == WeaponExecutionType.OrbitingMelee)
                DamageAtOrbitingWeapon(level);
        }

        private void DamageAtOrbitingWeapon(WeaponLevelData level)
        {
            PopulateMeleeTargets(level);

            int targetLimit = Mathf.Max(1, level.maximumTargets);
            int targetsHit = 0;
            for (int i = 0; i < meleeTargets.Count && targetsHit < targetLimit; i++)
            {
                IDamageable target = meleeTargets[i];
                int id = target.Transform.gameObject.GetInstanceID();
                if (nextMeleeHitTimes.TryGetValue(id, out float nextHit) && Time.time < nextHit) continue;

                DamageContext damage = DamageContext.Weapon(
                    level.damage,
                    owner != null ? owner.transform : transform,
                    DamageElementUtility.FromSpiritName(
                        spiritOwner != null && spiritOwner.Definition != null
                            ? spiritOwner.Definition.SpiritName
                            : string.Empty));
                int strikeCount = upgradeStats != null
                    ? upgradeStats.GetMeleeStrikeCount(1)
                    : 1;
                for (int strike = 0; strike < strikeCount; strike++)
                {
                    target.TakeDamage(damage);
                }
                float attackSpeed = upgradeStats != null
                    ? upgradeStats.GetMultiplier(UpgradeStat.AttackSpeed)
                    : 1f;
                nextMeleeHitTimes[id] = Time.time + hitCooldownPerEnemy / attackSpeed;
                targetsHit++;
            }
        }

        private void PopulateMeleeTargets(WeaponLevelData level)
        {
            meleeTargets.Clear();
            meleeTargetIds.Clear();

            if (orbitingWeaponHitbox != null)
            {
                meleeColliderResults.Clear();
                ContactFilter2D filter = ContactFilter2D.noFilter;
                Physics2D.OverlapCollider(orbitingWeaponHitbox, filter, meleeColliderResults);

                Faction ownerFaction = owner != null ? owner.Faction : Faction.Player;
                for (int i = 0; i < meleeColliderResults.Count; i++)
                {
                    Collider2D overlap = meleeColliderResults[i];
                    IDamageable target = overlap != null
                        ? overlap.GetComponentInParent<IDamageable>() : null;
                    if (target == null || !target.IsAlive || target.Faction == ownerFaction)
                        continue;

                    int id = target.Transform.gameObject.GetInstanceID();
                    if (meleeTargetIds.Add(id)) meleeTargets.Add(target);
                }

                return;
            }

            float hitRadius = Mathf.Max(0.05f, level.hitRadius *
                (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSize) : 1f));
            CombatTargeting.FindAllNonAlloc(
                orbitingWeapon.position,
                hitRadius,
                owner != null ? owner.Faction : Faction.Player,
                meleeTargets);
        }

        public void SetFirePointOverride(Transform point) => firePointOverride = point;

        public void SetVisualPointOverride(Transform point) => visualPointOverride = point;

        private static Vector2 SpreadDirection(Vector2 center, int index, int count, float spread)
        {
            if (count <= 1) return center.normalized;
            float centerAngle = Mathf.Atan2(center.y, center.x) * Mathf.Rad2Deg;
            float angle = centerAngle - spread * 0.5f + spread * index / (count - 1);
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private void UpdateProjectileVisual()
        {
            WeaponLevelData level = ActiveLevel;
            if (level == null || level.weaponPrefab == null)
            {
                ReleaseProjectileVisual();
                return;
            }

            Transform origin = fireFromVisualPoint && visualPointOverride != null
                ? visualPointOverride
                : firePointOverride != null ? firePointOverride : transform;
            Transform visualOrigin = visualPointOverride != null ? visualPointOverride : origin;
            if (projectileWeaponVisual == null)
            {
                projectileWeaponVisual = SceneObjectPool.Spawn(
                    level.weaponPrefab,
                    visualOrigin.position,
                    Quaternion.identity,
                    PoolCategory.Effects).transform;
            }

            projectileWeaponVisual.position = visualOrigin.position;
            float weaponSize = upgradeStats != null
                ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSize)
                : 1f;
            projectileWeaponVisual.localScale =
                level.weaponPrefab.transform.localScale * weaponSize;
            IDamageable target = CombatTargeting.FindClosest(
                origin.position,
                level.targetingRange,
                owner != null ? owner.Faction : Faction.Player);
            if (target == null) return;

            Vector2 direction = target.Transform.position - visualOrigin.position;
            if (direction.sqrMagnitude <= 0.0001f) return;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectileWeaponVisual.rotation = Quaternion.Euler(
                0f, 0f, angle + projectileVisualRotationOffset);
        }

        private void ReleaseProjectileVisual()
        {
            if (projectileWeaponVisual == null) return;
            SceneObjectPool.ReleaseOrDestroy(projectileWeaponVisual.gameObject);
            projectileWeaponVisual = null;
        }

        private void OnDisable()
        {
            nextMeleeHitTimes.Clear();
            ReleaseProjectileVisual();
            if (orbitingWeapon != null)
            {
                SceneObjectPool.ReleaseOrDestroy(orbitingWeapon.gameObject);
                orbitingWeapon = null;
                orbitingWeaponHitbox = null;
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
            if (orbitingWeaponHitbox is BoxCollider2D box)
            {
                Matrix4x4 previousMatrix = Gizmos.matrix;
                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.offset, box.size);
                Gizmos.matrix = previousMatrix;
            }
            else
            {
                Gizmos.DrawWireSphere(hitPosition, Mathf.Max(0.05f, level.hitRadius));
            }
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

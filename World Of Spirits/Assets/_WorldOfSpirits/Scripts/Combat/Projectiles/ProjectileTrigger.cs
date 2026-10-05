using System;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Combat
{
    public enum ProjectileTriggerEvent { EnemyHit, LifetimeExpired, Despawn }
    public enum ProjectileTriggerAction { AreaDamage, SpawnEffect, SpawnProjectiles }
    public enum ProjectileTriggerUnlock { Always, WeaponLevel, UpgradeCard }

    [Serializable]
    public sealed class ProjectileTrigger
    {
        public ProjectileTriggerEvent when;
        public ProjectileTriggerAction action;
        public ProjectileTriggerUnlock unlock;
        [Min(1)] public int requiredWeaponLevel = 2;
        public UpgradeCardDefinition requiredUpgrade;
        [Min(1)] public int requiredCardLevel = 1;
        [Tooltip("Each trigger runs once per shot by default. Disable for piercing shots that should trigger on each new enemy.")]
        public bool oncePerShot = true;
        [Tooltip("Fraction of the original projectile damage used by the area, child projectiles, or PersistentDamageZone ticks. Pure visuals do not deal damage.")]
        [Min(0f)] public float damageMultiplier = 1f;
        [Min(0.05f)] public float areaRadius = 1.5f;
        [Tooltip("A visual or damage-zone prefab. For a travelling wave use Spawn Projectiles instead.")]
        public GameObject effectPrefab;
        [Min(0.05f)] public float effectDuration = 2f;
        public ProjectileBase projectilePrefab;
        [Range(1, 16)] public int projectileCount = 3;
        [Range(0f, 360f)] public float spreadAngle = 60f;
        [Min(0.1f)] public float projectileSpeed = 10f;

        public bool IsUnlocked(int weaponLevel, UpgradeRuntimeStats stats)
        {
            return unlock switch
            {
                ProjectileTriggerUnlock.Always => true,
                ProjectileTriggerUnlock.WeaponLevel => weaponLevel >= requiredWeaponLevel,
                ProjectileTriggerUnlock.UpgradeCard => requiredUpgrade != null && stats != null &&
                    stats.GetCardLevel(requiredUpgrade.Id) >= requiredCardLevel,
                _ => false
            };
        }
    }
}

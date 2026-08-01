using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    public class DataDrivenAbility : SpiritAbility
    {
        [SerializeField] private AbilityDefinition definition;

        private readonly List<Transform> orbitingObjects = new List<Transform>();
        private readonly List<IDamageable> targetBuffer = new List<IDamageable>(64);
        private readonly HashSet<Transform> chainHitBuffer = new HashSet<Transform>();
        private GameObject followingArea;
        private float followingAreaDisableTime;
        private AbilityLevelData ActiveLevel => definition != null ? definition.GetLevel(CurrentLevel) : null;

        public AbilityDefinition Definition => definition;

        protected override float GetCooldown()
        {
            return ActiveLevel != null ? ScaleCooldown(ActiveLevel.cooldown) : base.GetCooldown();
        }

        protected override bool CanCast(SpiritAbilityContext context)
        {
            return definition != null && ActiveLevel != null;
        }

        protected override void Cast(SpiritAbilityContext context)
        {
            AbilityLevelData level = ActiveLevel;
            switch (definition.ExecutionType)
            {
                case AbilityExecutionType.Projectile: CastProjectiles(context, level); break;
                case AbilityExecutionType.Area: CastArea(context, level); break;
                case AbilityExecutionType.SpawnEffect: SpawnEffects(context, level); break;
                case AbilityExecutionType.Orbiting: EnsureOrbiting(level); break;
                case AbilityExecutionType.Chain: CastChain(level); break;
                case AbilityExecutionType.Self: ApplyEffects(context.Player, context.Player, level.effects); break;
                case AbilityExecutionType.FollowingArea: ActivateFollowingArea(context, level); break;
            }
        }

        private void Update()
        {
            if (followingArea != null && followingArea.activeSelf && Time.time >= followingAreaDisableTime)
                followingArea.SetActive(false);

            if (definition == null || definition.ExecutionType != AbilityExecutionType.Orbiting || ActiveLevel == null)
            {
                return;
            }

            EnsureOrbiting(ActiveLevel);
            float radius = ActiveLevel.orbitRadius *
                (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            float angleOffset = Time.time * ActiveLevel.orbitSpeed;
            for (int i = 0; i < orbitingObjects.Count; i++)
            {
                if (orbitingObjects[i] == null) continue;
                float angle = (angleOffset + 360f * i / orbitingObjects.Count) * Mathf.Deg2Rad;
                orbitingObjects[i].position = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
        }

        private void OnDisable()
        {
            foreach (Transform item in orbitingObjects)
            {
                if (item != null) SceneObjectPool.ReleaseOrDestroy(item.gameObject);
            }
            orbitingObjects.Clear();
            if (followingArea != null)
            {
                SceneObjectPool.ReleaseOrDestroy(followingArea);
                followingArea = null;
            }
        }

        private void ActivateFollowingArea(SpiritAbilityContext context, AbilityLevelData level)
        {
            if (level.spawnedEffectPrefab == null || context.Player == null) return;

            if (followingArea == null)
            {
                followingArea = SceneObjectPool.Spawn(
                    level.spawnedEffectPrefab, context.Player.position, Quaternion.identity,
                    PoolCategory.FloorEffects, context.Player);
                followingArea.name = level.spawnedEffectPrefab.name + " (Following Area)";
                followingArea.transform.localPosition = Vector3.zero;
                PersistentDamageZone zone = followingArea.GetComponent<PersistentDamageZone>();
                if (zone != null)
                {
                    zone.SetReusable(true);
                    zone.SetOwner(context.Player);
                    zone.ConfigureUpgradeModifiers(UpgradeStats);
                    zone.ConfigureDamageSource(CreateSpiritDamage(0f));
                }
            }

            followingArea.transform.SetParent(context.Player, false);
            followingArea.transform.localPosition = Vector3.zero;
            followingArea.SetActive(true);
            followingAreaDisableTime = Time.time + Mathf.Max(0.05f,
                UpgradeStats != null ? UpgradeStats.ScaleDuration(level.activeDuration) : level.activeDuration);
        }

        private void CastProjectiles(SpiritAbilityContext context, AbilityLevelData level)
        {
            AbilityProjectileData data = level.projectile;
            if (data.projectilePrefab == null) return;
            Vector2 center = ResolveDirection(context, level);
            int count = Mathf.Max(1, data.count + (UpgradeStats != null ? Mathf.RoundToInt(UpgradeStats.GetFlat(UpgradeStat.MultiShot)) : 0));
            float centerAngle = Mathf.Atan2(center.y, center.x) * Mathf.Rad2Deg;
            for (int i = 0; i < count; i++)
            {
                float angle;
                if (data.spreadMode == ProjectileSpreadMode.Random)
                    angle = centerAngle + Random.Range(-data.spreadAngle * 0.5f, data.spreadAngle * 0.5f);
                else
                    angle = centerAngle - data.spreadAngle * 0.5f + (count > 1 ? data.spreadAngle * i / (count - 1) : 0f);

                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                ProjectileBase projectile = ProjectilePool.Spawn(
                    data.projectilePrefab, transform.position, Quaternion.identity);
                projectile.ConfigureHoming(data.homeOnEnemies, data.homingStrength, data.homingRange);
                projectile.ConfigureUpgradeModifiers(UpgradeStats);
                DamageContext damage = CreateSpiritDamage(data.damage);
                projectile.ConfigureDamageContext(damage);
                if (projectile is ConfigurableProjectile configurable)
                {
                    configurable.Configure(data.pierceCount, data.explosionRadius, data.growthPerSecond,
                        data.appliesStatus, data.status, data.statusDuration, data.statusStrength,
                        data.bounceCount, data.bounceRange);
                }
                float speed = data.speed * (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.ProjectileSpeed) : 1f);
                projectile.Launch(direction, speed, damage.BaseDamage, Faction.Player);
            }
        }

        private void CastArea(SpiritAbilityContext context, AbilityLevelData level)
        {
            CombatTargeting.FindAllNonAlloc(
                transform.position, level.areaRadius * (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.AreaSize) : 1f), Faction.Player, targetBuffer);
            foreach (IDamageable target in targetBuffer)
                ApplyEffects(target.Transform, context.Player, level.effects);
        }

        private void SpawnEffects(SpiritAbilityContext context, AbilityLevelData level)
        {
            if (level.spawnedEffectPrefab == null) return;
            for (int i = 0; i < Mathf.Max(1, level.spawnCount); i++)
            {
                Vector3 position = ResolvePosition(context, level);
                GameObject spawned = SceneObjectPool.Spawn(
                    level.spawnedEffectPrefab, position, Quaternion.identity,
                    PoolCategory.FloorEffects);
                PersistentDamageZone zone = spawned.GetComponent<PersistentDamageZone>();
                if (zone != null)
                {
                    zone.SetOwner(context.Player);
                    zone.ConfigureUpgradeModifiers(UpgradeStats);
                    zone.ConfigureDamageSource(CreateSpiritDamage(0f));
                }
                else SceneObjectPool.ReleaseAfter(spawned, Mathf.Max(0.1f,
                    UpgradeStats != null ? UpgradeStats.ScaleDuration(level.activeDuration) : level.activeDuration));
            }
        }

        private void EnsureOrbiting(AbilityLevelData level)
        {
            GameObject prefab = level.spawnedEffectPrefab;
            if (prefab == null) return;
            orbitingObjects.RemoveAll(item => item == null);
            int desiredCount = UpgradeStats != null
                ? UpgradeStats.GetProjectileCount(Mathf.Max(1, level.spawnCount))
                : Mathf.Max(1, level.spawnCount);
            while (orbitingObjects.Count < desiredCount)
            {
                Transform spawned = SceneObjectPool.Spawn(
                    prefab, transform.position, Quaternion.identity,
                    PoolCategory.Effects, transform).transform;
                PersistentDamageZone zone = spawned.GetComponent<PersistentDamageZone>();
                if (zone != null)
                {
                    zone.SetOwner(transform);
                    zone.ConfigureUpgradeModifiers(UpgradeStats);
                    zone.ConfigureDamageSource(CreateSpiritDamage(0f));
                }
                orbitingObjects.Add(spawned);
            }
            while (orbitingObjects.Count > desiredCount)
            {
                Transform item = orbitingObjects[orbitingObjects.Count - 1];
                orbitingObjects.RemoveAt(orbitingObjects.Count - 1);
                SceneObjectPool.ReleaseOrDestroy(item.gameObject);
            }
        }

        private void CastChain(AbilityLevelData level)
        {
            chainHitBuffer.Clear();
            Vector3 position = transform.position;
            for (int i = 0; i < level.chainCount; i++)
            {
                IDamageable next = FindClosestUnhit(position, level.chainRange, chainHitBuffer);
                if (next == null) break;
                ApplyEffects(next.Transform, OwnerSpirit != null ? OwnerSpirit.transform : transform, level.effects);
                chainHitBuffer.Add(next.Transform);
                position = next.Transform.position;
            }
        }

        private IDamageable FindClosestUnhit(Vector3 position, float range, HashSet<Transform> hit)
        {
            IDamageable best = null;
            float bestDistance = range * range;
            CombatTargeting.FindAllNonAlloc(position, range, Faction.Player, targetBuffer);
            foreach (IDamageable candidate in targetBuffer)
            {
                float distance = (candidate.Transform.position - position).sqrMagnitude;
                if (!hit.Contains(candidate.Transform) && distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }
            return best;
        }

        private void ApplyEffects(Transform target, Transform player, IReadOnlyList<AbilityEffectData> effects)
        {
            if (target == null) return;
            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            LivingEntity living = target.GetComponentInParent<LivingEntity>();
            foreach (AbilityEffectData effect in effects)
            {
                switch (effect.effectType)
                {
                    case AbilityEffectType.Damage:
                        damageable?.TakeDamage(CreateSpiritDamage(effect.value));
                        break;
                    case AbilityEffectType.ApplyStatus:
                        if (damageable is IStatusEffectReceiver receiver) receiver.ApplyStatus(
                            effect.status,
                            UpgradeStats != null ? UpgradeStats.ScaleDuration(effect.duration) : effect.duration,
                            effect.value,
                            CreateSpiritDamage(effect.value));
                        break;
                    case AbilityEffectType.Pull: ApplyForce(target, transform.position - target.position,
                        UpgradeStats != null ? UpgradeStats.ScaleForce(effect.value) : effect.value); break;
                    case AbilityEffectType.Knockback: ApplyForce(target, target.position - transform.position,
                        UpgradeStats != null ? UpgradeStats.ScaleForce(effect.value) : effect.value); break;
                    case AbilityEffectType.Heal: living?.Heal(
                        UpgradeStats != null ? UpgradeStats.ScaleHealing(effect.value) : effect.value); break;
                    case AbilityEffectType.Shield: living?.AddShield(
                        UpgradeStats != null ? UpgradeStats.ScaleShield(effect.value) : effect.value,
                        UpgradeStats != null ? UpgradeStats.ScaleDuration(effect.duration) : effect.duration); break;
                    case AbilityEffectType.GrantRevive: living?.GrantRevive(Mathf.Max(1, Mathf.RoundToInt(effect.value))); break;
                    case AbilityEffectType.SpawnEffect:
                        if (effect.effectPrefab != null)
                        {
                            GameObject spawnedEffect = SceneObjectPool.Spawn(
                                effect.effectPrefab, target.position, Quaternion.identity,
                                PoolCategory.FloorEffects);
                            SceneObjectPool.ReleaseAfter(
                                spawnedEffect, Mathf.Max(0.1f,
                                    UpgradeStats != null ? UpgradeStats.ScaleDuration(effect.duration) : effect.duration));
                        }
                        break;
                }
            }
        }

        private static void ApplyForce(Transform target, Vector3 direction, float force)
        {
            Rigidbody2D body = target.GetComponent<Rigidbody2D>();
            if (body != null && force > 0f) body.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }

        private Vector2 ResolveDirection(SpiritAbilityContext context, AbilityLevelData level)
        {
            Transform target = ResolveTarget(context, level);
            if (target != null && target != context.Player) return (target.position - transform.position).normalized;
            return context.Player != null ? (Vector2)context.Player.right : Vector2.right;
        }

        private Vector3 ResolvePosition(SpiritAbilityContext context, AbilityLevelData level)
        {
            Transform target = ResolveTarget(context, level);
            Vector3 origin = context.Player != null ? context.Player.position : transform.position;
            return definition.TargetingMode == AbilityTargetingMode.RandomPositionNearPlayer
                ? origin + (Vector3)(Random.insideUnitCircle * level.areaRadius)
                : target != null ? target.position : origin;
        }

        private Transform ResolveTarget(SpiritAbilityContext context, AbilityLevelData level)
        {
            if (definition.TargetingMode == AbilityTargetingMode.Self || definition.TargetingMode == AbilityTargetingMode.AroundPlayer ||
                definition.TargetingMode == AbilityTargetingMode.PlayerFacing || definition.TargetingMode == AbilityTargetingMode.RandomPositionNearPlayer)
                return context.Player;
            CombatTargeting.FindAllNonAlloc(
                transform.position, level.targetingRange, Faction.Player, targetBuffer);
            if (targetBuffer.Count == 0) return null;
            if (definition.TargetingMode == AbilityTargetingMode.RandomEnemy)
                return targetBuffer[Random.Range(0, targetBuffer.Count)].Transform;
            LivingEntity selected = null;
            foreach (IDamageable candidate in targetBuffer)
            {
                LivingEntity living = candidate as LivingEntity;
                if (living == null) continue;
                if (selected == null || definition.TargetingMode == AbilityTargetingMode.StrongestEnemy && living.MaxHealth > selected.MaxHealth ||
                    definition.TargetingMode == AbilityTargetingMode.LowestHealthEnemy && living.CurrentHealth < selected.CurrentHealth)
                    selected = living;
            }
            if (selected != null && definition.TargetingMode != AbilityTargetingMode.ClosestEnemy) return selected.transform;
            return CombatTargeting.FindClosest(transform.position, level.targetingRange, Faction.Player)?.Transform;
        }
    }
}

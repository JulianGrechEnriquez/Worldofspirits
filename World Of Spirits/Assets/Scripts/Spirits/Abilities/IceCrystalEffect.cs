using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    /// <summary>
    /// A pooled crystal that grows at the target position, then deals one burst
    /// of ice damage and optionally freezes everything in its radius.
    /// </summary>
    public sealed class IceCrystalEffect : MonoBehaviour, IScenePoolable
    {
        [SerializeField, Range(0.01f, 1f)] private float startingScale = 0.15f;
        [SerializeField, Range(0.1f, 1f)] private float growthPortion = 0.72f;

        private readonly List<IDamageable> targets = new List<IDamageable>(32);
        private Animator effectAnimator;
        private DamageContext damageSource;
        private UpgradeRuntimeStats upgradeStats;
        private float damage;
        private float radius;
        private float lifetime;
        private float freezeDuration;
        private float freezeStrength;
        private float freezeChance;
        private float elapsed;
        private bool applyFreeze;
        private bool configured;
        private Vector3 fullScale;

        private void Awake()
        {
            effectAnimator = GetComponentInChildren<Animator>(true);
            fullScale = transform.localScale;
        }

        public void Configure(
            DamageContext source,
            UpgradeRuntimeStats stats,
            float burstDamage,
            float explosionRadius,
            float growthDuration,
            bool freezes,
            float statusChance,
            float statusDuration,
            float statusStrength)
        {
            damageSource = source;
            upgradeStats = stats;
            damage = Mathf.Max(0f, burstDamage);
            radius = Mathf.Max(0.1f, explosionRadius) *
                (stats != null ? stats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            lifetime = Mathf.Max(0.1f,
                stats != null ? stats.ScaleDuration(growthDuration) : growthDuration);
            applyFreeze = freezes;
            freezeChance = Mathf.Clamp01(statusChance);
            freezeDuration = Mathf.Max(0f,
                stats != null ? stats.ScaleDuration(statusDuration) : statusDuration);
            freezeStrength = Mathf.Max(0f, statusStrength);
            elapsed = 0f;
            configured = true;
            transform.localScale = fullScale * startingScale;

            if (effectAnimator != null)
            {
                effectAnimator.Rebind();
                effectAnimator.Update(0f);
                AnimationClip[] clips = effectAnimator.runtimeAnimatorController != null
                    ? effectAnimator.runtimeAnimatorController.animationClips
                    : null;
                if (clips != null && clips.Length > 0)
                    effectAnimator.speed = Mathf.Max(0.01f, clips[0].length / lifetime);
            }
        }

        private void Update()
        {
            if (!configured) return;

            elapsed += Time.deltaTime;
            float growthTime = Mathf.Max(0.05f, lifetime * growthPortion);
            float normalizedGrowth = Mathf.Clamp01(elapsed / growthTime);
            float easedGrowth = 1f - Mathf.Pow(1f - normalizedGrowth, 3f);
            transform.localScale = Vector3.LerpUnclamped(
                fullScale * startingScale, fullScale, easedGrowth);

            if (elapsed >= lifetime)
                Explode();
        }

        private void Explode()
        {
            configured = false;
            CombatTargeting.FindAllNonAlloc(
                transform.position, radius, Faction.Player, targets);

            for (int i = 0; i < targets.Count; i++)
            {
                IDamageable target = targets[i];
                if (target == null || !target.IsAlive) continue;

                DamageContext hit = damageSource.WithBaseDamage(damage);
                target.TakeDamage(hit);
                if (applyFreeze && Random.value <= freezeChance &&
                    target is IStatusEffectReceiver receiver)
                    receiver.ApplyStatus(
                        CombatStatus.Freeze, freezeDuration, freezeStrength, hit);
            }

            SceneObjectPool.ReleaseOrDestroy(gameObject);
        }

        public void OnSpawnedFromPool(GameObject prefab)
        {
            IceCrystalEffect source = prefab.GetComponent<IceCrystalEffect>();
            if (source != null)
            {
                startingScale = source.startingScale;
                growthPortion = source.growthPortion;
                fullScale = source.transform.localScale;
            }

            elapsed = 0f;
            configured = false;
            targets.Clear();
            transform.localScale = fullScale * startingScale;
        }

        public void OnReturnedToPool()
        {
            configured = false;
            targets.Clear();
            transform.localScale = fullScale;
            if (effectAnimator != null) effectAnimator.speed = 1f;
        }
    }
}

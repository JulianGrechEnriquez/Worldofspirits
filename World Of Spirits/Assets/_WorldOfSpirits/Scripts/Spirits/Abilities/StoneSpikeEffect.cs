using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    public sealed class StoneSpikeEffect : MonoBehaviour
    {
        [SerializeField] private Transform artwork;
        private readonly List<IDamageable> targets = new List<IDamageable>(32);
        private DamageContext damage;
        private float radius, duration, startTime, bleedDuration, bleedStrength;
        private bool erupted, appliesBleed;

        public void Configure(DamageContext source, AbilityLevelData level, UpgradeRuntimeStats stats)
        {
            damage = source;
            radius = level.areaRadius * (stats != null ? stats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            duration = stats != null ? stats.ScaleDuration(level.activeDuration) : level.activeDuration;
            appliesBleed = level.projectile.appliesStatus;
            bleedDuration = stats != null ? stats.ScaleDuration(level.projectile.statusDuration) : level.projectile.statusDuration;
            bleedStrength = level.projectile.statusStrength;
            startTime = Time.time;
            erupted = false;
            transform.localScale = Vector3.one * radius / 0.65f;
            if (artwork != null) artwork.localScale = new Vector3(1f, 0.1f, 1f);
        }

        private void Update()
        {
            float age = Time.time - startTime;
            if (age >= duration)
            {
                SceneObjectPool.ReleaseOrDestroy(gameObject);
                return;
            }
            if (artwork != null)
            {
                float height = Mathf.Lerp(0.1f, 1f, Mathf.Clamp01(age / 0.15f));
                height *= Mathf.Clamp01((duration - age) / 0.2f);
                artwork.localScale = new Vector3(1f, height, 1f);
            }
            if (!erupted && age >= 0.12f) Erupt();
        }

        private void Erupt()
        {
            erupted = true;
            CombatTargeting.FindAllNonAlloc(transform.position, radius, Faction.Player, targets);
            foreach (IDamageable target in targets)
            {
                target.TakeDamage(damage);
                if (appliesBleed && target is IStatusEffectReceiver receiver)
                    receiver.ApplyStatus(CombatStatus.Bleed, bleedDuration, bleedStrength, damage);
            }
        }

        private void OnDisable()
        {
            targets.Clear();
            erupted = false;
        }
    }
}

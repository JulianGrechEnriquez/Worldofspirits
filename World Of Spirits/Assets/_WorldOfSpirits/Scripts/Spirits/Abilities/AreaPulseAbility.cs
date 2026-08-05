using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    public class AreaPulseAbility : SpiritAbility
    {
        private readonly System.Collections.Generic.List<IDamageable> targets =
            new System.Collections.Generic.List<IDamageable>(64);
        [SerializeField] private LevelScaling radius = new LevelScaling();
        [SerializeField] private LevelScaling damage = new LevelScaling();
        [SerializeField] private LevelScaling force = new LevelScaling();
        [SerializeField] private bool pullInward;
        [SerializeField] private bool appliesStatus;
        [SerializeField] private CombatStatus status;
        [SerializeField] private LevelScaling statusDuration = new LevelScaling();
        [SerializeField] private LevelScaling statusStrength = new LevelScaling();

        protected override void Cast(SpiritAbilityContext context)
        {
            float effectRadius = radius.Evaluate(CurrentLevel) *
                (UpgradeStats != null ? UpgradeStats.GetMultiplier(UpgradeStat.AreaSize) : 1f);
            float effectDamage = damage.Evaluate(CurrentLevel);
            float effectForce = UpgradeStats != null
                ? UpgradeStats.ScaleForce(force.Evaluate(CurrentLevel))
                : force.Evaluate(CurrentLevel);
            float effectStatusDuration = UpgradeStats != null
                ? UpgradeStats.ScaleDuration(statusDuration.Evaluate(CurrentLevel))
                : statusDuration.Evaluate(CurrentLevel);
            float effectStatusStrength = statusStrength.Evaluate(CurrentLevel);
            CombatTargeting.FindAllNonAlloc(transform.position, effectRadius, Faction.Player, targets);
            foreach (IDamageable target in targets)
            {
                DamageContext damageContext = CreateSpiritDamage(effectDamage);
                target.TakeDamage(damageContext);
                if (appliesStatus && target is IStatusEffectReceiver receiver)
                {
                    receiver.ApplyStatus(
                        status, effectStatusDuration, effectStatusStrength, damageContext);
                }
                Rigidbody2D body = target.Transform.GetComponent<Rigidbody2D>();
                if (body != null && effectForce > 0f)
                {
                    Vector2 direction = (target.Transform.position - transform.position).normalized;
                    body.AddForce((pullInward ? -direction : direction) * effectForce, ForceMode2D.Impulse);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = pullInward ? Color.cyan : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius.Evaluate(Application.isPlaying ? CurrentLevel : 1));
        }
    }
}

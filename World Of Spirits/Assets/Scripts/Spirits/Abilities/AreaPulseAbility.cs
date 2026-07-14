using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public class AreaPulseAbility : SpiritAbility
    {
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
            float effectRadius = radius.Evaluate(CurrentLevel);
            foreach (IDamageable target in CombatTargeting.FindAll(transform.position, effectRadius, Faction.Player))
            {
                target.TakeDamage(damage.Evaluate(CurrentLevel));
                if (appliesStatus && target is IStatusEffectReceiver receiver)
                {
                    receiver.ApplyStatus(status, statusDuration.Evaluate(CurrentLevel), statusStrength.Evaluate(CurrentLevel));
                }
                Rigidbody2D body = target.Transform.GetComponent<Rigidbody2D>();
                if (body != null && force.Evaluate(CurrentLevel) > 0f)
                {
                    Vector2 direction = (target.Transform.position - transform.position).normalized;
                    body.AddForce((pullInward ? -direction : direction) * force.Evaluate(CurrentLevel), ForceMode2D.Impulse);
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

using UnityEngine;

namespace WorldOfSpirits.Spirits
{
    public abstract class SpiritAbility : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float cooldown = 1f;
        [SerializeField] private bool primarySpiritOnly;
        [SerializeField] private bool castWhileMoving = true;
        [SerializeField] private bool castWhileStandingStill;

        private float nextCastTime;

        public void TickAbility(SpiritAbilityContext context)
        {
            if (!isActiveAndEnabled || Time.time < nextCastTime || primarySpiritOnly && !context.IsPrimary)
            {
                return;
            }

            // Support spirits continue casting in either movement state. Movement
            // restrictions only control the currently selected primary spirit.
            bool movementStateAllowed = !context.IsPrimary ||
                (context.PlayerIsMoving ? castWhileMoving : castWhileStandingStill);
            if (!movementStateAllowed || !CanCast(context))
            {
                return;
            }

            Cast(context);
            nextCastTime = Time.time + cooldown;
        }

        protected virtual bool CanCast(SpiritAbilityContext context) => true;
        protected abstract void Cast(SpiritAbilityContext context);
    }

    public readonly struct SpiritAbilityContext
    {
        public SpiritAbilityContext(Transform player, bool playerIsMoving, bool isPrimary)
        {
            Player = player;
            PlayerIsMoving = playerIsMoving;
            IsPrimary = isPrimary;
        }

        public Transform Player { get; }
        public bool PlayerIsMoving { get; }
        public bool IsPrimary { get; }
    }
}

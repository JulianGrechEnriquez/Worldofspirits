using System.Collections;
using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    public abstract class BossAttackBase : MonoBehaviour
    {
        [Header("Selection")]
        [SerializeField, Min(0f)] private float weight = 1f;
        [SerializeField, Min(0f)] private float minimumRange;
        [SerializeField, Min(0f)] private float maximumRange = 100f;
        [SerializeField, Min(0)] private int minimumPhase;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float recoveryDuration = 0.8f;

        public float Weight => weight;
        public float RecoveryDuration => recoveryDuration;

        public virtual bool CanExecute(BossContext context)
        {
            if (!isActiveAndEnabled || context.Target == null || context.Boss.CurrentPhase < minimumPhase)
                return false;
            float distanceSquared = (context.Target.position - transform.position).sqrMagnitude;
            return distanceSquared >= minimumRange * minimumRange &&
                   distanceSquared <= maximumRange * maximumRange;
        }

        public abstract IEnumerator Execute(BossContext context);
        public virtual void Cancel() { }

        protected static IEnumerator Wait(float seconds)
        {
            if (seconds > 0f) yield return new WaitForSeconds(seconds);
        }
    }
}

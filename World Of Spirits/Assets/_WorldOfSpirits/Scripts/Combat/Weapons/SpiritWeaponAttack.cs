using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public abstract class SpiritWeaponAttack : MonoBehaviour
    {
        private float nextAttackTime;

        protected virtual float AttackCooldown => 0.75f;

        protected virtual void Update()
        {
            if (Time.time < nextAttackTime || !CanAttack())
            {
                return;
            }

            PerformAttack();
            nextAttackTime = Time.time + Mathf.Max(0.05f, AttackCooldown);
        }

        protected abstract bool CanAttack();
        protected abstract void PerformAttack();
    }
}

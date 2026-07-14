using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public interface IDamageable
    {
        Faction Faction { get; }
        bool IsAlive { get; }
        Transform Transform { get; }
        void TakeDamage(float amount);
    }
}

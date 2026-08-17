using System;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Enemies
{
    public interface IBoss : IDamageable
    {
        string BossName { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        event Action<float, float> HealthChanged;
        event Action BossDefeated;
    }
}

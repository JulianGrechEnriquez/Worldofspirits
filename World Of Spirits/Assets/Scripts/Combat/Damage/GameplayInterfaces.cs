using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Combat
{
    public interface ITargetable
    {
        Faction Faction { get; }
        bool IsAlive { get; }
        Transform Transform { get; }
    }

    public interface IHealable
    {
        void Heal(float amount);
    }

    public interface IRewardSource
    {
        float ExperienceReward { get; }
    }

    public interface IEnemyClassification
    {
        bool IsElite { get; }
        bool IsBoss { get; }
    }

    public interface IUpgradeable
    {
        bool TryApply(UpgradeCardDefinition card);
    }
}

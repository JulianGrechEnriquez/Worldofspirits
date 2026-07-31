using System;
using UnityEngine;

namespace WorldOfSpirits.Progression.Upgrades
{
    public sealed class PlayerLevelSystem : MonoBehaviour
    {
        [Header("Level Curve")]
        [SerializeField, Min(1)] private int startingLevel = 1;
        [SerializeField, Min(1f)] private float firstLevelExperience = 10f;
        [SerializeField, Min(1f)] private float experienceGrowth = 1.18f;

        private int level;
        private float experience;
        private float requiredExperience;
        private UpgradeRuntimeStats runtimeStats;

        public int Level => level;
        public float Experience => experience;
        public float RequiredExperience => requiredExperience;
        public event Action<int> LevelGained;
        public event Action<float, float> ExperienceChanged;

        private void Awake()
        {
            runtimeStats = GetComponent<UpgradeRuntimeStats>();
            level = Mathf.Max(1, startingLevel);
            requiredExperience = CalculateRequirement(level);
        }

        public void AddExperience(float amount)
        {
            if (amount <= 0f) return;
            experience += amount * (runtimeStats != null ? runtimeStats.GetMultiplier(UpgradeStat.ExperienceGain) : 1f);
            while (experience >= requiredExperience)
            {
                experience -= requiredExperience;
                level++;
                requiredExperience = CalculateRequirement(level);
                LevelGained?.Invoke(level);
            }
            ExperienceChanged?.Invoke(experience, requiredExperience);
        }

        private float CalculateRequirement(int targetLevel) =>
            firstLevelExperience * Mathf.Pow(experienceGrowth, Mathf.Max(0, targetLevel - 1));

        private void OnValidate()
        {
            startingLevel = Mathf.Max(1, startingLevel);
            firstLevelExperience = Mathf.Max(1f, firstLevelExperience);
            experienceGrowth = Mathf.Max(1f, experienceGrowth);
        }
    }
}

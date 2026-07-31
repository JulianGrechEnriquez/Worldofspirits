using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerProgressionHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Image experienceFill;
        [SerializeField] private TMP_Text experienceText;

        private PlayerLevelSystem levelSystem;

        private void Start()
        {
            levelSystem = FindFirstObjectByType<PlayerLevelSystem>();
            if (levelSystem == null)
            {
                Debug.LogError("The progression HUD could not find the player's PlayerLevelSystem.", this);
                enabled = false;
                return;
            }

            levelSystem.LevelGained += RefreshLevel;
            levelSystem.ExperienceChanged += RefreshExperience;
            RefreshExperience(levelSystem.Experience, levelSystem.RequiredExperience);
        }

        private void OnDestroy()
        {
            if (levelSystem == null) return;
            levelSystem.LevelGained -= RefreshLevel;
            levelSystem.ExperienceChanged -= RefreshExperience;
        }

        private void RefreshLevel(int level)
        {
            if (levelText != null) levelText.text = $"LEVEL {level}";
        }

        private void RefreshExperience(float current, float required)
        {
            RefreshLevel(levelSystem.Level);
            if (experienceFill != null)
                experienceFill.fillAmount = required > 0f ? Mathf.Clamp01(current / required) : 0f;
            if (experienceText != null)
                experienceText.text = $"{Mathf.FloorToInt(current)} / {Mathf.CeilToInt(required)} XP";
        }
    }
}

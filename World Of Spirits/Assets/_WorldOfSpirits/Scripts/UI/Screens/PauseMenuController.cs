using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using WorldOfSpirits.Core;
using WorldOfSpirits.Player;
using WorldOfSpirits.Progression.Upgrades;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class PauseMenuController : MonoBehaviour
    {
        [Header("Scene UI References")]
        [SerializeField] private TMP_Text playerStatsText;
        [SerializeField] private TMP_Text spiritStatsText;
        [SerializeField] private TMP_Text pickedCardsText;
        [SerializeField] private List<UpgradeCardView> pickedCardViews = new List<UpgradeCardView>();

        private PlayerCharacter player;
        private PlayerLevelSystem levels;
        private UpgradeRuntimeStats upgrades;
        private SpiritManager spirits;
        private void Awake()
        {
            FindPlayerData();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged += OnGameStateChanged;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= OnGameStateChanged;
        }

        public void TogglePause()
        {
            if (GameManager.Instance != null) GameManager.Instance.TogglePause();
        }

        public void Pause()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            if (newState != GameState.Paused) return;
            FindPlayerData();
            RefreshContents();
        }

        private void FindPlayerData()
        {
            if (player == null) player = FindFirstObjectByType<PlayerCharacter>();
            if (player == null) return;
            levels = player.GetComponent<PlayerLevelSystem>();
            upgrades = player.GetComponent<UpgradeRuntimeStats>();
            spirits = player.GetComponent<SpiritManager>();
        }

        private void RefreshContents()
        {
            if (playerStatsText != null) playerStatsText.text = BuildPlayerStats();
            if (spiritStatsText != null) spiritStatsText.text = BuildSpiritStats();
            RefreshPickedCards();
        }

        private string BuildPlayerStats()
        {
            if (player == null) return "Player not found";
            float movement = upgrades != null ? upgrades.GetMultiplier(UpgradeStat.MovementSpeed) : 1f;
            StringBuilder text = new StringBuilder();
            text.AppendLine($"<b>Level</b>  {(levels != null ? levels.Level : 1)}");
            if (levels != null) text.AppendLine($"<b>XP</b>  {levels.Experience:0.#} / {levels.RequiredExperience:0.#}");
            text.AppendLine($"<b>Health</b>  {player.CurrentHealth:0.#} / {player.MaxHealth:0.#}");
            text.AppendLine($"<b>Shield</b>  {player.CurrentShield:0.#}");
            text.AppendLine($"<b>Move Speed</b>  {player.MoveSpeed * movement:0.##}");
            if (upgrades != null)
            {
                text.AppendLine($"<b>Damage</b>  x{upgrades.GetMultiplier(UpgradeStat.AttackDamage):0.##}");
                text.AppendLine($"<b>Attack Speed</b>  x{upgrades.GetMultiplier(UpgradeStat.AttackSpeed):0.##}");
                text.AppendLine($"<b>Spirit Damage</b>  x{upgrades.GetMultiplier(UpgradeStat.SpiritDamage):0.##}");
                text.AppendLine($"<b>Armor</b>  {upgrades.GetFlat(UpgradeStat.Armor):0.#}");
                text.AppendLine($"<b>Critical Chance</b>  {upgrades.GetFlat(UpgradeStat.CriticalChance) * 100f:0.#}%");
                text.AppendLine($"<b>Regeneration</b>  {upgrades.GetFlat(UpgradeStat.HealthRegeneration):0.##}/s");
            }
            return text.ToString();
        }

        private string BuildSpiritStats()
        {
            if (spirits == null || spirits.SpiritCount == 0) return "No spirits contracted yet.";
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < spirits.SpiritCount; i++)
            {
                SpiritMember spirit = spirits.GetSpiritAt(i);
                if (spirit == null) continue;
                string spiritName = spirit.Definition != null && !string.IsNullOrWhiteSpace(spirit.Definition.SpiritName)
                    ? spirit.Definition.SpiritName : spirit.name.Replace("(Clone)", string.Empty);
                text.AppendLine($"<b>{(i == 0 ? "★ " : string.Empty)}{spiritName}</b>");
                text.AppendLine($"Weapon Level  {spirit.Progression.WeaponLevel}");
                int count = spirit.Definition != null && spirit.Definition.RuntimeAbilities.Count > 0
                    ? spirit.Definition.RuntimeAbilities.Count : spirit.Definition != null ? spirit.Definition.Abilities.Count : 0;
                for (int ability = 0; ability < count; ability++)
                {
                    string abilityName = spirit.Definition.RuntimeAbilities.Count > 0
                        ? spirit.Definition.RuntimeAbilities[ability].name : spirit.Definition.Abilities[ability].AbilityName;
                    text.AppendLine($"{abilityName}  Lv.{spirit.Progression.GetAbilityLevel(ability)}");
                }
                text.AppendLine();
            }
            return text.ToString();
        }

        private void RefreshPickedCards()
        {
            if (pickedCardsText != null) pickedCardsText.gameObject.SetActive(false);
            Dictionary<string, UpgradeCardDefinition> definitions = new Dictionary<string, UpgradeCardDefinition>();
            foreach (UpgradeCatalog catalog in Resources.FindObjectsOfTypeAll<UpgradeCatalog>())
                foreach (UpgradeCardDefinition card in catalog.Cards)
                    if (card != null) definitions[card.Id] = card;

            int viewIndex = 0;
            if (upgrades != null)
            foreach (KeyValuePair<string, int> pair in upgrades.CardLevels)
            {
                if (viewIndex >= pickedCardViews.Count) break;
                if (!definitions.TryGetValue(pair.Key, out UpgradeCardDefinition card)) continue;
                pickedCardViews[viewIndex].BindDisplay(card, pair.Value);
                viewIndex++;
            }

            for (int i = viewIndex; i < pickedCardViews.Count; i++)
                pickedCardViews[i].Hide();
        }
    }
}

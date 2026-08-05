using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.UI
{
    /// <summary>
    /// Development-only picker for applying any authored upgrade directly to
    /// the current run. Toggle with F4 or the persistent on-screen button.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UpgradeTestPanel : MonoBehaviour
    {
        [SerializeField] private UpgradeCatalog catalog;
        [SerializeField] private bool visible;
        [SerializeField] private Key toggleKey = Key.F4;

        private readonly List<UpgradeCardDefinition> visibleCards =
            new List<UpgradeCardDefinition>(128);
        private UpgradeRuntimeStats runtimeStats;
        private Rect windowRect;
        private Vector2 scrollPosition;
        private string search = string.Empty;
        private string statusMessage = "Choose an upgrade to add it to this run.";
        private UpgradeCategory? categoryFilter;
        private GUIStyle titleStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle statusStyle;

        private void Awake()
        {
            FindRuntimeStats();
            windowRect = new Rect(
                Mathf.Max(12f, (Screen.width - 820f) * 0.5f),
                55f,
                Mathf.Min(820f, Screen.width - 24f),
                Mathf.Max(360f, Screen.height - 90f));
            RebuildVisibleCards();
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                visible = !visible;
            }

            if (runtimeStats == null) FindRuntimeStats();
        }

        private void FindRuntimeStats()
        {
            runtimeStats = FindFirstObjectByType<UpgradeRuntimeStats>();
        }

        private void OnGUI()
        {
            EnsureStyles();

            string toggleLabel = visible ? "Close Upgrade Tester [F4]" : "Open Upgrade Tester [F4]";
            if (GUI.Button(new Rect(Screen.width - 224f, 12f, 212f, 36f), toggleLabel))
                visible = !visible;

            if (!visible) return;

            windowRect.width = Mathf.Min(820f, Screen.width - 24f);
            windowRect.height = Mathf.Max(360f, Screen.height - 90f);
            windowRect.x = Mathf.Clamp(windowRect.x, 0f, Screen.width - windowRect.width);
            windowRect.y = Mathf.Clamp(windowRect.y, 0f, Screen.height - windowRect.height);
            windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "UPGRADE TESTER");
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.Space(4f);
            GUILayout.Label(
                "Apply any upgrade directly to the current run. Spirit upgrades require that spirit's contract first.",
                descriptionStyle);

            if (runtimeStats == null)
            {
                GUILayout.Label("Player UpgradeRuntimeStats not found. Start the run before applying cards.", statusStyle);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(52f));
            string nextSearch = GUILayout.TextField(search ?? string.Empty);
            if (!string.Equals(nextSearch, search, StringComparison.Ordinal))
            {
                search = nextSearch;
                RebuildVisibleCards();
            }
            if (GUILayout.Button("Clear", GUILayout.Width(62f)))
            {
                search = string.Empty;
                RebuildVisibleCards();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawCategoryButton("All", null);
            foreach (UpgradeCategory category in Enum.GetValues(typeof(UpgradeCategory)))
                DrawCategoryButton(category.ToString(), category);
            GUILayout.EndHorizontal();

            GUILayout.Label(
                $"Showing {visibleCards.Count} / {(catalog != null ? catalog.Cards.Count : 0)} upgrades",
                descriptionStyle);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < visibleCards.Count; i++)
                DrawCard(visibleCards[i]);
            GUILayout.EndScrollView();

            GUILayout.Label(statusMessage, statusStyle);
            GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 28f));
        }

        private void DrawCategoryButton(string label, UpgradeCategory? category)
        {
            bool selected = categoryFilter == category;
            GUI.enabled = !selected;
            if (GUILayout.Button(label, GUILayout.MinWidth(54f)))
            {
                categoryFilter = category;
                RebuildVisibleCards();
            }
            GUI.enabled = true;
        }

        private void DrawCard(UpgradeCardDefinition card)
        {
            if (card == null) return;

            int currentLevel = runtimeStats != null
                ? runtimeStats.GetCardLevel(card.Id)
                : 0;
            bool atMaximum = !card.RepeatableAfterMaximum &&
                currentLevel >= card.MaximumLevel;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(
                $"{card.CardName}  [{card.Rarity}]  {card.Category}",
                cardTitleStyle);
            GUILayout.Label(card.Description, descriptionStyle);
            GUILayout.Label(
                $"Level {currentLevel}/{card.MaximumLevel}" +
                (card.TargetSpirit != null ? $"  •  {card.TargetSpirit.SpiritName}" : string.Empty),
                descriptionStyle);
            GUILayout.EndVertical();

            GUI.enabled = runtimeStats != null && !atMaximum;
            string buttonText = atMaximum ? "MAX" : "APPLY";
            if (GUILayout.Button(buttonText, GUILayout.Width(92f), GUILayout.Height(54f)))
                ApplyCard(card);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void ApplyCard(UpgradeCardDefinition card)
        {
            if (runtimeStats == null)
            {
                FindRuntimeStats();
                if (runtimeStats == null)
                {
                    statusMessage = "Could not apply: the player upgrade system is not active.";
                    return;
                }
            }

            bool applied = runtimeStats.TryApply(card);
            int level = runtimeStats.GetCardLevel(card.Id);
            statusMessage = applied
                ? $"Applied {card.CardName} — now level {level}."
                : $"Could not apply {card.CardName}. Check its contract, spirit, or maximum-level requirement.";
        }

        private void RebuildVisibleCards()
        {
            visibleCards.Clear();
            if (catalog == null) return;

            string query = (search ?? string.Empty).Trim();
            for (int i = 0; i < catalog.Cards.Count; i++)
            {
                UpgradeCardDefinition card = catalog.Cards[i];
                if (card == null || categoryFilter.HasValue && card.Category != categoryFilter.Value)
                    continue;
                if (query.Length > 0 &&
                    card.CardName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    (string.IsNullOrEmpty(card.Description) ||
                     card.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0))
                    continue;
                visibleCards.Add(card);
            }

            visibleCards.Sort((left, right) =>
            {
                int category = left.Category.CompareTo(right.Category);
                return category != 0
                    ? category
                    : string.Compare(left.CardName, right.CardName, StringComparison.OrdinalIgnoreCase);
            });
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.65f, 0.9f, 1f) }
            };
            descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.92f, 0.95f) }
            };
            statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.85f, 0.35f) }
            };
        }
    }
}

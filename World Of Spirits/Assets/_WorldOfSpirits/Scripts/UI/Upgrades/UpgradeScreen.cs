using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WorldOfSpirits.Progression.Upgrades
{
    public sealed class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private UpgradeSelectionManager selectionManager;
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Assign three pre-created card views. No runtime Instantiate is used.")]
        [SerializeField] private List<UpgradeCardView> cardViews = new List<UpgradeCardView>(3);
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollLabel;
        [SerializeField] private TMP_Text spiritDustLabel;
        private UpgradeRuntimeStats runtimeStats;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (selectionManager != null) runtimeStats = selectionManager.GetComponent<UpgradeRuntimeStats>();
            if (rerollButton == null) rerollButton = transform.Find("Reroll Button")?.GetComponent<Button>();
            if (rerollLabel == null && rerollButton != null) rerollLabel = rerollButton.GetComponentInChildren<TMP_Text>();
            if (spiritDustLabel == null) spiritDustLabel = transform.Find("Spirit Dust")?.GetComponent<TMP_Text>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (selectionManager == null) return;
            selectionManager.ChoicesReady += Show;
            selectionManager.CardChosen += OnChosen;
            if (rerollButton != null) rerollButton.onClick.AddListener(OnRerollClicked);
            if (selectionManager.SpiritDustWallet != null)
                selectionManager.SpiritDustWallet.BalanceChanged += OnSpiritDustChanged;
            RefreshCurrency();
        }

        private void OnDisable()
        {
            if (selectionManager == null) return;
            selectionManager.ChoicesReady -= Show;
            selectionManager.CardChosen -= OnChosen;
            if (rerollButton != null) rerollButton.onClick.RemoveListener(OnRerollClicked);
            if (selectionManager.SpiritDustWallet != null)
                selectionManager.SpiritDustWallet.BalanceChanged -= OnSpiritDustChanged;
        }

        private void Show(IReadOnlyList<UpgradeCardDefinition> choices)
        {
            SetVisible(true);
            RefreshCurrency();
            for (int i = 0; i < cardViews.Count; i++)
            {
                if (i < choices.Count)
                {
                    int level = runtimeStats != null ? runtimeStats.GetCardLevel(choices[i].Id) : 0;
                    cardViews[i].Bind(selectionManager, choices[i], i, level);
                }
                else cardViews[i].Hide();
            }
        }

        private void OnRerollClicked()
        {
            selectionManager?.TryReroll();
            RefreshCurrency();
        }

        private void OnSpiritDustChanged(int unused) => RefreshCurrency();

        private void RefreshCurrency()
        {
            if (selectionManager == null || selectionManager.SpiritDustWallet == null) return;
            int balance = selectionManager.SpiritDustWallet.Balance;
            int cost = selectionManager.RerollSpiritDustCost;
            if (spiritDustLabel != null) spiritDustLabel.text = $"Spirit Dust: {balance}";
            if (rerollLabel != null) rerollLabel.text = $"Reroll ({cost} Spirit Dust)";
            if (rerollButton != null) rerollButton.interactable = balance >= cost;
        }

        private void OnChosen(UpgradeCardDefinition unused)
        {
            if (!selectionManager.IsSelectionOpen) SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}

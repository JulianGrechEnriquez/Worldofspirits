using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Progression.Upgrades
{
    public sealed class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private UpgradeSelectionManager selectionManager;
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Assign three pre-created card views. No runtime Instantiate is used.")]
        [SerializeField] private List<UpgradeCardView> cardViews = new List<UpgradeCardView>(3);
        private UpgradeRuntimeStats runtimeStats;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (selectionManager != null) runtimeStats = selectionManager.GetComponent<UpgradeRuntimeStats>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (selectionManager == null) return;
            selectionManager.ChoicesReady += Show;
            selectionManager.CardChosen += OnChosen;
        }

        private void OnDisable()
        {
            if (selectionManager == null) return;
            selectionManager.ChoicesReady -= Show;
            selectionManager.CardChosen -= OnChosen;
        }

        private void Show(IReadOnlyList<UpgradeCardDefinition> choices)
        {
            SetVisible(true);
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

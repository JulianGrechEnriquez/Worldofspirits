using UnityEngine;
using UnityEngine.UI;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class StarterSpiritUnlockTestButton : MonoBehaviour
    {
        public enum TestAction
        {
            UnlockAllSpirits,
            ResetUnlocks
        }

        [SerializeField] private TestAction action;

        private Button button;
        private StarterSpiritSelectionController selectionController;

        private void Awake()
        {
            button = GetComponent<Button>();
            selectionController = GetComponentInParent<StarterSpiritSelectionController>();
        }

        private void OnEnable()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            if (selectionController == null)
                selectionController = GetComponentInParent<StarterSpiritSelectionController>();

            if (selectionController == null) return;

            if (action == TestAction.UnlockAllSpirits)
                selectionController.UnlockAllSpiritsForTesting();
            else
                selectionController.ResetSpiritUnlocksForTesting();
        }
    }
}

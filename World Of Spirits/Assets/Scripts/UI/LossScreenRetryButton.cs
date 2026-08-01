using UnityEngine;
using UnityEngine.UI;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class LossScreenRetryButton : MonoBehaviour
    {
        private Button button;
        private LossScreenController controller;

        private void Awake()
        {
            button = GetComponent<Button>();
            controller = FindFirstObjectByType<LossScreenController>();
        }

        private void OnEnable()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(Retry);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(Retry);
        }

        private void Retry()
        {
            if (controller == null) controller = FindFirstObjectByType<LossScreenController>();
            if (controller != null) controller.Retry();
        }
    }
}

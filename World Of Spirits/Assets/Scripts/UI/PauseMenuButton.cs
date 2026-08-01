using UnityEngine;
using UnityEngine.EventSystems;

namespace WorldOfSpirits.UI
{
    public sealed class PauseMenuButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private PauseMenuController controller;
        [SerializeField] private bool resumeButton;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (controller == null) return;
            if (resumeButton) controller.Resume();
            else controller.TogglePause();
        }
    }
}

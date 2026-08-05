using System;
using UnityEngine;
using UnityEngine.UI;

namespace WorldOfSpirits.UI
{
    public enum UIAction
    {
        TogglePause,
        Pause,
        Resume,
        RetryCurrentScene,
        ReturnToPreviousState,
        ShowScreen,
        HideScreen
    }

    public enum UIScreen
    {
        GameHud,
        Progression,
        PauseControls,
        PauseMenu,
        UpgradeSelection,
        MainMenu,
        LossScreen
    }

    public readonly struct UIActionRequest
    {
        public UIActionRequest(UIAction action, UIScreen screen)
        {
            Action = action;
            Screen = screen;
        }

        public UIAction Action { get; }
        public UIScreen Screen { get; }
    }

    public static class UIActionSignals
    {
        public static event Action<UIActionRequest> Raised;

        public static void Raise(UIAction action, UIScreen screen)
        {
            Raised?.Invoke(new UIActionRequest(action, screen));
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIActionButton : MonoBehaviour
    {
        [SerializeField] private UIAction action;
        [SerializeField] private UIScreen screen;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(RaiseAction);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(RaiseAction);
        }

        private void RaiseAction()
        {
            UIActionSignals.Raise(action, screen);
        }
    }
}

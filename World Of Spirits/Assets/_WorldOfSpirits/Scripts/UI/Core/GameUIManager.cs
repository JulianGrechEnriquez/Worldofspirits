using UnityEngine;
using UnityEngine.SceneManagement;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class GameUIManager : MonoBehaviour
    {
        [Header("Always-authored Canvas Objects")]
        [SerializeField] private GameObject gameHudCanvas;
        [SerializeField] private GameObject progressionCanvas;
        [SerializeField] private GameObject pauseCanvas;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private CanvasGroup upgradeCanvasGroup;
        [SerializeField] private GameObject mainMenuCanvas;
        [SerializeField] private GameObject lossScreen;

        private GameManager gameManager;
        private bool gameManagerSubscribed;

        private void OnEnable()
        {
            UIActionSignals.Raised += OnUIAction;
            BindGameManager();
        }

        private void Start()
        {
            BindGameManager();
            ApplyState(gameManager != null ? gameManager.CurrentState : GameState.Playing);
        }

        private void OnDisable()
        {
            UIActionSignals.Raised -= OnUIAction;
            if (gameManager != null && gameManagerSubscribed)
            {
                gameManager.StateChanged -= OnGameStateChanged;
                gameManagerSubscribed = false;
            }
        }

        private void BindGameManager()
        {
            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
                if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
            }

            if (gameManager != null && !gameManagerSubscribed)
            {
                gameManager.StateChanged += OnGameStateChanged;
                gameManagerSubscribed = true;
            }
        }

        private void OnUIAction(UIActionRequest request)
        {
            BindGameManager();

            switch (request.Action)
            {
                case UIAction.TogglePause:
                    if (gameManager != null) gameManager.TogglePause();
                    break;
                case UIAction.Pause:
                    if (gameManager != null) gameManager.SetState(GameState.Paused);
                    break;
                case UIAction.Resume:
                    if (gameManager != null) gameManager.SetState(GameState.Playing);
                    break;
                case UIAction.RetryCurrentScene:
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
                case UIAction.ReturnToPreviousState:
                    if (gameManager != null) gameManager.ReturnToPreviousState();
                    break;
                case UIAction.ShowScreen:
                    SetScreenVisible(request.Screen, true);
                    break;
                case UIAction.HideScreen:
                    SetScreenVisible(request.Screen, false);
                    break;
            }
        }

        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            ApplyState(newState);
        }

        public void ApplyState(GameState state)
        {
            bool gameplayVisible = state != GameState.MainMenu && state != GameState.GameOver;
            if (gameHudCanvas != null) gameHudCanvas.SetActive(gameplayVisible);
            if (progressionCanvas != null) progressionCanvas.SetActive(gameplayVisible);
            if (pauseCanvas != null) pauseCanvas.SetActive(gameplayVisible);
            if (pauseMenu != null) pauseMenu.SetActive(state == GameState.Paused);
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(state == GameState.MainMenu);
            if (lossScreen != null) lossScreen.SetActive(state == GameState.GameOver);

            if (upgradeCanvasGroup != null)
            {
                bool levelUp = state == GameState.LevelUp;
                upgradeCanvasGroup.alpha = levelUp ? 1f : 0f;
                upgradeCanvasGroup.interactable = levelUp;
                upgradeCanvasGroup.blocksRaycasts = levelUp;
            }
        }

        public void Show(UIScreen screen)
        {
            SetScreenVisible(screen, true);
        }

        public void Hide(UIScreen screen)
        {
            SetScreenVisible(screen, false);
        }

        private void SetScreenVisible(UIScreen screen, bool visible)
        {
            switch (screen)
            {
                case UIScreen.GameHud:
                    SetActive(gameHudCanvas, visible);
                    break;
                case UIScreen.Progression:
                    SetActive(progressionCanvas, visible);
                    break;
                case UIScreen.PauseControls:
                    SetActive(pauseCanvas, visible);
                    break;
                case UIScreen.PauseMenu:
                    SetActive(pauseMenu, visible);
                    break;
                case UIScreen.UpgradeSelection:
                    SetCanvasGroupVisible(upgradeCanvasGroup, visible);
                    break;
                case UIScreen.MainMenu:
                    SetActive(mainMenuCanvas, visible);
                    break;
                case UIScreen.LossScreen:
                    SetActive(lossScreen, visible);
                    break;
            }
        }

        private static void SetActive(GameObject target, bool visible)
        {
            if (target != null) target.SetActive(visible);
        }

        private static void SetCanvasGroupVisible(CanvasGroup target, bool visible)
        {
            if (target == null) return;
            target.alpha = visible ? 1f : 0f;
            target.interactable = visible;
            target.blocksRaycasts = visible;
        }
    }
}

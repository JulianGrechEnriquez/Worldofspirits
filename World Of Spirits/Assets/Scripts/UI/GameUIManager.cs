using UnityEngine;
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

        private void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null) gameManager.StateChanged += OnGameStateChanged;
            ApplyState(gameManager != null ? gameManager.CurrentState : GameState.Playing);
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.StateChanged -= OnGameStateChanged;
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
    }
}

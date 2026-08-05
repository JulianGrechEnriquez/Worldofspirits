using System;
using UnityEngine;

namespace WorldOfSpirits.Core
{
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameState initialState = GameState.Playing;

        private static GameManager instance;
        private GameState previousState;

        public static GameManager Instance => instance;
        public GameState CurrentState { get; private set; }
        public GameState PreviousState => previousState;
        public bool IsPaused => CurrentState == GameState.Paused;

        public event Action<GameState, GameState> StateChanged;
        public event Action<GameState, GameState> GameStateChanged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            CurrentState = initialState;
            previousState = initialState;
            ApplyTimeScale(CurrentState);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public void SetState(GameState newState)
        {
            if (newState == CurrentState) return;
            GameState oldState = CurrentState;
            previousState = oldState;
            CurrentState = newState;
            ApplyTimeScale(newState);
            StateChanged?.Invoke(oldState, newState);
            GameStateChanged?.Invoke(oldState, newState);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing) SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused) SetState(GameState.Playing);
        }

        public void ReturnToPreviousState()
        {
            SetState(previousState == CurrentState ? GameState.Playing : previousState);
        }

        private static void ApplyTimeScale(GameState state)
        {
            Time.timeScale = state == GameState.Paused ||
                state == GameState.LevelUp ||
                state == GameState.GameOver ? 0f : 1f;
        }
    }
}

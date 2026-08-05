using UnityEngine;
using WorldOfSpirits.Core;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class LossScreenController : MonoBehaviour
    {
        private PlayerCharacter player;

        private void OnEnable()
        {
            BindPlayer();
        }

        private void Start()
        {
            BindPlayer();
        }

        private void OnDisable()
        {
            UnbindPlayer();
        }

        private void BindPlayer()
        {
            if (player != null) return;
            player = FindFirstObjectByType<PlayerCharacter>();
            if (player != null) player.PlayerDied += HandlePlayerDied;
        }

        private void UnbindPlayer()
        {
            if (player != null) player.PlayerDied -= HandlePlayerDied;
            player = null;
        }

        private static void HandlePlayerDied()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.GameOver);
        }
    }
}

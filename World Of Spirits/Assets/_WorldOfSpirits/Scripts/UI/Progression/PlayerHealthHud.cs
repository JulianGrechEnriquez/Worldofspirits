using TMPro;
using UnityEngine;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHud : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private TMP_Text healthText;

        private bool subscribed;

        private void Awake()
        {
            if (player == null) player = FindFirstObjectByType<PlayerCharacter>();
            if (healthText == null) healthText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            BindAndRefresh();
        }

        private void Start() => BindAndRefresh();

        private void OnDisable()
        {
            if (player != null && subscribed)
                player.HealthChanged -= Refresh;
            subscribed = false;
        }

        private void BindAndRefresh()
        {
            if (player == null) player = FindFirstObjectByType<PlayerCharacter>();
            if (healthText == null) healthText = GetComponent<TMP_Text>();
            if (player == null || healthText == null) return;

            if (!subscribed)
            {
                player.HealthChanged += Refresh;
                subscribed = true;
            }
            Refresh(player.CurrentHealth, player.MaxHealth);
        }

        private void Refresh(float current, float maximum)
        {
            healthText.SetText("HP  {0:0} / {1:0}", current, maximum);
        }
    }
}

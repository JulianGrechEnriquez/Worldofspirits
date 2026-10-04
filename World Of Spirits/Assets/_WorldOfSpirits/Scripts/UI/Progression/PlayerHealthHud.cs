using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHud : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthyColor = new Color(0.88f, 0.23f, 0.29f);
        [SerializeField] private Color criticalColor = new Color(1f, 0.43f, 0.22f);

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
            if (player == null || (healthText == null && healthFill == null)) return;

            if (!subscribed)
            {
                player.HealthChanged += Refresh;
                subscribed = true;
            }
            Refresh(player.CurrentHealth, player.MaxHealth);
        }

        private void Refresh(float current, float maximum)
        {
            if (healthText != null)
                healthText.SetText("HP  {0:0} / {1:0}", current, maximum);
            if (healthFill != null)
            {
                float fraction = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
                healthFill.fillAmount = fraction;
                healthFill.color = fraction <= 0.25f ? criticalColor : healthyColor;
            }
        }
    }
}

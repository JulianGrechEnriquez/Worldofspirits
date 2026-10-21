using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfSpirits.Enemies;
using WorldOfSpirits.Spawning;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class BossUI : MonoBehaviour
    {
        [SerializeField] private SpawnDirector spawnDirector;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text bossNameText;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private TMP_Text bossHealthText;
        [SerializeField, Min(0.05f)] private float healthBarDrainSpeed = 0.75f;
        [Tooltip("Player level and XP HUD hidden while a boss encounter is active.")]
        [SerializeField] private GameObject playerProgressionHud;

        private IBoss boundBoss;
        private bool progressionHudWasVisible;
        private bool hasCapturedProgressionState;
        private float targetHealthFill = 1f;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (spawnDirector == null) spawnDirector = FindFirstObjectByType<SpawnDirector>();
            if (spawnDirector != null)
            {
                spawnDirector.BossStarted += OnBossStarted;
                spawnDirector.BossDefeated += OnBossDefeated;
            }
        }

        private void OnDisable()
        {
            if (spawnDirector != null)
            {
                spawnDirector.BossStarted -= OnBossStarted;
                spawnDirector.BossDefeated -= OnBossDefeated;
            }
            Unbind();
            RestorePlayerProgressionHud();
        }

        public void Bind(IBoss boss)
        {
            Unbind();
            boundBoss = boss;
            if (boundBoss == null) return;
            boundBoss.HealthChanged += OnHealthChanged;
            boundBoss.BossDefeated += OnBoundBossDefeated;
            if (bossNameText != null) bossNameText.text = boundBoss.BossName;
            ApplyHealth(boundBoss.CurrentHealth, boundBoss.MaxHealth, true);
            if (playerProgressionHud != null)
            {
                progressionHudWasVisible = playerProgressionHud.activeSelf;
                hasCapturedProgressionState = true;
                playerProgressionHud.SetActive(false);
            }
            SetVisible(true);
        }

        private void Unbind()
        {
            if (boundBoss == null) return;
            boundBoss.HealthChanged -= OnHealthChanged;
            boundBoss.BossDefeated -= OnBoundBossDefeated;
            boundBoss = null;
        }

        private void OnBossStarted(EnemyBase enemy)
        {
            if (enemy is IBoss boss) Bind(boss);
        }

        private void OnBossDefeated(EnemyBase enemy) => Hide();
        private void OnBoundBossDefeated() => Hide();

        private void OnHealthChanged(float current, float maximum)
        {
            ApplyHealth(current, maximum, false);
        }

        private void Update()
        {
            if (healthBarFill == null || boundBoss == null) return;
            healthBarFill.fillAmount = Mathf.MoveTowards(
                healthBarFill.fillAmount,
                targetHealthFill,
                healthBarDrainSpeed * Time.unscaledDeltaTime);
        }

        private void ApplyHealth(float current, float maximum, bool immediate)
        {
            targetHealthFill = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
            if (immediate && healthBarFill != null)
                healthBarFill.fillAmount = targetHealthFill;
            if (bossHealthText != null)
                bossHealthText.SetText("{0:0} / {1:0}", current, maximum);
        }

        private void Hide()
        {
            SetVisible(false);
            Unbind();
            RestorePlayerProgressionHud();
        }

        private void RestorePlayerProgressionHud()
        {
            if (playerProgressionHud != null && hasCapturedProgressionState)
                playerProgressionHud.SetActive(progressionHudWasVisible);
            hasCapturedProgressionState = false;
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}

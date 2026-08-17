using System.Collections;
using TMPro;
using UnityEngine;
using WorldOfSpirits.Enemies;
using WorldOfSpirits.Spawning;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class RunTimerHud : MonoBehaviour
    {
        [SerializeField] private SpawnDirector spawnDirector;
        [SerializeField] private BossEncounterController bossEncounterController;
        [SerializeField] private TMP_Text timerText;

        [Header("Boss Warning")]
        [SerializeField] private Color warningColor = new Color(1f, 0.2f, 0.05f, 1f);
        [SerializeField, Min(0.05f)] private float flashInterval = 0.25f;

        private int displayedSecond = -1;
        private Color normalColor;
        private Coroutine flashRoutine;

        private void Awake()
        {
            if (spawnDirector == null)
            {
                spawnDirector = FindFirstObjectByType<SpawnDirector>();
            }

            if (timerText == null)
            {
                timerText = GetComponent<TMP_Text>();
            }

            if (bossEncounterController == null)
                bossEncounterController = FindFirstObjectByType<BossEncounterController>();

            if (spawnDirector == null || timerText == null)
            {
                Debug.LogError("Run timer requires a SpawnDirector and TMP text component.", this);
                enabled = false;
                return;
            }

            Refresh(0);
            normalColor = timerText.color;
        }

        private void OnEnable()
        {
            if (bossEncounterController == null)
                bossEncounterController = FindFirstObjectByType<BossEncounterController>();
            if (bossEncounterController != null)
                bossEncounterController.BossCountdownStarted += BeginBossWarning;
            if (spawnDirector != null)
                spawnDirector.BossStarted += HandleBossStarted;
        }

        private void Start()
        {
            // OnEnable runs before Awake has auto-resolved scene references.
            if (bossEncounterController != null)
            {
                bossEncounterController.BossCountdownStarted -= BeginBossWarning;
                bossEncounterController.BossCountdownStarted += BeginBossWarning;
            }
            if (spawnDirector != null)
            {
                spawnDirector.BossStarted -= HandleBossStarted;
                spawnDirector.BossStarted += HandleBossStarted;
            }
        }

        private void OnDisable()
        {
            if (bossEncounterController != null)
                bossEncounterController.BossCountdownStarted -= BeginBossWarning;
            if (spawnDirector != null)
                spawnDirector.BossStarted -= HandleBossStarted;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        private void Update()
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(spawnDirector.ElapsedRunTime));
            if (totalSeconds == displayedSecond)
            {
                return;
            }

            Refresh(totalSeconds);
        }

        private void Refresh(int totalSeconds)
        {
            displayedSecond = totalSeconds;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.SetText("{0:00}:{1:00}", minutes, seconds);
        }

        private void BeginBossWarning(float duration)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(duration));
        }

        private IEnumerator FlashRoutine(float duration)
        {
            float endTime = Time.unscaledTime + duration;
            bool warning = false;
            while (Time.unscaledTime < endTime)
            {
                warning = !warning;
                timerText.color = warning ? warningColor : normalColor;
                yield return new WaitForSecondsRealtime(flashInterval);
            }
            timerText.color = normalColor;
            flashRoutine = null;
        }

        private void HandleBossStarted(EnemyBase boss)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = null;
            timerText.color = normalColor;
            gameObject.SetActive(false);
        }
    }
}

using TMPro;
using UnityEngine;
using WorldOfSpirits.Spawning;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class RunTimerHud : MonoBehaviour
    {
        [SerializeField] private SpawnDirector spawnDirector;
        [SerializeField] private TMP_Text timerText;

        private int displayedSecond = -1;

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

            if (spawnDirector == null || timerText == null)
            {
                Debug.LogError("Run timer requires a SpawnDirector and TMP text component.", this);
                enabled = false;
                return;
            }

            Refresh(0);
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
    }
}

using UnityEngine;
using UnityEngine.UI;
using WorldOfSpirits.Spawning;

namespace WorldOfSpirits.UI.DebugTools
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class BossEncounterTestButton : MonoBehaviour
    {
        [SerializeField] private BossEncounterController encounterController;

        private Button button;

        private void Awake()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            gameObject.SetActive(false);
            return;
#endif
            button = GetComponent<Button>();
            if (encounterController == null)
                encounterController = FindFirstObjectByType<BossEncounterController>();
            button.onClick.AddListener(StartTestEncounter);
            button.interactable = encounterController != null;
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(StartTestEncounter);
        }

        public void StartTestEncounter()
        {
            if (encounterController == null) return;
            if (encounterController.BeginBossCountdown())
                button.interactable = false;
        }
    }
}

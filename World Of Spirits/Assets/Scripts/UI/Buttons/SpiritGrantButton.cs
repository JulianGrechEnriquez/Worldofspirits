using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.UI
{
    [RequireComponent(typeof(Button))]
    public class SpiritGrantButton : MonoBehaviour
    {
        [SerializeField] private GameObject spiritPrefab;
        [SerializeField] private SpiritManager spiritManager;
        [SerializeField] private bool disableAfterSuccess = true;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (spiritManager == null)
            {
                spiritManager = FindFirstObjectByType<SpiritManager>();
            }

            button.onClick.AddListener(GrantSpirit);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(GrantSpirit);
            }
        }

        public void GrantSpirit()
        {
            string label = GetButtonLabel();
            if (spiritManager == null)
            {
                Debug.LogError($"[{label}] Cannot grant a spirit because SpiritManager was not found.", this);
                return;
            }

            if (spiritPrefab == null)
            {
                Debug.LogError($"[{label}] No matching spirit prefab is assigned.", this);
                return;
            }

            bool added = spiritManager.TryAddSpirit(spiritPrefab);
            if (added && disableAfterSuccess)
            {
                button.interactable = false;
            }
        }

        private string GetButtonLabel()
        {
            TMP_Text label = GetComponentInChildren<TMP_Text>(true);
            if (label != null && !string.IsNullOrWhiteSpace(label.text) && label.text != "Button")
            {
                return label.text.Trim();
            }

            return name.Replace("Give", string.Empty).Replace("Sprite", string.Empty).Trim();
        }
    }
}

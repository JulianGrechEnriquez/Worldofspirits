using TMPro;
using UnityEngine;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class ShepherdLeadCompanionHud : MonoBehaviour
    {
        private TMP_Text label;
        private SpiritManager spirits;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            spirits = FindFirstObjectByType<SpiritManager>();
        }

        private void Update()
        {
            if (spirits == null) spirits = FindFirstObjectByType<SpiritManager>();
            bool visible = spirits != null && spirits.CompanionsOnly && spirits.PrimarySpirit != null;
            label.enabled = visible;
            if (visible)
                label.text = $"LEAD: {spirits.GetSpiritNameAt(0)}   +1 level to unlocked abilities   [Tab to change]";
        }
    }
}

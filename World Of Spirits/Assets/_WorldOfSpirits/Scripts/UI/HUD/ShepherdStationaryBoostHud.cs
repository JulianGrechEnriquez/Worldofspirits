using TMPro;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class ShepherdStationaryBoostHud : MonoBehaviour
    {
        private TMP_Text label;
        private UpgradeRuntimeStats stats;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            stats = FindFirstObjectByType<UpgradeRuntimeStats>();
        }

        private void Update()
        {
            if (stats == null) stats = FindFirstObjectByType<UpgradeRuntimeStats>();
            bool active = stats != null && stats.ShepherdStationaryBoostActive;
            label.enabled = active;
            if (!active) return;
            label.SetText("STATIONARY BOOST   Damage +{0:0}%   Cast speed +{1:0}%   Orbit speed +{2:0}%",
                (stats.CompanionDamageMultiplier - 1f) * 100f,
                (stats.CompanionCastSpeedMultiplier - 1f) * 100f,
                (stats.CompanionOrbitSpeedMultiplier - 1f) * 100f);
        }
    }
}

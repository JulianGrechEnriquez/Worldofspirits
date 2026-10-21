using TMPro;
using UnityEngine;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class WarriorArmamentHud : MonoBehaviour
    {
        private TMP_Text label;
        private SpiritWarriorController warrior;
        private float nextRefresh;
        private void Awake() => label = GetComponent<TMP_Text>();
        private void Update()
        {
            if (warrior == null) warrior = FindFirstObjectByType<SpiritWarriorController>();
            bool visible = warrior != null && warrior.Active && warrior.CurrentArmament != null;
            label.enabled = visible;
            if (!visible || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.1f;
            var armament = warrior.CurrentArmament;
            label.color = armament.colour;
            string dash = warrior.IsDashing ? "DASHING" : warrior.DashRemaining <= 0f
                ? "DASH READY" : $"DASH {warrior.DashRemaining:0.0}s";
            label.text = $"{armament.pairedWeaponName}  |  {armament.armourName}\n" +
                $"SPACE / SHIFT: {dash}   •   Momentum {warrior.Momentum * 100f:0}%   •   TAB: rotate";
        }
    }
}

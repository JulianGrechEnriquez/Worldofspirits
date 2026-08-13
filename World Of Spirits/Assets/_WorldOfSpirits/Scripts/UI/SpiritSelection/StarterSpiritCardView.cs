using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class StarterSpiritCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image portrait;
        [SerializeField] private Image border;
        [SerializeField] private TMP_Text spiritNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text weaponNameText;
        [SerializeField] private TMP_Text firstAbilityText;

        private GameObject spiritPrefab;
        private Action<GameObject> selected;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(Select);

            LayoutElement layout = GetComponent<LayoutElement>();
            if (layout == null) layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 260f;
            layout.preferredHeight = 360f;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;
        }

        public void Bind(GameObject prefab, Action<GameObject> onSelected)
        {
            spiritPrefab = prefab;
            selected = onSelected;
            SpiritMember member = prefab != null ? prefab.GetComponent<SpiritMember>() : null;
            SpiritDefinition definition = member != null ? member.Definition : null;
            if (definition == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.name = $"Starter Card - {definition.SpiritName}";
            if (spiritNameText != null) spiritNameText.text = definition.SpiritName;
            if (weaponNameText != null)
                weaponNameText.text = definition.RuntimeWeapon != null
                    ? definition.RuntimeWeapon.WeaponName : definition.Weapon.WeaponName;

            AbilityDefinition firstAbility = definition.RuntimeAbilities.Count > 0
                ? definition.RuntimeAbilities[0] : null;
            if (firstAbilityText != null)
                firstAbilityText.text = firstAbility != null
                    ? firstAbility.AbilityName : "No starting ability";
            if (descriptionText != null)
                descriptionText.text = firstAbility != null
                    ? firstAbility.Description : $"A {definition.Shape} spirit.";

            SpriteRenderer preview = prefab.GetComponentInChildren<SpriteRenderer>(true);
            if (portrait != null)
            {
                portrait.sprite = definition.CardPortrait != null
                    ? definition.CardPortrait
                    : preview != null ? preview.sprite : null;
                portrait.color = preview != null ? preview.color : Color.white;
                portrait.preserveAspect = true;
            }
            if (border != null) border.color = GetSpiritColor(definition.SpiritName);
            gameObject.SetActive(true);
        }

        private void Select()
        {
            if (spiritPrefab != null) selected?.Invoke(spiritPrefab);
        }

        private static Color GetSpiritColor(string spiritName)
        {
            string name = spiritName.ToLowerInvariant();
            if (name.Contains("fire")) return new Color(1f, 0.28f, 0.08f, 1f);
            if (name.Contains("ice")) return new Color(0.2f, 0.85f, 1f, 1f);
            if (name.Contains("earth")) return new Color(0.45f, 0.75f, 0.25f, 1f);
            if (name.Contains("water")) return new Color(0.1f, 0.5f, 1f, 1f);
            if (name.Contains("wind")) return new Color(0.55f, 1f, 0.75f, 1f);
            if (name.Contains("lightning")) return new Color(1f, 0.9f, 0.15f, 1f);
            if (name.Contains("poison")) return new Color(0.65f, 0.25f, 0.9f, 1f);
            return Color.white;
        }
    }
}

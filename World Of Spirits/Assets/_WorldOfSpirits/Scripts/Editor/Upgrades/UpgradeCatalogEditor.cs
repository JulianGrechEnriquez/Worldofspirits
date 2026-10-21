using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.EditorTools
{
    [CustomEditor(typeof(UpgradeCatalog))]
    public sealed class UpgradeCatalogEditor : UnityEditor.Editor
    {
        private int groupFilter;
        private string search = "";
        private bool editReferences;

        public override void OnInspectorGUI()
        {
            var catalog = (UpgradeCatalog)target;
            EditorGUILayout.LabelField("Upgrade Catalog", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Group describes who an upgrade belongs to. Category describes its effect; rarity is separate.", MessageType.Info);
            groupFilter = GUILayout.Toolbar(groupFilter, new[] { "All", "General", "Character", "Spirit" });
            search = EditorGUILayout.TextField("Search", search);
            int count = 0;
            string previousSection = null;
            foreach (var card in catalog.Cards)
            {
                if (card == null) continue;
                if (groupFilter > 0 && (int)card.Group != groupFilter - 1) continue;
                string section = card.Group == UpgradeGroup.Spirit ? card.TargetSpirit.SpiritName :
                    card.Group == UpgradeGroup.Character ? card.RequiredCharacter.ToString() : "General";
                if (!string.IsNullOrWhiteSpace(search) &&
                    (card.CardName + " " + section + " " + card.Category + " " + card.Rarity)
                        .IndexOf(search, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (previousSection != section)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(section, EditorStyles.boldLabel);
                    previousSection = section;
                }
                EditorGUILayout.ObjectField(card.CardName + " [" + card.Rarity + "]", card,
                    typeof(UpgradeCardDefinition), false);
                count++;
            }
            EditorGUILayout.LabelField($"Showing {count} / {catalog.Cards.Count} cards");
            if (GUILayout.Button("Organize Existing Cards (preserve GUIDs)"))
                UpgradeCatalogOrganizer.Organize(catalog);
            editReferences = EditorGUILayout.Foldout(editReferences, "Edit catalog references", true);
            if (editReferences)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cards"), true);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

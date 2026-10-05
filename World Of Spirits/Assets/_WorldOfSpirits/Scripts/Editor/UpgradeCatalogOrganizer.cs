using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.EditorTools
{
    public static class UpgradeCatalogOrganizer
    {
        public const string Root = "Assets/_WorldOfSpirits/Data/Upgrades";

        [MenuItem("World of Spirits/Upgrades/Organize Existing Catalog")]
        public static void OrganizeMainCatalog()
        {
            Organize(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(Root + "/Main Upgrade Catalog.asset"));
        }

        public static string FolderFor(UpgradeGroup group, UpgradeCharacter character,
            UpgradeCategory category, UpgradeRarity rarity, string spiritName = null)
        {
            if (group == UpgradeGroup.Spirit)
            {
                string subtype = category == UpgradeCategory.SpiritContract ? "Contracts" :
                    category == UpgradeCategory.Weapon ? "Weapons" :
                    category == UpgradeCategory.Evolution ? "Ascensions" : "Abilities";
                return Root + "/Spirits/" + Safe(spiritName) + "/" + subtype;
            }
            return group == UpgradeGroup.Character
                ? Root + "/Characters/" + character + "/" + rarity
                : Root + "/General/" + rarity;
        }

        public static void Organize(UpgradeCatalog catalog)
        {
            if (catalog == null) throw new InvalidOperationException("Upgrade catalog not found.");
            var cards = new List<UpgradeCardDefinition>();
            var ids = new HashSet<string>();
            foreach (var card in catalog.Cards)
            {
                if (card == null || !ids.Add(card.Id))
                    throw new InvalidOperationException("Resolve missing cards or duplicate IDs before organizing.");
                cards.Add(card);
            }

            // Move existing assets through Unity so GUIDs and scene references remain intact.
            foreach (var card in cards)
            {
                if (card.Id == "spirit_master")
                {
                    var so = new SerializedObject(card);
                    so.FindProperty("requiredCharacter").intValue = (int)UpgradeCharacter.SpiritTamer;
                    so.FindProperty("category").intValue = (int)UpgradeCategory.CharacterAbility;
                    so.ApplyModifiedProperties();
                }
                string folder = FolderFor(card.Group, card.RequiredCharacter, card.Category,
                    card.Rarity, card.TargetSpirit != null ? card.TargetSpirit.SpiritName : null);
                EnsureFolder(folder);
                string source = AssetDatabase.GetAssetPath(card);
                string destination = folder + "/" + Path.GetFileName(source);
                if (source == destination) continue;
                string error = AssetDatabase.MoveAsset(source, destination);
                if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            }
            cards.Sort((a, b) => string.Compare(AssetDatabase.GetAssetPath(a),
                AssetDatabase.GetAssetPath(b), StringComparison.OrdinalIgnoreCase));
            var serialized = new SerializedObject(catalog);
            var list = serialized.FindProperty("cards");
            for (int i = 0; i < cards.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            serialized.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            foreach (string legacy in new[] { Root + "/Player", Root + "/Legendary" })
                if (AssetDatabase.IsValidFolder(legacy) && Directory.GetFileSystemEntries(legacy).Length == 0)
                    AssetDatabase.DeleteAsset(legacy);
            Debug.Log($"Organized {cards.Count} upgrades into General, Character and Spirit folders.");
        }

        public static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string Safe(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Unassigned";
            foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value;
        }
    }
}

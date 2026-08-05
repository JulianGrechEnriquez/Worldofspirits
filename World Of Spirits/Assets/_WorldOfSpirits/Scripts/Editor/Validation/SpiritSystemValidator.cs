using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.EditorTools
{
    public class SpiritSystemValidator : EditorWindow
    {
        private readonly List<string> results = new List<string>();
        private Vector2 scroll;

        [MenuItem("World of Spirits/Validate Spirit System")]
        private static void Open()
        {
            GetWindow<SpiritSystemValidator>("Spirit Validator").RunValidation();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Spirit System Validation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Checks definitions, levels, prefabs, indexes, projectiles, and required physics components.", MessageType.Info);
            if (GUILayout.Button("Run Validation")) RunValidation();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (string result in results) EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }

        private void RunValidation()
        {
            results.Clear();
            ValidateDefinitions();
            ValidatePrefabs();
            if (results.Count == 0) results.Add("✓ No spirit-system problems found.");
            Repaint();
        }

        private void ValidateDefinitions()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:AbilityDefinition"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AbilityDefinition ability = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path);
                if (string.IsNullOrWhiteSpace(ability.AbilityName)) Add("ERROR", path, "Ability name is empty.");
                if (ability.Levels.Count == 0) Add("ERROR", path, "No levels are defined.");
                for (int i = 0; i < ability.Levels.Count; i++)
                {
                    AbilityLevelData level = ability.Levels[i];
                    if (level.level != i + 1) Add("WARNING", path, $"Level entry {i + 1} is numbered {level.level}.");
                    if (string.IsNullOrWhiteSpace(level.upgradeDescription)) Add("WARNING", path, $"Level {i + 1} has no upgrade description.");
                    if (ability.ExecutionType == AbilityExecutionType.Projectile && level.projectile.projectilePrefab == null)
                        Add("TODO", path, $"Level {i + 1} needs a projectile prefab.");
                    if ((ability.ExecutionType == AbilityExecutionType.SpawnEffect ||
                         ability.ExecutionType == AbilityExecutionType.Orbiting ||
                         ability.ExecutionType == AbilityExecutionType.FollowingArea) && level.spawnedEffectPrefab == null)
                        Add("TODO", path, $"Level {i + 1} needs an effect prefab.");
                }
            }
        }

        private void ValidatePrefabs()
        {
            foreach (string guid in AssetDatabase.FindAssets(
                         "t:Prefab", new[] { "Assets/_WorldOfSpirits/Prefabs/Spirits" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                SpiritMember member = prefab.GetComponent<SpiritMember>();
                if (prefab.name.EndsWith("Spirit") && member == null) Add("ERROR", path, "SpiritMember is missing.");
                if (member == null) continue;

                SerializedObject memberData = new SerializedObject(member);
                SpiritDefinition definition = memberData.FindProperty("definition").objectReferenceValue as SpiritDefinition;
                if (definition == null) Add("WARNING", path, "No explicit SpiritDefinition; prefab-name fallback will be used.");
                DataDrivenAbility[] abilities = prefab.GetComponentsInChildren<DataDrivenAbility>(true);
                HashSet<int> indexes = new HashSet<int>();
                foreach (DataDrivenAbility ability in abilities)
                {
                    SerializedObject data = new SerializedObject(ability);
                    int index = data.FindProperty("abilityIndex").intValue;
                    if (!indexes.Add(index)) Add("ERROR", path, $"Duplicate ability index {index}.");
                    if (ability.Definition == null) Add("ERROR", path, $"Ability at index {index} has no definition.");
                }
            }

            foreach (string guid in AssetDatabase.FindAssets(
                         "t:Prefab", new[] { "Assets/_WorldOfSpirits/Prefabs/Projectiles" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab.GetComponent<ProjectileBase>() == null) continue;
                if (prefab.GetComponent<Rigidbody2D>() == null) Add("ERROR", path, "Projectile requires Rigidbody2D.");
                if (prefab.GetComponent<Collider2D>() == null) Add("ERROR", path, "Projectile requires Collider2D.");
            }
        }

        private void Add(string severity, string path, string message)
        {
            results.Add($"{severity}: {path}\n{message}");
        }
    }
}

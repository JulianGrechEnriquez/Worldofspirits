using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Progression.Upgrades;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.EditorTools
{
    /// <summary>Creates the common game-content assets without editing scripts.</summary>
    public sealed class ContentCreatorWindow : EditorWindow
    {
        private const string AbilityRoot = "Assets/_WorldOfSpirits/Data/Abilities";
        private const string UpgradeRoot = "Assets/_WorldOfSpirits/Data/Upgrades";

        private int tab;
        private SpiritDefinition spirit;
        private GameObject spiritPrefab;
        private UpgradeCatalog catalog;
        private string abilityName = "New Ability";
        private string abilityDescription = "Describe what this ability does.";
        private AbilityExecutionType abilityType = AbilityExecutionType.Projectile;
        private AbilityTargetingMode targeting = AbilityTargetingMode.ClosestEnemy;
        private ProjectileBase projectilePrefab;
        private GameObject effectPrefab;
        private float cooldown = 1f;
        private float damage = 10f;
        private float range = 12f;
        private float radius = 3f;
        private int levelCount = 5;
        private bool createAbilityUpgrade = true;

        private string upgradeName = "New Upgrade";
        private string upgradeDescription = "+10% damage.";
        private UpgradeCategory upgradeCategory = UpgradeCategory.Player;
        private UpgradeRarity upgradeRarity = UpgradeRarity.Common;
        private UpgradeStat upgradeStat = UpgradeStat.AttackDamage;
        private ModifierOperation upgradeOperation = ModifierOperation.Multiply;
        private float valuePerLevel = 0.1f;
        private int maximumLevel = 5;
        private float baseWeight = 100f;

        [MenuItem("World of Spirits/Content Creator")]
        private static void Open() => GetWindow<ContentCreatorWindow>("Content Creator");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("World of Spirits Content Creator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use this window for standard content. New behaviour still needs code once, but variants can then be made as data.",
                MessageType.Info);
            tab = GUILayout.Toolbar(tab, new[] { "New Ability", "New Upgrade" });
            EditorGUILayout.Space(6f);
            if (tab == 0) DrawAbilityTab();
            else DrawUpgradeTab();
        }

        private void DrawAbilityTab()
        {
            spirit = (SpiritDefinition)EditorGUILayout.ObjectField("Spirit Definition", spirit, typeof(SpiritDefinition), false);
            spiritPrefab = (GameObject)EditorGUILayout.ObjectField("Spirit Prefab", spiritPrefab, typeof(GameObject), false);
            catalog = (UpgradeCatalog)EditorGUILayout.ObjectField("Upgrade Catalog", catalog, typeof(UpgradeCatalog), false);
            EditorGUILayout.Space(4f);
            abilityName = EditorGUILayout.TextField("Name", abilityName);
            abilityDescription = EditorGUILayout.TextArea(abilityDescription, GUILayout.MinHeight(42f));
            abilityType = (AbilityExecutionType)EditorGUILayout.EnumPopup("Type", abilityType);
            targeting = (AbilityTargetingMode)EditorGUILayout.EnumPopup("Targeting", targeting);
            cooldown = EditorGUILayout.FloatField("Cooldown", cooldown);
            damage = EditorGUILayout.FloatField("Damage", damage);
            range = EditorGUILayout.FloatField("Targeting Range", range);
            radius = EditorGUILayout.FloatField("Area Radius", radius);
            levelCount = EditorGUILayout.IntSlider("Upgrade Levels", levelCount, 1, 10);
            if (abilityType == AbilityExecutionType.Projectile)
                projectilePrefab = (ProjectileBase)EditorGUILayout.ObjectField("Projectile Prefab", projectilePrefab, typeof(ProjectileBase), false);
            else if (abilityType != AbilityExecutionType.Self && abilityType != AbilityExecutionType.Chain)
                effectPrefab = (GameObject)EditorGUILayout.ObjectField("Effect Prefab", effectPrefab, typeof(GameObject), false);
            createAbilityUpgrade = EditorGUILayout.ToggleLeft("Create an upgrade card", createAbilityUpgrade);
            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(spirit == null || spiritPrefab == null || string.IsNullOrWhiteSpace(abilityName)))
            {
                if (GUILayout.Button("Create Ability and Add It to Spirit", GUILayout.Height(32f)))
                    CreateAbility();
            }
        }

        private void DrawUpgradeTab()
        {
            catalog = (UpgradeCatalog)EditorGUILayout.ObjectField("Upgrade Catalog", catalog, typeof(UpgradeCatalog), false);
            upgradeName = EditorGUILayout.TextField("Name", upgradeName);
            upgradeDescription = EditorGUILayout.TextArea(upgradeDescription, GUILayout.MinHeight(42f));
            upgradeCategory = (UpgradeCategory)EditorGUILayout.EnumPopup("Category", upgradeCategory);
            upgradeRarity = (UpgradeRarity)EditorGUILayout.EnumPopup("Rarity", upgradeRarity);
            upgradeStat = (UpgradeStat)EditorGUILayout.EnumPopup("Effect", upgradeStat);
            upgradeOperation = (ModifierOperation)EditorGUILayout.EnumPopup("Operation", upgradeOperation);
            valuePerLevel = EditorGUILayout.FloatField("Value Per Level", valuePerLevel);
            maximumLevel = EditorGUILayout.IntSlider("Maximum Level", maximumLevel, 1, 20);
            baseWeight = EditorGUILayout.FloatField("Offer Weight", baseWeight);
            if (upgradeStat == UpgradeStat.SpiritCapacity)
                EditorGUILayout.HelpBox("Use Add with a value of 1. Each level creates one extra spirit slot around the player.", MessageType.Info);
            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(catalog == null || string.IsNullOrWhiteSpace(upgradeName)))
            {
                if (GUILayout.Button("Create Upgrade Card", GUILayout.Height(32f)))
                    CreateUpgradeCard(upgradeName, upgradeDescription, upgradeCategory, upgradeRarity,
                        upgradeStat, upgradeOperation, valuePerLevel, maximumLevel, baseWeight, null, -1);
            }
        }

        private void CreateAbility()
        {
            EnsureFolder(AbilityRoot);
            string folder = AbilityRoot + "/" + SafeName(spirit.SpiritName);
            EnsureFolder(folder);
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + SafeName(abilityName) + ".asset");
            AbilityDefinition ability = CreateInstance<AbilityDefinition>();
            ability.name = abilityName.Trim();
            ability.Configure(abilityName.Trim(), abilityDescription, abilityType, targeting, BuildLevels());
            AssetDatabase.CreateAsset(ability, path);

            List<AbilityDefinition> abilities = new List<AbilityDefinition>(spirit.RuntimeAbilities) { ability };
            int abilityIndex = abilities.Count - 1;
            spirit.SetRuntimeAbilities(abilities);
            EditorUtility.SetDirty(spirit);
            AddRunnerToPrefab(ability, abilityIndex);
            if (createAbilityUpgrade && catalog != null)
            {
                CreateUpgradeCard(abilityName + " Upgrade", "Improve " + abilityName + ".",
                    UpgradeCategory.SpiritAbility, UpgradeRarity.Common, UpgradeStat.SpiritDamage,
                    ModifierOperation.Multiply, 0f, levelCount, 100f, spirit, abilityIndex);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = ability;
            EditorGUIUtility.PingObject(ability);
            Debug.Log($"Created {abilityName} and attached it to {spiritPrefab.name}.");
        }

        private AbilityLevelData[] BuildLevels()
        {
            AbilityLevelData[] levels = new AbilityLevelData[levelCount];
            for (int i = 0; i < levels.Length; i++)
            {
                float scale = 1f + i * 0.2f;
                levels[i] = new AbilityLevelData
                {
                    level = i + 1,
                    upgradeDescription = $"Level {i + 1}: stronger {abilityName}.",
                    cooldown = Mathf.Max(0.05f, cooldown * (1f - i * 0.04f)),
                    targetingRange = Mathf.Max(0.1f, range),
                    areaRadius = Mathf.Max(0f, radius * scale),
                    activeDuration = 3f,
                    spawnCount = 1,
                    chainCount = 1 + i,
                    chainRange = Mathf.Max(0.1f, range),
                    spawnedEffectPrefab = effectPrefab,
                    projectile = new AbilityProjectileData
                    {
                        projectilePrefab = projectilePrefab,
                        damage = Mathf.Max(0f, damage * scale),
                        speed = 10f,
                        count = 1,
                        spreadMode = ProjectileSpreadMode.EvenlySpaced
                    },
                    effects = new List<AbilityEffectData>
                    {
                        new AbilityEffectData { effectType = AbilityEffectType.Damage, value = Mathf.Max(0f, damage * scale) }
                    }
                };
            }
            return levels;
        }

        private void AddRunnerToPrefab(AbilityDefinition ability, int abilityIndex)
        {
            string prefabPath = AssetDatabase.GetAssetPath(spiritPrefab);
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
                throw new System.InvalidOperationException("Spirit Prefab must be a prefab asset, not a scene object.");
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                GameObject abilityObject = new GameObject(abilityName + " Ability");
                abilityObject.transform.SetParent(root.transform, false);
                DataDrivenAbility runner = abilityObject.AddComponent<DataDrivenAbility>();
                SerializedObject serializedRunner = new SerializedObject(runner);
                serializedRunner.FindProperty("definition").objectReferenceValue = ability;
                serializedRunner.FindProperty("abilityIndex").intValue = abilityIndex;
                serializedRunner.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private void CreateUpgradeCard(string cardName, string description, UpgradeCategory category,
            UpgradeRarity rarity, UpgradeStat stat, ModifierOperation operation, float value, int maxLevel,
            float weight, SpiritDefinition targetSpirit, int abilityIndex)
        {
            EnsureFolder(UpgradeRoot);
            string path = AssetDatabase.GenerateUniqueAssetPath(UpgradeRoot + "/" + SafeName(cardName) + ".asset");
            UpgradeCardDefinition card = CreateInstance<UpgradeCardDefinition>();
            card.name = cardName.Trim();
            AssetDatabase.CreateAsset(card, path);
            SerializedObject serializedCard = new SerializedObject(card);
            serializedCard.FindProperty("cardId").stringValue = SafeName(cardName).ToLowerInvariant();
            serializedCard.FindProperty("cardName").stringValue = cardName.Trim();
            serializedCard.FindProperty("description").stringValue = description;
            serializedCard.FindProperty("category").enumValueIndex = (int)category;
            serializedCard.FindProperty("rarity").enumValueIndex = (int)rarity;
            serializedCard.FindProperty("maximumLevel").intValue = Mathf.Max(1, maxLevel);
            serializedCard.FindProperty("baseWeight").floatValue = Mathf.Max(0f, weight);
            serializedCard.FindProperty("targetSpirit").objectReferenceValue = targetSpirit;
            serializedCard.FindProperty("abilityIndex").intValue = abilityIndex;
            SerializedProperty modifiers = serializedCard.FindProperty("modifiers");
            modifiers.arraySize = 1;
            SerializedProperty modifier = modifiers.GetArrayElementAtIndex(0);
            modifier.FindPropertyRelative("stat").enumValueIndex = (int)stat;
            modifier.FindPropertyRelative("operation").enumValueIndex = (int)operation;
            modifier.FindPropertyRelative("valuePerLevel").floatValue = value;
            serializedCard.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedCatalog = new SerializedObject(catalog);
            SerializedProperty cards = serializedCatalog.FindProperty("cards");
            cards.InsertArrayElementAtIndex(cards.arraySize);
            cards.GetArrayElementAtIndex(cards.arraySize - 1).objectReferenceValue = card;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static void EnsureFolder(string path)
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

        private static string SafeName(string value)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return string.IsNullOrWhiteSpace(value) ? "New Content" : value.Trim();
        }
    }
}

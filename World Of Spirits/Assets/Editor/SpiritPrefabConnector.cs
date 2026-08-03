using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.EditorTools
{
    public static class SpiritPrefabConnector
    {
        private const string FirePrefabPath = "Assets/Prefabs/Spirits/Fire Spirit.prefab";
        private const string EarthPrefabPath = "Assets/Prefabs/Spirits/Earth Spirit.prefab";
        private const string IcePrefabPath = "Assets/Prefabs/Spirits/Ice Spirit.prefab";

        [MenuItem("World of Spirits/Connect Existing Spirit Prefabs")]
        public static void ConnectExistingSpiritPrefabs()
        {
            ConnectFire();
            ConnectEarth();
            ConnectIce();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Connected the available Fire, Earth, and Ice spirit prefabs. Placeholder ability children identify missing gameplay prefabs.");
        }

        private static void ConnectFire()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(FirePrefabPath);
            if (root == null) return;

            SpiritMember member = GetOrAdd<SpiritMember>(root);
            AssignDefinition(member, "Assets/ScriptableObjects/Spirits/Fire Spirit.asset");

            GameObject bowObject = GetOrCreateChild(root, "Fire Bow Weapon");
            AutoProjectileWeapon bow = GetOrAdd<AutoProjectileWeapon>(bowObject);
            SerializedObject bowData = new SerializedObject(bow);
            GameObject bowProjectile = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Projectiles/FireFeather.prefab");
            bowData.FindProperty("projectilePrefab").objectReferenceValue =
                bowProjectile != null ? bowProjectile.GetComponent<ProjectileBase>() : null;
            bowData.FindProperty("firePoint").objectReferenceValue = bowObject.transform;
            bowData.FindProperty("damage").floatValue = 10f;
            bowData.FindProperty("projectileSpeed").floatValue = 12f;
            bowData.FindProperty("attackCooldown").floatValue = 0.75f;
            bowData.FindProperty("targetingRange").floatValue = 12f;
            bowData.ApplyModifiedPropertiesWithoutUndo();

            GameObject featherObject = GetOrCreateChild(root, "Ability 1 - Fiery Feathers");
            ProjectilePatternAbility feathers = GetOrAdd<ProjectilePatternAbility>(featherObject);
            ConfigureAbility(feathers, 0, 1.2f);
            SerializedObject featherData = new SerializedObject(feathers);
            GameObject projectileObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/FireFeather.prefab");
            featherData.FindProperty("projectilePrefab").objectReferenceValue =
                projectileObject != null ? projectileObject.GetComponent<ProjectileBase>() : null;
            featherData.FindProperty("pattern").enumValueIndex = (int)ProjectilePattern.AimedFan;
            featherData.FindProperty("spreadAngle").floatValue = 45f;
            featherData.FindProperty("spreadMode").enumValueIndex = (int)ProjectileSpreadMode.EvenlySpaced;
            featherData.FindProperty("homeOnEnemies").boolValue = true;
            featherData.FindProperty("homingStrength").floatValue = 6f;
            featherData.FindProperty("homingRange").floatValue = 10f;
            featherData.FindProperty("targetingRange").floatValue = 15f;
            SetIntegerScaling(featherData, "projectileCount", 3, 0);
            SetFloatScaling(featherData, "damage", 10f, 3f);
            SetFloatScaling(featherData, "speed", 10f, 0.5f);
            featherData.ApplyModifiedPropertiesWithoutUndo();

            ConfigurePlaceholder<SpawnEffectAbility>(root, "Ability 2 - Fiery Talons (Needs Fire Trail Prefab)", 1, 1.5f);
            ConfigurePlaceholder<SpawnEffectAbility>(root, "Ability 3 - Phoenix Dive (Needs Dive Prefab)", 2, 4f);
            PrefabUtility.SaveAsPrefabAsset(root, FirePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConnectEarth()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(EarthPrefabPath);
            if (root == null) return;

            GetOrAdd<SpiritMember>(root);
            GameObject hammerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Stone Hammer.prefab");
            if (hammerPrefab != null && root.transform.Find("Stone Hammer Weapon") == null)
            {
                GameObject hammer = (GameObject)PrefabUtility.InstantiatePrefab(hammerPrefab, root.transform);
                hammer.name = "Stone Hammer Weapon";
                hammer.transform.localPosition = Vector3.zero;
            }

            GameObject quicksandObject = GetOrCreateChild(root, "Ability 2 - Quicksand Domain");
            AreaPulseAbility quicksand = GetOrAdd<AreaPulseAbility>(quicksandObject);
            ConfigureAbility(quicksand, 1, 1f);
            SerializedObject quicksandData = new SerializedObject(quicksand);
            quicksandData.FindProperty("pullInward").boolValue = true;
            quicksandData.FindProperty("appliesStatus").boolValue = true;
            quicksandData.FindProperty("status").enumValueIndex = (int)CombatStatus.Slow;
            SetFloatScaling(quicksandData, "radius", 3f, 0.75f);
            SetFloatScaling(quicksandData, "damage", 0f, 2f);
            SetFloatScaling(quicksandData, "force", 1f, 0.5f);
            SetFloatScaling(quicksandData, "statusDuration", 1.5f, 0.5f);
            SetFloatScaling(quicksandData, "statusStrength", 0.25f, 0.1f);
            quicksandData.ApplyModifiedPropertiesWithoutUndo();

            ConfigurePlaceholder<ProjectilePatternAbility>(root, "Ability 1 - Boulder Throw (Needs Boulder Projectile)", 0, 2f);
            ConfigurePlaceholder<SpawnEffectAbility>(root, "Ability 3 - Stone Spikes (Needs Spike Prefab)", 2, 2.5f);
            PrefabUtility.SaveAsPrefabAsset(root, EarthPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConnectIce()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(IcePrefabPath);
            if (root == null) return;

            SpiritMember member = GetOrAdd<SpiritMember>(root);
            AssignDefinition(member, "Assets/ScriptableObjects/Spirits/Ice Spirit.asset");
            ConfigurePlaceholder<OrbitingProjectileAbility>(root, "Ability 1 - Orbital Snowball (Needs Snowball Prefab)", 0, 1f);
            ConfigurePlaceholder<ProjectilePatternAbility>(root, "Ability 2 - Avalanche (Needs Snowball Projectile)", 1, 2f);
            ConfigurePlaceholder<SpawnEffectAbility>(root, "Ability 3 - Ice Crystal (Needs Crystal Prefab)", 2, 3f);
            PrefabUtility.SaveAsPrefabAsset(root, IcePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void AssignDefinition(SpiritMember member, string assetPath)
        {
            SerializedObject data = new SerializedObject(member);
            data.FindProperty("definition").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SpiritDefinition>(assetPath);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T ConfigurePlaceholder<T>(GameObject root, string childName, int index, float cooldown)
            where T : SpiritAbility
        {
            T ability = GetOrAdd<T>(GetOrCreateChild(root, childName));
            ConfigureAbility(ability, index, cooldown);
            return ability;
        }

        private static void ConfigureAbility(SpiritAbility ability, int index, float cooldown)
        {
            SerializedObject data = new SerializedObject(ability);
            data.FindProperty("abilityIndex").intValue = index;
            data.FindProperty("cooldown").floatValue = cooldown;
            data.FindProperty("primarySpiritOnly").boolValue = false;
            data.FindProperty("castWhileMoving").boolValue = true;
            data.FindProperty("castWhileStandingStill").boolValue = false;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloatScaling(SerializedObject data, string property, float baseValue, float perLevel)
        {
            data.FindProperty(property).FindPropertyRelative("baseValue").floatValue = baseValue;
            data.FindProperty(property).FindPropertyRelative("increasePerLevel").floatValue = perLevel;
        }

        private static void SetIntegerScaling(SerializedObject data, string property, int baseValue, int perLevel)
        {
            data.FindProperty(property).FindPropertyRelative("baseValue").intValue = baseValue;
            data.FindProperty(property).FindPropertyRelative("increasePerLevel").intValue = perLevel;
        }

        private static GameObject GetOrCreateChild(GameObject root, string name)
        {
            Transform child = root.transform.Find(name);
            if (child != null) return child.gameObject;
            GameObject created = new GameObject(name);
            created.transform.SetParent(root.transform, false);
            return created;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}

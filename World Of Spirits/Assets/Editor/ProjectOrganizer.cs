using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.EditorTools
{
    [InitializeOnLoad]
    public static class ProjectOrganizer
    {
        static ProjectOrganizer()
        {
            // Run only while one of the known legacy paths still exists. All moves
            // use AssetDatabase so GUID references remain intact.
            if (NeedsOrganization())
                EditorApplication.delayCall += OrganizeProjectFiles;
        }

        [MenuItem("World of Spirits/Organize Project Files %#o")]
        public static void OrganizeProjectFiles()
        {
            EnsureFolder("Assets/Settings/Rendering");
            EnsureFolder("Assets/Settings/Input");
            EnsureFolder("Assets/Art/UI/Progression");
            EnsureFolder("Assets/ScriptableObjects/Weapons");
            EnsureFolder("Assets/Prefabs/Weapons");
            EnsureFolder("Assets/Prefabs/Enemies/Regular");
            EnsureFolder("Assets/Prefabs/Enemies/Bosses");
            EnsureFolder("Assets/Prefabs/Projectiles");
            EnsureFolder("Assets/Prefabs/Spirits/Effects");
            EnsureFolder("Assets/Prefabs/Pickups/Experience");
            EnsureFolder("Assets/Prefabs/UI/SpiritSelection");
            EnsureFolder("Assets/Scripts/Progression/Pickups");
            EnsureFolder("Assets/Scripts/Progression/Unlocks");
            EnsureFolder("Assets/Scripts/UI/Progression");
            EnsureFolder("Assets/Scripts/UI/SpiritSelection");

            MoveIfPresent("Assets/Docs", "Assets/Documentation");
            MoveIfPresent(
                "Assets/DefaultVolumeProfile.asset",
                "Assets/Settings/Rendering/DefaultVolumeProfile.asset");
            MoveIfPresent(
                "Assets/UniversalRenderPipelineGlobalSettings.asset",
                "Assets/Settings/Rendering/UniversalRenderPipelineGlobalSettings.asset");
            MoveIfPresent(
                "Assets/InputSystem_Actions.inputactions",
                "Assets/Settings/Input/InputSystem_Actions.inputactions");
            MoveIfPresent(
                "Assets/Art/xpfill.png",
                "Assets/Art/UI/Progression/ExperienceBarFill.png");

            MoveIfPresent(
                "Assets/ScriptableObjects/Wepons/Stone Hamer.asset",
                "Assets/ScriptableObjects/Weapons/Stone Hammer.asset");
            MoveIfPresent(
                "Assets/Prefabs/Spirits/Stone Hamer 1.prefab",
                "Assets/Prefabs/Weapons/Stone Hammer.prefab");
            MoveIfPresent(
                "Assets/Prefabs/Spirits/Stone Hamer Holder.prefab",
                "Assets/Prefabs/Weapons/Stone Hammer Holder.prefab");
            MoveIfPresent(
                "Assets/Prefabs/Spirits/IceBall.prefab",
                "Assets/Prefabs/Weapons/Ice Ball.prefab");
            MoveIfPresent(
                "Assets/Prefabs/Spirits/boulder.prefab",
                "Assets/Prefabs/Projectiles/Boulder.prefab");
            MoveIfPresent(
                "Assets/Prefabs/Spirits/Quicksand Domain.prefab",
                "Assets/Prefabs/Spirits/Effects/Quicksand Domain.prefab");
            MoveIfPresent(
                "Assets/Prefabs/Enemies/FireTank.prefab",
                "Assets/Prefabs/Enemies/Regular/FireTank.prefab");
            MoveIfPresent(
                "Assets/Prefabs/Pickups/Experience Orb.prefab",
                "Assets/Prefabs/Pickups/Experience/Experience Orb.prefab");
            MoveIfPresent(
                "Assets/Prefabs/Spirte Card.prefab",
                "Assets/Prefabs/UI/SpiritSelection/Spirit Card.prefab");

            MoveIfPresent(
                "Assets/Scripts/Progression/ExperienceOrb.cs",
                "Assets/Scripts/Progression/Pickups/ExperienceOrb.cs");
            MoveIfPresent(
                "Assets/Scripts/Progression/ExperienceOrbService.cs",
                "Assets/Scripts/Progression/Pickups/ExperienceOrbService.cs");
            MoveIfPresent(
                "Assets/Scripts/Progression/SpiritUnlockProgress.cs",
                "Assets/Scripts/Progression/Unlocks/SpiritUnlockProgress.cs");
            MoveIfPresent(
                "Assets/Scripts/UI/PlayerProgressionHud.cs",
                "Assets/Scripts/UI/Progression/PlayerProgressionHud.cs");
            MoveIfPresent(
                "Assets/Scripts/UI/ProgressionInterface.cs",
                "Assets/Scripts/UI/Progression/ProgressionInterface.cs");
            MoveIfPresent(
                "Assets/Scripts/UI/StarterSpiritCardView.cs",
                "Assets/Scripts/UI/SpiritSelection/StarterSpiritCardView.cs");
            MoveIfPresent(
                "Assets/Scripts/UI/StarterSpiritSelectionController.cs",
                "Assets/Scripts/UI/SpiritSelection/StarterSpiritSelectionController.cs");

            DeleteFolderIfEmpty("Assets/ScriptableObjects/Wepons");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Project files organized. Unity asset references were preserved.");
        }

        private static bool NeedsOrganization()
        {
            string[] legacyPaths =
            {
                "Assets/Docs",
                "Assets/DefaultVolumeProfile.asset",
                "Assets/UniversalRenderPipelineGlobalSettings.asset",
                "Assets/InputSystem_Actions.inputactions",
                "Assets/Art/xpfill.png",
                "Assets/Prefabs/Spirte Card.prefab",
                "Assets/Prefabs/Enemies/FireTank.prefab",
                "Assets/Prefabs/Pickups/Experience Orb.prefab",
                "Assets/Scripts/Progression/ExperienceOrb.cs",
                "Assets/Scripts/Progression/ExperienceOrbService.cs",
                "Assets/Scripts/Progression/SpiritUnlockProgress.cs",
                "Assets/Scripts/UI/PlayerProgressionHud.cs",
                "Assets/Scripts/UI/ProgressionInterface.cs",
                "Assets/Scripts/UI/StarterSpiritCardView.cs",
                "Assets/Scripts/UI/StarterSpiritSelectionController.cs",
                "Assets/ScriptableObjects/Wepons/Stone Hamer.asset"
            };

            foreach (string path in legacyPaths)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                    return true;
            }

            return false;
        }

        [MenuItem("World of Spirits/Create Enemy Prefab From Selection", true)]
        private static bool ValidateCreateEnemyPrefab()
        {
            return Selection.activeGameObject != null &&
                   Selection.activeGameObject.scene.IsValid();
        }

        [MenuItem("World of Spirits/Create Enemy Prefab From Selection")]
        public static void CreateEnemyPrefabFromSelection()
        {
            GameObject enemy = Selection.activeGameObject;
            if (enemy == null || !enemy.scene.IsValid())
            {
                Debug.LogWarning("Select an enemy GameObject in the Hierarchy first.");
                return;
            }

            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyBase == null || body == null || enemyCollider == null)
            {
                Debug.LogError(
                    "The selected object is not ready to become an enemy prefab. " +
                    "It needs EnemyBase (for example ChasingEnemy), Rigidbody2D, and Collider2D.",
                    enemy);
                return;
            }

            if (enemy.GetComponent<ContactDamage>() == null)
            {
                Undo.AddComponent<ContactDamage>(enemy);
            }

            body.gravityScale = 0f;
            body.freezeRotation = true;

            EnsureFolder("Assets/Prefabs/Enemies/Regular");
            string cleanName = enemy.name.Replace("(Clone)", string.Empty).Trim();
            int copyMarker = cleanName.LastIndexOf(" (", System.StringComparison.Ordinal);
            if (copyMarker > 0 && cleanName.EndsWith(")"))
            {
                cleanName = cleanName.Substring(0, copyMarker);
            }

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                $"Assets/Prefabs/Enemies/Regular/{cleanName}.prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                enemy, prefabPath, InteractionMode.UserAction);

            if (prefab == null)
            {
                Debug.LogError($"Unity could not create the enemy prefab at {prefabPath}.", enemy);
                return;
            }

            EditorSceneManager.MarkSceneDirty(enemy.scene);
            AssetDatabase.SaveAssets();
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log(
                $"Created enemy prefab at {prefabPath}. The scene object is now connected to it.",
                prefab);
        }

        private static void MoveIfPresent(string source, string destination)
        {
            if (AssetDatabase.LoadMainAssetAtPath(source) == null ||
                AssetDatabase.LoadMainAssetAtPath(destination) != null)
            {
                return;
            }

            string error = AssetDatabase.MoveAsset(source, destination);
            if (!string.IsNullOrEmpty(error))
                Debug.LogError($"Could not move {source}: {error}");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string name = path.Substring(slash + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void DeleteFolderIfEmpty(string path)
        {
            if (AssetDatabase.IsValidFolder(path) &&
                AssetDatabase.FindAssets(string.Empty, new[] { path }).Length == 0)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
    }
}

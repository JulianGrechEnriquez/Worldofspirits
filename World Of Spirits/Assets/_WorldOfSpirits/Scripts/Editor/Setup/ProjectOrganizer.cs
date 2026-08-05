using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.EditorTools
{
    public static class ProjectOrganizer
    {
        private const string Root = "Assets/_WorldOfSpirits";

        [MenuItem("World of Spirits/Create Required Project Folders")]
        public static void CreateRequiredFolders()
        {
            string[] folders =
            {
                Root + "/Animations",
                Root + "/Art/UI/Progression",
                Root + "/Audio",
                Root + "/Data/Abilities",
                Root + "/Data/Enemies/Definitions",
                Root + "/Data/Enemies/Movement Profiles",
                Root + "/Data/Spirits",
                Root + "/Data/Upgrades",
                Root + "/Data/Weapons",
                Root + "/Documentation",
                Root + "/Prefabs/Abilities",
                Root + "/Prefabs/Enemies/Bosses",
                Root + "/Prefabs/Enemies/Regular",
                Root + "/Prefabs/Pickups/Experience",
                Root + "/Prefabs/Projectiles",
                Root + "/Prefabs/Spirits/Effects",
                Root + "/Prefabs/UI/SpiritSelection",
                Root + "/Prefabs/Weapons",
                Root + "/Scenes",
                Root + "/Scripts/Progression/Pickups",
                Root + "/Scripts/Progression/Unlocks",
                Root + "/Scripts/UI/Core",
                Root + "/Scripts/UI/Progression",
                Root + "/Scripts/UI/Screens",
                Root + "/Scripts/UI/SpiritSelection",
                Root + "/Scripts/UI/Upgrades",
                Root + "/Settings/Input",
                Root + "/Settings/Rendering"
            };

            foreach (string folder in folders) EnsureFolder(folder);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Required World of Spirits folders are ready.");
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
                    "The selected object needs EnemyBase, Rigidbody2D, and Collider2D.",
                    enemy);
                return;
            }

            if (enemy.GetComponent<ContactDamage>() == null)
                Undo.AddComponent<ContactDamage>(enemy);

            body.gravityScale = 0f;
            body.freezeRotation = true;

            string folder = Root + "/Prefabs/Enemies/Regular";
            EnsureFolder(folder);
            string cleanName = enemy.name.Replace("(Clone)", string.Empty).Trim();
            int copyMarker = cleanName.LastIndexOf(" (", System.StringComparison.Ordinal);
            if (copyMarker > 0 && cleanName.EndsWith(")"))
                cleanName = cleanName.Substring(0, copyMarker);

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{folder}/{cleanName}.prefab");
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
            Debug.Log($"Created enemy prefab at {prefabPath}.", prefab);
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
    }
}

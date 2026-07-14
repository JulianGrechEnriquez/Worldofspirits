using UnityEditor;
using UnityEngine;

namespace WorldOfSpirits.EditorTools
{
    [InitializeOnLoad]
    public static class ProjectOrganizer
    {
        static ProjectOrganizer()
        {
            // Run once for the current untidy layout. Future organization remains
            // available from the menu and will not repeat after the files move.
            if (AssetDatabase.LoadMainAssetAtPath(
                    "Assets/ScriptableObjects/Wepons/Stone Hamer.asset") != null)
                EditorApplication.delayCall += OrganizeProjectFiles;
        }

        [MenuItem("World of Spirits/Organize Project Files %#o")]
        public static void OrganizeProjectFiles()
        {
            EnsureFolder("Assets/ScriptableObjects/Weapons");
            EnsureFolder("Assets/Prefabs/Weapons");

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

            DeleteFolderIfEmpty("Assets/ScriptableObjects/Wepons");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Project files organized. Unity asset references were preserved.");
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

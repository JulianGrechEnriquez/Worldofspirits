using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.EditorTools
{
    /// <summary>Reconnects ability assets without changing weapons or authored tuning.</summary>
    public static class SpiritPrefabConnector
    {
        [MenuItem("World of Spirits/Connect Existing Spirit Abilities")]
        public static void ConnectExistingSpiritPrefabs()
        {
            int connected = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/_WorldOfSpirits/Prefabs/Spirits" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                SpiritMember member = prefab.GetComponent<SpiritMember>();
                if (member == null || member.Definition == null) continue;
                AbilityAssetGenerator.ConnectPrefab(path, member.Definition);
                connected++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Connected data-driven abilities on {connected} spirit prefabs.");
        }
    }
}

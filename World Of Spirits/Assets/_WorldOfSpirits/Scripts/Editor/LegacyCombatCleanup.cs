using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.EditorTools
{
    /// <summary>One-time migration cleanup for superseded combat components.</summary>
    public static class LegacyCombatCleanup
    {
        private static readonly Type[] LegacyComponentTypes =
        {
            typeof(ProjectilePatternAbility),
            typeof(AreaPulseAbility),
            typeof(SpawnEffectAbility),
            typeof(OrbitingProjectileAbility),
            typeof(AutoProjectileWeapon)
        };

        private static readonly string[] PrefabPaths =
        {
            "Assets/_WorldOfSpirits/Prefabs/Spirits/Fire Spirit.prefab",
            "Assets/_WorldOfSpirits/Prefabs/Spirits/Earth Spirit.prefab",
            "Assets/_WorldOfSpirits/Prefabs/Spirits/Ice Spirit.prefab"
        };

        private static readonly string[] ScriptPaths =
        {
            "Assets/_WorldOfSpirits/Scripts/Spirits/Abilities/ProjectilePatternAbility.cs",
            "Assets/_WorldOfSpirits/Scripts/Spirits/Abilities/RadialProjectileAbility.cs",
            "Assets/_WorldOfSpirits/Scripts/Spirits/Abilities/AreaPulseAbility.cs",
            "Assets/_WorldOfSpirits/Scripts/Spirits/Abilities/SpawnEffectAbility.cs",
            "Assets/_WorldOfSpirits/Scripts/Spirits/Abilities/OrbitingProjectileAbility.cs",
            "Assets/_WorldOfSpirits/Scripts/Spirits/Abilities/ChainLightningAbility.cs",
            "Assets/_WorldOfSpirits/Scripts/Combat/Weapons/AutoProjectileWeapon.cs",
            "Assets/_WorldOfSpirits/Scripts/Combat/Projectiles/DamageProjectile.cs"
        };

        [MenuItem("World of Spirits/Cleanup Migrated Legacy Combat")]
        public static void Execute()
        {
            int componentsRemoved = 0;
            foreach (string prefabPath in PrefabPaths)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        MonoBehaviour behaviour = behaviours[i];
                        if (behaviour == null || !IsLegacyComponent(behaviour)) continue;

                        GameObject owner = behaviour.gameObject;
                        UnityEngine.Object.DestroyImmediate(behaviour);
                        componentsRemoved++;
                        if (owner.GetComponents<Component>().Length == 1 && owner.transform.childCount == 0)
                            UnityEngine.Object.DestroyImmediate(owner);
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            for (int i = 0; i < ScriptPaths.Length; i++)
            {
                AssetDatabase.DeleteAsset(ScriptPaths[i]);
            }

            // This helper is deliberately one-use so it cannot become another
            // redundant editor script after the migration is complete.
            AssetDatabase.DeleteAsset("Assets/_WorldOfSpirits/Scripts/Editor/LegacyCombatCleanup.cs");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Removed {componentsRemoved} migrated legacy combat components and deleted unused legacy scripts.");
        }

        private static bool IsLegacyComponent(MonoBehaviour behaviour)
        {
            Type type = behaviour.GetType();
            for (int i = 0; i < LegacyComponentTypes.Length; i++)
            {
                if (type == LegacyComponentTypes[i]) return true;
            }
            return false;
        }
    }
}

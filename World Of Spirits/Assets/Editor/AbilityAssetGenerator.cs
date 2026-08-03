using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.EditorTools
{
    public static class AbilityAssetGenerator
    {
        private const string AbilityFolder = "Assets/ScriptableObjects/Abilities/Generated";
        private const string SpiritFolder = "Assets/ScriptableObjects/Spirits";

        private sealed class Spec
        {
            public string Spirit, Name, Description;
            public AbilityExecutionType Type;
            public AbilityTargetingMode Target;
            public string[] Upgrades;
            public Spec(string spirit, string name, string description, AbilityExecutionType type,
                AbilityTargetingMode target, params string[] upgrades)
            { Spirit = spirit; Name = name; Description = description; Type = type; Target = target; Upgrades = upgrades; }
        }

        [MenuItem("World of Spirits/Generate Data-Driven Ability Assets")]
        public static void GenerateAll()
        {
            EnsureFolder("Assets/ScriptableObjects");
            EnsureFolder("Assets/ScriptableObjects/Abilities");
            EnsureFolder(AbilityFolder);
            EnsureFolder(SpiritFolder);

            Dictionary<string, List<AbilityDefinition>> bySpirit = new Dictionary<string, List<AbilityDefinition>>();
            foreach (Spec spec in GetSpecs())
            {
                AbilityDefinition ability = CreateOrUpdateAbility(spec);
                if (!bySpirit.TryGetValue(spec.Spirit, out List<AbilityDefinition> list))
                {
                    list = new List<AbilityDefinition>();
                    bySpirit.Add(spec.Spirit, list);
                }
                list.Add(ability);
            }

            foreach (KeyValuePair<string, List<AbilityDefinition>> entry in bySpirit)
            {
                SpiritDefinition spirit = EnsureSpiritDefinition(entry.Key);
                spirit.SetRuntimeAbilities(entry.Value);
                EditorUtility.SetDirty(spirit);
                ConnectPrefab(entry.Key, spirit, entry.Value);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated {bySpirit.Sum(item => item.Value.Count)} data-driven ability assets for {bySpirit.Count} spirits.");
        }

        private static AbilityDefinition CreateOrUpdateAbility(Spec spec)
        {
            string path = $"{AbilityFolder}/{spec.Spirit} - {spec.Name}.asset";
            AbilityDefinition asset = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<AbilityDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            AbilityLevelData[] levels = new AbilityLevelData[spec.Upgrades.Length];
            for (int i = 0; i < levels.Length; i++) levels[i] = BuildLevel(spec, i);
            asset.Configure(spec.Name, spec.Description, spec.Type, spec.Target, levels);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static AbilityLevelData BuildLevel(Spec spec, int index)
        {
            AbilityLevelData level = new AbilityLevelData
            {
                level = index + 1,
                upgradeDescription = spec.Upgrades[index],
                cooldown = Mathf.Max(0.3f, 2f - index * 0.12f),
                targetingRange = 15f,
                areaRadius = 3f + index * 0.5f,
                orbitRadius = 1.5f + index * 0.1f,
                orbitSpeed = 100f + index * 20f,
                spawnCount = 1,
                chainCount = 3 + index,
                chainRange = 5f + index,
                projectile = new AbilityProjectileData
                {
                    count = 1,
                    speed = 9f + index * 0.5f,
                    damage = 10f + index * 3f,
                    spreadAngle = 35f,
                    spreadMode = ProjectileSpreadMode.EvenlySpaced,
                    homingStrength = 6f,
                    homingRange = 10f
                }
            };

            string all = string.Join(" ", spec.Upgrades.Take(index + 1)).ToLowerInvariant();
            if (spec.Name == "Fiery Feathers")
            {
                GameObject fireFeather = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/FireFeather.prefab");
                level.projectile.projectilePrefab = fireFeather != null ? fireFeather.GetComponent<ProjectileBase>() : null;
                level.projectile.count = index == 0 ? 3 : 5;
                level.projectile.homeOnEnemies = true;
                level.projectile.explosionRadius = index >= 3 ? 1.5f : 0f;
                level.projectile.appliesStatus = index >= 4;
                level.projectile.status = CombatStatus.Burn;
            }
            else if (all.Contains("2 blades")) level.projectile.count = 2;
            else if (all.Contains("4 blades")) level.projectile.count = 4;
            else if (all.Contains("3 needles")) level.projectile.count = 3;
            else if (all.Contains("5 needles")) level.projectile.count = 5;
            else if (all.Contains("2 poison blobs")) level.projectile.count = 2;

            if (spec.Name == "Razor Wind")
            {
                level.projectile.spreadAngle = 360f;
                level.projectile.count = index == 0 ? 2 : 4;
            }
            if (spec.Name == "Tidal Wave")
            {
                level.projectile.count = index == 0 ? 1 : index < 3 ? 2 : 4;
                level.projectile.spreadAngle = index == 0 ? 0f : index < 3 ? 180f : 270f;
            }
            if (spec.Name == "Lightning Strike") level.spawnCount = index == 0 ? 3 : 6;
            if (spec.Name == "Chain Lightning Bolt") level.chainCount = index == 0 ? 3 : 5;
            if (spec.Name == "Boulder Throw")
            {
                GameObject boulder = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Spirits/boulder.prefab");
                level.projectile.projectilePrefab = boulder != null ? boulder.GetComponent<ProjectileBase>() : null;
                level.projectile.bounceCount = 2 + index;
                level.projectile.bounceRange = 5f + index;
            }
            if (spec.Name == "Orbital Snowball")
            {
                level.spawnCount = index == 0 ? 2 : index < 3 ? 3 : 4;
                level.spawnedEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Prefabs/Abilities/Orbital Snowball.prefab");
            }
            if (spec.Name == "Acid Spray") level.projectile.spreadAngle = 45f + index * 12f;

            if (all.Contains("pierce")) level.projectile.pierceCount = 3;
            if (all.Contains("explode")) level.projectile.explosionRadius = 1.5f;
            if (all.Contains("freeze")) { level.projectile.appliesStatus = true; level.projectile.status = CombatStatus.Freeze; }
            if (all.Contains("poison")) { level.projectile.appliesStatus = true; level.projectile.status = CombatStatus.Poison; }
            if (all.Contains("larger") || all.Contains("wider")) level.areaRadius += 1f;
            if (all.Contains("two whirlpools") || all.Contains("two clouds") || all.Contains("two tornadoes")) level.spawnCount = 2;
            if (all.Contains("3 orbs") || all.Contains("3 snowballs")) level.spawnCount = 3;
            if (all.Contains("additional orb") || all.Contains("additional snowball")) level.spawnCount = 4;

            AddDefaultEffects(spec, level, index);
            return level;
        }

        private static void AddDefaultEffects(Spec spec, AbilityLevelData level, int index)
        {
            if (spec.Type == AbilityExecutionType.Area || spec.Type == AbilityExecutionType.Chain)
                level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.Damage, value = 8f + index * 4f });
            if (spec.Name.Contains("Quicksand"))
            {
                level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.ApplyStatus, status = CombatStatus.Slow, value = 0.25f + index * 0.1f, duration = 2f });
                if (index >= 3) level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.Pull, value = 1f + index });
            }
            if (spec.Name.Contains("Thunder Roar"))
            {
                level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.Knockback, value = 2f + index });
                if (index >= 1) level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.ApplyStatus, status = CombatStatus.Stun, duration = 1f, value = 1f });
            }
            if (spec.Name == "Healing") level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.Heal, value = 10f + index * 5f });
            if (spec.Name == "Shields") level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.Shield, value = 20f + index * 10f, duration = 5f });
            if (spec.Name == "Phoenix Dive" && index == spec.Upgrades.Length - 1)
                level.effects.Add(new AbilityEffectData { effectType = AbilityEffectType.GrantRevive, value = 1f });
        }

        private static SpiritDefinition EnsureSpiritDefinition(string spiritName)
        {
            string path = $"{SpiritFolder}/{spiritName} Spirit.asset";
            SpiritDefinition asset = AssetDatabase.LoadAssetAtPath<SpiritDefinition>(path);
            if (asset != null) return asset;

            Dictionary<string, (string shape, string weapon)> data = new Dictionary<string, (string, string)>
            {
                ["Fire"] = ("Phoenix", "Fire Bow"), ["Earth"] = ("Golem", "Stone Hammer"),
                ["Water"] = ("Leviathan", "Water Trident"), ["Wind"] = ("Roc", "Chakrams"),
                ["Ice"] = ("Yeti", "Ice Gauntlets"), ["Lightning"] = ("Thunder Dragon", "Lightning Spear"),
                ["Poison"] = ("Scorpion", "Poison Daggers"), ["Necrotic"] = ("Bat", "Necrotic Katana"),
                ["Holy"] = ("Biblical Angel", "Holy Sword")
            };
            (string shape, string weapon) values = data[spiritName];
            asset = ScriptableObject.CreateInstance<SpiritDefinition>();
            asset.Configure(spiritName + " Spirit", values.shape,
                new SpiritWeaponDefinition(values.weapon, "The primary weapon used while standing still.", "Weapon unlocked"));
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void ConnectPrefab(string spiritName, SpiritDefinition spirit, List<AbilityDefinition> abilities)
        {
            string path = $"Assets/Prefabs/Spirits/{spiritName} Spirit.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) return;
            SpiritMember member = root.GetComponent<SpiritMember>() ?? root.AddComponent<SpiritMember>();
            SerializedObject memberData = new SerializedObject(member);
            memberData.FindProperty("definition").objectReferenceValue = spirit;
            memberData.ApplyModifiedPropertiesWithoutUndo();

            SpiritAbility[] oldAbilities = root.GetComponentsInChildren<SpiritAbility>(true);
            foreach (SpiritAbility old in oldAbilities)
                if (!(old is DataDrivenAbility)) old.enabled = false;

            for (int i = 0; i < abilities.Count; i++)
            {
                string childName = $"Runtime Ability {i + 1} - {abilities[i].AbilityName}";
                Transform child = root.transform.Find(childName);
                GameObject objectWithAbility = child != null ? child.gameObject : new GameObject(childName);
                objectWithAbility.transform.SetParent(root.transform, false);
                DataDrivenAbility runner = objectWithAbility.GetComponent<DataDrivenAbility>() ?? objectWithAbility.AddComponent<DataDrivenAbility>();
                SerializedObject runnerData = new SerializedObject(runner);
                runnerData.FindProperty("abilityIndex").intValue = i;
                runnerData.FindProperty("definition").objectReferenceValue = abilities[i];
                runnerData.FindProperty("castWhileMoving").boolValue = true;
                runnerData.FindProperty("castWhileStandingStill").boolValue = false;
                runnerData.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private static IEnumerable<Spec> GetSpecs()
        {
            yield return new Spec("Fire", "Fiery Feathers", "Shoots homing feathers in a fan shape.", AbilityExecutionType.Projectile, AbilityTargetingMode.ClosestEnemy, "3 feathers", "+2 feathers", "Increased damage", "Exploding feathers", "Burning feathers leave fire patches");
            yield return new Spec("Fire", "Fiery Talons", "Leaves a trail of fire behind the player.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.AroundPlayer, "Larger trail", "Longer duration", "More damage", "Trail spreads to nearby enemies", "Burning enemies explode");
            yield return new Spec("Fire", "Phoenix Dive", "A flaming phoenix dives through enemies.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.ClosestEnemy, "More damage", "Multiple dives", "Leaves fire zones", "Larger area", "Revives once per run");
            yield return new Spec("Earth", "Boulder Throw", "Throws bouncing boulders.", AbilityExecutionType.Projectile, AbilityTargetingMode.ClosestEnemy, "More bounces", "More damage", "Splits into smaller rocks", "Stuns enemies", "Explodes on final bounce");
            yield return new Spec("Earth", "Quicksand Domain", "Slows enemies around the player.", AbilityExecutionType.Area, AbilityTargetingMode.AroundPlayer, "Bigger radius", "Stronger slow", "Damage over time", "Pulls enemies inward", "Immobilizes elites briefly");
            yield return new Spec("Earth", "Stone Spikes", "Stone pillars erupt from the ground.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.RandomPositionNearPlayer, "More spikes", "Larger spikes", "Faster spawn rate", "Bleed effect", "Chain eruptions");
            yield return new Spec("Water", "Tidal Wave", "A wave crashes outward, knocking enemies back.", AbilityExecutionType.Projectile, AbilityTargetingMode.PlayerFacing, "Wave in front", "Additional wave behind", "Wider waves", "Waves left and right");
            yield return new Spec("Water", "Whirlpool", "Summons whirlpools that pull enemies inward.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.RandomPositionNearPlayer, "One whirlpool", "Increased radius", "Two whirlpools", "Damage over time");
            yield return new Spec("Water", "Rain Clouds", "Rain clouds follow enemies and damage them.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.ClosestEnemy, "One cloud", "Two clouds", "Increased rain damage", "Clouds move faster");
            yield return new Spec("Wind", "Razor Wind", "Wind blades shoot outward from the player.", AbilityExecutionType.Projectile, AbilityTargetingMode.AroundPlayer, "2 blades", "4 blades", "Increased projectile speed", "Blades pierce enemies");
            yield return new Spec("Wind", "Tornado", "Creates a moving tornado.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.RandomPositionNearPlayer, "One tornado", "Larger tornado", "Increased pull strength", "Two tornadoes");
            yield return new Spec("Ice", "Orbital Snowball", "Snowballs orbit around the player and damage enemies they touch.", AbilityExecutionType.Orbiting, AbilityTargetingMode.AroundPlayer, "2 snowballs", "3 snowballs", "Increased rotation speed and freeze chance", "Additional snowball");
            yield return new Spec("Ice", "Avalanche", "Throws a snowball that grows as it travels.", AbilityExecutionType.Projectile, AbilityTargetingMode.ClosestEnemy, "Small snowball", "Faster growth", "Increased damage", "Freeze enemies hit");
            yield return new Spec("Ice", "Ice Crystal", "Spawns an ice crystal that grows then explodes.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.ClosestEnemy, "Small crystal", "Faster growth", "Increased damage", "Freeze enemies hit");
            yield return new Spec("Lightning", "Lightning Strike", "Lightning strikes random enemies.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.RandomEnemy, "3 strikes", "6 strikes", "Increased damage", "Small area of effect");
            yield return new Spec("Lightning", "Chain Lightning Bolt", "A lightning bolt jumps between enemies.", AbilityExecutionType.Chain, AbilityTargetingMode.ClosestEnemy, "3 jumps", "5 jumps", "Increased damage", "Increased range");
            yield return new Spec("Lightning", "Thunder Roar", "A lightning pulse surrounds the player.", AbilityExecutionType.Area, AbilityTargetingMode.AroundPlayer, "1 ring", "Pushes and stuns enemies", "3 rings", "Increased range");
            yield return new Spec("Poison", "Toxic Glob", "Poison blobs explode into toxic pools.", AbilityExecutionType.Projectile, AbilityTargetingMode.ClosestEnemy, "1 poison blob", "2 poison blobs", "Larger poison pools", "Pools last longer");
            yield return new Spec("Poison", "Venom Needles", "Rapidly fires piercing poison needles.", AbilityExecutionType.Projectile, AbilityTargetingMode.ClosestEnemy, "3 needles", "5 needles", "Increased piercing", "Increased attack speed");
            yield return new Spec("Poison", "Acid Spray", "Sprays acid in a cone.", AbilityExecutionType.Projectile, AbilityTargetingMode.PlayerFacing, "Wider cone", "Longer range", "Increased damage", "Melts enemy armor", "Leaves acid pools");
            yield return new Spec("Holy", "Healing", "Restores player health.", AbilityExecutionType.Self, AbilityTargetingMode.Self, "Basic healing", "More healing", "Shorter cooldown", "Greater healing");
            yield return new Spec("Holy", "Shields", "Protects the player with a temporary shield.", AbilityExecutionType.Self, AbilityTargetingMode.Self, "Basic shield", "Stronger shield", "Longer duration", "Greater shield");
            yield return new Spec("Holy", "Light Beams", "Calls down beams of holy light.", AbilityExecutionType.SpawnEffect, AbilityTargetingMode.RandomEnemy, "One beam", "Two beams", "Increased damage", "Larger area");
        }
    }
}

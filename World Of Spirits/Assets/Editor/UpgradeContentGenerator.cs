#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Progression.Upgrades;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.EditorTools
{
    public static class UpgradeContentGenerator
    {
        private const string Root = "Assets/ScriptableObjects/Upgrades";

        private readonly struct Seed
        {
            public Seed(string name, string description, UpgradeRarity rarity, int max, UpgradeStat stat,
                float value, string icon, UpgradeCategory category = UpgradeCategory.Player)
            { Name = name; Description = description; Rarity = rarity; Max = max; Stat = stat; Value = value; Icon = icon; Category = category; }
            public readonly string Name, Description, Icon;
            public readonly UpgradeRarity Rarity;
            public readonly UpgradeCategory Category;
            public readonly UpgradeStat Stat;
            public readonly int Max;
            public readonly float Value;
        }

        [MenuItem("World of Spirits/Upgrades/Generate Starter Catalog")]
        public static void Generate()
        {
            EnsureFolder(Root);
            EnsureFolder(Root + "/Player");
            EnsureFolder(Root + "/Legendary");
            EnsureFolder(Root + "/Spirits");
            List<UpgradeCardDefinition> cards = new List<UpgradeCardDefinition>(128);
            AddSeeds(cards, PlayerSeeds, Root + "/Player");
            AddSeeds(cards, LegendarySeeds, Root + "/Legendary");
            AddSpiritCards(cards);

            UpgradeCatalog catalog = LoadOrCreate<UpgradeCatalog>(Root + "/Main Upgrade Catalog.asset");
            SerializedObject catalogObject = new SerializedObject(catalog);
            SerializedProperty list = catalogObject.FindProperty("cards");
            list.arraySize = cards.Count;
            for (int i = 0; i < cards.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            catalogObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Selection.activeObject = catalog;
            Debug.Log($"Generated {cards.Count} upgrade cards and selected the Main Upgrade Catalog.");
        }

        private static void AddSeeds(List<UpgradeCardDefinition> output, Seed[] seeds, string folder)
        {
            for (int i = 0; i < seeds.Length; i++)
            {
                Seed seed = seeds[i];
                string id = Slug(seed.Name);
                UpgradeCardDefinition card = LoadOrCreate<UpgradeCardDefinition>($"{folder}/{Safe(seed.Name)}.asset");
                Configure(card, id, seed.Name, seed.Description, seed.Icon, seed.Category, seed.Rarity,
                    seed.Max, seed.Rarity == UpgradeRarity.Legendary ? 15 : 100, 1, null, -1, null,
                    seed.Stat, seed.Value);
                output.Add(card);
            }
        }

        private static void AddSpiritCards(List<UpgradeCardDefinition> output)
        {
            string[] guids = AssetDatabase.FindAssets("t:SpiritDefinition");
            for (int i = 0; i < guids.Length; i++)
            {
                SpiritDefinition spirit = AssetDatabase.LoadAssetAtPath<SpiritDefinition>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (spirit == null) continue;
                string spiritName = string.IsNullOrWhiteSpace(spirit.SpiritName) ? spirit.name : spirit.SpiritName;
                GameObject prefab = FindSpiritPrefab(spiritName);
                SpiritMember prefabMember = prefab != null ? prefab.GetComponentInChildren<SpiritMember>(true) : null;
                if (prefabMember == null || prefabMember.Definition != spirit)
                {
                    continue;
                }
                string folder = Root + "/Spirits/" + Safe(spiritName);
                EnsureFolder(folder);

                UpgradeCardDefinition contract = LoadOrCreate<UpgradeCardDefinition>($"{folder}/Contract the {Safe(spiritName)}.asset");
                Configure(contract, "contract_" + Slug(spiritName), "Contract the " + spiritName,
                    $"Adds the {spiritName} to your formation and unlocks its weapon and ability upgrade paths.",
                    "Ancient spirit seal and elemental silhouette", UpgradeCategory.SpiritContract, UpgradeRarity.Rare,
                    1, 18, 4, spirit, -1, prefab, UpgradeStat.Luck, 0f);
                output.Add(contract);

                int weaponMax = spirit.RuntimeWeapon != null ? spirit.RuntimeWeapon.MaxLevel : spirit.Weapon.MaxLevel;
                UpgradeCardDefinition weapon = LoadOrCreate<UpgradeCardDefinition>($"{folder}/{Safe(spiritName)} Weapon Mastery.asset");
                Configure(weapon, "weapon_" + Slug(spiritName), spiritName + " Weapon Mastery",
                    "Improves the manifested weapon to its next authored level.", "Elemental weapon on a radiant anvil",
                    UpgradeCategory.Weapon, UpgradeRarity.Common, Mathf.Max(1, weaponMax - 1), 100, 2,
                    spirit, -1, null, UpgradeStat.AttackDamage, 0f);
                output.Add(weapon);

                int abilityCount = spirit.RuntimeAbilities.Count > 0 ? spirit.RuntimeAbilities.Count : spirit.Abilities.Count;
                for (int ability = 0; ability < abilityCount; ability++)
                {
                    string abilityName = spirit.RuntimeAbilities.Count > 0 ? spirit.RuntimeAbilities[ability].AbilityName : spirit.Abilities[ability].AbilityName;
                    int max = spirit.RuntimeAbilities.Count > 0 ? spirit.RuntimeAbilities[ability].MaxLevel : spirit.Abilities[ability].MaxLevel;
                    UpgradeCardDefinition abilityCard = LoadOrCreate<UpgradeCardDefinition>($"{folder}/{Safe(abilityName)}.asset");
                    Configure(abilityCard, "ability_" + Slug(spiritName) + "_" + ability, abilityName,
                        ability == 0 ? "Improves this spirit's first ability." : "Unlocks or improves this advanced spirit ability.",
                        "Elemental ability glyph", UpgradeCategory.SpiritAbility,
                        ability == 0 ? UpgradeRarity.Common : UpgradeRarity.Uncommon, Mathf.Max(1, max), 100, ability * 4 + 2,
                        spirit, ability, null, UpgradeStat.SpiritDamage, 0f);
                    output.Add(abilityCard);
                }

                UpgradeCardDefinition evolution = LoadOrCreate<UpgradeCardDefinition>($"{folder}/{Safe(spiritName)} Ascension.asset");
                Configure(evolution, "evolution_" + Slug(spiritName), spiritName + " Ascension",
                    "Transforms the spirit's appearance and empowers its complete kit. Add its unique behaviour to the spirit prefab.",
                    "Transformed spirit breaking through a luminous card", UpgradeCategory.Evolution, UpgradeRarity.Legendary,
                    1, 3, 25, spirit, -1, null, UpgradeStat.SpiritDamage, 0.5f);
                output.Add(evolution);
            }
        }

        private static void Configure(UpgradeCardDefinition card, string id, string title, string description,
            string iconTheme, UpgradeCategory category, UpgradeRarity rarity, int max, float weight, int minimumLevel,
            SpiritDefinition spirit, int ability, GameObject prefab, UpgradeStat stat, float value)
        {
            SerializedObject so = new SerializedObject(card);
            so.FindProperty("cardId").stringValue = id;
            so.FindProperty("cardName").stringValue = title;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("suggestedIconTheme").stringValue = iconTheme;
            so.FindProperty("category").enumValueIndex = (int)category;
            so.FindProperty("rarity").enumValueIndex = (int)rarity;
            so.FindProperty("maximumLevel").intValue = Mathf.Max(1, max);
            so.FindProperty("baseWeight").floatValue = weight;
            so.FindProperty("targetSpirit").objectReferenceValue = spirit;
            so.FindProperty("abilityIndex").intValue = ability;
            so.FindProperty("spiritPrefab").objectReferenceValue = prefab;
            SerializedProperty modifiers = so.FindProperty("modifiers");
            modifiers.arraySize = Mathf.Approximately(value, 0f) ? 0 : 1;
            if (modifiers.arraySize > 0)
            {
                SerializedProperty modifier = modifiers.GetArrayElementAtIndex(0);
                modifier.FindPropertyRelative("stat").enumValueIndex = (int)stat;
                modifier.FindPropertyRelative("operation").enumValueIndex = (int)ModifierOperation.Add;
                modifier.FindPropertyRelative("valuePerLevel").floatValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(card);
        }

        private static GameObject FindSpiritPrefab(string spiritName)
        {
            string[] ids = AssetDatabase.FindAssets($"{spiritName} t:Prefab", new[] { "Assets/Prefabs/Spirits" });
            return ids.Length > 0 ? AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(ids[0])) : null;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
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

        private static string Safe(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value;
        }
        private static string Slug(string value) => Safe(value.Trim().ToLowerInvariant().Replace(' ', '_'));

        private static readonly Seed[] PlayerSeeds =
        {
            new Seed("Vitality", "+15 maximum health per level.", UpgradeRarity.Common, 5, UpgradeStat.MaxHealth, 15, "Red heart and spirit leaf"),
            new Seed("Tamer's Might", "+10% attack damage.", UpgradeRarity.Common, 5, UpgradeStat.AttackDamage, .10f, "Cracked training stone"),
            new Seed("Quick Hands", "+8% attack speed.", UpgradeRarity.Common, 5, UpgradeStat.AttackSpeed, .08f, "Fast spectral hands"),
            new Seed("Keen Eye", "+4% critical chance.", UpgradeRarity.Uncommon, 5, UpgradeStat.CriticalChance, .04f, "Glowing eye and crosshair"),
            new Seed("Crushing Crits", "+20% critical damage.", UpgradeRarity.Uncommon, 5, UpgradeStat.CriticalDamage, .20f, "Shattered golden star"),
            new Seed("Fleet Foot", "+7% movement speed.", UpgradeRarity.Common, 5, UpgradeStat.MovementSpeed, .07f, "Wind-wrapped boot"),
            new Seed("Flow State", "+6% cooldown reduction.", UpgradeRarity.Uncommon, 5, UpgradeStat.CooldownReduction, .06f, "Circular flowing runes"),
            new Seed("Spirit Magnet", "+20% pickup radius.", UpgradeRarity.Common, 5, UpgradeStat.PickupRadius, .20f, "Crystal magnet"),
            new Seed("Elemental Focus", "+12% elemental damage.", UpgradeRarity.Uncommon, 5, UpgradeStat.ElementalDamage, .12f, "Four elemental motes"),
            new Seed("Fortune's Favor", "+10 luck.", UpgradeRarity.Rare, 3, UpgradeStat.Luck, 10, "Lucky spirit charm"),
            new Seed("Studious Tamer", "+12% experience gained.", UpgradeRarity.Common, 5, UpgradeStat.ExperienceGain, .12f, "Open glowing bestiary"),
            new Seed("Golden Pact", "+15% gold gained.", UpgradeRarity.Uncommon, 5, UpgradeStat.GoldGain, .15f, "Contract wrapped in coins"),
            new Seed("Spirit Mending", "Regenerate 1 health each second.", UpgradeRarity.Uncommon, 5, UpgradeStat.HealthRegeneration, 1, "Green spirit stitching a heart"),
            new Seed("Evasive Step", "+3% chance to dodge damage.", UpgradeRarity.Rare, 5, UpgradeStat.DodgeChance, .03f, "Afterimage sidestep"),
            new Seed("Crystal Ward", "Improves generated shields by 15%.", UpgradeRarity.Rare, 4, UpgradeStat.ShieldGeneration, .15f, "Layered crystal shield"),
            new Seed("Restorative Bond", "+15% healing received.", UpgradeRarity.Uncommon, 4, UpgradeStat.HealingPower, .15f, "Linked green souls"),
            new Seed("Spirit Commander", "+10% damage from spirit abilities.", UpgradeRarity.Uncommon, 5, UpgradeStat.SpiritDamage, .10f, "Tamer directing three spirits"),
            new Seed("Expansive Aura", "+8% area size.", UpgradeRarity.Uncommon, 5, UpgradeStat.AreaSize, .08f, "Expanding magic rings"),
            new Seed("Forceful Casting", "+12% knockback strength.", UpgradeRarity.Common, 5, UpgradeStat.Knockback, .12f, "Enemy pushed by a wave"),
            new Seed("Swift Projectiles", "+12% projectile speed.", UpgradeRarity.Common, 5, UpgradeStat.ProjectileSpeed, .12f, "Comet-like spirit bolt"),
            new Seed("Elite Hunter", "+15% damage against elite enemies.", UpgradeRarity.Rare, 4, UpgradeStat.EliteDamage, .15f, "Crowned monster target"),
            new Seed("Merciful End", "Raises the low-health execute threshold by 2%.", UpgradeRarity.Epic, 3, UpgradeStat.ExecuteThreshold, .02f, "Fading enemy silhouette"),
            new Seed("Colossal Casting", "+10% projectile and weapon size.", UpgradeRarity.Rare, 4, UpgradeStat.ProjectileSize, .10f, "Small bolt becoming enormous"),
            new Seed("Penetrating Force", "+1 projectile pierce.", UpgradeRarity.Rare, 3, UpgradeStat.Pierce, 1, "Bolt piercing three targets"),
            new Seed("Rebounding Essence", "+1 ricochet.", UpgradeRarity.Epic, 2, UpgradeStat.Ricochet, 1, "Zigzagging elemental bolt"),
            new Seed("Seeking Spirits", "Improves homing strength.", UpgradeRarity.Rare, 4, UpgradeStat.Homing, .20f, "Curved arrows around a target"),
            new Seed("Twin Invocation", "+1 additional projectile where supported.", UpgradeRarity.Epic, 2, UpgradeStat.MultiShot, 1, "Mirrored spell bolts"),
            new Seed("Lingering Magic", "+15% effect duration.", UpgradeRarity.Uncommon, 5, UpgradeStat.Duration, .15f, "Hourglass filled with magic"),
            new Seed("Spirit Armor", "+8 armor, reducing incoming damage.", UpgradeRarity.Uncommon, 5, UpgradeStat.Armor, 8, "Spectral shoulder armor"),
            new Seed("Magnetic Surge", "Greatly increases pickup radius.", UpgradeRarity.Rare, 3, UpgradeStat.PickupRadius, .50f, "Screen-wide magnetic pulse")
        };

        private static readonly Seed[] LegendarySeeds =
        {
            new Seed("Elemental Master", "All elemental power surges by 50%.", UpgradeRarity.Legendary, 1, UpgradeStat.ElementalDamage, .5f, "All elements in one sigil", UpgradeCategory.Legendary),
            new Seed("Avatar State", "Spirit ability damage rises by 60%.", UpgradeRarity.Legendary, 1, UpgradeStat.SpiritDamage, .6f, "Tamer surrounded by spirit avatars", UpgradeCategory.Legendary),
            new Seed("Spirit Lord", "Weapons and abilities deal 40% more damage.", UpgradeRarity.Legendary, 1, UpgradeStat.AttackDamage, .4f, "Crown of spirit flames", UpgradeCategory.Legendary),
            new Seed("Ancient Crystal", "Gain 100 maximum health.", UpgradeRarity.Legendary, 1, UpgradeStat.MaxHealth, 100, "Primordial crystal heart", UpgradeCategory.Legendary),
            new Seed("Echo of the Ancients", "Gain two additional projectiles where supported.", UpgradeRarity.Legendary, 1, UpgradeStat.MultiShot, 2, "Three ancient echoes", UpgradeCategory.Legendary),
            new Seed("Infinite Momentum", "Movement speed rises by 35%.", UpgradeRarity.Legendary, 1, UpgradeStat.MovementSpeed, .35f, "Endless wind trail", UpgradeCategory.Legendary),
            new Seed("Arcane Overflow", "Cooldowns recover 30% faster.", UpgradeRarity.Legendary, 1, UpgradeStat.CooldownReduction, .3f, "Overflowing arcane vessel", UpgradeCategory.Legendary),
            new Seed("Giant Soul", "All attack areas grow by 45%.", UpgradeRarity.Legendary, 1, UpgradeStat.AreaSize, .45f, "Giant spirit shadow", UpgradeCategory.Legendary),
            new Seed("Unbroken Pact", "Regenerate 5 health each second.", UpgradeRarity.Legendary, 1, UpgradeStat.HealthRegeneration, 5, "Unbroken glowing contract", UpgradeCategory.Legendary),
            new Seed("Fate Weaver", "Gain 50 luck.", UpgradeRarity.Legendary, 1, UpgradeStat.Luck, 50, "Golden threads of fate", UpgradeCategory.Legendary),
            new Seed("Phantom Form", "Gain 15% dodge chance.", UpgradeRarity.Legendary, 1, UpgradeStat.DodgeChance, .15f, "Transparent tamer afterimage", UpgradeCategory.Legendary),
            new Seed("Titan Breaker", "Deal 60% more damage to elites.", UpgradeRarity.Legendary, 1, UpgradeStat.EliteDamage, .6f, "Broken titan crown", UpgradeCategory.Legendary),
            new Seed("Final Judgment", "Greatly raises execute threshold.", UpgradeRarity.Legendary, 1, UpgradeStat.ExecuteThreshold, .1f, "Judgment blade over fading foe", UpgradeCategory.Legendary),
            new Seed("World Piercer", "Projectiles gain three pierces.", UpgradeRarity.Legendary, 1, UpgradeStat.Pierce, 3, "Beam crossing an army", UpgradeCategory.Legendary),
            new Seed("Endless Rebound", "Projectiles gain three ricochets.", UpgradeRarity.Legendary, 1, UpgradeStat.Ricochet, 3, "Infinite bouncing rune", UpgradeCategory.Legendary),
            new Seed("Living Arsenal", "Attack speed rises by 40%.", UpgradeRarity.Legendary, 1, UpgradeStat.AttackSpeed, .4f, "Orbiting spirit weapons", UpgradeCategory.Legendary),
            new Seed("Perfect Resonance", "Critical chance rises by 25%.", UpgradeRarity.Legendary, 1, UpgradeStat.CriticalChance, .25f, "Perfectly aligned wave rings", UpgradeCategory.Legendary),
            new Seed("Cataclysmic Crits", "Critical damage rises by 100%.", UpgradeRarity.Legendary, 1, UpgradeStat.CriticalDamage, 1f, "Golden catastrophic impact", UpgradeCategory.Legendary),
            new Seed("Timeless Sorcery", "Effects last 75% longer.", UpgradeRarity.Legendary, 1, UpgradeStat.Duration, .75f, "Frozen clock in magic", UpgradeCategory.Legendary),
            new Seed("Singularity", "Attacks grow dramatically larger.", UpgradeRarity.Legendary, 1, UpgradeStat.ProjectileSize, .6f, "Elemental singularity", UpgradeCategory.Legendary)
        };
    }
}
#endif

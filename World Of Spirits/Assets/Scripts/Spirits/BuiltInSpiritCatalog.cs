using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Spirits
{
    public static class BuiltInSpiritCatalog
    {
        private static readonly Dictionary<string, SpiritDefinition> Definitions = CreateDefinitions();

        public static SpiritDefinition Find(string spiritName)
        {
            if (string.IsNullOrWhiteSpace(spiritName))
            {
                return null;
            }

            string cleanedName = spiritName.Replace("(Clone)", string.Empty).Replace("Spirit", string.Empty).Trim();
            return Definitions.TryGetValue(cleanedName, out SpiritDefinition definition) ? definition : null;
        }

        private static Dictionary<string, SpiritDefinition> CreateDefinitions()
        {
            Dictionary<string, SpiritDefinition> definitions = new Dictionary<string, SpiritDefinition>(StringComparer.OrdinalIgnoreCase);
            Add(definitions, "Fire", "Phoenix", "Fire Bow",
                Ability("Fiery Feathers", "Shoots homing feathers in a fan shape.", "3 feathers", "+2 feathers", "Increased damage", "Exploding feathers", "Burning feathers leave fire patches"),
                Ability("Fiery Talons", "Leaves a trail of fire behind the player.", "Larger trail", "Longer duration", "More damage", "Trail spreads to nearby enemies", "Burning enemies explode"),
                Ability("Phoenix Dive", "A flaming phoenix dives through enemies.", "More damage", "Multiple dives", "Leaves fire zones", "Larger area", "Revives once per run"));
            Add(definitions, "Earth", "Golem", "Stone Hammer",
                Ability("Quicksand Domain", "Slows enemies around the player.", "Bigger radius", "Stronger slow", "Damage over time", "Pulls enemies inward", "Immobilizes elites briefly"),
                Ability("Boulder Throw", "Throws bouncing boulders.", "More bounces", "More damage", "Splits into smaller rocks", "Stuns enemies", "Explodes on final bounce"),
                Ability("Stone Spikes", "Stone pillars erupt from the ground.", "More spikes", "Larger spikes", "Faster spawn rate", "Bleed effect", "Chain eruptions"));
            Add(definitions, "Water", "Leviathan", "Water Trident",
                Ability("Tidal Wave", "A wave crashes outward, knocking enemies back.", "Fires a wave in front of the player", "Fires an additional wave behind the player", "Waves become wider", "Fires waves to the left and right"),
                Ability("Whirlpool", "Summons whirlpools that pull enemies inward.", "Summons one whirlpool near the player", "Whirlpool radius increased", "Summons two whirlpools", "Whirlpool deals damage over time"),
                Ability("Rain Clouds", "Rain clouds follow enemies and continuously damage them.", "One cloud", "Two clouds", "Increased rain damage", "Clouds move faster"));
            Add(definitions, "Wind", "Roc", "Chakrams",
                Ability("Razor Wind", "Wind blades shoot outward from the player.", "2 blades", "4 blades", "Increased projectile speed", "Blades pierce enemies"),
                Ability("Tornado", "Creates a moving tornado.", "One tornado", "Larger tornado", "Increased pull strength", "Two tornadoes"));
            Add(definitions, "Ice", "Yeti", "Ice Gauntlets",
                Ability("Frozen Orbs", "Ice orbs orbit around the player.", "2 orbs", "3 orbs", "Increased rotation speed and freeze chance", "Additional orb"),
                Ability("Avalanche", "Throws a snowball that grows larger as it travels.", "Small snowball", "Faster growth", "Increased damage", "Freeze enemies hit"),
                Ability("Ice Crystal", "Spawns an ice crystal that grows, then explodes.", "Small crystal", "Faster growth", "Increased damage", "Freeze enemies hit"));
            Add(definitions, "Lightning", "Thunder Dragon", "Lightning Spear",
                Ability("Lightning Strike", "Lightning strikes random enemies.", "3 strikes", "6 strikes", "Increased damage", "Each strike creates a small area of effect"),
                Ability("Chain Lightning Bolt", "Shoots a lightning bolt that jumps between enemies.", "3 jumps", "5 jumps", "Increased damage", "Increased range"),
                Ability("Thunder Roar", "A pulse of lightning surrounds the player.", "1 ring of lightning", "Pushes enemies back and stuns them", "3 rings of lightning", "Increased range"));
            Add(definitions, "Poison", "Scorpion", "Poison Daggers",
                Ability("Toxic Glob", "Launches poisonous blobs that explode into toxic pools.", "1 poison blob", "2 poison blobs", "Larger poison pools", "Pools last longer"),
                Ability("Venom Needles", "Rapidly fires piercing poison needles.", "3 needles", "5 needles", "Increased piercing", "Increased attack speed"),
                Ability("Acid Spray", "Sprays acid in a cone in front of the player.", "Wider cone", "Longer range", "Increased damage", "Melts enemy armor", "Leaves acid pools"));
            Add(definitions, "Necrotic", "Bat", "Necrotic Katana");
            Add(definitions, "Holy", "Biblical Angel", "Holy Sword",
                Ability("Healing", "To be designed."), Ability("Shields", "To be designed."), Ability("Light Beams", "To be designed."));
            return definitions;
        }

        private static void Add(Dictionary<string, SpiritDefinition> definitions, string name, string shape,
            string weaponName, params SpiritAbilityDefinition[] abilities)
        {
            SpiritDefinition definition = ScriptableObject.CreateInstance<SpiritDefinition>();
            definition.name = name + " Spirit";
            definition.hideFlags = HideFlags.HideAndDontSave;
            definition.Configure(name + " Spirit", shape,
                new SpiritWeaponDefinition(weaponName, "The primary spirit channels this weapon while the player is standing still.", "Weapon unlocked"), abilities);
            definitions.Add(name, definition);
        }

        private static SpiritAbilityDefinition Ability(string name, string description, params string[] levels)
        {
            return new SpiritAbilityDefinition(name, description, levels);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Spirits
{
    [CreateAssetMenu(fileName = "Spirit Definition", menuName = "World of Spirits/Spirit Definition")]
    public class SpiritDefinition : ScriptableObject
    {
        [SerializeField] private string spiritName;
        [SerializeField] private string shape;
        [SerializeField] private SpiritWeaponDefinition weapon = new SpiritWeaponDefinition();
        [SerializeField] private List<SpiritAbilityDefinition> abilities = new List<SpiritAbilityDefinition>();

        public string SpiritName => spiritName;
        public string Shape => shape;
        public SpiritWeaponDefinition Weapon => weapon;
        public IReadOnlyList<SpiritAbilityDefinition> Abilities => abilities;

        public void Configure(string name, string spiritShape, SpiritWeaponDefinition spiritWeapon,
            params SpiritAbilityDefinition[] spiritAbilities)
        {
            spiritName = name;
            shape = spiritShape;
            weapon = spiritWeapon;
            abilities = new List<SpiritAbilityDefinition>(spiritAbilities);
        }
    }

    [Serializable]
    public class SpiritWeaponDefinition
    {
        [SerializeField] private string weaponName;
        [TextArea(2, 4)] [SerializeField] private string description;
        [SerializeField] private List<SpiritLevelDefinition> levels = new List<SpiritLevelDefinition>();

        public string WeaponName => weaponName;
        public string Description => description;
        public IReadOnlyList<SpiritLevelDefinition> Levels => levels;
        public int MaxLevel => Mathf.Max(1, levels.Count);

        public SpiritWeaponDefinition(string name, string weaponDescription, params string[] levelEffects)
        {
            weaponName = name;
            description = weaponDescription;
            levels = SpiritLevelDefinition.CreateLevels(levelEffects);
        }

        public SpiritWeaponDefinition() { }
    }

    [Serializable]
    public class SpiritAbilityDefinition
    {
        [SerializeField] private string abilityName;
        [TextArea(2, 4)] [SerializeField] private string description;
        [SerializeField] private List<SpiritLevelDefinition> levels = new List<SpiritLevelDefinition>();

        public string AbilityName => abilityName;
        public string Description => description;
        public IReadOnlyList<SpiritLevelDefinition> Levels => levels;
        public int MaxLevel => Mathf.Max(1, levels.Count);

        public SpiritAbilityDefinition() { }

        public SpiritAbilityDefinition(string name, string abilityDescription, params string[] levelEffects)
        {
            abilityName = name;
            description = abilityDescription;
            levels = SpiritLevelDefinition.CreateLevels(levelEffects);
        }
    }

    [Serializable]
    public class SpiritLevelDefinition
    {
        [Min(1)] [SerializeField] private int level = 1;
        [TextArea(1, 3)] [SerializeField] private string effect;

        public int Level => level;
        public string Effect => effect;

        public SpiritLevelDefinition(int levelNumber, string levelEffect)
        {
            level = levelNumber;
            effect = levelEffect;
        }

        public static List<SpiritLevelDefinition> CreateLevels(IReadOnlyList<string> effects)
        {
            List<SpiritLevelDefinition> result = new List<SpiritLevelDefinition>();
            for (int i = 0; i < effects.Count; i++)
            {
                result.Add(new SpiritLevelDefinition(i + 1, effects[i]));
            }

            return result;
        }
    }
}

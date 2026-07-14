using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Spirits
{
    [Serializable]
    public class SpiritProgression
    {
        [SerializeField, Min(1)] private int weaponLevel = 1;
        [SerializeField] private List<int> abilityLevels = new List<int>();

        public int WeaponLevel => weaponLevel;

        public void Initialize(SpiritDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            weaponLevel = Mathf.Clamp(weaponLevel, 1, definition.Weapon.MaxLevel);
            while (abilityLevels.Count < definition.Abilities.Count)
            {
                abilityLevels.Add(1);
            }

            if (abilityLevels.Count > definition.Abilities.Count)
            {
                abilityLevels.RemoveRange(definition.Abilities.Count, abilityLevels.Count - definition.Abilities.Count);
            }

            for (int i = 0; i < abilityLevels.Count; i++)
            {
                abilityLevels[i] = Mathf.Clamp(abilityLevels[i], 1, definition.Abilities[i].MaxLevel);
            }
        }

        public int GetAbilityLevel(int abilityIndex)
        {
            return abilityIndex >= 0 && abilityIndex < abilityLevels.Count ? abilityLevels[abilityIndex] : 0;
        }

        public bool TryLevelWeapon(SpiritDefinition definition)
        {
            if (definition == null || weaponLevel >= definition.Weapon.MaxLevel)
            {
                return false;
            }

            weaponLevel++;
            return true;
        }

        public bool TryLevelAbility(SpiritDefinition definition, int abilityIndex)
        {
            if (definition == null || abilityIndex < 0 || abilityIndex >= definition.Abilities.Count)
            {
                return false;
            }

            Initialize(definition);
            if (abilityLevels[abilityIndex] >= definition.Abilities[abilityIndex].MaxLevel)
            {
                return false;
            }

            abilityLevels[abilityIndex]++;
            return true;
        }
    }
}

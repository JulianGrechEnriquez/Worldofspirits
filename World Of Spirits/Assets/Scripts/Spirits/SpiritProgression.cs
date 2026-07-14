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

            int weaponMaxLevel = definition.RuntimeWeapon != null
                ? definition.RuntimeWeapon.MaxLevel : definition.Weapon.MaxLevel;
            weaponLevel = Mathf.Clamp(weaponLevel, 1, weaponMaxLevel);
            int abilityCount = definition.RuntimeAbilities.Count > 0
                ? definition.RuntimeAbilities.Count
                : definition.Abilities.Count;
            while (abilityLevels.Count < abilityCount)
            {
                abilityLevels.Add(1);
            }

            if (abilityLevels.Count > abilityCount)
            {
                abilityLevels.RemoveRange(abilityCount, abilityLevels.Count - abilityCount);
            }

            for (int i = 0; i < abilityLevels.Count; i++)
            {
                int maxLevel = definition.RuntimeAbilities.Count > 0
                    ? definition.RuntimeAbilities[i].MaxLevel
                    : definition.Abilities[i].MaxLevel;
                abilityLevels[i] = Mathf.Clamp(abilityLevels[i], 1, maxLevel);
            }
        }

        public int GetAbilityLevel(int abilityIndex)
        {
            return abilityIndex >= 0 && abilityIndex < abilityLevels.Count ? abilityLevels[abilityIndex] : 0;
        }

        public bool TryLevelWeapon(SpiritDefinition definition)
        {
            int maxLevel = definition != null && definition.RuntimeWeapon != null
                ? definition.RuntimeWeapon.MaxLevel
                : definition != null ? definition.Weapon.MaxLevel : 0;
            if (definition == null || weaponLevel >= maxLevel)
            {
                return false;
            }

            weaponLevel++;
            return true;
        }

        public bool TryLevelAbility(SpiritDefinition definition, int abilityIndex)
        {
            int abilityCount = definition != null && definition.RuntimeAbilities.Count > 0
                ? definition.RuntimeAbilities.Count
                : definition != null ? definition.Abilities.Count : 0;
            if (definition == null || abilityIndex < 0 || abilityIndex >= abilityCount)
            {
                return false;
            }

            Initialize(definition);
            int maxLevel = definition.RuntimeAbilities.Count > 0
                ? definition.RuntimeAbilities[abilityIndex].MaxLevel
                : definition.Abilities[abilityIndex].MaxLevel;
            if (abilityLevels[abilityIndex] >= maxLevel)
            {
                return false;
            }

            abilityLevels[abilityIndex]++;
            return true;
        }
    }
}

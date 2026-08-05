using System.Text;
using UnityEngine;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.Progression
{
    public static class SpiritUnlockProgress
    {
        private const string KeyPrefix = "WorldOfSpirits.UnlockedSpirit.";

        public static bool IsUnlocked(SpiritDefinition definition) =>
            definition != null && PlayerPrefs.GetInt(KeyPrefix + GetId(definition), 0) == 1;

        public static bool Unlock(SpiritDefinition definition)
        {
            if (definition == null) return false;
            string key = KeyPrefix + GetId(definition);
            if (PlayerPrefs.GetInt(key, 0) == 1) return false;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static void Forget(SpiritDefinition definition)
        {
            if (definition != null) PlayerPrefs.DeleteKey(KeyPrefix + GetId(definition));
        }

        private static string GetId(SpiritDefinition definition)
        {
            string source = string.IsNullOrWhiteSpace(definition.SpiritName)
                ? definition.name : definition.SpiritName;
            StringBuilder id = new StringBuilder(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                char character = char.ToLowerInvariant(source[i]);
                if (char.IsLetterOrDigit(character)) id.Append(character);
                else if (id.Length > 0 && id[id.Length - 1] != '_') id.Append('_');
            }
            return id.ToString().Trim('_');
        }
    }
}

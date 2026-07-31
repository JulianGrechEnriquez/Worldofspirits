using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Progression.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade Catalog", menuName = "World of Spirits/Upgrades/Catalog")]
    public sealed class UpgradeCatalog : ScriptableObject
    {
        [SerializeField] private List<UpgradeCardDefinition> cards = new List<UpgradeCardDefinition>();
        public IReadOnlyList<UpgradeCardDefinition> Cards => cards;

        private void OnValidate()
        {
            HashSet<string> ids = new HashSet<string>();
            for (int i = 0; i < cards.Count; i++)
            {
                UpgradeCardDefinition card = cards[i];
                if (card != null && !ids.Add(card.Id))
                    Debug.LogWarning($"Duplicate upgrade ID '{card.Id}' in {name}.", this);
            }
        }
    }
}

using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.Player
{
    [CreateAssetMenu(menuName = "World of Spirits/Warrior Armament")]
    public sealed class WarriorArmamentDefinition : ScriptableObject
    {
        public DamageElement element;
        public string pairedWeaponName;
        public string armourName;
        [TextArea] public string description;
        public Color colour = Color.white;
        public Sprite equipmentSprite;
        [Min(0.1f)] public float equipmentHeight = 1f;
        public WarriorPairedThrustWeapon thrustWeaponPrefab;
        [Min(0f)] public float damage = 14f;
        [Min(0.1f)] public float cooldown = 1.2f;
        [Min(0.1f)] public float range = 3f;
        [Range(0f, 0.5f)] public float damageReduction = 0.1f;
    }
}

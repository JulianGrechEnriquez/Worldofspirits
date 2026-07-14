using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public class SpiritMember : MonoBehaviour
    {
        [Header("Spirit Data")]
        [SerializeField] private SpiritDefinition definition;
        [SerializeField] private SpiritProgression progression = new SpiritProgression();

        private Renderer[] renderers;
        private AutoProjectileWeapon[] primaryWeapons;
        private SpiritAbility[] abilities;

        private void Awake()
        {
            if (definition == null)
            {
                definition = BuiltInSpiritCatalog.Find(name);
            }

            progression.Initialize(definition);
            CacheComponents();
        }

        public SpiritDefinition Definition => definition;
        public SpiritProgression Progression => progression;

        public bool TryLevelWeapon() => progression.TryLevelWeapon(definition);
        public bool TryLevelAbility(int abilityIndex) => progression.TryLevelAbility(definition, abilityIndex);

        public void ApplyState(Transform player, Transform playerProjectileSpawner, bool isPrimary, bool playerIsMoving)
        {
            if (renderers == null)
            {
                CacheComponents();
            }

            // The primary spirit is channelled into its weapon while stationary.
            bool spiritVisible = !isPrimary || playerIsMoving;
            foreach (Renderer spiritRenderer in renderers)
            {
                spiritRenderer.enabled = spiritVisible;
            }

            // Every spirit attacks continuously. While stationary, the primary
            // spirit is channelled through the player's projectile spawn point.
            // While moving, it fires visibly from its own spirit spawn point.
            foreach (AutoProjectileWeapon weapon in primaryWeapons)
            {
                weapon.enabled = true;
                bool usePlayerSpawner = isPrimary && !playerIsMoving;
                weapon.SetFirePointOverride(usePlayerSpawner ? playerProjectileSpawner : null);
            }

            SpiritAbilityContext context = new SpiritAbilityContext(player, playerIsMoving, isPrimary);
            foreach (SpiritAbility ability in abilities)
            {
                ability.TickAbility(context);
            }
        }

        private void CacheComponents()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            primaryWeapons = GetComponentsInChildren<AutoProjectileWeapon>(true);
            abilities = GetComponentsInChildren<SpiritAbility>(true);
        }
    }
}

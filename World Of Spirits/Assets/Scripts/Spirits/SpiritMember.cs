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
        private SpiritWeaponAttack[] weapons;
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

            // A spirit's weapon is active only while that spirit occupies the
            // main slot and the player is standing still.
            bool weaponIsActive = isPrimary && !playerIsMoving;
            foreach (SpiritWeaponAttack weapon in weapons)
            {
                weapon.enabled = weaponIsActive;

                if (weapon is AutoProjectileWeapon projectileWeapon)
                {
                    projectileWeapon.SetFirePointOverride(
                        weaponIsActive ? playerProjectileSpawner : null);
                }
                else if (weapon is DataDrivenWeapon dataDrivenWeapon)
                {
                    dataDrivenWeapon.SetFirePointOverride(
                        weaponIsActive ? playerProjectileSpawner : null);
                }
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
            weapons = GetComponentsInChildren<SpiritWeaponAttack>(true);
            abilities = GetComponentsInChildren<SpiritAbility>(true);
        }
    }
}

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
        private Animator[] animators;

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

        public void ApplyState(Transform player, Transform playerProjectileSpawner, bool isPrimary, bool playerIsMoving,
            bool combatLocked = false)
        {
            if (renderers == null)
            {
                CacheComponents();
            }

            // The primary spirit is channelled into its weapon while stationary.
            bool spiritVisible = combatLocked || !isPrimary || playerIsMoving;
            foreach (Renderer spiritRenderer in renderers)
            {
                spiritRenderer.enabled = spiritVisible;
            }

            // A spirit's weapon is active only while that spirit occupies the
            // main slot and the player is standing still.
            bool weaponIsActive = !combatLocked && isPrimary && !playerIsMoving;
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

            if (combatLocked)
            {
                return;
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
            animators = GetComponentsInChildren<Animator>(true);
        }

        public float PlayTransitionAnimation(bool remerging)
        {
            if (animators == null)
            {
                CacheComponents();
            }

            float longestDuration = 0f;
            foreach (Animator animator in animators)
            {
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                string stateName = FindTransitionState(animator, remerging, out float duration);
                if (string.IsNullOrEmpty(stateName))
                {
                    continue;
                }

                animator.Play(stateName, 0, 0f);
                longestDuration = Mathf.Max(longestDuration, duration);
            }

            return longestDuration;
        }

        public void PlayIdleAnimation()
        {
            if (animators == null)
            {
                CacheComponents();
            }

            foreach (Animator animator in animators)
            {
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                {
                    string normalizedName = clip.name.ToLowerInvariant();
                    bool isIdle = normalizedName.Contains("idle") || normalizedName.Contains("ideal");
                    if (isIdle && animator.HasState(0, Animator.StringToHash(clip.name)))
                    {
                        animator.Play(clip.name, 0, 0f);
                        break;
                    }
                }
            }
        }

        private static string FindTransitionState(Animator animator, bool remerging, out float duration)
        {
            duration = 0f;
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                string normalizedName = clip.name.ToLowerInvariant();
                bool isMatch = remerging
                    ? normalizedName.Contains("remerg") || normalizedName.Contains("re-merg")
                    : normalizedName.Contains("change") && !normalizedName.Contains("remerg");

                if (!isMatch || !animator.HasState(0, Animator.StringToHash(clip.name)))
                {
                    continue;
                }

                duration = clip.length;
                return clip.name;
            }

            return null;
        }
    }
}

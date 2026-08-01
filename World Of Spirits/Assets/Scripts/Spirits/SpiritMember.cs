using System;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    public class SpiritMember : MonoBehaviour
    {
        [Header("Spirit Data")]
        [SerializeField] private SpiritDefinition definition;
        [SerializeField] private SpiritProgression progression = new SpiritProgression();

        [Header("Formation Animations (optional)")]
        [SerializeField] private string changeAnimationState;
        [SerializeField] private string remergeAnimationState;
        [SerializeField] private string idleAnimationState;

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
        public event Action<SpiritMember, int> WeaponLeveled;
        public event Action<SpiritMember, int, int> AbilityLeveled;

        public bool TryLevelWeapon()
        {
            if (!progression.TryLevelWeapon(definition)) return false;
            WeaponLeveled?.Invoke(this, progression.WeaponLevel);
            return true;
        }

        public bool TryLevelAbility(int abilityIndex)
        {
            if (!progression.TryLevelAbility(definition, abilityIndex)) return false;
            AbilityLeveled?.Invoke(this, abilityIndex, progression.GetAbilityLevel(abilityIndex));
            return true;
        }

        public void ApplyState(
            Transform player,
            Transform playerProjectileSpawner,
            System.Collections.Generic.IReadOnlyList<Transform> meleeWeaponSlots,
            bool isPrimary,
            bool playerIsMoving,
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
            foreach (SpiritWeaponAttack weapon in weapons)
            {
                bool spiritOwnsWeapon = !combatLocked && isPrimary;
                bool weaponCanAttack = spiritOwnsWeapon && !playerIsMoving;
                bool isThrustMelee = weapon is ThrustMeleeWeaponBase;
                weapon.enabled = isThrustMelee
                    ? spiritOwnsWeapon
                    : weaponCanAttack;

                if (weapon is AutoProjectileWeapon projectileWeapon)
                {
                    projectileWeapon.SetFirePointOverride(
                        weaponCanAttack ? playerProjectileSpawner : null);
                }
                else if (weapon is DataDrivenWeapon dataDrivenWeapon)
                {
                    dataDrivenWeapon.SetFirePointOverride(
                        weaponCanAttack ? playerProjectileSpawner : null);
                    dataDrivenWeapon.SetVisualPointOverride(
                        weaponCanAttack && meleeWeaponSlots != null && meleeWeaponSlots.Count > 0
                            ? meleeWeaponSlots[0]
                            : null);
                }
                else if (weapon is ThrustMeleeWeaponBase thrustMeleeWeapon)
                {
                    thrustMeleeWeapon.SetOriginOverride(
                        spiritOwnsWeapon ? player : null);
                    thrustMeleeWeapon.SetWeaponSlots(
                        spiritOwnsWeapon ? meleeWeaponSlots : null);
                    thrustMeleeWeapon.SetCombatActive(weaponCanAttack);
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

                string configuredState = remerging ? remergeAnimationState : changeAnimationState;
                string stateName;
                float duration;
                if (!string.IsNullOrWhiteSpace(configuredState))
                {
                    stateName = FindPlayableState(animator, configuredState);
                    duration = FindClipDuration(animator, configuredState);
                }
                else
                {
                    stateName = FindTransitionState(animator, remerging, out duration);
                }

                if (string.IsNullOrEmpty(stateName))
                {
                    if (!string.IsNullOrWhiteSpace(configuredState))
                    {
                        Debug.LogWarning($"[{name}] Animator state '{configuredState}' was not found.", this);
                    }
                    continue;
                }

                animator.enabled = true;
                animator.Play(stateName, 0, 0f);
                animator.Update(0f);
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

                if (!string.IsNullOrWhiteSpace(idleAnimationState))
                {
                    string configuredIdle = FindPlayableState(animator, idleAnimationState);
                    if (!string.IsNullOrEmpty(configuredIdle))
                    {
                        animator.enabled = true;
                        animator.Play(configuredIdle, 0, 0f);
                        animator.Update(0f);
                    }
                    else
                    {
                        Debug.LogWarning($"[{name}] Animator state '{idleAnimationState}' was not found.", this);
                    }

                    continue;
                }

                foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                {
                    string normalizedName = clip.name.ToLowerInvariant();
                    bool isIdle = normalizedName.Contains("idle") || normalizedName.Contains("ideal");
                    if (!isIdle)
                    {
                        continue;
                    }

                    string stateName = FindPlayableState(animator,
                        clip.name,
                        clip.name.Replace("_Idle", " ideal"),
                        clip.name.Replace("_Idle", " Idle"),
                        clip.name.Replace("Idle", "ideal"));
                    if (!string.IsNullOrEmpty(stateName))
                    {
                        animator.Play(stateName, 0, 0f);
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

                if (!isMatch)
                {
                    continue;
                }

                string stateName = FindPlayableState(animator, clip.name);
                if (string.IsNullOrEmpty(stateName))
                {
                    continue;
                }

                duration = clip.length;
                return stateName;
            }

            return null;
        }

        private static string FindPlayableState(Animator animator, params string[] candidateNames)
        {
            foreach (string candidateName in candidateNames)
            {
                if (string.IsNullOrWhiteSpace(candidateName))
                {
                    continue;
                }

                string fullStateName = $"Base Layer.{candidateName}";
                if (animator.HasState(0, Animator.StringToHash(fullStateName)))
                {
                    return fullStateName;
                }

                if (animator.HasState(0, Animator.StringToHash(candidateName)))
                {
                    return candidateName;
                }
            }

            return null;
        }

        private static float FindClipDuration(Animator animator, string stateName)
        {
            string normalizedState = stateName.Replace(" ", string.Empty)
                .Replace("_", string.Empty).ToLowerInvariant();
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                string normalizedClip = clip.name.Replace(" ", string.Empty)
                    .Replace("_", string.Empty).ToLowerInvariant();
                if (normalizedClip == normalizedState)
                {
                    return clip.length;
                }
            }

            return 0f;
        }
    }
}

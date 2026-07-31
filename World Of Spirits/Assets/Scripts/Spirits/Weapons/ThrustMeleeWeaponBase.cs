using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.Spirits
{
    /// <summary>
    /// Reusable foundation for melee weapons that rest beside the player,
    /// thrust toward a target, deal spatially queried damage, and return.
    /// Subclasses only define their execution type and optional hit hooks.
    /// </summary>
    public abstract class ThrustMeleeWeaponBase : SpiritWeaponAttack
    {
        private enum PunchPhase
        {
            Idle,
            Extending,
            Returning
        }

        [Header("Gauntlet Objects")]
        [SerializeField] private Transform leftGauntlet;
        [SerializeField] private Transform rightGauntlet;

        [Header("Weapon Data")]
        [SerializeField] private WeaponDefinition definition;

        [Header("Resting Formation")]
        [SerializeField, Min(0.1f)] private float sideOffset = 0.65f;
        [SerializeField, Min(0f)] private float forwardRestOffset = 0.15f;

        [Header("Punch")]
        [SerializeField, Min(0.1f)] private float punchDistance = 2.25f;
        [SerializeField, Min(0.05f)] private float extendDuration = 0.12f;
        [SerializeField, Min(0.05f)] private float returnDuration = 0.18f;
        [SerializeField, Min(0.05f)] private float attackCooldown = 0.4f;
        [SerializeField, Min(0.1f)] private float targetingRange = 7f;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float baseDamage = 18f;
        [SerializeField, Min(0f)] private float damageIncreasePerLevel = 0.25f;
        [SerializeField, Min(0.05f)] private float hitRadius = 0.55f;
        [SerializeField, Min(1)] private int maximumTargetsPerPunch = 4;
        [SerializeField, Min(0f)] private float freezeDuration = 0.2f;

        [Header("Visuals")]
        [SerializeField] private float spriteRotationOffset = -90f;

        private readonly List<IDamageable> targetBuffer = new List<IDamageable>(16);
        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private SpiritMember spiritOwner;
        private Renderer[] gauntletRenderers;
        private Transform fallbackLeftGauntlet;
        private Transform fallbackRightGauntlet;
        private Transform primaryWeaponSlot;
        private Transform secondaryWeaponSlot;
        private GameObject pooledVisualPrefab;
        private Transform originOverride;
        private Transform activeGauntlet;
        private Vector2 punchDirection = Vector2.right;
        private Vector3 punchStart;
        private Vector3 punchEnd;
        private float phaseTime;
        private int targetsHit;
        private bool useLeftNext = true;
        private bool combatActive;
        private PunchPhase phase;
        private UpgradeRuntimeStats upgradeStats;

        protected abstract WeaponExecutionType ExpectedExecutionType { get; }

        private WeaponLevelData ActiveLevel =>
            definition != null && spiritOwner != null
                ? definition.GetLevel(spiritOwner.Progression.WeaponLevel)
                : null;

        protected override float AttackCooldown =>
            (ActiveLevel != null ? ActiveLevel.attackCooldown : attackCooldown) /
            (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.AttackSpeed) : 1f);

        protected virtual void Awake()
        {
            spiritOwner = GetComponentInParent<SpiritMember>();
            upgradeStats = GetComponentInParent<UpgradeRuntimeStats>();
            fallbackLeftGauntlet = leftGauntlet;
            fallbackRightGauntlet = rightGauntlet;
            CacheRenderers();

            WeaponLevelData firstLevel =
                definition != null ? definition.GetLevel(1) : null;
            if (firstLevel != null && firstLevel.weaponPrefab != null)
            {
                pooledVisualPrefab = firstLevel.weaponPrefab;
                SceneObjectPool.Preload(
                    pooledVisualPrefab,
                    2,
                    PoolCategory.Effects);
            }
        }

        protected override void Update()
        {
            UpdateGauntletAnimation();
            if (phase == PunchPhase.Idle)
            {
                base.Update();
            }
        }

        protected override bool CanAttack()
        {
            return phase == PunchPhase.Idle &&
                   combatActive &&
                   leftGauntlet != null &&
                   rightGauntlet != null &&
                   (definition == null ||
                    definition.ExecutionType == ExpectedExecutionType);
        }

        protected override void PerformAttack()
        {
            Transform center = ActiveOrigin;
            IDamageable target = CombatTargeting.FindClosest(
                center.position,
                ActiveLevel != null ? ActiveLevel.targetingRange : targetingRange,
                Faction.Player);
            if (target == null)
            {
                return;
            }

            Vector2 toTarget = target.Transform.position - center.position;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            punchDirection = toTarget.normalized;
            activeGauntlet = useLeftNext ? leftGauntlet : rightGauntlet;
            useLeftNext = !useLeftNext;
            punchStart = GetRestPosition(activeGauntlet == leftGauntlet);
            float activePunchDistance =
                ActiveLevel != null ? ActiveLevel.punchDistance : punchDistance;
            punchEnd = punchStart + (Vector3)(punchDirection * activePunchDistance);
            phaseTime = 0f;
            targetsHit = 0;
            hitTargets.Clear();
            phase = PunchPhase.Extending;
            FaceDirection(activeGauntlet, punchDirection);
        }

        public void SetOriginOverride(Transform origin)
        {
            bool originChanged = originOverride != origin;
            originOverride = origin;
            if (originChanged && phase == PunchPhase.Idle)
            {
                PositionAtRest();
            }
        }

        public void SetWeaponSlots(IReadOnlyList<Transform> slots)
        {
            Transform newPrimarySlot =
                slots != null && slots.Count > 0 ? slots[0] : null;
            Transform newSecondarySlot =
                slots != null && slots.Count > 1 ? slots[1] : null;
            bool slotsChanged =
                primaryWeaponSlot != newPrimarySlot ||
                secondaryWeaponSlot != newSecondarySlot;
            primaryWeaponSlot = newPrimarySlot;
            secondaryWeaponSlot = newSecondarySlot;

            if (isActiveAndEnabled && slots != null)
            {
                EnsurePooledVisuals();
                if (slotsChanged &&
                    leftGauntlet != null &&
                    leftGauntlet != fallbackLeftGauntlet &&
                    primaryWeaponSlot != null)
                {
                    leftGauntlet.SetParent(primaryWeaponSlot, false);
                }
                if (slotsChanged &&
                    rightGauntlet != null &&
                    rightGauntlet != fallbackRightGauntlet &&
                    secondaryWeaponSlot != null)
                {
                    rightGauntlet.SetParent(secondaryWeaponSlot, false);
                }
                if (slotsChanged)
                {
                    PositionAtRest();
                }
            }
        }

        public void SetVisualsActive(bool active)
        {
            if (gauntletRenderers == null)
            {
                return;
            }

            for (int i = 0; i < gauntletRenderers.Length; i++)
            {
                if (gauntletRenderers[i] != null)
                {
                    gauntletRenderers[i].enabled = active;
                }
            }
        }

        public void SetCombatActive(bool active)
        {
            if (combatActive == active)
            {
                return;
            }

            combatActive = active;
            SetVisualsActive(active);
            if (!active)
            {
                phase = PunchPhase.Idle;
                activeGauntlet = null;
                hitTargets.Clear();
                PositionAtRest();
            }
        }

        private Transform ActiveOrigin =>
            originOverride != null ? originOverride : transform;

        private void UpdateGauntletAnimation()
        {
            if (leftGauntlet == null || rightGauntlet == null)
            {
                return;
            }

            if (phase == PunchPhase.Idle)
            {
                PositionAtRest();
                return;
            }

            phaseTime += Time.deltaTime;
            if (phase == PunchPhase.Extending)
            {
                float duration = ActiveLevel != null
                    ? ActiveLevel.extendDuration
                    : extendDuration;
                float progress = Mathf.Clamp01(phaseTime / duration);
                // Ease out gives the punch a quick, weighty start.
                float eased = 1f - (1f - progress) * (1f - progress);
                activeGauntlet.position = Vector3.LerpUnclamped(punchStart, punchEnd, eased);
                DamageAt(activeGauntlet.position);
                if (progress >= 1f)
                {
                    phase = PunchPhase.Returning;
                    phaseTime = 0f;
                }
            }
            else
            {
                // The player may move during the punch, so the return target is
                // recalculated continuously.
                Vector3 currentRestPosition =
                    GetRestPosition(activeGauntlet == leftGauntlet);
                float duration = ActiveLevel != null
                    ? ActiveLevel.returnDuration
                    : returnDuration;
                float progress = Mathf.Clamp01(phaseTime / duration);
                float eased = progress * progress * (3f - 2f * progress);
                activeGauntlet.position =
                    Vector3.LerpUnclamped(punchEnd, currentRestPosition, eased);
                if (progress >= 1f)
                {
                    phase = PunchPhase.Idle;
                    activeGauntlet = null;
                    PositionAtRest();
                }
            }

            // The inactive fist remains anchored beside the moving player.
            Transform restingGauntlet =
                activeGauntlet == leftGauntlet ? rightGauntlet : leftGauntlet;
            if (restingGauntlet != null)
            {
                restingGauntlet.position =
                    GetRestPosition(restingGauntlet == leftGauntlet);
            }
        }

        private void DamageAt(Vector2 position)
        {
            int targetLimit = ActiveLevel != null
                ? ActiveLevel.maximumTargets
                : maximumTargetsPerPunch;
            if (targetsHit >= targetLimit)
            {
                return;
            }

            float activeHitRadius =
                ActiveLevel != null ? ActiveLevel.hitRadius : hitRadius;
            CombatTargeting.FindAllNonAlloc(
                position,
                activeHitRadius,
                Faction.Player,
                targetBuffer);

            for (int i = 0;
                 i < targetBuffer.Count && targetsHit < targetLimit;
                 i++)
            {
                IDamageable target = targetBuffer[i];
                int targetId = target.Transform.gameObject.GetInstanceID();
                if (!hitTargets.Add(targetId))
                {
                    continue;
                }

                int level = spiritOwner != null
                    ? spiritOwner.Progression.WeaponLevel
                    : 1;
                float damage = ActiveLevel != null
                    ? ActiveLevel.damage
                    : baseDamage *
                      (1f + damageIncreasePerLevel * Mathf.Max(0, level - 1));
                if (upgradeStats != null) damage = upgradeStats.ScaleWeaponDamage(damage);
                target.TakeDamage(damage);
                float activeStatusDuration = ActiveLevel != null
                    ? ActiveLevel.statusDuration
                    : freezeDuration;
                CombatStatus activeStatus = ActiveLevel != null
                    ? ActiveLevel.status
                    : CombatStatus.Freeze;
                if (activeStatusDuration > 0f &&
                    target is IStatusEffectReceiver statusReceiver)
                {
                    statusReceiver.ApplyStatus(
                        activeStatus,
                        activeStatusDuration,
                        1f);
                }
                targetsHit++;
                OnTargetHit(target, level);
            }
        }

        /// <summary>
        /// Optional specialization hook for effects that are not represented
        /// by WeaponLevelData. The shared damage and status logic has already
        /// run when this method is called.
        /// </summary>
        protected virtual void OnTargetHit(IDamageable target, int weaponLevel)
        {
        }

        private void PositionAtRest()
        {
            if (leftGauntlet == null || rightGauntlet == null)
            {
                return;
            }
            leftGauntlet.position = GetRestPosition(true);
            rightGauntlet.position = GetRestPosition(false);
        }

        private Vector3 GetRestPosition(bool left)
        {
            Transform slot = left ? primaryWeaponSlot : secondaryWeaponSlot;
            if (slot != null)
            {
                return slot.position;
            }

            Vector2 facing = punchDirection.sqrMagnitude > 0.001f
                ? punchDirection
                : Vector2.right;
            Vector2 side = new Vector2(-facing.y, facing.x);
            float sideSign = left ? 1f : -1f;
            return ActiveOrigin.position +
                (Vector3)(facing * forwardRestOffset + side * sideOffset * sideSign);
        }

        private void FaceDirection(Transform gauntlet, Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gauntlet.rotation =
                Quaternion.Euler(0f, 0f, angle + spriteRotationOffset);
        }

        protected virtual void OnEnable()
        {
            phase = PunchPhase.Idle;
            activeGauntlet = null;
            hitTargets.Clear();
            EnsurePooledVisuals();
            PositionAtRest();
            SetVisualsActive(combatActive);
        }

        protected virtual void OnDisable()
        {
            phase = PunchPhase.Idle;
            activeGauntlet = null;
            hitTargets.Clear();
            combatActive = false;
            ReleasePooledVisuals();
        }

        private void EnsurePooledVisuals()
        {
            WeaponLevelData level = ActiveLevel ??
                (definition != null ? definition.GetLevel(1) : null);
            GameObject visualPrefab =
                level != null ? level.weaponPrefab : null;
            if (visualPrefab == null)
            {
                return;
            }

            pooledVisualPrefab = visualPrefab;
            if (leftGauntlet == null || leftGauntlet == fallbackLeftGauntlet)
            {
                GameObject left = SceneObjectPool.Spawn(
                    visualPrefab,
                    primaryWeaponSlot != null
                        ? primaryWeaponSlot.position
                        : ActiveOrigin.position,
                    Quaternion.identity,
                    PoolCategory.Effects,
                    primaryWeaponSlot);
                leftGauntlet = left.transform;
            }

            if (rightGauntlet == null || rightGauntlet == fallbackRightGauntlet)
            {
                GameObject right = SceneObjectPool.Spawn(
                    visualPrefab,
                    secondaryWeaponSlot != null
                        ? secondaryWeaponSlot.position
                        : ActiveOrigin.position,
                    Quaternion.identity,
                    PoolCategory.Effects,
                    secondaryWeaponSlot);
                rightGauntlet = right.transform;
            }

            CacheRenderers();
        }

        private void ReleasePooledVisuals()
        {
            if (pooledVisualPrefab == null)
            {
                SetVisualsActive(false);
                return;
            }

            if (leftGauntlet != null && leftGauntlet != fallbackLeftGauntlet)
            {
                SceneObjectPool.ReleaseOrDestroy(leftGauntlet.gameObject);
            }
            if (rightGauntlet != null && rightGauntlet != fallbackRightGauntlet)
            {
                SceneObjectPool.ReleaseOrDestroy(rightGauntlet.gameObject);
            }

            leftGauntlet = fallbackLeftGauntlet;
            rightGauntlet = fallbackRightGauntlet;
            CacheRenderers();
            SetVisualsActive(false);
        }

        private void CacheRenderers()
        {
            gauntletRenderers = new[]
            {
                leftGauntlet != null ? leftGauntlet.GetComponent<Renderer>() : null,
                rightGauntlet != null ? rightGauntlet.GetComponent<Renderer>() : null
            };
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            sideOffset = Mathf.Max(0.1f, sideOffset);
            punchDistance = Mathf.Max(0.1f, punchDistance);
            extendDuration = Mathf.Max(0.05f, extendDuration);
            returnDuration = Mathf.Max(0.05f, returnDuration);
            attackCooldown = Mathf.Max(0.05f, attackCooldown);
            hitRadius = Mathf.Max(0.05f, hitRadius);
            maximumTargetsPerPunch = Mathf.Max(1, maximumTargetsPerPunch);
        }
#endif
    }
}

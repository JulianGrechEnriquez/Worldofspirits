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

        [Header("Hitbox Debug")]
        [SerializeField] private bool drawHitboxes = true;
        [SerializeField] private Color targetingGizmoColor = new Color(0.15f, 0.8f, 1f, 0.45f);
        [SerializeField] private Color punchPathGizmoColor = new Color(1f, 0.8f, 0.1f, 0.8f);
        [SerializeField] private Color activeHitboxGizmoColor = new Color(1f, 0.15f, 0.1f, 0.9f);

        private readonly List<IDamageable> targetBuffer = new List<IDamageable>(16);
        private readonly List<Collider2D> colliderBuffer = new List<Collider2D>(16);
        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private SpiritMember spiritOwner;
        private Renderer[] gauntletRenderers;
        private Collider2D leftGauntletHitbox;
        private Collider2D rightGauntletHitbox;
        private Collider2D leftEchoGauntletHitbox;
        private Collider2D rightEchoGauntletHitbox;
        private Transform leftEchoGauntlet;
        private Transform rightEchoGauntlet;
        private Transform fallbackLeftGauntlet;
        private Transform fallbackRightGauntlet;
        private Vector3 fallbackLeftScale;
        private Vector3 fallbackRightScale;
        private Transform primaryWeaponSlot;
        private Transform secondaryWeaponSlot;
        private Transform tertiaryWeaponSlot;
        private Transform quaternaryWeaponSlot;
        private GameObject pooledVisualPrefab;
        private Transform originOverride;
        private Transform activeGauntlet;
        private Transform activeEchoGauntlet;
        private Transform punchTarget;
        private Vector2 punchDirection = Vector2.right;
        private Vector3 punchStart;
        private Vector3 punchEnd;
        private Vector3 echoPunchStart;
        private Vector3 echoPunchEnd;
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
            fallbackLeftScale = leftGauntlet != null ? leftGauntlet.localScale : Vector3.one;
            fallbackRightScale = rightGauntlet != null ? rightGauntlet.localScale : Vector3.one;
            CacheRenderers();

            WeaponLevelData firstLevel =
                definition != null ? definition.GetLevel(1) : null;
            if (firstLevel != null && firstLevel.weaponPrefab != null)
            {
                pooledVisualPrefab = firstLevel.weaponPrefab;
                SceneObjectPool.Preload(
                    pooledVisualPrefab,
                    4,
                    PoolCategory.Effects);
            }
        }

        protected override void Update()
        {
            SyncEchoGauntlets();
            ApplyWeaponSize();
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

            activeGauntlet = useLeftNext ? leftGauntlet : rightGauntlet;
            useLeftNext = !useLeftNext;
            activeEchoGauntlet = activeGauntlet == leftGauntlet
                ? leftEchoGauntlet
                : rightEchoGauntlet;
            punchStart = GetRestPosition(activeGauntlet);
            if (activeEchoGauntlet != null)
                echoPunchStart = GetRestPosition(activeEchoGauntlet);
            punchTarget = target.Transform;
            if (!UpdatePunchAim())
            {
                activeGauntlet = null;
                activeEchoGauntlet = null;
                punchTarget = null;
                return;
            }

            phaseTime = 0f;
            targetsHit = 0;
            hitTargets.Clear();
            phase = PunchPhase.Extending;
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
            Transform newTertiarySlot =
                slots != null && slots.Count > 2 ? slots[2] : null;
            Transform newQuaternarySlot =
                slots != null && slots.Count > 3 ? slots[3] : null;
            bool slotsChanged =
                primaryWeaponSlot != newPrimarySlot ||
                secondaryWeaponSlot != newSecondarySlot ||
                tertiaryWeaponSlot != newTertiarySlot ||
                quaternaryWeaponSlot != newQuaternarySlot;
            primaryWeaponSlot = newPrimarySlot;
            secondaryWeaponSlot = newSecondarySlot;
            tertiaryWeaponSlot = newTertiarySlot;
            quaternaryWeaponSlot = newQuaternarySlot;

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
                if (slotsChanged && leftEchoGauntlet != null &&
                    tertiaryWeaponSlot != null)
                {
                    leftEchoGauntlet.SetParent(tertiaryWeaponSlot, false);
                }
                if (slotsChanged && rightEchoGauntlet != null &&
                    quaternaryWeaponSlot != null)
                {
                    rightEchoGauntlet.SetParent(quaternaryWeaponSlot, false);
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

            if (leftGauntletHitbox != null) leftGauntletHitbox.enabled = active;
            if (rightGauntletHitbox != null) rightGauntletHitbox.enabled = active;
            if (leftEchoGauntletHitbox != null) leftEchoGauntletHitbox.enabled = active;
            if (rightEchoGauntletHitbox != null) rightEchoGauntletHitbox.enabled = active;
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
                activeEchoGauntlet = null;
                punchTarget = null;
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
                // Track the target during extension so a fast enemy cannot
                // sidestep the alternating second punch.
                UpdatePunchAim();
                float duration = ActiveLevel != null
                    ? ActiveLevel.extendDuration
                    : extendDuration;
                float progress = Mathf.Clamp01(phaseTime / duration);
                // Ease out gives the punch a quick, weighty start.
                float eased = 1f - (1f - progress) * (1f - progress);
                activeGauntlet.position = Vector3.LerpUnclamped(punchStart, punchEnd, eased);
                if (activeEchoGauntlet != null)
                    activeEchoGauntlet.position = Vector3.LerpUnclamped(
                        echoPunchStart, echoPunchEnd, eased);
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
                    GetRestPosition(activeGauntlet);
                Vector3 currentEchoRestPosition = activeEchoGauntlet != null
                    ? GetRestPosition(activeEchoGauntlet)
                    : Vector3.zero;
                float duration = ActiveLevel != null
                    ? ActiveLevel.returnDuration
                    : returnDuration;
                float progress = Mathf.Clamp01(phaseTime / duration);
                float eased = progress * progress * (3f - 2f * progress);
                activeGauntlet.position =
                    Vector3.LerpUnclamped(punchEnd, currentRestPosition, eased);
                if (activeEchoGauntlet != null)
                    activeEchoGauntlet.position = Vector3.LerpUnclamped(
                        echoPunchEnd, currentEchoRestPosition, eased);
                if (progress >= 1f)
                {
                    phase = PunchPhase.Idle;
                    activeGauntlet = null;
                    activeEchoGauntlet = null;
                    punchTarget = null;
                    PositionAtRest();
                }
            }

            PositionInactiveGauntletsAtRest();
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
                (ActiveLevel != null ? ActiveLevel.hitRadius : hitRadius) *
                (upgradeStats != null ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSize) : 1f);
            PopulatePunchTargets(position, activeHitRadius);

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
                DamageContext damageContext = DamageContext.Weapon(
                    damage,
                    spiritOwner != null ? spiritOwner.transform : transform,
                    DamageElementUtility.FromSpiritName(
                        spiritOwner != null && spiritOwner.Definition != null
                            ? spiritOwner.Definition.SpiritName
                            : string.Empty));
                int strikeCount = upgradeStats != null
                    ? upgradeStats.GetMeleeStrikeCount(1)
                    : 1;
                for (int strike = 0; strike < strikeCount; strike++)
                {
                    target.TakeDamage(damageContext);
                }
                float activeStatusDuration = ActiveLevel != null
                    ? ActiveLevel.statusDuration
                    : freezeDuration;
                if (upgradeStats != null) activeStatusDuration = upgradeStats.ScaleDuration(activeStatusDuration);
                CombatStatus activeStatus = ActiveLevel != null
                    ? ActiveLevel.status
                    : CombatStatus.Freeze;
                if (activeStatusDuration > 0f &&
                    target is IStatusEffectReceiver statusReceiver)
                {
                    statusReceiver.ApplyStatus(
                        activeStatus,
                        activeStatusDuration,
                        1f,
                        damageContext);
                }
                targetsHit++;
                OnTargetHit(target, level);
            }
        }

        private void PopulatePunchTargets(Vector2 fallbackPosition, float fallbackRadius)
        {
            Collider2D activeHitbox = GetGauntletHitbox(activeGauntlet);
            if (activeHitbox == null || !activeHitbox.enabled)
            {
                CombatTargeting.FindAllNonAlloc(
                    fallbackPosition,
                    fallbackRadius,
                    Faction.Player,
                    targetBuffer);
                return;
            }

            targetBuffer.Clear();
            colliderBuffer.Clear();
            Physics2D.SyncTransforms();
            Physics2D.OverlapCollider(
                activeHitbox,
                ContactFilter2D.noFilter,
                colliderBuffer);

            for (int i = 0; i < colliderBuffer.Count; i++)
            {
                Collider2D overlap = colliderBuffer[i];
                IDamageable target = overlap != null
                    ? overlap.GetComponentInParent<IDamageable>()
                    : null;
                if (target != null && target.IsAlive && target.Faction != Faction.Player)
                    targetBuffer.Add(target);
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

        private bool UpdatePunchAim()
        {
            if (punchTarget == null || !punchTarget.gameObject.activeInHierarchy)
            {
                return false;
            }

            Vector2 toTarget = punchTarget.position - punchStart;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            punchDirection = toTarget.normalized;
            float activePunchDistance =
                ActiveLevel != null ? ActiveLevel.punchDistance : punchDistance;
            punchEnd = punchStart + (Vector3)(punchDirection * activePunchDistance);
            FaceDirection(activeGauntlet, punchDirection);
            if (activeEchoGauntlet != null)
            {
                echoPunchEnd = echoPunchStart +
                    (Vector3)(punchDirection * activePunchDistance);
                FaceDirection(activeEchoGauntlet, punchDirection);
            }
            return true;
        }

        private void PositionAtRest()
        {
            if (leftGauntlet == null || rightGauntlet == null)
            {
                return;
            }
            leftGauntlet.position = GetRestPosition(leftGauntlet);
            rightGauntlet.position = GetRestPosition(rightGauntlet);
            if (leftEchoGauntlet != null)
                leftEchoGauntlet.position = GetRestPosition(leftEchoGauntlet);
            if (rightEchoGauntlet != null)
                rightEchoGauntlet.position = GetRestPosition(rightEchoGauntlet);
        }

        private void PositionInactiveGauntletsAtRest()
        {
            Transform[] gauntlets =
                { leftGauntlet, rightGauntlet, leftEchoGauntlet, rightEchoGauntlet };
            for (int i = 0; i < gauntlets.Length; i++)
            {
                Transform gauntlet = gauntlets[i];
                if (gauntlet != null && gauntlet != activeGauntlet && gauntlet != activeEchoGauntlet)
                    gauntlet.position = GetRestPosition(gauntlet);
            }
        }

        private Vector3 GetRestPosition(Transform gauntlet)
        {
            bool isLeft = gauntlet == leftGauntlet || gauntlet == leftEchoGauntlet;
            bool isEcho = gauntlet == leftEchoGauntlet || gauntlet == rightEchoGauntlet;
            Transform slot = !isEcho
                ? (isLeft ? primaryWeaponSlot : secondaryWeaponSlot)
                : (isLeft ? tertiaryWeaponSlot : quaternaryWeaponSlot);
            if (slot != null)
            {
                return slot.position;
            }

            Vector2 facing = punchDirection.sqrMagnitude > 0.001f
                ? punchDirection
                : Vector2.right;
            Vector2 side = new Vector2(-facing.y, facing.x);
            float sideSign = isLeft ? 1f : -1f;
            float formationDistance = isEcho ? sideOffset * 2f : sideOffset;
            return ActiveOrigin.position +
                (Vector3)(facing * forwardRestOffset + side * formationDistance * sideSign);
        }

        private void FaceDirection(Transform gauntlet, Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gauntlet.rotation =
                Quaternion.Euler(0f, 0f, angle + spriteRotationOffset);
        }

        protected virtual void OnEnable()
        {
            if (upgradeStats != null)
            {
                upgradeStats.UpgradeApplied -= HandleUpgradeApplied;
                upgradeStats.UpgradeApplied += HandleUpgradeApplied;
            }
            phase = PunchPhase.Idle;
            activeGauntlet = null;
            activeEchoGauntlet = null;
            punchTarget = null;
            hitTargets.Clear();
            EnsurePooledVisuals();
            PositionAtRest();
            SetVisualsActive(combatActive);
        }

        protected virtual void OnDisable()
        {
            if (upgradeStats != null)
                upgradeStats.UpgradeApplied -= HandleUpgradeApplied;
            phase = PunchPhase.Idle;
            activeGauntlet = null;
            activeEchoGauntlet = null;
            punchTarget = null;
            hitTargets.Clear();
            combatActive = false;
            ReleasePooledVisuals();
        }

        private void HandleUpgradeApplied(UpgradeCardDefinition unusedCard, int unusedLevel)
        {
            SyncEchoGauntlets();
            ApplyWeaponSize();
            if (phase == PunchPhase.Idle) PositionAtRest();
            SetVisualsActive(combatActive);
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

            SyncEchoGauntlets();
            CacheRenderers();
        }

        private void SyncEchoGauntlets()
        {
            bool shouldHaveEchoPair = pooledVisualPrefab != null &&
                upgradeStats != null && upgradeStats.GetMeleeStrikeCount(1) > 1;
            bool hasEchoPair = leftEchoGauntlet != null && rightEchoGauntlet != null;
            if (shouldHaveEchoPair == hasEchoPair) return;

            if (shouldHaveEchoPair)
            {
                leftEchoGauntlet = SceneObjectPool.Spawn(
                    pooledVisualPrefab, ActiveOrigin.position, Quaternion.identity,
                    PoolCategory.Effects, tertiaryWeaponSlot).transform;
                rightEchoGauntlet = SceneObjectPool.Spawn(
                    pooledVisualPrefab, ActiveOrigin.position, Quaternion.identity,
                    PoolCategory.Effects, quaternaryWeaponSlot).transform;
            }
            else
            {
                if (leftEchoGauntlet != null)
                    SceneObjectPool.ReleaseOrDestroy(leftEchoGauntlet.gameObject);
                if (rightEchoGauntlet != null)
                    SceneObjectPool.ReleaseOrDestroy(rightEchoGauntlet.gameObject);
                leftEchoGauntlet = null;
                rightEchoGauntlet = null;
            }

            CacheRenderers();
            if (phase == PunchPhase.Idle) PositionAtRest();
            SetVisualsActive(combatActive);
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
            if (leftEchoGauntlet != null)
                SceneObjectPool.ReleaseOrDestroy(leftEchoGauntlet.gameObject);
            if (rightEchoGauntlet != null)
                SceneObjectPool.ReleaseOrDestroy(rightEchoGauntlet.gameObject);

            leftGauntlet = fallbackLeftGauntlet;
            rightGauntlet = fallbackRightGauntlet;
            leftEchoGauntlet = null;
            rightEchoGauntlet = null;
            CacheRenderers();
            SetVisualsActive(false);
        }

        private void CacheRenderers()
        {
            gauntletRenderers = new[]
            {
                leftGauntlet != null ? leftGauntlet.GetComponent<Renderer>() : null,
                rightGauntlet != null ? rightGauntlet.GetComponent<Renderer>() : null,
                leftEchoGauntlet != null ? leftEchoGauntlet.GetComponent<Renderer>() : null,
                rightEchoGauntlet != null ? rightEchoGauntlet.GetComponent<Renderer>() : null
            };
            leftGauntletHitbox = leftGauntlet != null
                ? leftGauntlet.GetComponentInChildren<Collider2D>(true)
                : null;
            rightGauntletHitbox = rightGauntlet != null
                ? rightGauntlet.GetComponentInChildren<Collider2D>(true)
                : null;
            leftEchoGauntletHitbox = leftEchoGauntlet != null
                ? leftEchoGauntlet.GetComponentInChildren<Collider2D>(true)
                : null;
            rightEchoGauntletHitbox = rightEchoGauntlet != null
                ? rightEchoGauntlet.GetComponentInChildren<Collider2D>(true)
                : null;
        }

        private Collider2D GetGauntletHitbox(Transform gauntlet)
        {
            if (gauntlet == leftGauntlet) return leftGauntletHitbox;
            if (gauntlet == rightGauntlet) return rightGauntletHitbox;
            if (gauntlet == leftEchoGauntlet) return leftEchoGauntletHitbox;
            return gauntlet == rightEchoGauntlet ? rightEchoGauntletHitbox : null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!drawHitboxes) return;

            WeaponLevelData level = ActiveLevel ??
                (definition != null ? definition.GetLevel(1) : null);
            Vector3 center = Application.isPlaying ? ActiveOrigin.position : transform.position;
            float range = level != null ? level.targetingRange : targetingRange;
            float radius = level != null ? level.hitRadius : hitRadius;
            float distance = level != null ? level.punchDistance : punchDistance;

            Gizmos.color = targetingGizmoColor;
            Gizmos.DrawWireSphere(center, range);

            Vector3 start = activeGauntlet != null
                ? punchStart
                : leftGauntlet != null ? leftGauntlet.position : center;
            Vector2 direction = punchDirection.sqrMagnitude > 0.001f
                ? punchDirection.normalized : Vector2.right;
            Vector3 end = activeGauntlet != null
                ? punchEnd
                : start + (Vector3)(direction * distance);

            Gizmos.color = punchPathGizmoColor;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(start, radius);
            Gizmos.DrawWireSphere(end, radius);

            if (activeGauntlet != null)
            {
                Gizmos.color = activeHitboxGizmoColor;
                Gizmos.DrawSphere(activeGauntlet.position, radius);
            }
        }

        private void ApplyWeaponSize()
        {
            float multiplier = upgradeStats != null
                ? upgradeStats.GetMultiplier(UpgradeStat.ProjectileSize)
                : 1f;
            Vector3 pooledScale = pooledVisualPrefab != null
                ? pooledVisualPrefab.transform.localScale
                : Vector3.one;

            if (leftGauntlet != null)
                leftGauntlet.localScale =
                    (leftGauntlet == fallbackLeftGauntlet ? fallbackLeftScale : pooledScale) * multiplier;
            if (rightGauntlet != null)
                rightGauntlet.localScale =
                    (rightGauntlet == fallbackRightGauntlet ? fallbackRightScale : pooledScale) * multiplier;
            if (leftEchoGauntlet != null)
                leftEchoGauntlet.localScale = pooledScale * multiplier;
            if (rightEchoGauntlet != null)
                rightEchoGauntlet.localScale = pooledScale * multiplier;
        }

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

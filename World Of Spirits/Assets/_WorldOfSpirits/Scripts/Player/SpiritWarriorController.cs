using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Progression.Upgrades;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(SpiritManager))]
    public sealed class SpiritWarriorController : MonoBehaviour
    {
        [SerializeField] private WarriorArmamentDefinition[] armaments;
        [SerializeField] private Material techniqueMaterial;
        [SerializeField, Min(0.1f)] private float dashCooldown = 3f;
        [SerializeField, Min(0.05f)] private float dashDuration = 0.2f;
        [SerializeField, Min(1f)] private float dashSpeedMultiplier = 3f;
        private readonly List<IDamageable> targets = new List<IDamageable>(64);
        private readonly HashSet<int> dashHits = new HashSet<int>();
        private readonly Dictionary<SpiritMember, float> pairReadyTimes = new Dictionary<SpiritMember, float>();
        private readonly Dictionary<SpiritMember, Transform[]> weaponAnchors = new Dictionary<SpiritMember, Transform[]>();
        private readonly List<GameObject> visualObjects = new List<GameObject>();
        private readonly Dictionary<SpiritMember, WarriorPairedThrustWeapon> pairedThrustWeapons = new Dictionary<SpiritMember, WarriorPairedThrustWeapon>();
        private readonly Dictionary<SpiritMember, Transform[]> pairedAnchors = new Dictionary<SpiritMember, Transform[]>();
        private SpriteRenderer leftEquipmentSprite, rightEquipmentSprite;
        private SpiritManager spirits;
        private PlayerMovement movement;
        private PlayerCharacter player;
        private UpgradeRuntimeStats stats;
        private WarriorArmamentDefinition dashArmament;
        private Vector2 dashDirection;
        private float dashEnds, dashReady, counterReady, effectEnds, nextThreatCheck;
        private bool threatNearby;
        private LineRenderer armourVisual, attackVisual, leftEquipment, rightEquipment;
        private Material visualMaterial;
        private readonly Shot[] shots = new Shot[8];
        private sealed class Shot
        {
            public LineRenderer visual;
            public bool active;
            public Vector2 position, direction;
            public float distance, damage;
            public WarriorArmamentDefinition armament;
        }

        public bool Active => isActiveAndEnabled && stats != null && stats.Character == UpgradeCharacter.SpiritWarrior &&
            player != null && player.IsAlive;
        public bool IsDashing => Active && !player.IsMovementDisabled && Time.time < dashEnds;
        public float DashRemaining => Mathf.Max(0f, dashReady - Time.time);
        public float Momentum { get; private set; }
        public float MovementMultiplier => Active && CurrentArmament != null &&
            CurrentArmament.element == DamageElement.Wind ? 1.12f : 1f;
        public Vector2 DashVelocity => IsDashing ? dashDirection * player.MoveSpeed *
            stats.GetMultiplier(UpgradeStat.MovementSpeed) * dashSpeedMultiplier : Vector2.zero;
        public float WeaponDamageMultiplier => Active ? 1f + Momentum * 0.2f : 1f;
        public WarriorArmamentDefinition CurrentArmament => FindArmament(spirits != null ? spirits.PrimarySpirit : null);

        private void Awake()
        {
            spirits = GetComponent<SpiritManager>();
            movement = GetComponent<PlayerMovement>();
            player = GetComponent<PlayerCharacter>();
            stats = GetComponent<UpgradeRuntimeStats>();
        }

        private void Start()
        {
            if (!Active) return;
            visualMaterial = techniqueMaterial != null ? new Material(techniqueMaterial) : new Material(Shader.Find("Sprites/Default"));
            armourVisual = MakeLine("Spirit Armour", 33, 0.045f);
            attackVisual = MakeLine("Paired Weapon Strike", 17, 0.12f);
            leftEquipment = MakeLine("Paired Weapon Left", 17, 0.07f);
            rightEquipment = MakeLine("Paired Weapon Right", 17, 0.07f);
            leftEquipmentSprite = MakeEquipmentSprite("Paired Weapon Art Left");
            rightEquipmentSprite = MakeEquipmentSprite("Paired Weapon Art Right");
            for (int i = 0; i < shots.Length; i++)
                shots[i] = new Shot { visual = MakeLine("Shard Launcher", 2, 0.1f) };
        }

        public WarriorArmamentDefinition FindArmament(SpiritMember spirit)
        {
            if (spirit == null || spirit.Definition == null || armaments == null) return null;
            var element = DamageElementUtility.FromSpiritName(spirit.Definition.SpiritName);
            foreach (var armament in armaments)
                if (armament != null && armament.element == element) return armament;
            return null;
        }

        // Every spirit gets separate anchors; melee weapons must never share transforms.
        public IReadOnlyList<Transform> GetWeaponAnchors(SpiritMember spirit, int index, int count)
        {
            if (!weaponAnchors.TryGetValue(spirit, out Transform[] anchors))
            {
                anchors = new Transform[4];
                for (int i = 0; i < anchors.Length; i++)
                {
                    var anchor = new GameObject(spirit.name + " Weapon Anchor " + i).transform;
                    anchor.SetParent(transform, false);
                    anchors[i] = anchor;
                }
                weaponAnchors.Add(spirit, anchors);
            }
            float angle = (90f + index * 360f / Mathf.Max(1, count)) * Mathf.Deg2Rad;
            Vector3 centre = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.9f;
            for (int i = 0; i < anchors.Length; i++)
                anchors[i].localPosition = centre + new Vector3((i % 2 == 0 ? -1f : 1f) * 0.22f, i >= 2 ? -0.3f : 0f);
            return anchors;
        }

        private void Update()
        {
            if (!Active || Time.timeScale <= 0f) return;
            if (Time.time >= nextThreatCheck)
            {
                threatNearby = CombatTargeting.FindClosest(transform.position, 8f, player.Faction) != null;
                nextThreatCheck = Time.time + 0.2f;
            }
            Momentum = Mathf.MoveTowards(Momentum, movement.IsMoving && threatNearby ? 1f : 0f,
                Time.deltaTime * (movement.IsMoving && threatNearby ? 0.25f : 0.35f));
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.leftShiftKey.wasPressedThisFrame)) TryDash();
            TickThrustWeapons();
            if (!spirits.IsChangingFormation) TickPair();
            TickShots();
            UpdateVisuals();
        }

        public bool TryDash()
        {
            if (!Active || Time.timeScale <= 0f || CurrentArmament == null ||
                player.IsMovementDisabled || spirits.IsChangingFormation || IsDashing || Time.time < dashReady) return false;
            dashDirection = movement.MoveDirection.sqrMagnitude > 0.01f
                ? movement.MoveDirection.normalized : movement.LastMoveDirection.normalized;
            dashArmament = CurrentArmament;
            dashEnds = Time.time + dashDuration;
            dashReady = Time.time + dashCooldown / Mathf.Max(0.1f, stats.GetMultiplier(UpgradeStat.CooldownReduction));
            dashHits.Clear();
            if (dashArmament.element == DamageElement.Water)
                player.AddShield(stats.ScaleShield(12f), 2f);
            return true;
        }

        private void FixedUpdate()
        {
            if (!IsDashing || Time.timeScale <= 0f) return;
            // One hit per target per dash, even when the target has several colliders.
            CombatTargeting.FindAllNonAlloc(transform.position, stats.ScaleArea(1.35f), player.Faction, targets);
            foreach (var target in targets)
            {
                if (!dashHits.Add(target.Transform.gameObject.GetInstanceID())) continue;
                Hit(target, dashArmament, 12f * (1f + Momentum), true);
            }
        }

        private void TickPair()
        {
            var spirit = spirits.PrimarySpirit;
            var armament = CurrentArmament;
            if (spirit == null || armament == null ||
                (pairReadyTimes.TryGetValue(spirit, out float ready) && Time.time < ready)) return;
            // Physical paired weapons have their own attack motion and cooldown.
            if (armament.thrustWeaponPrefab != null) return;
            var target = CombatTargeting.FindClosest(transform.position, stats.ScaleArea(armament.range), player.Faction);
            if (target == null) return;
            pairReadyTimes[spirit] = Time.time + armament.cooldown /
                Mathf.Max(0.1f, stats.GetMultiplier(UpgradeStat.AttackSpeed));
            float damage = armament.damage * (1f + 0.2f * (spirit.Progression.WeaponLevel - 1));
            Vector2 direction = (target.Transform.position - transform.position).normalized;
            if (armament.element == DamageElement.Ice) LaunchShards(direction, armament, damage);
            else Strike(armament, direction, damage, armament.range, false);
        }

        public float ReduceIncomingDamage(float amount, DamageContext context)
        {
            var armament = CurrentArmament;
            if (!Active || armament == null || amount <= 0f) return amount;
            float reduction = armament.damageReduction;
            if (armament.element == DamageElement.Earth && !context.IsStatusDamage)
            {
                Vector2 incoming = context.Source != null ?
                    (Vector2)(context.Source.position - transform.position).normalized : Vector2.zero;
                Vector2 facing = IsDashing ? dashDirection : movement.MoveDirection.sqrMagnitude > 0.01f
                    ? movement.MoveDirection.normalized : movement.LastMoveDirection;
                if (incoming != Vector2.zero && Vector2.Dot(facing, incoming) >= 0.3f)
                {
                    reduction = IsDashing ? 0.6f : 0.35f;
                    if (Time.time >= counterReady)
                    {
                        counterReady = Time.time + 1.2f;
                        // Blocking supports the physical shield bash; it does not deal an invisible cone hit.
                        if (armament.thrustWeaponPrefab == null)
                            Strike(armament, facing, armament.damage, armament.range, true);
                    }
                }
            }
            return amount * (1f - Mathf.Clamp(reduction, 0f, 0.6f));
        }

        private void Strike(WarriorArmamentDefinition armament, Vector2 direction, float damage, float range, bool dash)
        {
            CombatTargeting.FindAllNonAlloc(transform.position, stats.ScaleArea(range), player.Faction, targets);
            // TakeDamage can cause death/pool callbacks; the targeting results are local to this controller.
            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                Vector2 offset = target.Transform.position - transform.position;
                if (Vector2.Dot(offset.normalized, direction) < 0.25f) continue;
                Hit(target, armament, damage, dash);
            }
            ShowArc(direction, stats.ScaleArea(range), armament.colour);
        }

        private void Hit(IDamageable target, WarriorArmamentDefinition armament, float damage, bool dash)
        {
            var entity = target as LivingEntity;
            if (entity == null || !entity.IsAlive) return;
            DamageContext context = DamageContext.Weapon(damage, transform, armament.element);
            target.TakeDamage(context);
            if (!entity.IsAlive) return;
            switch (armament.element)
            {
                case DamageElement.Fire:
                    entity.ApplyStatus(CombatStatus.Burn, stats.ScaleDuration(1.5f), 3f, context); break;
                case DamageElement.Ice:
                    entity.ApplyStatus(CombatStatus.Slow, stats.ScaleDuration(dash ? 2f : 1f), 0.35f, context); break;
                case DamageElement.Earth:
                    entity.ApplyStatus(CombatStatus.Stun, stats.ScaleDuration(dash ? 0.4f : 0.2f), 0f, context); break;
                case DamageElement.Water:
                case DamageElement.Wind:
                    // Crowd agents use kinematic bodies; route impulse through their steering.
                    entity.ApplyStatus(CombatStatus.Slow, stats.ScaleDuration(0.8f), 0.3f, context);
                    Vector2 away = entity.transform.position - transform.position;
                    Vector2 impulse = away.normalized * stats.ScaleForce(armament.element == DamageElement.Water && !dash ? -2f : 3f);
                    var crowd = entity.GetComponent<WorldOfSpirits.Crowd.EnemyCrowdAgent>();
                    if (crowd != null) crowd.ApplyExternalAcceleration(impulse, 1f);
                    else
                    {
                        var body = entity.GetComponent<Rigidbody2D>();
                        if (body != null && body.bodyType == RigidbodyType2D.Dynamic)
                            body.AddForce(impulse, ForceMode2D.Impulse);
                    }
                    break;
            }
        }

        private void LaunchShards(Vector2 direction, WarriorArmamentDefinition armament, float damage)
        {
            int count = Mathf.Min(5, stats.GetProjectileCount(3));
            for (int i = 0; i < count; i++)
            {
                Shot slot = null;
                foreach (var shot in shots) if (shot != null && !shot.active) {slot = shot; break;}
                if (slot == null) break;
                float angle = Mathf.Atan2(direction.y, direction.x) + (i - (count - 1) * 0.5f) * 0.12f;
                slot.direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                slot.position = transform.position;
                slot.distance = 0f;
                slot.damage = damage;
                slot.armament = armament;
                slot.active = true;
                slot.visual.startColor = slot.visual.endColor = armament.colour;
            }
        }

        private void TickShots()
        {
            foreach (var shot in shots)
            {
                if (shot == null || !shot.active) continue;
                Vector2 from = shot.position;
                float step = 12f * stats.GetMultiplier(UpgradeStat.ProjectileSpeed) * Time.deltaTime;
                shot.position += shot.direction * step;
                shot.distance += step;
                float radius = 0.2f * stats.GetMultiplier(UpgradeStat.ProjectileSize);
                CombatTargeting.FindAllNonAlloc((from + shot.position) * 0.5f, step * 0.5f + radius, player.Faction, targets);
                IDamageable nearest = null;
                float nearestAlong = float.MaxValue;
                foreach (var target in targets)
                {
                    Vector2 offset = (Vector2)target.Transform.position - from;
                    float along = Mathf.Clamp(Vector2.Dot(offset, shot.direction), 0f, step);
                    if ((offset - shot.direction * along).sqrMagnitude > (radius + 0.35f) * (radius + 0.35f)) continue;
                    if (along < nearestAlong) {nearest = target; nearestAlong = along;}
                }
                if (nearest != null) {Hit(nearest, shot.armament, shot.damage, false); shot.active = false;}
                if (shot.distance >= shot.armament.range) shot.active = false;
                shot.visual.enabled = shot.active;
                shot.visual.SetPosition(0, shot.position);
                shot.visual.SetPosition(1, shot.position - shot.direction * 0.4f);
            }
        }

        private LineRenderer MakeLine(string label, int points, float width)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            visualObjects.Add(go);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = visualMaterial;
            line.positionCount = points;
            line.startWidth = line.endWidth = width;
            line.sortingOrder = 25;
            line.enabled = false;
            return line;
        }

        private void ShowArc(Vector2 direction, float radius, Color colour)
        {
            if (attackVisual == null) return;
            effectEnds = Time.time + 0.15f;
            attackVisual.startColor = attackVisual.endColor = colour;
            float facing = Mathf.Atan2(direction.y, direction.x);
            for (int i = 0; i < attackVisual.positionCount; i++)
            {
                float angle = facing + Mathf.Lerp(-1.2f, 1.2f, i / (float)(attackVisual.positionCount - 1));
                attackVisual.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        private void UpdateVisuals()
        {
            if (armourVisual == null) return;
            var armament = CurrentArmament;
            armourVisual.enabled = armament != null;
            if (armament != null)
            {
                Color colour = armament.colour;
                colour.a = IsDashing ? 0.9f : 0.35f;
                armourVisual.startColor = armourVisual.endColor = colour;
                float radius = IsDashing ? 0.85f : 0.65f;
                for (int i = 0; i < armourVisual.positionCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / (armourVisual.positionCount - 1);
                    armourVisual.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                }
            }
            attackVisual.enabled = Time.time < effectEnds;
            UpdateEquipmentVisuals(armament);
        }

        private void UpdateEquipmentVisuals(WarriorArmamentDefinition armament)
        {
            bool spriteEquipment = armament != null && armament.equipmentSprite != null;
            bool physicalEquipment = armament != null && armament.thrustWeaponPrefab != null;
            UpdateEquipmentSprites(armament, spriteEquipment && !physicalEquipment);
            leftEquipment.enabled = armament != null && !spriteEquipment && !physicalEquipment;
            rightEquipment.enabled = armament != null && !spriteEquipment && !physicalEquipment &&
                (armament.element == DamageElement.Fire || armament.element == DamageElement.Wind);
            if (armament == null || spriteEquipment || physicalEquipment) return;
            Vector2 forward = IsDashing ? dashDirection : movement.LastMoveDirection;
            Vector2 side = new Vector2(-forward.y, forward.x);
            for (int hand = 0; hand < 2; hand++)
            {
                var visual = hand == 0 ? leftEquipment : rightEquipment;
                if (!visual.enabled) continue;
                visual.startColor = visual.endColor = armament.colour;
                Vector2 origin = (Vector2)transform.position + forward * 0.45f +
                    side * (hand == 0 ? -0.55f : 0.55f);
                for (int i = 0; i < visual.positionCount; i++)
                {
                    float t = i / (float)(visual.positionCount - 1);
                    Vector2 local;
                    switch (armament.element)
                    {
                        case DamageElement.Earth:
                            float angle = t * Mathf.PI * 2f;
                            local = new Vector2(Mathf.Cos(angle) * 0.35f, Mathf.Sin(angle) * 0.5f);
                            break;
                        case DamageElement.Water:
                            local = new Vector2(Mathf.Sin(t * 7f + Time.time * 4f) * 0.18f, t * 1.15f);
                            break;
                        case DamageElement.Wind:
                            float fanAngle = Mathf.Lerp(-1.2f, 1.2f, t);
                            local = i % 2 == 0 ? new Vector2(Mathf.Sin(fanAngle), Mathf.Cos(fanAngle)) * 0.55f : Vector2.zero;
                            break;
                        case DamageElement.Ice:
                            local = new Vector2(Mathf.Sin(t * Mathf.PI * 2f) * 0.18f, t < 0.5f ? t * 1.4f : (1f - t) * 1.4f);
                            break;
                        default:
                            local = new Vector2((t - 0.5f) * 0.55f, i % 4 == 0 ? 0.55f : 0f);
                            break;
                    }
                    visual.SetPosition(i, origin + side * local.x + forward * local.y);
                }
            }
        }

        private SpriteRenderer MakeEquipmentSprite(string label)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 26;
            renderer.enabled = false;
            return renderer;
        }

        private void UpdateEquipmentSprites(WarriorArmamentDefinition armament, bool visible)
        {
            if (leftEquipmentSprite == null) return;
            leftEquipmentSprite.enabled = visible;
            rightEquipmentSprite.enabled = visible && armament.element == DamageElement.Wind;
            if (!visible) return;
            Vector2 forward = IsDashing ? dashDirection : movement.LastMoveDirection;
            Vector2 side = new Vector2(-forward.y, forward.x);
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f;
            for (int i = 0; i < 2; i++)
            {
                var renderer = i == 0 ? leftEquipmentSprite : rightEquipmentSprite;
                if (!renderer.enabled) continue;
                renderer.sprite = armament.equipmentSprite;
                renderer.transform.position = transform.position + (Vector3)(forward * 0.35f + side * (i == 0 ? -0.65f : 0.65f));
                renderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                renderer.transform.localScale = Vector3.one * armament.equipmentHeight /
                    Mathf.Max(0.01f, renderer.sprite.bounds.size.y);
                renderer.flipX = i == 1;
            }
        }

        private void TickThrustWeapons()
        {
            var spirit = spirits.PrimarySpirit;
            var armament = CurrentArmament;
            bool enabledForMain = Active && !spirits.IsChangingFormation && !player.IsMovementDisabled;
            if (enabledForMain && spirit != null && armament != null && armament.thrustWeaponPrefab != null &&
                !pairedThrustWeapons.ContainsKey(spirit))
            {
                var weapon = Instantiate(armament.thrustWeaponPrefab, transform);
                weapon.name = armament.pairedWeaponName;
                weapon.BindSpiritOwner(spirit);
                pairedThrustWeapons.Add(spirit, weapon);
                var anchors = new Transform[4];
                for (int i = 0; i < anchors.Length; i++)
                {
                    anchors[i] = new GameObject(armament.pairedWeaponName + " Rest " + i).transform;
                    anchors[i].SetParent(transform, false);
                }
                pairedAnchors.Add(spirit, anchors);
            }
            Vector2 facing = movement.LastMoveDirection;
            Vector2 side = new Vector2(-facing.y, facing.x);
            foreach (var pair in pairedThrustWeapons)
            {
                bool active = enabledForMain && pair.Key == spirit;
                var weapon = pair.Value;
                if (weapon == null) continue;
                if (!active)
                {
                    weapon.SetCombatActive(false);
                    weapon.enabled = false;
                    continue;
                }
                var anchors = pairedAnchors[pair.Key];
                for (int i = 0; i < anchors.Length; i++)
                    anchors[i].localPosition = facing * 0.15f + side *
                        ((i % 2 == 0 ? -1f : 1f) * (i < 2 ? 0.85f : 1.25f));
                weapon.enabled = true;
                weapon.SetOriginOverride(transform);
                weapon.SetWeaponSlots(anchors);
                weapon.SetCombatActive(true);
            }
        }

        private void OnDisable()
        {
            foreach (var pair in pairedThrustWeapons)
                if (pair.Value != null) {pair.Value.SetCombatActive(false); pair.Value.enabled = false;}
            if (leftEquipmentSprite != null) leftEquipmentSprite.enabled = false;
            if (rightEquipmentSprite != null) rightEquipmentSprite.enabled = false;
            dashEnds = dashReady = counterReady = 0f;
            Momentum = 0f;
            pairReadyTimes.Clear();
            dashHits.Clear();
            foreach (var shot in shots) if (shot != null) {shot.active = false; shot.visual.enabled = false;}
            foreach (var go in visualObjects) if (go != null) go.GetComponent<LineRenderer>().enabled = false;
        }

        private void OnDestroy()
        {
            if (visualMaterial != null) Destroy(visualMaterial);
        }
    }
}

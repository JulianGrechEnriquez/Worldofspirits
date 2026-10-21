using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.Spirits
{
    public sealed class SpiritCreature : LivingEntity
    {
        private static readonly List<SpiritCreature> creatures = new List<SpiritCreature>();
        [SerializeField] private float recoverySeconds = 18f;
        [SerializeField] private float attackDamage = 12f;
        [SerializeField] private float attackInterval = 0.8f;
        private SpiritBeastmasterController commander;
        private SpiritMember member;
        private Renderer[] visuals;
        private SpiritAbility[] abilities;
        private Collider2D hitbox;
        private float returnTime, nextAttack, protectedUntil;
        private int index;
        public bool Recovering { get; private set; }
        public float RecoveryRemaining => Mathf.Max(0f, returnTime - Time.time);
        public override Faction Faction => global::WorldOfSpirits.Combat.Faction.Player;

        public void Initialize(SpiritBeastmasterController owner, int slot)
        {
            commander = owner; index = slot;
            transform.SetParent(null, true);
            member = GetComponent<SpiritMember>();
            visuals = GetComponentsInChildren<Renderer>(true);
            hitbox = gameObject.AddComponent<CircleCollider2D>();
            ((CircleCollider2D)hitbox).radius = 0.4f;
            hitbox.isTrigger = true;
            gameObject.layer = owner.gameObject.layer;
            abilities = GetComponentsInChildren<SpiritAbility>(true);
            foreach (var ability in abilities)
            {
                ability.BindCreatureCaster(owner.transform);
                ability.enabled = true;
            }
            if (!creatures.Contains(this)) creatures.Add(this);
        }

        protected override void OnDisable() { creatures.Remove(this); base.OnDisable(); }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (commander != null && !creatures.Contains(this)) creatures.Add(this);
        }

        public static SpiritCreature FindThreat(Vector2 position, Vector2 playerPosition)
        {
            SpiritCreature nearest = null;
            float best = Mathf.Min(64f, (playerPosition - position).sqrMagnitude);
            foreach (var creature in creatures)
            {
                if (creature == null || !creature.isActiveAndEnabled || creature.commander == null ||
                    !creature.commander.isActiveAndEnabled || creature.Recovering || !creature.IsAlive) continue;
                float d = ((Vector2)creature.transform.position - position).sqrMagnitude;
                if (d < best) { nearest = creature; best = d; }
            }
            return nearest;
        }

        public override void TakeDamage(DamageContext context)
        {
            if (!Recovering && Time.time >= protectedUntil) base.TakeDamage(context);
        }

        protected override void Die()
        {
            Recovering = true;
            returnTime = Time.time + recoverySeconds;
            hitbox.enabled = false;
            foreach (var ability in abilities) ability.enabled = false;
            foreach (var visual in visuals) if (visual != null) visual.enabled = false;
        }

        private void Update()
        {
            if (commander == null || !commander.isActiveAndEnabled || Time.timeScale <= 0f) return;
            if (Recovering)
            {
                if (Time.time < returnTime) return;
                OnSpawnedFromPool(null);
                Recovering = false;
                protectedUntil = Time.time + 1.5f;
                transform.position = commander.transform.position + Offset;
                hitbox.enabled = true;
                foreach (var ability in abilities) ability.enabled = true;
                foreach (var visual in visuals) if (visual != null) visual.enabled = true;
            }
            Vector2 anchor = commander.Order == CreatureOrder.Follow
                ? (Vector2)commander.transform.position : commander.Destination;
            LivingEntity target = commander.FocusTarget;
            float best = 16f;
            if (target == null)
                foreach (var entity in ActiveEntities)
                {
                    if (entity == null || !entity.IsAlive || entity.Faction != global::WorldOfSpirits.Combat.Faction.Enemy) continue;
                    float distance = ((Vector2)entity.transform.position - anchor).sqrMagnitude;
                    if (distance < best) { best = distance; target = entity; }
                }
            Vector2 destination = target != null ? (Vector2)target.transform.position : anchor + (Vector2)Offset;
            transform.position = Vector2.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);
            var context = new SpiritAbilityContext(transform, Vector2.Distance(transform.position, destination) > 0.1f,
                true, true, true);
            foreach (var ability in abilities) ability.TickAbility(context);
            if (target == null || Vector2.Distance(transform.position, target.transform.position) > 1f || Time.time < nextAttack) return;
            target.TakeDamage(DamageContext.Spirit(attackDamage, commander.transform,
                DamageElementUtility.FromSpiritName(member.Definition != null ? member.Definition.SpiritName : name)));
            nextAttack = Time.time + attackInterval;
        }

        private Vector3 Offset => Quaternion.Euler(0, 0, index * 120f) * Vector3.up * 1.4f;
        private void OnGUI()
        {
            var camera = Camera.main;
            if (camera == null || Recovering || Time.timeScale <= 0f) return;
            Vector3 p = camera.WorldToScreenPoint(transform.position + Vector3.up * 0.7f);
            GUI.Box(new Rect(p.x - 22, Screen.height - p.y, 44, 6), GUIContent.none);
            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(p.x - 21, Screen.height - p.y + 1, 42 * CurrentHealth / MaxHealth, 4), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}

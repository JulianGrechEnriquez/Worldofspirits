using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.Player
{
    public enum CreatureOrder { Follow, AttackMove, Guard }

    public sealed class SpiritBeastmasterController : MonoBehaviour
    {
        [Header("Player Spirit Bolt")]
        [SerializeField] private SpiritBolt boltPrefab;
        [SerializeField, Min(0.05f)] private float boltInterval = 0.35f;
        [SerializeField, Min(0f)] private float boltDamage = 6f;
        [SerializeField, Min(1f)] private float boltSpeed = 14f;
        private float nextBoltTime;
        private PlayerCharacter player;
        public CreatureOrder Order { get; private set; }
        public Vector2 Destination { get; private set; }
        public LivingEntity FocusTarget { get; private set; }
        private SpiritManager spirits;
        private Camera view;
        private int focusVersion;
        private WorldOfSpirits.Core.PooledSceneObject focusPool;
        private void Awake() { spirits = GetComponent<SpiritManager>(); player = GetComponent<PlayerCharacter>(); }

        public bool TryFireBolt(Vector2 point)
        {
            if (!isActiveAndEnabled || Time.timeScale <= 0f || player == null || !player.IsAlive ||
                boltPrefab == null || Time.time < nextBoltTime) return false;
            Vector2 direction = point - (Vector2)transform.position;
            if (direction.sqrMagnitude < 0.001f) return false;
            var bolt = ProjectilePool.Spawn(boltPrefab, transform.position + (Vector3)(direction.normalized * 0.5f), Quaternion.identity);
            bolt.ConfigureDamageContext(new DamageContext(boltDamage, transform));
            bolt.Launch(direction.normalized, boltSpeed, boltDamage, Faction.Player);
            nextBoltTime = Time.time + boltInterval;
            return true;
        }

        public void IssueOrder(CreatureOrder order, Vector2 point)
        {
            Order = order;
            Destination = point;
            FocusTarget = null;
        }

        public void Focus(LivingEntity target)
        {
            if (target == null || !target.IsAlive || target.Faction != Faction.Enemy) return;
            FocusTarget = target;
            focusPool = target.GetComponent<WorldOfSpirits.Core.PooledSceneObject>();
            focusVersion = focusPool != null ? focusPool.SpawnVersion : 0;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (FocusTarget != null && (!FocusTarget.IsAlive || !FocusTarget.isActiveAndEnabled ||
                (focusPool != null && focusPool.SpawnVersion != focusVersion))) FocusTarget = null;
            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
                IssueOrder(CreatureOrder.Follow, transform.position);
            if (Mouse.current == null || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) return;
            if (view == null) view = Camera.main;
            if (view == null) return;
            Vector2 point = view.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) TryFireBolt(point);
            if (Mouse.current.rightButton.wasPressedThisFrame) IssueOrder(CreatureOrder.Guard, point);
            else if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                LivingEntity clicked = null;
                float distance = 0.75f * 0.75f;
                foreach (var entity in LivingEntity.ActiveEntities)
                {
                    if (entity == null || !entity.IsAlive || entity.Faction != Faction.Enemy) continue;
                    float d = ((Vector2)entity.transform.position - point).sqrMagnitude;
                    if (d < distance) { clicked = entity; distance = d; }
                }
                if (clicked != null) Focus(clicked);
                else IssueOrder(CreatureOrder.AttackMove, point);
            }
        }

        private void OnGUI()
        {
            if (Time.timeScale <= 0f || spirits == null) return;
            GUI.Box(new Rect(12, Screen.height - 100, 490, 90),
                "Beastmaster: " + Order + "\nLMB: attack / move   RMB: guard   Q: recall   Hold Space: spirit bolt");
            for (int i = 0; i < spirits.SpiritCount; i++)
            {
                var member = spirits.GetSpiritAt(i);
                var creature = member != null ? member.GetComponent<SpiritCreature>() : null;
                if (creature == null) continue;
                GUI.Label(new Rect(24 + i * 155, Screen.height - 56, 155, 40),
                    spirits.GetSpiritNameAt(i) + "\n" + (creature.Recovering
                    ? "Recovering " + creature.RecoveryRemaining.ToString("0") + "s"
                    : creature.CurrentHealth.ToString("0") + " / " + creature.MaxHealth.ToString("0")));
            }
            if (view == null) return;
            Vector3 marker = view.WorldToScreenPoint(FocusTarget != null ? FocusTarget.transform.position :
                Order == CreatureOrder.Follow ? transform.position : (Vector3)Destination);
            GUI.Label(new Rect(marker.x - 24, Screen.height - marker.y - 20, 100, 30),
                FocusTarget != null ? "[ TARGET ]" : Order == CreatureOrder.Guard ? "[ GUARD ]" : "[ + ]");
        }
    }
}

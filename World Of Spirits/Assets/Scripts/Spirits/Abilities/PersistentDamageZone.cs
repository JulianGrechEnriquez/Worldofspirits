using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Spirits
{
    [RequireComponent(typeof(Collider2D))]
    public class PersistentDamageZone : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damagePerTick = 5f;
        [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;
        [SerializeField, Min(0.05f)] private float duration = 3f;
        [SerializeField] private bool destroyAfterDuration = true;
        [SerializeField] private Faction ownerFaction = Faction.Player;
        [SerializeField, Min(0f)] private float pullForce;
        [SerializeField] private bool followOwner;

        private readonly Dictionary<int, float> nextHitTimes = new Dictionary<int, float>();
        private Transform owner;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Start()
        {
            if (destroyAfterDuration)
                Destroy(gameObject, duration);
        }

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
        }

        public void SetReusable(bool reusable)
        {
            destroyAfterDuration = !reusable;
        }

        private void Update()
        {
            if (followOwner && owner != null)
            {
                transform.position = owner.position;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || target.Faction == ownerFaction)
            {
                return;
            }

            int id = target.Transform.gameObject.GetInstanceID();
            if (nextHitTimes.TryGetValue(id, out float nextHit) && Time.time < nextHit)
            {
                return;
            }

            target.TakeDamage(damagePerTick);
            nextHitTimes[id] = Time.time + tickInterval;
            Rigidbody2D body = target.Transform.GetComponent<Rigidbody2D>();
            if (body != null && pullForce > 0f)
            {
                body.AddForce((transform.position - target.Transform.position).normalized * pullForce, ForceMode2D.Force);
            }
        }
    }
}

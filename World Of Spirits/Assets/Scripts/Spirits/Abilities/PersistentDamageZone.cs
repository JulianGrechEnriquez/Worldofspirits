using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.Spirits
{
    [RequireComponent(typeof(Collider2D))]
    public class PersistentDamageZone : MonoBehaviour, IScenePoolable
    {
        private sealed class Occupant
        {
            public IDamageable Target;
            public Rigidbody2D Body;
            public float NextHitTime;
            public int ColliderCount;
        }

        [SerializeField, Min(0f)] private float damagePerTick = 5f;
        [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;
        [SerializeField, Min(0.05f)] private float duration = 3f;
        [SerializeField] private bool destroyAfterDuration = true;
        [SerializeField] private Faction ownerFaction = Faction.Player;
        [SerializeField, Min(0f)] private float pullForce;
        [SerializeField] private bool followOwner;

        private readonly Dictionary<int, Occupant> occupants = new Dictionary<int, Occupant>();
        private readonly List<int> occupantsToRemove = new List<int>();
        private Transform owner;
        private float disableTime;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnEnable()
        {
            disableTime = Time.time + duration;
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
            if (destroyAfterDuration && Time.time >= disableTime)
            {
                SceneObjectPool.ReleaseOrDestroy(gameObject);
                return;
            }

            if (followOwner && owner != null)
            {
                transform.position = owner.position;
            }

            occupantsToRemove.Clear();
            foreach (KeyValuePair<int, Occupant> pair in occupants)
            {
                Occupant occupant = pair.Value;
                if (occupant.Target == null || !occupant.Target.IsAlive)
                {
                    occupantsToRemove.Add(pair.Key);
                    continue;
                }

                if (Time.time >= occupant.NextHitTime)
                {
                    occupant.Target.TakeDamage(damagePerTick);
                    occupant.NextHitTime = Time.time + tickInterval;
                }
            }

            for (int i = 0; i < occupantsToRemove.Count; i++)
            {
                occupants.Remove(occupantsToRemove[i]);
            }
        }

        private void FixedUpdate()
        {
            if (pullForce <= 0f)
            {
                return;
            }

            foreach (Occupant occupant in occupants.Values)
            {
                if (occupant.Body != null && occupant.Target != null && occupant.Target.IsAlive)
                {
                    occupant.Body.AddForce(
                        (transform.position - occupant.Target.Transform.position).normalized * pullForce,
                        ForceMode2D.Force);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive || target.Faction == ownerFaction)
            {
                return;
            }

            int id = target.Transform.gameObject.GetInstanceID();
            if (occupants.TryGetValue(id, out Occupant existing))
            {
                existing.ColliderCount++;
                return;
            }

            occupants.Add(id, new Occupant
            {
                Target = target,
                Body = target.Transform.GetComponent<Rigidbody2D>(),
                NextHitTime = Time.time,
                ColliderCount = 1
            });
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null)
            {
                return;
            }

            int id = target.Transform.gameObject.GetInstanceID();
            if (occupants.TryGetValue(id, out Occupant occupant) && --occupant.ColliderCount <= 0)
            {
                occupants.Remove(id);
            }
        }

        private void OnDisable()
        {
            occupants.Clear();
        }

        public void OnSpawnedFromPool(GameObject prefab)
        {
            PersistentDamageZone prefabZone = prefab.GetComponent<PersistentDamageZone>();
            if (prefabZone != null)
            {
                damagePerTick = prefabZone.damagePerTick;
                tickInterval = prefabZone.tickInterval;
                duration = prefabZone.duration;
                destroyAfterDuration = prefabZone.destroyAfterDuration;
                ownerFaction = prefabZone.ownerFaction;
                pullForce = prefabZone.pullForce;
                followOwner = prefabZone.followOwner;
            }

            owner = null;
            disableTime = Time.time + duration;
        }

        public void OnReturnedToPool()
        {
            occupants.Clear();
            owner = null;
        }
    }
}

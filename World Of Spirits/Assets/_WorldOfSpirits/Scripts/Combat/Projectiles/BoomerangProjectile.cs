using System.Collections.Generic;
using UnityEngine;

namespace WorldOfSpirits.Combat
{
    public sealed class BoomerangProjectile : ProjectileBase
    {
        [Header("Boomerang")]
        [SerializeField, Min(0.5f)] private float outboundDistance = 7f;
        [SerializeField, Min(0.1f)] private float returnSpeedMultiplier = 1.35f;
        [SerializeField, Min(0.05f)] private float catchDistance = 0.45f;
        [SerializeField, Min(0.25f)] private float maximumReturnDuration = 3f;
        [SerializeField] private float spinSpeed = -900f;

        private readonly HashSet<int> outwardHits = new HashSet<int>();
        private readonly HashSet<int> returnHits = new HashSet<int>();
        private Transform owner;
        private Vector2 launchPosition;
        private float activeSpeed;
        private float returnStartTime;
        private bool returning;

        public override void Launch(Vector2 direction, float speed, float damage, Faction ownerFaction)
        {
            outwardHits.Clear();
            returnHits.Clear();
            owner = DamageSourceContext.Source;
            launchPosition = transform.position;
            activeSpeed = Mathf.Max(0.1f, speed);
            returning = false;
            base.Launch(direction, speed, damage, ownerFaction);
        }

        protected override void Update()
        {
            if (!returning)
            {
                base.Update();
                if (!isActiveAndEnabled) return;

                if (((Vector2)transform.position - launchPosition).sqrMagnitude >=
                    outboundDistance * outboundDistance)
                {
                    BeginReturn();
                }
            }
            else
            {
                if (owner == null || Time.time - returnStartTime >= maximumReturnDuration)
                {
                    Despawn();
                    return;
                }

                Vector2 toOwner = owner.position - transform.position;
                if (toOwner.sqrMagnitude <= catchDistance * catchDistance)
                {
                    Despawn();
                    return;
                }

                Body.linearVelocity = toOwner.normalized * activeSpeed * returnSpeedMultiplier;
            }

            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }

        protected override void OnLifetimeExpired()
        {
            if (!returning) BeginReturn();
            else Despawn();
        }

        protected override void OnHit(IDamageable target)
        {
            HashSet<int> hitSet = returning ? returnHits : outwardHits;
            int targetId = target.Transform.gameObject.GetInstanceID();
            if (hitSet.Add(targetId))
                target.TakeDamage(DamageSourceContext);
        }

        private void BeginReturn()
        {
            returning = true;
            returnStartTime = Time.time;
        }

        protected override void ResetPooledConfiguration(ProjectileBase prefab)
        {
            if (prefab is not BoomerangProjectile source) return;
            outboundDistance = source.outboundDistance;
            returnSpeedMultiplier = source.returnSpeedMultiplier;
            catchDistance = source.catchDistance;
            maximumReturnDuration = source.maximumReturnDuration;
            spinSpeed = source.spinSpeed;
        }
    }
}

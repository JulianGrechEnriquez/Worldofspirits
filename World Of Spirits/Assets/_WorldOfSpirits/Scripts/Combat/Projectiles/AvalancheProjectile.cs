using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Combat
{
    /// <summary>A growing snowball that sweeps enemies along until it expires.</summary>
    public sealed class AvalancheProjectile : ConfigurableProjectile
    {
        private readonly List<AvalancheCapturedEnemy> passengers = new List<AvalancheCapturedEnemy>();
        protected override bool ConsumePierceOnHit => false;

        public override void Launch(Vector2 direction, float speed, float damage, Faction ownerFaction)
        {
            ReleasePassengers();
            base.Launch(direction, speed, damage, ownerFaction);
        }

        protected override void OnHit(IDamageable target)
        {
            base.OnHit(target);
            if (!target.IsAlive || target.Transform == null ||
                !target.Transform.gameObject.activeInHierarchy) return;

            EnemyBase enemy = target.Transform.GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsBoss) return;
            AvalancheCapturedEnemy passenger = enemy.GetComponent<AvalancheCapturedEnemy>();
            if (passenger == null) passenger = enemy.gameObject.AddComponent<AvalancheCapturedEnemy>();
            if (passenger.TryCapture(this)) passengers.Add(passenger);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            for (int i = passengers.Count - 1; i >= 0; i--)
            {
                AvalancheCapturedEnemy passenger = passengers[i];
                if (passenger == null || passenger.Owner != this)
                {
                    passengers.RemoveAt(i);
                    continue;
                }
                passenger.Follow();
            }
        }

        protected override void OnLifetimeExpired()
        {
            ReleasePassengers();
            base.OnLifetimeExpired();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ReleasePassengers();
        }

        private void ReleasePassengers()
        {
            foreach (AvalancheCapturedEnemy passenger in passengers)
                if (passenger != null && passenger.Owner == this) passenger.Release();
            passengers.Clear();
        }
    }
}

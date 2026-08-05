using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    public class ChasingEnemy : EnemyBase
    {
        [SerializeField, Min(0f)] private float stoppingDistance = 0.1f;

        protected override void MoveTowardsTarget()
        {
            Vector2 offset = Target.position - transform.position;
            Body.linearVelocity = offset.sqrMagnitude > stoppingDistance * stoppingDistance
                ? offset.normalized * MoveSpeed
                : Vector2.zero;
        }
    }
}

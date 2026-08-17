using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    public readonly struct BossContext
    {
        public BossContext(BossEnemyBase boss, Transform target, BossMovement movement)
        {
            Boss = boss;
            Target = target;
            Movement = movement;
        }

        public BossEnemyBase Boss { get; }
        public Transform Target { get; }
        public BossMovement Movement { get; }
    }
}

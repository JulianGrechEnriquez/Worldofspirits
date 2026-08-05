using UnityEngine;

namespace WorldOfSpirits.Enemies
{
    public class StationaryBoss : BossEnemyBase
    {
        protected override void UpdateBoss(Transform playerTarget)
        {
            // Intentionally stationary. Add timed boss attacks here or derive a
            // specialized boss such as FirePhoenixBoss from BossEnemyBase.
        }
    }
}

using UnityEngine;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.Enemies
{
    public static class EnemyPool
    {
        public static T Spawn<T>(T enemyPrefab, Vector3 position, Quaternion rotation)
            where T : EnemyBase
        {
            return SceneObjectPool.Spawn(
                enemyPrefab, position, rotation, PoolCategory.Enemies);
        }

        public static EnemyBase Spawn(
            EnemyBase enemyPrefab, Vector3 position, Quaternion rotation)
        {
            return SceneObjectPool.Spawn(
                enemyPrefab, position, rotation, PoolCategory.Enemies);
        }
    }
}

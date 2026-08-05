using UnityEngine;
using WorldOfSpirits.Core;

namespace WorldOfSpirits.Combat
{
    public static class ProjectilePool
    {
        public static ProjectileBase Spawn(ProjectileBase prefab, Vector3 position, Quaternion rotation)
        {
            ProjectileBase projectile = SceneObjectPool.Spawn(
                prefab, position, rotation, PoolCategory.Projectiles);
            projectile.AssignPool(prefab);
            return projectile;
        }

        public static void Release(ProjectileBase projectile, ProjectileBase prefab)
        {
            SceneObjectPool.ReleaseOrDestroy(projectile.gameObject);
        }
    }
}

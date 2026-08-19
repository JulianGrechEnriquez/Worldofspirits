using UnityEngine;
using UnityEngine.Tilemaps;
using WorldOfSpirits.Crowd;

namespace WorldOfSpirits.Environment
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class LavaPoolTilemapBuilder : MonoBehaviour
    {
        [SerializeField] private TileBase lavaTile;

        private static readonly Vector2Int[] PoolCenters =
        {
            new Vector2Int(1, -10),
            new Vector2Int(12, -3),
            new Vector2Int(25, -11),
            new Vector2Int(31, 0)
        };

        private static readonly Vector2Int[] PoolRadii =
        {
            new Vector2Int(2, 1),
            new Vector2Int(2, 2),
            new Vector2Int(3, 1),
            new Vector2Int(1, 2)
        };

        private void OnEnable()
        {
            BuildPools();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UnityEditor.EditorApplication.delayCall += BuildPools;
            }
        }
#endif

        [ContextMenu("Rebuild Lava Pools")]
        public void BuildPools()
        {
            if (this == null || lavaTile == null)
            {
                return;
            }

            Transform child = transform.Find("Lava Pools");
            GameObject poolObject;
            if (child == null)
            {
                poolObject = new GameObject("Lava Pools");
                poolObject.transform.SetParent(transform, false);
            }
            else
            {
                poolObject = child.gameObject;
            }

            int lavaLayer = LayerMask.NameToLayer("Lava");
            poolObject.layer = lavaLayer >= 0 ? lavaLayer : gameObject.layer;

            Tilemap tilemap = poolObject.GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = poolObject.AddComponent<Tilemap>();
            }

            TilemapRenderer renderer = poolObject.GetComponent<TilemapRenderer>();
            if (renderer == null)
            {
                renderer = poolObject.AddComponent<TilemapRenderer>();
            }
            renderer.sortingOrder = -90;

            TilemapCollider2D collider = poolObject.GetComponent<TilemapCollider2D>();
            if (collider == null)
            {
                collider = poolObject.AddComponent<TilemapCollider2D>();
            }
            collider.isTrigger = false;

            tilemap.ClearAllTiles();
            for (int pool = 0; pool < PoolCenters.Length; pool++)
            {
                PaintPool(tilemap, PoolCenters[pool], PoolRadii[pool]);
            }
            tilemap.CompressBounds();

            EnemyCrowdAgent[] agents =
                FindObjectsByType<EnemyCrowdAgent>(FindObjectsSortMode.None);
            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i] != null && agents[i].IsFlying)
                {
                    agents[i].RefreshTerrainCollision();
                }
            }
        }

        private void PaintPool(Tilemap tilemap, Vector2Int center, Vector2Int radius)
        {
            for (int y = -radius.y; y <= radius.y; y++)
            {
                for (int x = -radius.x; x <= radius.x; x++)
                {
                    float normalizedX = radius.x == 0 ? 0f : x / (float)radius.x;
                    float normalizedY = radius.y == 0 ? 0f : y / (float)radius.y;
                    if (normalizedX * normalizedX + normalizedY * normalizedY <= 1.15f)
                    {
                        tilemap.SetTile(
                            new Vector3Int(center.x + x, center.y + y, 0),
                            lavaTile);
                    }
                }
            }
        }
    }
}

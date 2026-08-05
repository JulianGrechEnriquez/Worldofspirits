using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.Crowd
{
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class CrowdSimulationManager : MonoBehaviour
    {
        private static readonly ProfilerMarker GridMarker =
            new ProfilerMarker("WorldOfSpirits.Crowd.GridUpdate");
        private static readonly ProfilerMarker SteeringMarker =
            new ProfilerMarker("WorldOfSpirits.Crowd.Steering");
        private static readonly ProfilerMarker MovementMarker =
            new ProfilerMarker("WorldOfSpirits.Crowd.Movement");

        [Header("Shared References")]
        [SerializeField] private Transform player;
        [SerializeField] private PlayerCharacter playerCharacter;

        [Header("Spatial Hash")]
        [SerializeField, Min(0.25f)] private float cellSize = 2f;
        [SerializeField, Min(16)] private int expectedEnemyCapacity = 1000;

        [Header("Staggering")]
        [Tooltip("Maximum agents that refresh expensive steering per physics step. Movement remains smooth for every agent.")]
        [SerializeField, Min(1)] private int steeringRefreshesPerStep = 160;

        [Header("Physics Layers")]
        [SerializeField, Range(0, 31)] private int enemyLayer = 6;
        [SerializeField, Range(0, 31)] private int playerLayer = 7;
        [SerializeField] private LayerMask obstacleLayers;

        [Header("Debug")]
        [SerializeField] private bool drawOccupiedCells;

        private readonly List<EnemyCrowdAgent> agents = new List<EnemyCrowdAgent>(1000);
        private readonly Dictionary<EnemyCrowdAgent, int> indices =
            new Dictionary<EnemyCrowdAgent, int>(1000);
        private readonly Dictionary<int, EnemyCrowdAgent> agentsByHandle =
            new Dictionary<int, EnemyCrowdAgent>(1000);
        private readonly List<int> sharedQueryResults = new List<int>(64);
        private readonly List<long> debugCellKeys = new List<long>(128);

        private SpatialHashGrid2D grid;
        private Camera gameplayCamera;
        private Collider2D playerCollider;
        private int nextHandle = 1;
        private int nextSteeringIndex;
        private int physicsStep;

        public Transform Player => player;
        public PlayerCharacter PlayerCharacter => playerCharacter;
        public Collider2D PlayerCollider => playerCollider;
        public float PlayerCollisionRadius => playerCollider != null
            ? Mathf.Max(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y)
            : 0.35f;
        public int AgentCount => agents.Count;
        public int ActiveCellCount => grid != null ? grid.ActiveCellCount : 0;

        private void Awake()
        {
            grid = new SpatialHashGrid2D(cellSize, expectedEnemyCapacity);
            gameplayCamera = Camera.main;
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);

            if (playerCharacter == null && player != null)
            {
                playerCharacter = player.GetComponent<PlayerCharacter>();
            }

            if (playerCharacter != null)
            {
                playerCollider = playerCharacter.GetComponent<Collider2D>();
            }
        }

        private void FixedUpdate()
        {
            if (player == null || agents.Count == 0)
            {
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            Vector2 playerPosition = player.position;
            physicsStep++;

            using (GridMarker.Auto())
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    EnemyCrowdAgent agent = agents[i];
                    if (agent != null && agent.IsSimulationActive)
                    {
                        grid.UpdatePosition(agent.Handle, agent.Position);
                    }
                }
            }

            using (SteeringMarker.Auto())
            {
                int refreshCount = Mathf.Min(steeringRefreshesPerStep, agents.Count);
                for (int i = 0; i < refreshCount; i++)
                {
                    if (nextSteeringIndex >= agents.Count)
                    {
                        nextSteeringIndex = 0;
                    }

                    EnemyCrowdAgent agent = agents[nextSteeringIndex++];
                    if (agent != null && agent.IsSimulationActive)
                    {
                        agent.RefreshSteering(this, playerPosition);
                    }
                }
            }

            using (MovementMarker.Auto())
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    EnemyCrowdAgent agent = agents[i];
                    if (agent != null && agent.IsSimulationActive)
                    {
                        agent.SimulateMovement(this, playerPosition, deltaTime, physicsStep);
                    }
                }
            }
        }

        public bool Register(EnemyCrowdAgent agent)
        {
            if (agent == null || indices.ContainsKey(agent))
            {
                return false;
            }

            int handle = nextHandle++;
            if (nextHandle == int.MaxValue)
            {
                nextHandle = 1;
            }

            agent.SetSimulationRegistration(this, handle);
            indices.Add(agent, agents.Count);
            agents.Add(agent);
            agentsByHandle.Add(handle, agent);
            grid.Add(handle, agent.Position);
            return true;
        }

        public void Unregister(EnemyCrowdAgent agent)
        {
            if (agent == null || !indices.TryGetValue(agent, out int removedIndex))
            {
                return;
            }

            int lastIndex = agents.Count - 1;
            EnemyCrowdAgent lastAgent = agents[lastIndex];
            agents[removedIndex] = lastAgent;
            agents.RemoveAt(lastIndex);
            indices.Remove(agent);
            agentsByHandle.Remove(agent.Handle);
            grid.Remove(agent.Handle);
            agent.ClearSimulationRegistration();

            if (removedIndex < agents.Count)
            {
                indices[lastAgent] = removedIndex;
            }

            if (nextSteeringIndex > agents.Count)
            {
                nextSteeringIndex = 0;
            }
        }

        public List<int> QueryNeighbours(Vector2 center, float radius)
        {
            sharedQueryResults.Clear();
            grid.QueryRadius(center, radius, sharedQueryResults);
            return sharedQueryResults;
        }

        public bool TryGetAgent(int handle, out EnemyCrowdAgent agent)
        {
            return agentsByHandle.TryGetValue(handle, out agent);
        }

        public bool HasAgentWithin(Vector2 center, float radius)
        {
            sharedQueryResults.Clear();
            return grid.QueryRadius(center, radius, sharedQueryResults) > 0;
        }

        public bool TryRepositionDistantAgent(
            EnemyCrowdAgent agent,
            Vector2 playerPosition,
            float radius)
        {
            if (agent == null)
            {
                return false;
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }

            // The handle provides a stable angle while the attempt offset
            // spreads retries around the complete ring.
            for (int attempt = 0; attempt < 8; attempt++)
            {
                float angle = (agent.Handle * 137.508f + attempt * 45f) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = playerPosition + direction * radius;

                if (IsVisible(candidate))
                {
                    continue;
                }

                if (obstacleLayers.value != 0 &&
                    Physics2D.OverlapCircle(candidate, agent.CollisionRadius, obstacleLayers) != null)
                {
                    continue;
                }

                agent.TeleportForSimulation(candidate);
                grid.UpdatePosition(agent.Handle, candidate);
                return true;
            }

            return false;
        }

        public Vector2 CalculateObstacleAvoidance(
            Vector2 origin,
            Vector2 direction,
            float radius,
            float distance,
            float alternativeAngle)
        {
            if (obstacleLayers.value == 0 || direction.sqrMagnitude < 0.001f)
            {
                return Vector2.zero;
            }

            RaycastHit2D hit = Physics2D.CircleCast(
                origin, radius, direction, distance, obstacleLayers);
            if (hit.collider == null)
            {
                return Vector2.zero;
            }

            Vector2 tangent = new Vector2(-hit.normal.y, hit.normal.x);
            if (Vector2.Dot(tangent, direction) < 0f)
            {
                tangent = -tangent;
            }

            Vector2 alternate = Rotate(direction, alternativeAngle);
            if (Vector2.Dot(alternate, tangent) < 0f)
            {
                alternate = Rotate(direction, -alternativeAngle);
            }

            return (tangent + alternate).normalized;
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos);
        }

        private bool IsVisible(Vector2 worldPosition)
        {
            if (gameplayCamera == null)
            {
                return false;
            }

            Vector3 viewport = gameplayCamera.WorldToViewportPoint(worldPosition);
            return viewport.z > 0f &&
                   viewport.x >= -0.05f && viewport.x <= 1.05f &&
                   viewport.y >= -0.05f && viewport.y <= 1.05f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            cellSize = Mathf.Max(0.25f, cellSize);
            expectedEnemyCapacity = Mathf.Max(16, expectedEnemyCapacity);
            steeringRefreshesPerStep = Mathf.Max(1, steeringRefreshesPerStep);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawOccupiedCells || grid == null)
            {
                return;
            }

            debugCellKeys.Clear();
            grid.CopyOccupiedCellKeys(debugCellKeys);
            Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.25f);
            for (int i = 0; i < debugCellKeys.Count; i++)
            {
                Vector2 center = grid.GetCellCenter(debugCellKeys[i]);
                Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0f));
            }
        }
#endif
    }
}

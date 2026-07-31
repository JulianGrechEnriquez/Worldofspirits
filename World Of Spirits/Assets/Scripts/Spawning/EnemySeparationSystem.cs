using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Spawning
{
    /// <summary>
    /// Applies lightweight local separation to pooled enemies.
    ///
    /// Put enemies on a physics layer that does not collide with itself, then
    /// use this system to keep the crowd visually separated. This avoids dense
    /// groups producing large numbers of enemy-to-enemy physics contacts.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemySpatialPartition))]
    public sealed class EnemySeparationSystem : MonoBehaviour
    {
        private readonly struct Agent
        {
            public Agent(EnemyBase enemy, Rigidbody2D body)
            {
                Enemy = enemy;
                Body = body;
            }

            public EnemyBase Enemy { get; }
            public Rigidbody2D Body { get; }
        }

        [Header("References")]
        [Tooltip("Spatial grid used to retrieve only nearby enemies.")]
        [SerializeField] private EnemySpatialPartition spatialPartition;

        [Header("Separation")]
        [Tooltip("Distance at which enemies begin steering away from one another.")]
        [SerializeField, Min(0.05f)] private float separationRadius = 0.75f;

        [Tooltip("Acceleration applied to overlapping enemies.")]
        [SerializeField, Min(0f)] private float separationStrength = 12f;

        [Tooltip("Maximum velocity change contributed by separation during one physics step.")]
        [SerializeField, Min(0f)] private float maximumVelocityChange = 1.5f;

        [Tooltip("Maximum nearby enemies considered for each agent. This caps work in extremely dense crowds.")]
        [SerializeField, Min(1)] private int maximumNeighbours = 8;

        [Header("Work Distribution")]
        [Tooltip("Maximum enemies processed per physics step. Use 0 to process every registered enemy.")]
        [SerializeField, Min(0)] private int agentsPerPhysicsStep = 80;

        private readonly List<Agent> agents = new List<Agent>(256);
        private readonly Dictionary<EnemyBase, int> agentIndices =
            new Dictionary<EnemyBase, int>(256);
        private readonly List<EnemyBase> nearbyEnemies = new List<EnemyBase>(32);

        private int nextAgentIndex;

        public int RegisteredCount => agents.Count;

        private void Awake()
        {
            if (spatialPartition == null)
            {
                spatialPartition = GetComponent<EnemySpatialPartition>();
            }
        }

        private void FixedUpdate()
        {
            int count = agents.Count;
            if (count == 0 || separationStrength <= 0f)
            {
                nextAgentIndex = 0;
                return;
            }

            int processedCount = agentsPerPhysicsStep <= 0
                ? count
                : Mathf.Min(agentsPerPhysicsStep, count);

            for (int i = 0; i < processedCount; i++)
            {
                if (nextAgentIndex >= agents.Count)
                {
                    nextAgentIndex = 0;
                }

                ApplySeparation(agents[nextAgentIndex++]);
            }
        }

        /// <summary>
        /// Registers one active pooled enemy and caches its Rigidbody2D.
        /// EnemyPool will call this after activating an instance.
        /// </summary>
        public bool Register(EnemyBase enemy)
        {
            if (enemy == null || agentIndices.ContainsKey(enemy))
            {
                return false;
            }

            if (!enemy.TryGetComponent(out Rigidbody2D body))
            {
                Debug.LogError(
                    $"Cannot register {enemy.name} for separation because it has no Rigidbody2D.",
                    enemy);
                return false;
            }

            agentIndices.Add(enemy, agents.Count);
            agents.Add(new Agent(enemy, body));
            spatialPartition.Register(enemy);
            return true;
        }

        /// <summary>
        /// Removes an enemy using a swap-back operation, avoiding list shifts.
        /// EnemyPool will call this before returning an instance to storage.
        /// </summary>
        public void Unregister(EnemyBase enemy)
        {
            if (enemy == null || !agentIndices.TryGetValue(enemy, out int removedIndex))
            {
                return;
            }

            int lastIndex = agents.Count - 1;
            Agent lastAgent = agents[lastIndex];
            agents[removedIndex] = lastAgent;
            agents.RemoveAt(lastIndex);
            agentIndices.Remove(enemy);

            if (removedIndex < agents.Count)
            {
                agentIndices[lastAgent.Enemy] = removedIndex;
            }

            spatialPartition.Unregister(enemy);

            if (nextAgentIndex > agents.Count)
            {
                nextAgentIndex = 0;
            }
        }

        private void ApplySeparation(Agent agent)
        {
            EnemyBase enemy = agent.Enemy;
            Rigidbody2D body = agent.Body;
            if (enemy == null || body == null || !enemy.isActiveAndEnabled || !enemy.IsAlive)
            {
                return;
            }

            Vector2 position = body.position;
            nearbyEnemies.Clear();
            spatialPartition.QueryRadius(position, separationRadius, nearbyEnemies);

            Vector2 separation = Vector2.zero;
            int neighboursUsed = 0;
            float radiusSquared = separationRadius * separationRadius;

            for (int i = 0; i < nearbyEnemies.Count && neighboursUsed < maximumNeighbours; i++)
            {
                EnemyBase neighbour = nearbyEnemies[i];
                if (neighbour == null || neighbour == enemy)
                {
                    continue;
                }

                Vector2 offset = position - (Vector2)neighbour.transform.position;
                float distanceSquared = offset.sqrMagnitude;
                if (distanceSquared >= radiusSquared)
                {
                    continue;
                }

                if (distanceSquared < 0.0001f)
                {
                    // Stable fallback direction for enemies occupying the exact
                    // same point; this avoids Random calls and zero vectors.
                    float sign = enemy.GetInstanceID() < neighbour.GetInstanceID() ? -1f : 1f;
                    offset = new Vector2(sign, 0f);
                    distanceSquared = 1f;
                }

                float distance = Mathf.Sqrt(distanceSquared);
                float falloff = 1f - (distance / separationRadius);
                separation += (offset / distance) * falloff;
                neighboursUsed++;
            }

            if (neighboursUsed == 0 || separation.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector2 velocityChange = separation * (separationStrength * Time.fixedDeltaTime);
            velocityChange = Vector2.ClampMagnitude(velocityChange, maximumVelocityChange);
            body.linearVelocity += velocityChange;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            separationRadius = Mathf.Max(0.05f, separationRadius);
            separationStrength = Mathf.Max(0f, separationStrength);
            maximumVelocityChange = Mathf.Max(0f, maximumVelocityChange);
            maximumNeighbours = Mathf.Max(1, maximumNeighbours);
            agentsPerPhysicsStep = Mathf.Max(0, agentsPerPhysicsStep);
        }
#endif
    }
}

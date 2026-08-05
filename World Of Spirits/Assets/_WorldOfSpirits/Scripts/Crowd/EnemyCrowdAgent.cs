using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Enemies;

namespace WorldOfSpirits.Crowd
{
    public enum EnemyCollisionLod
    {
        Near,
        Medium,
        Far,
        Dormant
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyBase))]
    public sealed class EnemyCrowdAgent : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private EnemyMovementProfile profile;

        [Header("Debug")]
        [SerializeField] private bool drawDebug;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private Animator[] animators;
        private EnemyBase enemy;
        private CrowdSimulationManager manager;
        private Vector2 cachedSeek;
        private Vector2 cachedSeparation;
        private Vector2 cachedAvoidance;
        private Vector2 finalSteering;
        private float nextNeighbourTime;
        private float nextObstacleTime;
        private float nextAttackTime;
        private int handle;
        private EnemyCollisionLod lod;

        public int Handle => handle;
        public Vector2 Position => body != null ? body.position : (Vector2)transform.position;
        public float CollisionRadius => profile != null ? profile.CollisionRadius : 0.4f;
        public float Weight => profile != null ? profile.Weight : 1f;
        public float PushStrength => profile != null ? profile.PushStrength : 1f;
        public bool IsSimulationActive =>
            isActiveAndEnabled && enemy != null && enemy.IsAlive && profile != null;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            enemy = GetComponent<EnemyBase>();
            animators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].cullingMode = AnimatorCullingMode.CullCompletely;
            }

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;

            // CrowdSimulationManager now owns movement while EnemyBase remains
            // enabled for health, status effects, and pooling callbacks.
            enemy.SetExternalMovement(true);
        }

        private void OnDisable()
        {
            if (manager != null)
            {
                manager.Unregister(this);
            }

            ResetRuntimeState();
        }

        public void SetSimulationRegistration(CrowdSimulationManager owner, int newHandle)
        {
            manager = owner;
            handle = newHandle;
            nextNeighbourTime = Time.time;
            nextObstacleTime = Time.time;
        }

        public void ClearSimulationRegistration()
        {
            manager = null;
            handle = 0;
        }

        public void ResetRuntimeState()
        {
            cachedSeek = Vector2.zero;
            cachedSeparation = Vector2.zero;
            cachedAvoidance = Vector2.zero;
            finalSteering = Vector2.zero;
            nextAttackTime = 0f;
            nextNeighbourTime = 0f;
            nextObstacleTime = 0f;
            lod = EnemyCollisionLod.Near;
        }

        public void RefreshSteering(CrowdSimulationManager simulation, Vector2 playerPosition)
        {
            Vector2 position = body.position;
            Vector2 toPlayer = playerPosition - position;
            float distanceSquared = toPlayer.sqrMagnitude;
            lod = ResolveLod(distanceSquared);
            SetAnimationActive(lod != EnemyCollisionLod.Dormant);
            cachedSeek = distanceSquared > 0.0001f ? toPlayer.normalized : Vector2.zero;

            float neighbourInterval = GetNeighbourInterval();
            if (Time.time >= nextNeighbourTime)
            {
                cachedSeparation = lod == EnemyCollisionLod.Far ||
                                   lod == EnemyCollisionLod.Dormant
                    ? Vector2.zero
                    : CalculateSeparation(simulation, position);
                nextNeighbourTime = Time.time + neighbourInterval;
            }

            if (lod == EnemyCollisionLod.Near && Time.time >= nextObstacleTime)
            {
                cachedAvoidance = simulation.CalculateObstacleAvoidance(
                    position,
                    cachedSeek,
                    profile.CollisionRadius,
                    profile.ObstacleCheckDistance,
                    profile.AlternativeDirectionAngle);
                nextObstacleTime = Time.time + profile.ObstacleInterval;
            }
            else if (lod != EnemyCollisionLod.Near)
            {
                cachedAvoidance = Vector2.zero;
            }

            finalSteering =
                cachedSeek * profile.SeekWeight +
                cachedSeparation * profile.SeparationWeight +
                cachedAvoidance * profile.AvoidanceWeight;
            finalSteering = Vector2.ClampMagnitude(
                finalSteering,
                profile.MaximumSteeringForce);

            // Separation may slow forward motion but should not reverse an
            // enemy away from its target.
            if (Vector2.Dot(finalSteering, cachedSeek) < 0.05f)
            {
                finalSteering += cachedSeek * 0.25f;
            }
        }

        public void SimulateMovement(
            CrowdSimulationManager simulation,
            Vector2 playerPosition,
            float deltaTime,
            int physicsStep)
        {
            if (lod == EnemyCollisionLod.Dormant)
            {
                SimulateDormantMovement(
                    simulation,
                    playerPosition,
                    deltaTime,
                    physicsStep);
                return;
            }

            Vector2 position = body.position;
            Vector2 toPlayer = playerPosition - position;
            float distance = toPlayer.magnitude;
            float stopDistance =
                profile.CollisionRadius + simulation.PlayerCollisionRadius;

            Vector2 desiredVelocity;
            if (distance <= stopDistance)
            {
                // Retain separation and a subtle deterministic tangent so a
                // dense crowd surrounds rather than stacks on the player.
                float side = (handle & 1) == 0 ? 1f : -1f;
                Vector2 tangent = distance > 0.001f
                    ? new Vector2(-toPlayer.y, toPlayer.x).normalized * side
                    : Vector2.right * side;
                desiredVelocity =
                    (cachedSeparation + tangent * 0.12f) * profile.MovementSpeed;
            }
            else
            {
                desiredVelocity = finalSteering.normalized * profile.MovementSpeed;
            }

            Vector2 velocity = Vector2.MoveTowards(
                body.linearVelocity,
                desiredVelocity,
                profile.Acceleration * deltaTime);
            body.linearVelocity = velocity;
            body.MovePosition(position + velocity * deltaTime);

            TryDamagePlayer(simulation, playerPosition);
        }

        public void TeleportForSimulation(Vector2 position)
        {
            body.position = position;
            body.linearVelocity = Vector2.zero;
            cachedSeek = Vector2.zero;
            cachedSeparation = Vector2.zero;
            cachedAvoidance = Vector2.zero;
            finalSteering = Vector2.zero;
        }

        private void SimulateDormantMovement(
            CrowdSimulationManager simulation,
            Vector2 playerPosition,
            float deltaTime,
            int physicsStep)
        {
            int interval = profile.DormantMovementStepInterval;
            if ((physicsStep + handle) % interval != 0)
            {
                return;
            }

            Vector2 position = body.position;
            Vector2 toPlayer = playerPosition - position;
            float repositionDistance = profile.RepositionDistance;
            if (toPlayer.sqrMagnitude > repositionDistance * repositionDistance &&
                simulation.TryRepositionDistantAgent(
                    this,
                    playerPosition,
                    profile.RepositionRadius))
            {
                return;
            }

            if (toPlayer.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            // Compensate for skipped physics steps so average travel speed is
            // stable while far-away work is distributed across frames.
            float simulatedDeltaTime = deltaTime * interval;
            float speed = profile.MovementSpeed * profile.DormantSpeedMultiplier;
            Vector2 velocity = toPlayer.normalized * speed;
            body.linearVelocity = velocity;
            body.MovePosition(position + velocity * simulatedDeltaTime);
        }

        private Vector2 CalculateSeparation(
            CrowdSimulationManager simulation,
            Vector2 position)
        {
            List<int> neighbours = simulation.QueryNeighbours(
                position,
                profile.SeparationRadius + profile.CollisionRadius);
            Vector2 force = Vector2.zero;
            int used = 0;

            for (int i = 0; i < neighbours.Count && used < profile.MaximumNeighbours; i++)
            {
                int neighbourHandle = neighbours[i];
                if (neighbourHandle == handle ||
                    !simulation.TryGetAgent(neighbourHandle, out EnemyCrowdAgent other) ||
                    !other.IsSimulationActive)
                {
                    continue;
                }

                Vector2 offset = position - other.Position;
                // Start steering before colliders touch. Using only the two
                // collision radii allowed fast enemies to occupy almost the
                // same position before any separation force was applied.
                float collisionDistance =
                    profile.CollisionRadius + other.CollisionRadius;
                float desiredDistance = Mathf.Max(
                    collisionDistance,
                    profile.SeparationRadius,
                    other.profile.SeparationRadius);
                float distanceSquared = offset.sqrMagnitude;
                if (distanceSquared >= desiredDistance * desiredDistance)
                {
                    continue;
                }

                Vector2 separationDirection;
                float distance;
                if (distanceSquared < 0.0001f)
                {
                    float angle = (handle * 137.508f) * Mathf.Deg2Rad;
                    separationDirection =
                        new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    distance = 0f;
                }
                else
                {
                    distance = Mathf.Sqrt(distanceSquared);
                    separationDirection = offset / distance;
                }

                float overlap = 1f - Mathf.Clamp01(distance / desiredDistance);
                float priority = other.PushStrength /
                    Mathf.Max(0.01f, profile.Weight + other.Weight);
                force += separationDirection *
                    overlap * priority * profile.SeparationStrength;
                used++;
            }

            return Vector2.ClampMagnitude(force, profile.MaximumSteeringForce);
        }

        private void TryDamagePlayer(
            CrowdSimulationManager simulation,
            Vector2 playerPosition)
        {
            if (simulation.PlayerCharacter == null || Time.time < nextAttackTime)
            {
                return;
            }

            Collider2D playerCollider = simulation.PlayerCollider;
            if (bodyCollider == null || playerCollider == null)
            {
                return;
            }

            ColliderDistance2D colliderDistance = bodyCollider.Distance(playerCollider);
            const float contactTolerance = 0.01f;
            if (!colliderDistance.isOverlapped && colliderDistance.distance > contactTolerance)
            {
                return;
            }

            Vector2 direction = playerPosition - body.position;
            simulation.PlayerCharacter.TryTakeContactDamage(
                profile.ContactDamage,
                direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.zero,
                profile.Knockback);
            nextAttackTime = Time.time + profile.AttackCooldown;
        }

        private EnemyCollisionLod ResolveLod(float distanceSquared)
        {
            if (distanceSquared <= profile.NearDistance * profile.NearDistance)
                return EnemyCollisionLod.Near;
            if (distanceSquared <= profile.MediumDistance * profile.MediumDistance)
                return EnemyCollisionLod.Medium;
            if (distanceSquared <= profile.FarDistance * profile.FarDistance)
                return EnemyCollisionLod.Far;
            return EnemyCollisionLod.Dormant;
        }

        private float GetNeighbourInterval()
        {
            switch (lod)
            {
                case EnemyCollisionLod.Near: return profile.NearNeighbourInterval;
                case EnemyCollisionLod.Medium: return profile.MediumNeighbourInterval;
                default: return profile.FarNeighbourInterval;
            }
        }

        private void SetAnimationActive(bool active)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator != null && animator.enabled != active)
                {
                    animator.enabled = active;
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!drawDebug || profile == null)
            {
                return;
            }

            Vector3 position = transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, profile.CollisionRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, profile.SeparationRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(position, position + (Vector3)cachedSeek);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(position, position + (Vector3)cachedSeparation);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(position, position + (Vector3)cachedAvoidance);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(position, position + (Vector3)finalSteering);
        }
#endif
    }
}

using UnityEngine;

namespace WorldOfSpirits.Crowd
{
    [CreateAssetMenu(
        fileName = "Enemy Movement Profile",
        menuName = "World of Spirits/Crowd/Enemy Movement Profile")]
    public sealed class EnemyMovementProfile : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float movementSpeed = 4f;
        [SerializeField, Min(0.1f)] private float acceleration = 18f;
        [SerializeField, Min(0f)] private float stoppingDistance = 0.8f;

        [Header("Size and Separation")]
        [SerializeField, Min(0.05f)] private float collisionRadius = 0.4f;
        [SerializeField, Min(0.05f)] private float separationRadius = 0.9f;
        [SerializeField, Min(0f)] private float separationStrength = 1.5f;
        [SerializeField, Min(0.01f)] private float weight = 1f;
        [SerializeField, Min(0f)] private float pushStrength = 1f;
        [SerializeField, Min(1)] private int maximumNeighbours = 6;

        [Header("Steering Weights")]
        [SerializeField, Min(0f)] private float seekWeight = 1f;
        [SerializeField, Min(0f)] private float separationWeight = 1.25f;
        [SerializeField, Min(0f)] private float avoidanceWeight = 1.5f;
        [SerializeField, Min(0f)] private float maximumSteeringForce = 2.5f;

        [Header("Obstacle Avoidance")]
        [SerializeField, Min(0f)] private float obstacleCheckDistance = 1.25f;
        [SerializeField, Range(5f, 85f)] private float alternativeDirectionAngle = 45f;

        [Header("Contact Damage")]
        [SerializeField, Min(0f)] private float contactDamage = 10f;
        [SerializeField, Min(0.05f)] private float attackCooldown = 0.75f;
        [SerializeField, Min(0f)] private float attackRange = 0.15f;
        [SerializeField, Min(0f)] private float knockback = 0f;

        [Header("Update Rates")]
        [SerializeField, Min(0.01f)] private float nearNeighbourInterval = 0.1f;
        [SerializeField, Min(0.01f)] private float mediumNeighbourInterval = 0.25f;
        [SerializeField, Min(0.01f)] private float farNeighbourInterval = 0.35f;
        [SerializeField, Min(0.01f)] private float obstacleInterval = 0.2f;

        [Header("Collision LOD Distances")]
        [SerializeField, Min(0f)] private float nearDistance = 8f;
        [SerializeField, Min(0f)] private float mediumDistance = 18f;
        [SerializeField, Min(0f)] private float farDistance = 35f;

        [Header("Very Far Enemies")]
        [Tooltip("Dormant enemies still chase at this percentage of normal speed.")]
        [SerializeField, Range(0.1f, 1f)] private float dormantSpeedMultiplier = 0.75f;
        [Tooltip("Dormant movement is processed once every this many physics steps.")]
        [SerializeField, Range(1, 12)] private int dormantMovementStepInterval = 4;
        [Tooltip("Enemies farther than this may be safely repositioned outside the camera.")]
        [SerializeField, Min(10f)] private float repositionDistance = 70f;
        [Tooltip("Distance from the player used when repositioning an extremely distant enemy.")]
        [SerializeField, Min(5f)] private float repositionRadius = 18f;

        public float MovementSpeed => movementSpeed;
        public float Acceleration => acceleration;
        public float StoppingDistance => stoppingDistance;
        public float CollisionRadius => collisionRadius;
        public float SeparationRadius => separationRadius;
        public float SeparationStrength => separationStrength;
        public float Weight => weight;
        public float PushStrength => pushStrength;
        public int MaximumNeighbours => maximumNeighbours;
        public float SeekWeight => seekWeight;
        public float SeparationWeight => separationWeight;
        public float AvoidanceWeight => avoidanceWeight;
        public float MaximumSteeringForce => maximumSteeringForce;
        public float ObstacleCheckDistance => obstacleCheckDistance;
        public float AlternativeDirectionAngle => alternativeDirectionAngle;
        public float ContactDamage => contactDamage;
        public float AttackCooldown => attackCooldown;
        public float AttackRange => attackRange;
        public float Knockback => knockback;
        public float NearNeighbourInterval => nearNeighbourInterval;
        public float MediumNeighbourInterval => mediumNeighbourInterval;
        public float FarNeighbourInterval => farNeighbourInterval;
        public float ObstacleInterval => obstacleInterval;
        public float NearDistance => nearDistance;
        public float MediumDistance => mediumDistance;
        public float FarDistance => farDistance;
        public float DormantSpeedMultiplier => dormantSpeedMultiplier;
        public int DormantMovementStepInterval => dormantMovementStepInterval;
        public float RepositionDistance => repositionDistance;
        public float RepositionRadius => repositionRadius;

#if UNITY_EDITOR
        private void OnValidate()
        {
            movementSpeed = Mathf.Max(0f, movementSpeed);
            acceleration = Mathf.Max(0.1f, acceleration);
            collisionRadius = Mathf.Max(0.05f, collisionRadius);
            separationRadius = Mathf.Max(collisionRadius, separationRadius);
            weight = Mathf.Max(0.01f, weight);
            maximumNeighbours = Mathf.Max(1, maximumNeighbours);
            mediumDistance = Mathf.Max(nearDistance, mediumDistance);
            farDistance = Mathf.Max(mediumDistance, farDistance);
            repositionDistance = Mathf.Max(farDistance + 5f, repositionDistance);
            repositionRadius = Mathf.Max(5f, repositionRadius);
        }
#endif
    }
}

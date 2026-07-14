using UnityEngine;
using UnityEngine.InputSystem;

namespace WorldOfSpirits.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float fallbackMoveSpeed = 5f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string moveXParameter = "MoveX";
        [SerializeField] private string moveYParameter = "MoveY";
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string lastMoveXParameter = "LastMoveX";
        [SerializeField] private string lastMoveYParameter = "LastMoveY";
        [SerializeField] private bool playDirectionalStates = true;

        private Rigidbody2D rb;
        private PlayerCharacter playerCharacter;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down;
        private bool hasMoveX;
        private bool hasMoveY;
        private bool hasSpeed;
        private bool hasLastMoveX;
        private bool hasLastMoveY;
        private string currentAnimationState;

        public bool IsMoving => moveInput.sqrMagnitude > 0.01f;
        public Vector2 MoveDirection => moveInput;
        public Vector2 LastMoveDirection => lastMoveDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerCharacter = GetComponent<PlayerCharacter>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            CacheAnimatorParameters();
        }

        private void Update()
        {
            ReadMovementInput();
            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            float speed = playerCharacter != null ? playerCharacter.MoveSpeed : fallbackMoveSpeed;
            rb.linearVelocity = moveInput * speed;
        }

        private void ReadMovementInput()
        {
            moveInput = Vector2.zero;

            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                moveInput.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                moveInput.y += 1f;
            }

            moveInput = Vector2.ClampMagnitude(moveInput, 1f);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = moveInput.normalized;
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null)
            {
                return;
            }

            if (hasMoveX)
            {
                animator.SetFloat(moveXParameter, moveInput.x);
            }

            if (hasMoveY)
            {
                animator.SetFloat(moveYParameter, moveInput.y);
            }

            if (hasSpeed)
            {
                animator.SetFloat(speedParameter, moveInput.sqrMagnitude);
            }

            if (hasLastMoveX)
            {
                animator.SetFloat(lastMoveXParameter, lastMoveDirection.x);
            }

            if (hasLastMoveY)
            {
                animator.SetFloat(lastMoveYParameter, lastMoveDirection.y);
            }

            if (playDirectionalStates)
            {
                PlayDirectionalState();
            }
        }

        private void PlayDirectionalState()
        {
            string stateName = GetDirectionalStateName();
            if (stateName == currentAnimationState)
            {
                return;
            }

            int stateHash = Animator.StringToHash(stateName);
            if (!animator.HasState(0, stateHash))
            {
                return;
            }

            animator.Play(stateHash);
            currentAnimationState = stateName;
        }

        private string GetDirectionalStateName()
        {
            Vector2 direction = moveInput.sqrMagnitude > 0.01f ? moveInput : lastMoveDirection;
            string movement = moveInput.sqrMagnitude > 0.01f ? "Run" : "Idle";

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return direction.x < 0f ? $"Player_{movement}_Left" : $"Player_{movement}_Right";
            }

            return direction.y > 0f ? $"Player_{movement}_Up" : $"Player_{movement}_Down";
        }

        private void CacheAnimatorParameters()
        {
            if (animator == null)
            {
                return;
            }

            hasMoveX = HasFloatParameter(moveXParameter);
            hasMoveY = HasFloatParameter(moveYParameter);
            hasSpeed = HasFloatParameter(speedParameter);
            hasLastMoveX = HasFloatParameter(lastMoveXParameter);
            hasLastMoveY = HasFloatParameter(lastMoveYParameter);
        }

        private bool HasFloatParameter(string parameterName)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Float)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

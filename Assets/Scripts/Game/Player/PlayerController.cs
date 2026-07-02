using MMORPG.Game.Player.States;
using MMORPG.Game.Player.Motor;
using UnityEngine;

namespace MMORPG.Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterControllerMotor))]
    [RequireComponent(typeof(PlayerAnimationDriver))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private int attackMouseButton = 0;

        [Header("Action Timing")]
        [SerializeField] private float attackDuration = 0.85f;

        private PlayerContext context;
        private PlayerIdleState idleState;
        private PlayerMoveState moveState;
        private PlayerJumpState jumpState;
        private PlayerAttackState attackState;
        private bool initialized;

        public PlayerInputSnapshot CurrentInput { get; private set; }

        public void Initialize(Transform followCamera)
        {
            CharacterController characterController = GetComponent<CharacterController>();
            CharacterControllerMotor motor = GetComponent<CharacterControllerMotor>();
            PlayerAnimationDriver animationDriver = GetComponent<PlayerAnimationDriver>();
            Animator animator = GetComponentInChildren<Animator>();

            motor.Initialize(characterController, followCamera);
            animationDriver.Initialize(animator);

            context = new PlayerContext(this, motor, animationDriver);
            idleState = new PlayerIdleState(context);
            moveState = new PlayerMoveState(context);
            jumpState = new PlayerJumpState(context);
            attackState = new PlayerAttackState(context, attackDuration);
            initialized = true;
            EnterIdleState();
        }

        private void Awake()
        {
            if (!initialized)
            {
                Initialize(UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform : null);
            }
        }

        private void Update()
        {
            CurrentInput = ReadInput();
            context?.StateMachine.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            context?.StateMachine.FixedTick(Time.fixedDeltaTime);
        }

        public void EnterIdleState()
        {
            context.StateMachine.ChangeState(idleState);
        }

        public void EnterMoveState()
        {
            context.StateMachine.ChangeState(moveState);
        }

        public void EnterJumpState()
        {
            context.StateMachine.ChangeState(jumpState);
        }

        public void EnterAttackState()
        {
            context.StateMachine.ChangeState(attackState);
        }

        public void EnterLocomotionByInput()
        {
            if (CurrentInput.HasMoveInput)
            {
                EnterMoveState();
            }
            else
            {
                EnterIdleState();
            }
        }

        private PlayerInputSnapshot ReadInput()
        {
            Vector2 move = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            bool runHeld = Input.GetKey(runKey);
            bool jumpPressed = Input.GetKeyDown(jumpKey);
            bool attackPressed = Input.GetMouseButtonDown(attackMouseButton);
            return new PlayerInputSnapshot(move, runHeld, jumpPressed, attackPressed);
        }
    }
}

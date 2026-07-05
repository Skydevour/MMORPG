using MMORPG.Game.Projectiles;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MMORPG.Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float jumpVelocity = 9f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 13f;
        [SerializeField] private float dashDuration = 0.16f;
        [SerializeField] private float dashCooldown = 0.35f;

        [Header("Shooting")]
        [SerializeField] private float shotCooldown = 0.16f;
        [SerializeField] private float muzzleOffsetX = 0.86f;
        [SerializeField] private float muzzleOffsetY = 0.9f;

        [Header("Ground Check")]
        [SerializeField] private float groundCastDistance = 0.06f;

        [Header("Stage Bounds")]
        [SerializeField] private float minStageX = -7.55f;
        [SerializeField] private float maxStageX = 7.55f;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private float moveInput;
        private float dashTimer;
        private float dashCooldownTimer;
        private float nextShotTime;
        private float originalGravity;
        private int facingDirection = 1;
        private bool jumpPressed;
        private bool dashPressed;
        private bool attackHeld;
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
        private ContactFilter2D groundFilter;
        private Transform visualRoot;

        public bool IsGrounded { get; private set; }
        public bool IsDashing { get; private set; }
        public bool IsShooting { get; private set; }
        public bool IsMoving => Mathf.Abs(moveInput) > 0.05f;
        public bool IsInvincible => IsDashing;
        public int FacingDirection => facingDirection;
        public Vector2 FootPosition => new Vector2(body.position.x, bodyCollider.bounds.min.y);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            originalGravity = body.gravityScale;
            groundFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundMask,
                useTriggers = false
            };
        }

        private void Update()
        {
            ReadInput();
            RefreshGrounded();
            TickDashTimers();
            HandleJump();
            HandleDashStart();
            HandleShooting();
            FaceMoveDirection();
        }

        private void FixedUpdate()
        {
            if (IsDashing)
            {
                body.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);
                ClampHorizontalPosition();
                return;
            }

            body.gravityScale = originalGravity;
            body.linearVelocity = new Vector2(moveInput * moveSpeed, body.linearVelocity.y);
            ClampHorizontalPosition();
        }

        private void ReadInput()
        {
            moveInput = 0f;
            jumpPressed = false;
            dashPressed = false;
            attackHeld = false;

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    moveInput -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    moveInput += 1f;
                }

                jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
                dashPressed = keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
                attackHeld = keyboard.jKey.isPressed || keyboard.zKey.isPressed;
            }

#else
            moveInput = Input.GetAxisRaw("Horizontal");
            jumpPressed = Input.GetButtonDown("Jump");
            dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            attackHeld = Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Z);
#endif

            moveInput = Mathf.Clamp(moveInput, -1f, 1f);
        }

        private void RefreshGrounded()
        {
            groundFilter.layerMask = groundMask;
            int hitCount = bodyCollider.Cast(Vector2.down, groundFilter, groundHits, groundCastDistance);
            IsGrounded = hitCount > 0;
        }

        private void TickDashTimers()
        {
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
            }

            if (!IsDashing)
            {
                return;
            }

            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                IsDashing = false;
                body.gravityScale = originalGravity;
            }
        }

        private void HandleJump()
        {
            if (!jumpPressed || !IsGrounded || IsDashing)
            {
                return;
            }

            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
            IsGrounded = false;
        }

        private void HandleDashStart()
        {
            if (!dashPressed || dashCooldownTimer > 0f)
            {
                return;
            }

            IsDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            body.gravityScale = 0f;
        }

        private void HandleShooting()
        {
            IsShooting = attackHeld;
            if (!attackHeld || Time.time < nextShotTime)
            {
                return;
            }

            nextShotTime = Time.time + shotCooldown;
            Vector3 muzzlePosition = (Vector3)FootPosition + new Vector3(facingDirection * muzzleOffsetX, muzzleOffsetY, 0f);
            PlayerProjectile.Spawn(muzzlePosition, facingDirection);
        }

        private void FaceMoveDirection()
        {
            if (moveInput > 0.05f)
            {
                facingDirection = 1;
            }
            else if (moveInput < -0.05f)
            {
                facingDirection = -1;
            }

            Transform flipTarget = visualRoot != null ? visualRoot : transform;
            Vector3 scale = flipTarget.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            flipTarget.localScale = scale;
        }

        private void ClampHorizontalPosition()
        {
            Vector2 position = body.position;
            float clampedX = Mathf.Clamp(position.x, minStageX, maxStageX);
            if (Mathf.Approximately(position.x, clampedX))
            {
                return;
            }

            body.position = new Vector2(clampedX, position.y);
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }

        public void SetVisualRoot(Transform target)
        {
            visualRoot = target;
        }
    }
}

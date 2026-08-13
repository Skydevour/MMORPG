using System;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Projectiles;
using MMORPG.Game.VFX;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MMORPG.Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerController2D : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float jumpVelocity = 10.5f;
        [SerializeField] private float secondJumpVelocity = 9.2f;
        [SerializeField] private int maxAirJumps = 1;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float groundAcceleration = 45f;
        [SerializeField] private float groundDeceleration = 55f;
        [SerializeField] private float airControl = 0.85f;
        [SerializeField] private float jumpCutMultiplier = 0.45f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 13f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 0.35f;
        [SerializeField] private float dodgeBurstYOffset = 0.18f;

        [Header("Shooting")]
        [SerializeField] private float shotCooldown = 0.16f;
        [SerializeField] private float muzzleOffsetX = 0.86f;
        [SerializeField] private float muzzleOffsetY = 0.28f;

        [Header("Second Jump Flip")]
        [SerializeField] private float airFlipDuration = 0.42f;

        [Header("Death Sequence")]
        [SerializeField] private float deathActionDuration = 0.38f;
        [SerializeField] private float ghostRiseDuration = 1.2f;
        [SerializeField] private float ghostRiseSpeed = 1.05f;

        [Header("Ground Check")]
        [SerializeField] private float groundCastDistance = 0.06f;

        [Header("Stage Bounds")]
        [SerializeField] private float minStageX = -7.55f;
        [SerializeField] private float maxStageX = 7.55f;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private PlayerEnergyController energy;
        private IDamageable superTarget;
        private float moveInput;
        private float dashTimer;
        private float dashCooldownTimer;
        private float nextShotTime;
        private float superTimer;
        private float originalGravity;
        private float deathSequenceTimer;
        private float damageInvincibilityTimer;
        private float hurtTimer;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private int facingDirection = 1;
        private int currentHealth;
        private bool isDead;
        private bool battleLocked;
        private bool jumpPressed;
        private bool jumpHeld;
        private bool dashPressed;
        private bool dodgePressed;
        private bool specialPressed;
        private bool attackHeld;
        private bool spawnDodgeBurstOnDashEnd;
        private int airJumpsRemaining;
        private bool airFlipping;
        private bool superEffectApplied;
        private float airFlipTimer;
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
        private ContactFilter2D groundFilter;
        private Transform visualRoot;
        private Quaternion visualBaseRotation = Quaternion.identity;
        private SpriteRenderer[] visualRenderers;
        private Color[] visualBaseColors;

        public bool IsGrounded { get; private set; }

        public bool IsDashing { get; private set; }

        public bool IsUsingSuper { get; private set; }

        public bool IsShooting { get; private set; }

        public bool IsMoving => Mathf.Abs(moveInput) > 0.05f;

        public bool IsInvincible => battleLocked || isDead || IsDashing || IsUsingSuper || damageInvincibilityTimer > 0f;

        public bool IsDead => isDead;

        public bool IsHurt => hurtTimer > 0f;

        public int MaxHealth { get; private set; }

        public int FacingDirection => facingDirection;

        public int CurrentHealth => currentHealth;

        public PlayerEnergyController Energy => energy;

        public Vector2 FootPosition => body == null ? (Vector2)transform.position : body.position;

        public event Action<int, int> HealthChanged;

        public event Action Died;

        private void Awake()
        {
            GameConfig config = GameConfigService.Current;
            moveSpeed = config.player.moveSpeed;
            jumpVelocity = config.player.jumpVelocity;
            secondJumpVelocity = config.player.secondJumpVelocity;
            maxAirJumps = config.player.maxAirJumps;
            coyoteTime = config.player.coyoteTime;
            jumpBufferTime = config.player.jumpBufferTime;
            deathActionDuration = config.player.deathActionDuration;
            ghostRiseDuration = config.player.ghostRiseDuration;
            ghostRiseSpeed = config.player.ghostRiseSpeed;
            groundAcceleration = config.player.groundAcceleration;
            groundDeceleration = config.player.groundDeceleration;
            airControl = config.player.airControl;
            jumpCutMultiplier = config.player.jumpCutMultiplier;
            dashSpeed = config.player.dashSpeed;
            dashDuration = config.player.dashDuration;
            dashCooldown = config.player.dashCooldown;
            dodgeBurstYOffset = config.player.dodgeBurstYOffset;
            shotCooldown = config.player.shotCooldown;
            muzzleOffsetX = config.player.muzzleOffsetX;
            muzzleOffsetY = config.player.muzzleOffsetY;
            airFlipDuration = config.player.airFlipDuration;
            groundCastDistance = config.player.groundCastDistance;
            minStageX = config.level.minStageX;
            maxStageX = config.level.maxStageX;

            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            CacheVisualRenderers();
            energy = GetComponent<PlayerEnergyController>();
            MaxHealth = config.player.maxHealth;
            currentHealth = MaxHealth;
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
            TickDamageTimers();

            if (isDead)
            {
                TickDeathSequence();
                return;
            }

            if (battleLocked || isDead)
            {
                moveInput = 0f;
                IsShooting = false;
                return;
            }

            ReadInput();
            RefreshGrounded();
            TickJumpTiming();
            TickDashTimers();
            TickSuper();

            if (IsUsingSuper)
            {
                FaceMoveDirection();
                UpdateAirFlip();
                return;
            }

            HandleJump();
            ApplyJumpCut();
            HandleDashStart();
            HandleSpecial();
            HandleShooting();
            FaceMoveDirection();
            UpdateAirFlip();
        }

        private void FixedUpdate()
        {
            if (battleLocked || isDead)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            if (IsDashing)
            {
                body.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);
                ClampHorizontalPosition();
                return;
            }

            if (IsUsingSuper)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            body.gravityScale = originalGravity;
            float targetVelocityX = moveInput * moveSpeed * (IsGrounded ? 1f : airControl);


            float nextVelocityX = Mathf.Abs(moveInput) > 0.05f
                ? targetVelocityX
                : Mathf.MoveTowards(body.linearVelocity.x, 0f, groundDeceleration * Time.fixedDeltaTime);
            body.linearVelocity = new Vector2(nextVelocityX, body.linearVelocity.y);
            ClampHorizontalPosition();
        }

        private void ReadInput()
        {
            moveInput = 0f;
            jumpPressed = false;
            jumpHeld = false;
            dashPressed = false;
            dodgePressed = false;
            specialPressed = false;
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
                jumpHeld = keyboard.spaceKey.isPressed;
                dashPressed = keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
                dodgePressed = keyboard.lKey.wasPressedThisFrame;
                specialPressed = keyboard.kKey.wasPressedThisFrame;
                attackHeld = keyboard.jKey.isPressed || keyboard.zKey.isPressed;
            }
#else
            moveInput = Input.GetAxisRaw("Horizontal");
            jumpPressed = Input.GetButtonDown("Jump");
            jumpHeld = Input.GetButton("Jump");
            dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            dodgePressed = Input.GetKeyDown(KeyCode.L);
            specialPressed = Input.GetKeyDown(KeyCode.K);
            attackHeld = Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Z);
#endif

            moveInput = Mathf.Clamp(moveInput, -1f, 1f);
        }

        private void RefreshGrounded()
        {
            groundFilter.layerMask = groundMask;
            int hitCount = bodyCollider.Cast(Vector2.down, groundFilter, groundHits, groundCastDistance);
            IsGrounded = hitCount > 0;
            if (IsGrounded && body.linearVelocity.y <= 0.05f)
            {
                airJumpsRemaining = maxAirJumps;
                StopAirFlip();
            }
        }

        private void TickJumpTiming()
        {
            coyoteTimer = IsGrounded
                ? coyoteTime
                : Mathf.Max(0f, coyoteTimer - Time.deltaTime);

            jumpBufferTimer = jumpPressed
                ? jumpBufferTime
                : Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
        }

        private void TickDamageTimers()
        {
            if (damageInvincibilityTimer > 0f)
            {
                damageInvincibilityTimer -= Time.deltaTime;
            }

            if (hurtTimer > 0f)
            {
                hurtTimer -= Time.deltaTime;
            }
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
            if (dashTimer > 0f)
            {
                return;
            }

            IsDashing = false;
            body.gravityScale = originalGravity;
            if (spawnDodgeBurstOnDashEnd)
            {
                SpawnDodgeBurst();
                spawnDodgeBurstOnDashEnd = false;
            }
        }

        private void TickSuper()
        {
            if (!IsUsingSuper)
            {
                return;
            }

            superTimer -= Time.deltaTime;
            if (superTimer <= 0f)
            {
                IsUsingSuper = false;
                body.gravityScale = originalGravity;
                superEffectApplied = false;
                Debug.Log("玩家大招演出结束。");
            }
        }

        private void HandleJump()
        {
            if (jumpBufferTimer <= 0f || IsDashing)
            {
                return;
            }

            jumpBufferTimer = 0f;

            if (IsGrounded || coyoteTimer > 0f)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
                IsGrounded = false;
                coyoteTimer = 0f;
                airJumpsRemaining = maxAirJumps;
                return;
            }

            if (airJumpsRemaining <= 0)
            {
                return;
            }

            airJumpsRemaining--;
            body.linearVelocity = new Vector2(body.linearVelocity.x, secondJumpVelocity);
            StartAirFlip();
        }

        private void ApplyJumpCut()
        {
            if (jumpHeld || IsDashing || IsUsingSuper || body.linearVelocity.y <= 0f)
            {
                return;
            }

            float minimumJumpVelocity = jumpVelocity * jumpCutMultiplier;
            if (body.linearVelocity.y > minimumJumpVelocity)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, minimumJumpVelocity);
            }
        }
        private void HandleDashStart()
        {
            bool wantsDodge = dashPressed || dodgePressed;
            if (!wantsDodge || dashCooldownTimer > 0f)
            {
                return;
            }

            IsDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            body.gravityScale = 0f;
            spawnDodgeBurstOnDashEnd = true;
            SpawnDodgeBurst();
        }

        private void HandleSpecial()
        {
            if (specialPressed)
            {
                TryActivateSuper();
            }
        }

        public bool TryActivateSuper()
        {
            if (battleLocked || isDead || IsDashing || IsUsingSuper || energy == null || !energy.TryConsumeAll())
            {
                return false;
            }

            IsUsingSuper = true;
            superTimer = GameConfigService.Current.special.duration;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            SpawnSuperEffect();

            if (!superEffectApplied && superTarget != null && !superTarget.IsDead)
            {
                superTarget.TakeDamage(GameConfigService.Current.special.damage);
                superEffectApplied = true;
            }

            Debug.Log($"玩家释放大招，消耗全部 {energy.MaxEnergy} 格能量。");
            return true;
        }

        private void HandleShooting()
        {
            IsShooting = attackHeld && !IsUsingSuper;
            if (!attackHeld || IsUsingSuper || Time.time < nextShotTime)
            {
                return;
            }

            nextShotTime = Time.time + GameConfigService.Current.player.shotCooldown;
            Vector3 muzzlePosition = (Vector3)FootPosition + new Vector3(
                facingDirection * GameConfigService.Current.player.muzzleOffsetX,
                GameConfigService.Current.player.muzzleOffsetY,
                0f);
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
            visualBaseRotation = visualRoot != null ? visualRoot.localRotation : Quaternion.identity;
        }

        public void SetSuperTarget(IDamageable target)
        {
            superTarget = target;
        }

        public void TakeDamage(int damage)
        {
            if (battleLocked || damage <= 0 || IsInvincible || currentHealth <= 0)
            {
                return;
            }

            GameConfig config = GameConfigService.Current;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            damageInvincibilityTimer = config.player.damageInvincibilityDuration;
            hurtTimer = config.player.hurtFlashDuration;
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            HitFlashEffect.Play(gameObject, config.player.hurtFlashDuration, 3, Color.white);
            CombatImpactEffect.SpawnHit((Vector3)FootPosition + Vector3.up * 0.5f, new Color(1f, 0.38f, 0.28f));
            ScreenShakeEffect.Shake(0.08f, 0.06f);
            Debug.Log($"玩家受到 {damage} 点伤害，剩余生命 {currentHealth}/{MaxHealth}。");

            if (currentHealth > 0)
            {
                return;
            }

            isDead = true;
            deathSequenceTimer = 0f;
            IsDashing = false;
            IsUsingSuper = false;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            bodyCollider.enabled = false;
            CombatImpactEffect.SpawnDeath((Vector3)FootPosition + Vector3.up * 0.55f, new Color(0.9f, 0.9f, 1f));
            Died?.Invoke();
            Debug.Log("玩家生命归零，战斗失败流程已触发。");
        }

        public void SetBattleLocked(bool locked)
        {
            battleLocked = locked;
            if (!locked)
            {
                return;
            }

            IsDashing = false;
            IsUsingSuper = false;
            body.gravityScale = originalGravity;
            body.linearVelocity = Vector2.zero;
        }

        private void TickDeathSequence()
        {
            deathSequenceTimer += Time.unscaledDeltaTime;
            if (visualRoot == null)
            {
                return;
            }

            if (deathSequenceTimer <= deathActionDuration)
            {
                float actionProgress = Mathf.Clamp01(deathSequenceTimer / deathActionDuration);
                visualRoot.localRotation = visualBaseRotation * Quaternion.Euler(0f, 0f, -180f * actionProgress * facingDirection);
                SetVisualAlpha(1f);
                return;
            }

            visualRoot.localRotation = visualBaseRotation;
            float ghostElapsed = deathSequenceTimer - deathActionDuration;
            float ghostProgress = Mathf.Clamp01(ghostElapsed / ghostRiseDuration);
            if (ghostProgress < 1f)
            {
                body.position += Vector2.up * ghostRiseSpeed * Time.unscaledDeltaTime;
                SetGhostAppearance(1f - ghostProgress);
            }
            else
            {
                SetGhostAppearance(0f);
            }
        }

        private void CacheVisualRenderers()
        {
            if (visualRenderers != null)
            {
                return;
            }

            visualRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            visualBaseColors = new Color[visualRenderers.Length];
            for (int index = 0; index < visualRenderers.Length; index++)
            {
                visualBaseColors[index] = visualRenderers[index].color;
            }
        }

        private void SetVisualAlpha(float alpha)
        {
            SetVisualAppearance(alpha, false);
        }

        private void SetGhostAppearance(float alpha)
        {
            SetVisualAppearance(alpha, true);
        }

        private void SetVisualAppearance(float alpha, bool ghost)
        {
            CacheVisualRenderers();
            for (int index = 0; index < visualRenderers.Length; index++)
            {
                Color color = visualBaseColors[index];
                if (ghost)
                {
                    color = Color.Lerp(color, new Color(0.72f, 0.9f, 1f, 1f), 0.72f);
                }

                color.a = Mathf.Clamp01(alpha);
                visualRenderers[index].color = color;
            }
        }

        private void StartAirFlip()
        {
            airFlipping = true;
            airFlipTimer = 0f;
        }

        private void StopAirFlip()
        {
            airFlipping = false;
            airFlipTimer = 0f;
            if (visualRoot != null)
            {
                visualRoot.localRotation = visualBaseRotation;
            }
        }

        private void UpdateAirFlip()
        {
            if (!airFlipping || visualRoot == null)
            {
                return;
            }

            airFlipTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(airFlipTimer / Mathf.Max(0.01f, airFlipDuration));
            float angle = -360f * progress * facingDirection;
            visualRoot.localRotation = visualBaseRotation * Quaternion.Euler(0f, 0f, angle);
            if (progress >= 1f)
            {
                StopAirFlip();
            }
        }

        private void SpawnDodgeBurst()
        {
            Vector3 smokePosition = (Vector3)FootPosition + new Vector3(0f, GameConfigService.Current.player.dodgeBurstYOffset, 0f);
            DodgeSmokeEffect.Spawn(smokePosition, facingDirection);
        }

        private void SpawnSuperEffect()
        {
            Vector3 effectPosition = (Vector3)FootPosition + new Vector3(
                facingDirection * 0.2f,
                GameConfigService.Current.special.effectYOffset,
                0f);
            SuperAttackEffect.Spawn(effectPosition, facingDirection);
        }
    }
}
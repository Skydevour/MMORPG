using System;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Projectiles;
using MMORPG.Game.VFX;
using MMORPG.Game.Audio;
using MMORPG.Framework.Timing;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MMORPG.Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerController2D : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float jumpVelocity = 10.5f;
        [SerializeField] private float secondJumpVelocity = 9.2f;
        [SerializeField] private int maxAirJumps = 1;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.12f;
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
        private float moveInput;
        private float dashTimer;
        private float dashCooldownTimer;
        private float nextShotTime;
        private float superTimer;
        private float originalGravity;
        private float deathSequenceTimer;
        private float damageInvincibilityTimer;
        private float hurtTimer;
        private float parryTimer;
        private float parryProtection;
        private float jumpLaunchVelocity;
        private float jumpCutAcceleration;
        private float jumpCutBlendDuration;
        private bool canCutJump;
        private bool cuttingJump;
        private bool bufferedJump, bufferedDash, bufferedSpecial;
        private bool suppressActions;
        public void SuppressActionInput()
        {
            suppressActions = true;
            bufferedJump = bufferedDash = bufferedSpecial = false;
            jumpBufferTimer = 0f;
        }
        public bool IsParrying => parryTimer > 0f;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private int facingDirection = 1;
        private int dashDirection = 1;
        private int flipDirection = 1;
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
        private float airFlipTimer;
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
        private ContactFilter2D groundFilter;
        private Transform visualRoot;
        private Transform visualMirror;
        private float uprightRemaining;
        private Quaternion uprightStart;
        private Quaternion visualBaseRotation = Quaternion.identity;
        private SpriteRenderer[] visualRenderers;
        private Color[] visualBaseColors;

        public bool IsGrounded { get; private set; }

        public bool IsDashing { get; private set; }

        public bool IsUsingSuper { get; private set; }

        public bool IsShooting { get; private set; }

        public bool IsMoving => Mathf.Abs(moveInput) > 0.05f;

        public bool IsInvincible => battleLocked || isDead || IsDashing || IsUsingSuper || damageInvincibilityTimer > 0f || parryProtection > 0f;

        public bool CanBreakPinkProjectile => !battleLocked && !isDead && !IsGrounded && !IsDashing && !IsUsingSuper;

        public bool IsDead => isDead;

        public bool IsHurt => hurtTimer > 0f;

        public int MaxHealth { get; private set; }

        public int FacingDirection => facingDirection;

        public int CurrentHealth => currentHealth;

        public PlayerEnergyController Energy => energy;

        public Vector2 FootPosition => body == null ? (Vector2)transform.position : body.position;
        public Vector3 MuzzlePosition => (Vector3)FootPosition + new Vector3(facingDirection * muzzleOffsetX, muzzleOffsetY, 0f);
        public float InvincibilityRemaining => Mathf.Max(0f, damageInvincibilityTimer);
        public bool IsGhost => isDead && deathSequenceTimer > deathActionDuration;
        public int JumpSequence { get; private set; }
        public bool IsAirFlipping => airFlipping;
        public float VerticalVelocity => body != null ? body.linearVelocity.y : 0f;

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
            airControl = config.player.airControl;
            jumpCutMultiplier = config.player.jumpCutMultiplier;
            jumpCutBlendDuration = config.player.jumpCutBlendDuration;
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
            if (Time.timeScale <= 0f)
            {
                if (BattleClock.HitStopped && !BattleClock.Paused && !battleLocked && !isDead)
                {
                    ReadInput(); bufferedJump |= jumpPressed; bufferedDash |= dashPressed || dodgePressed; bufferedSpecial |= specialPressed;
                }
                return;
            }
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
                UpdateAirFlip();
                return;
            }

            ReadInput();
            jumpPressed |= bufferedJump; dashPressed |= bufferedDash; specialPressed |= bufferedSpecial;
            bufferedJump = bufferedDash = bufferedSpecial = false;
            RefreshGrounded();
            TickJumpTiming();
            TickDashTimers();
            TickSuper();
            FaceMoveDirection();

            if (IsUsingSuper)
            {
                UpdateAirFlip();
                return;
            }

            HandleJump();
            RequestJumpCut();
            HandleDashStart();
            HandleSpecial();
            HandleShooting();
            UpdateAirFlip();
        }

        private void FixedUpdate()
        {
            if (battleLocked || isDead)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            if (IsDashing && dashTimer <= 0f) FinishDash();
            if (IsDashing)
            {
                float step = Mathf.Min(dashTimer, Time.fixedDeltaTime);
                body.linearVelocity = new Vector2(dashDirection * dashSpeed * step / Time.fixedDeltaTime, 0f);
                dashTimer = Mathf.Max(0f, dashTimer - step);
                ClampHorizontalPosition();
                return;
            }

            if (IsUsingSuper)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            body.gravityScale = originalGravity;
            ApplyJumpCut();
            float targetVelocityX = moveInput * moveSpeed * (IsGrounded ? 1f : airControl);


            body.linearVelocity = new Vector2(targetVelocityX, body.linearVelocity.y);
            ClampHorizontalPosition();
        }

        private void ReadInput()
        {
            bool actionHeld = false;
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
                actionHeld = jumpHeld || attackHeld || keyboard.lKey.isPressed || keyboard.kKey.isPressed || keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            }
#else
            moveInput = Input.GetAxisRaw("Horizontal");
            jumpPressed = Input.GetButtonDown("Jump");
            jumpHeld = Input.GetButton("Jump");
            dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            dodgePressed = Input.GetKeyDown(KeyCode.L);
            specialPressed = Input.GetKeyDown(KeyCode.K);
            attackHeld = Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Z);
            actionHeld = jumpHeld || attackHeld || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif

            if (suppressActions)
            {
                jumpPressed = dashPressed = dodgePressed = specialPressed = attackHeld = false;
                if (!actionHeld) suppressActions = false;
            }

            moveInput = Mathf.Clamp(moveInput, -1f, 1f);
        }

        private void RefreshGrounded()
        {
            groundFilter.layerMask = groundMask;
            int hitCount = bodyCollider.Cast(Vector2.down, groundFilter, groundHits, groundCastDistance);
            IsGrounded = false;
            for (int index = 0; index < hitCount && body.linearVelocity.y <= 0.05f; index++)
            {
                if (groundHits[index].normal.y > 0.65f) IsGrounded = true;
            }
            if (IsGrounded && body.linearVelocity.y <= 0.05f)
            {
                ClearJumpCut();
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
            parryTimer = Mathf.Max(0f, parryTimer - Time.deltaTime);
            parryProtection = Mathf.Max(0f, parryProtection - Time.deltaTime);
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

            if (dashTimer > 0f)
            {
                return;
            }

            FinishDash();
        }

        private void FinishDash()
        {
            if (!IsDashing) return;
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
                Debug.Log("玩家大招演出结束。");
            }
        }

        private void HandleJump()
        {
            if (jumpBufferTimer <= 0f || IsDashing)
            {
                return;
            }

            if (IsGrounded || coyoteTimer > 0f)
            {
                jumpBufferTimer = 0f;
                JumpSequence++;
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
                BeginJumpCut(jumpVelocity);
                IsGrounded = false;
                coyoteTimer = 0f;
                airJumpsRemaining = maxAirJumps;
                BattleAudio.Play("jump", transform.position);
                return;
            }

            if (airJumpsRemaining <= 0)
            {
                return;
            }

            airJumpsRemaining--;
            jumpBufferTimer = 0f;
            JumpSequence++;
            body.linearVelocity = new Vector2(body.linearVelocity.x, secondJumpVelocity);
            BeginJumpCut(secondJumpVelocity);
            StartAirFlip();
            BattleAudio.Play("double_jump", transform.position);
        }

        private void BeginJumpCut(float launchVelocity)
        {
            jumpLaunchVelocity = launchVelocity;
            canCutJump = true;
            cuttingJump = false;
        }

        private void ClearJumpCut()
        {
            canCutJump = cuttingJump = false;
            jumpCutAcceleration = 0f;
        }

        private void RequestJumpCut()
        {
            if (!canCutJump || cuttingJump || jumpHeld || IsParrying || IsDashing || IsUsingSuper) return;
            float target = jumpLaunchVelocity * jumpCutMultiplier;
            jumpCutAcceleration = Mathf.Max(0f, body.linearVelocity.y - target) / jumpCutBlendDuration;
            cuttingJump = jumpCutAcceleration > 0f;
            canCutJump = false;
        }

        private void ApplyJumpCut()
        {
            if (!cuttingJump) return;
            float target = jumpLaunchVelocity * jumpCutMultiplier;
            if (body.linearVelocity.y <= target) { cuttingJump = false; return; }
            // 在固定物理步内收束，额外减速只降低速度，不抵消自然重力。
            body.linearVelocity = new Vector2(body.linearVelocity.x,
                Mathf.MoveTowards(body.linearVelocity.y, target, jumpCutAcceleration * Time.fixedDeltaTime));
        }
        private void HandleDashStart()
        {
            bool wantsDodge = dashPressed || dodgePressed;
            if (!wantsDodge || dashCooldownTimer > 0f)
            {
                return;
            }

            IsDashing = true;
            ClearJumpCut();
            dashDirection = facingDirection;
            StopAirFlip();
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
            if (Time.timeScale <= 0f || battleLocked || isDead || IsDashing || IsUsingSuper || energy == null || !energy.TryConsumeAll())
            {
                return false;
            }

            IsUsingSuper = true;
            ClearJumpCut();
            StopAirFlip();
            superTimer = GameConfigService.Current.special.duration;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            SpawnSuperEffect();
            BattleStats.Active?.RecordSuperUse();

            Debug.Log($"玩家释放大招，消耗全部 {energy.MaxEnergy} 格能量。");
            return true;
        }

        private void HandleShooting()
        {
            IsShooting = attackHeld && !IsUsingSuper && !IsDashing;
            if (!IsShooting || Time.time < nextShotTime)
            {
                return;
            }

            nextShotTime = Time.time + shotCooldown;
            PlayerProjectile.Spawn(MuzzlePosition, facingDirection);
            BattleAudio.Play("shot", MuzzlePosition);
            PooledBattleEffect.Spawn(MuzzlePosition, new Color(1f, 0.95f, 0.6f), 0.12f, 2, 0.04f);
        }

        private void FaceMoveDirection()
        {
            if (IsDashing || IsUsingSuper) return;
            if (moveInput > 0.05f)
            {
                facingDirection = 1;
            }
            else if (moveInput < -0.05f)
            {
                facingDirection = -1;
            }

            Transform flipTarget = visualMirror != null ? visualMirror : visualRoot;
            if (flipTarget == null || airFlipping || uprightRemaining > 0f) return;
            Vector3 scale = flipTarget.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            flipTarget.localScale = scale;
        }

        private void ClampHorizontalPosition()
        {
            Vector2 position = body.position;
            float halfWidth = bodyCollider.bounds.extents.x;
            float centerOffset = bodyCollider.bounds.center.x - transform.position.x;
            float left = minStageX + halfWidth - centerOffset;
            float right = maxStageX - halfWidth - centerOffset;
            float clampedX = Mathf.Clamp(position.x, left, right);
            if (!Mathf.Approximately(position.x, clampedX))
            {
                position.x = clampedX;
                body.position = position;
            }
            // 限制本物理步的位移，避免持续顶住边界时越界、回拉交替造成抖动。
            float nextX = position.x + body.linearVelocity.x * Time.fixedDeltaTime;
            float allowedX = Mathf.Clamp(nextX, left, right);
            if (!Mathf.Approximately(nextX, allowedX))
                body.linearVelocity = new Vector2((allowedX - position.x) / Time.fixedDeltaTime, body.linearVelocity.y);
        }

        public void SetCombatRightEdge(float edge)
        {
            maxStageX = Mathf.Min(GameConfigService.Current.level.maxStageX, edge);
            ClampHorizontalPosition();
        }

        public float CombatRightEdge => maxStageX;

        public void SetVisualRoot(Transform target, Transform mirror = null)
        {
            visualRoot = target;
            visualMirror = mirror;
            visualBaseRotation = visualRoot != null ? visualRoot.localRotation : Quaternion.identity;
        }

        public bool TryBreakPinkProjectile(BossProjectile projectile)
        {
            if (projectile == null || !CanBreakPinkProjectile || !projectile.TryCollectByJump()) return false;
            var feedback = GameConfigService.Current.feedback;
            StopAirFlip();
            parryTimer = feedback.parryPose;
            ClearJumpCut();
            parryProtection = Mathf.Max(parryProtection, feedback.parryProtection);
            body.linearVelocity = new Vector2(body.linearVelocity.x, feedback.parryBounce);
            BattleClock.Stop(feedback.parryStop);
            BattleAudio.Play("parry_success", transform.position);
            return true;
        }

        public void GrantInvincibility(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            damageInvincibilityTimer = Mathf.Max(damageInvincibilityTimer, seconds);
        }

        public void TakeDamage(int damage)
        {
            ReceiveHit(new HitContext(damage, (Vector3)FootPosition + Vector3.up * 0.5f));
        }

        public HitResult ReceiveHit(HitContext hit)
        {
            int damage = hit.Damage;
            if (battleLocked || damage <= 0 || IsInvincible || currentHealth <= 0)
            {
                return IsInvincible ? HitResult.Invulnerable : HitResult.Ignored;
            }

            GameConfig config = GameConfigService.Current;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            damageInvincibilityTimer = config.player.damageInvincibilityDuration;
            hurtTimer = config.player.hurtFlashDuration;
            StopAirFlip();
            BattleStats.Active?.RecordHitTaken();
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            HitFlashEffect.Play(gameObject, config.player.hurtFlashDuration, config.player.hurtFlashCount, Color.white);
            CombatImpactEffect.SpawnHit(hit.Point, new Color(1f, 0.38f, 0.28f), direction: hit.Direction);
            ScreenShakeEffect.Shake(config.player.hitShakeDuration, config.player.hitShakeStrength);
            BattleClock.Stop(config.feedback.hurtStop);
            BattleAudio.Play("hurt", hit.Point);
            Debug.Log($"玩家受到 {damage} 点伤害，剩余生命 {currentHealth}/{MaxHealth}。");

            if (currentHealth > 0)
            {
                return HitResult.Applied;
            }

            isDead = true;
            ClearJumpCut();
            airFlipping = false;
            uprightRemaining = 0f;
            BattleAudio.Play("death", transform.position);
            HitFlashEffect flash = GetComponent<HitFlashEffect>();
            if (flash != null) flash.enabled = false;
            deathSequenceTimer = 0f;
            IsDashing = false;
            IsUsingSuper = false;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            bodyCollider.enabled = false;
            CombatImpactEffect.SpawnDeath((Vector3)FootPosition + Vector3.up * 0.55f, new Color(0.9f, 0.9f, 1f));
            Died?.Invoke();
            Debug.Log("玩家生命归零，战斗失败流程已触发。");
            return HitResult.Killed;
        }

        public void SetBattleLocked(bool locked)
        {
            battleLocked = locked;
            bufferedJump = bufferedDash = bufferedSpecial = false;
            if (!locked)
            {
                return;
            }

            SuppressActionInput();
            ClearJumpCut();
            spawnDodgeBurstOnDashEnd = false;
            coyoteTimer = 0f;
            moveInput = 0f;
            IsShooting = false;
            StopAirFlip();

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
                visualRoot.localRotation = visualBaseRotation;
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
            if (IsHurt || IsParrying) return;
            uprightRemaining = 0f;
            airFlipping = true;
            flipDirection = facingDirection;
            airFlipTimer = 0f;
        }

        private void StopAirFlip()
        {
            if (!airFlipping) return;
            airFlipping = false;
            airFlipTimer = 0f;
            if (visualRoot != null)
            {
                uprightStart = visualRoot.localRotation;
                uprightRemaining = GameConfigService.Current.animation.hero.uprightDuration;
            }
        }

        private void UpdateAirFlip()
        {
            if (visualRoot != null && uprightRemaining > 0f)
            {
                float duration = Mathf.Max(0.001f, GameConfigService.Current.animation.hero.uprightDuration);
                uprightRemaining = Mathf.Max(0f, uprightRemaining - Time.deltaTime);
                visualRoot.localRotation = Quaternion.Slerp(uprightStart, visualBaseRotation, 1f - uprightRemaining / duration);
            }
            if (!airFlipping || visualRoot == null)
            {
                return;
            }

            airFlipTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(airFlipTimer / Mathf.Max(0.01f, airFlipDuration));
            float angle = -360f * progress * flipDirection;
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
            BattleAudio.Play(IsDashing ? "dash_start" : "dash_end", smokePosition);
        }

        private void SpawnSuperEffect()
        {
            Vector3 effectPosition = (Vector3)FootPosition + new Vector3(
                facingDirection * 0.2f,
                GameConfigService.Current.special.effectYOffset,
                0f);
            SuperAttackEffect.Spawn(effectPosition, facingDirection);
        }

        private void LateUpdate()
        {
            if (isDead) return;
            float alpha = IsDashing ? 0f : damageInvincibilityTimer > 0f
                ? (Mathf.FloorToInt(damageInvincibilityTimer * 12f) % 2 == 0 ? 0.35f : 1f) : 1f;
            SetVisualAlpha(alpha);
        }
    }
}

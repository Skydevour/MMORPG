using System;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using MMORPG.Game.Projectiles;
using MMORPG.Game.VFX;
using UnityEngine;

namespace MMORPG.Game.Bosses.Potato
{
    public enum PotatoBossGrammar
    {
        LaneShotgun,
        LobbedSeed,
        RootBelch,
        InsectSwarm,
        FinalShotgun
    }

    public sealed class PotatoBossController : MonoBehaviour, MMORPG.Game.Bosses.IBossActor
    {
        private bool encounterMode;
        private int attackCycle;
        public Transform Root => transform;
        public string DisplayName => "土豆";
        public void ConfigureEncounter(BossFormConfig form)
        {
            encounterMode = true;
            currentHealth = maxHealth = form.maxHealth;
            currentPhase = 1;
            nextAttackTimer = config.encounter.safetyDuration;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
        private PlayerController2D target;
        private GameConfig config;
        private int currentHealth;
        private int maxHealth;
        private PotatoAttackSequence attackSequence;
        private bool loggedAttackDelay;
        private bool releasePresentationPending;
        private float nextAttackTimer;
        private PotatoBossGrammar currentGrammar;
        private float transitionLockTimer;
        private bool transitionLocked;
        private bool battleLocked;
        private bool lastAttackWasLob;
        private int currentPhase = 1;
        private bool aiEnabled = true;
        public void SetAiEnabled(bool enabled) => aiEnabled = enabled;
        public float AttackVisualProgress => attackSequence?.VisualProgress ?? 0f;

        public bool IsDead { get; private set; }

        public bool IsAttacking => !IsDead && !battleLocked && !transitionLocked && attackSequence != null &&
            (attackSequence.Clock.IsActive || releasePresentationPending);

        public int CurrentHealth => currentHealth;

        public int MaxHealth => maxHealth;

        public int CurrentPhase => currentPhase;

        public event Action<int, int> HealthChanged;

        public event Action Died;

        public event Action<int> PhaseChanged;

        public void Initialize(PlayerController2D playerTarget)
        {
            target = playerTarget;
            nextAttackTimer = UnityEngine.Random.Range(config.boss.attackCooldownMin * 0.6f, config.boss.attackCooldownMax);
        }

        private void Awake()
        {
            config = GameConfigService.Current;
            maxHealth = config.boss.maxHealth;
            currentHealth = maxHealth;
            currentPhase = GetPhaseForHealth(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f || IsDead || battleLocked || !aiEnabled)
            {
                return;
            }

            releasePresentationPending = false;
            TickTransitionLock();

            if (transitionLocked)
            {
                return;
            }

            if (IsAttacking)
            {
                TickAttackSequence(Time.deltaTime);
                return;
            }

            nextAttackTimer -= Time.deltaTime;
            if (nextAttackTimer <= 0f)
            {
                BeginAttack();
            }
        }

        public void TakeDamage(int damage)
        {
            ReceiveHit(new HitContext(damage, transform.position + Vector3.up * 0.8f));
        }

        public HitResult ReceiveHit(HitContext hit)
        {
            int damage = hit.Damage;
            if (IsDead || battleLocked || transitionLocked || damage <= 0)
            {
                return HitResult.Ignored;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            HitFlashEffect.Play(gameObject, 0.14f, 3, Color.white);
            CombatImpactEffect.SpawnHit(hit.Point, new Color(1f, 0.68f, 0.22f), direction: hit.Direction);
            MMORPG.Game.Audio.BattleAudio.Play("boss_hit", hit.Point);
            Debug.Log($"土豆 Boss 受到 {damage} 点伤害，剩余生命 {currentHealth}/{maxHealth}。");

            if (currentHealth <= 0)
            {
                MarkDead();
                return HitResult.Killed;
            }

            int newPhase = GetPhaseForHealth(currentHealth, maxHealth);
            if (!encounterMode && newPhase > currentPhase)
            {
                StartPhaseTransition(newPhase);
            }
            return HitResult.Applied;
        }

        public void MarkDead()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            CancelAttack();
            foreach (Collider2D collider in GetComponents<Collider2D>())
            {
                collider.enabled = false;
            }

            ClearActiveHazards();
            CombatImpactEffect.SpawnDeath(transform.position + Vector3.up * 0.75f, new Color(0.95f, 0.62f, 0.2f));
            ScreenShakeEffect.Shake(0.25f, 0.12f);
            Died?.Invoke();
            Debug.Log("土豆 Boss 已被击败，胜利流程已触发。");
        }

        public void SetBattleLocked(bool locked)
        {
            battleLocked = locked;
            if (locked)
            {
                CancelAttack();
                transitionLocked = false;
                transitionLockTimer = 0f;
            }
        }

        public bool IsInTransition => transitionLocked;

        private void TickTransitionLock()
        {
            if (!transitionLocked)
            {
                return;
            }

            transitionLockTimer -= Time.deltaTime;
            if (transitionLockTimer <= 0f)
            {
                transitionLocked = false;
                nextAttackTimer = UnityEngine.Random.Range(config.boss.attackCooldownMin * 0.6f, config.boss.attackCooldownMax);
                Debug.Log("Boss 阶段切换安全窗口结束，恢复攻击。");
            }
        }

        private void StartPhaseTransition(int newPhase)
        {
            currentPhase = newPhase;
            CancelAttack();
            transitionLocked = true;
            transitionLockTimer = config.boss.phaseTransitionDuration;
            ClearActiveHazards();
            BattleStats.Active?.RecordPhaseTransition();
            PhaseChanged?.Invoke(newPhase);
            Debug.Log($"土豆 Boss 进入阶段 {newPhase}，切换期间清空所有 Boss 投射物并暂停攻击 {config.boss.phaseTransitionDuration:0.0} 秒。");
        }

        private void ClearActiveHazards()
        {
            BossProjectile.DespawnAllActive();
            BossRootHazard.DespawnAllActive();
            BossInsect.DespawnAllActive();
            BossLobbedSeed.DespawnAllActive();
        }

        private void CancelAttack()
        {
            releasePresentationPending = false;
            attackSequence?.Clock.Cancel();
        }

        private void OnDisable() => CancelAttack();

        private void TickAttackSequence(float deltaTime)
        {
            int shotIndex = attackSequence.Clock.Advance(deltaTime);
            if (shotIndex < 0) return;
            // 零恢复时最后释放已完成逻辑，但本渲染帧仍须显示开嘴姿态。
            releasePresentationPending = true;
            if (!loggedAttackDelay && attackSequence.Clock.DiscardedTime >= 0.1f)
            {
                loggedAttackDelay = true;
                Debug.Log($"土豆 Boss 攻击跨帧限流：第 {shotIndex + 1} 发，舍弃余量 {attackSequence.Clock.DiscardedTime:0.000} 秒，本轮延长且保留后续弹体。");
            }
            ExecuteAttackStep(shotIndex);
        }

        private void BeginAttack()
        {
            if (encounterMode)
            {
                var e = config.encounter;
                attackCycle++;
                currentGrammar = PotatoBossGrammar.LaneShotgun;
                nextAttackTimer = UnityEngine.Random.Range(e.potatoCooldownMin, e.potatoCooldownMax);
                attackSequence = new PotatoAttackSequence(e.potatoTell, e.potatoInterval, 3, e.potatoRecovery);
                loggedAttackDelay = false;
                MMORPG.Game.Audio.BattleAudio.Play("inhale", transform.position);
                return;
            }
            BossPhaseConfig phase = GetPhaseConfig(CurrentPhase);
            currentGrammar = SelectGrammar(phase);
            nextAttackTimer = UnityEngine.Random.Range(phase.attackCooldownMin, phase.attackCooldownMax);
            attackSequence = new PotatoAttackSequence(config.boss.attackAnticipation,
                phase.attackShotInterval, GetGrammarShotCount(currentGrammar, phase), config.boss.attackRecovery);
            loggedAttackDelay = false;
        }

        private PotatoBossGrammar SelectGrammar(BossPhaseConfig phase)
        {
            switch (CurrentPhase)
            {
                case 1:
                    if (lastAttackWasLob)
                    {
                        lastAttackWasLob = false;
                        return PotatoBossGrammar.LaneShotgun;
                    }

                    lastAttackWasLob = UnityEngine.Random.value < phase.lobChance;
                    return lastAttackWasLob ? PotatoBossGrammar.LobbedSeed : PotatoBossGrammar.LaneShotgun;
                case 2:
                    return UnityEngine.Random.value < phase.rootChance ? PotatoBossGrammar.RootBelch : PotatoBossGrammar.LaneShotgun;
                default:
                    return UnityEngine.Random.value < phase.insectChance ? PotatoBossGrammar.InsectSwarm : PotatoBossGrammar.FinalShotgun;
            }
        }

        private static int GetGrammarShotCount(PotatoBossGrammar grammar, BossPhaseConfig phase)
        {
            switch (grammar)
            {
                case PotatoBossGrammar.LobbedSeed:
                case PotatoBossGrammar.RootBelch:
                case PotatoBossGrammar.InsectSwarm:
                    return 1;
                default:
                    return phase.attackShotCount;
            }
        }

        private void ExecuteAttackStep(int shotIndex)
        {
            BossPhaseConfig phase = GetPhaseConfig(CurrentPhase);
            switch (currentGrammar)
            {
                case PotatoBossGrammar.LobbedSeed:
                    FireLobbedSeed();
                    break;
                case PotatoBossGrammar.RootBelch:
                    FireRootHazard();
                    break;
                case PotatoBossGrammar.InsectSwarm:
                    FireInsectSwarm(phase);
                    break;
                case PotatoBossGrammar.FinalShotgun:
                    FireAttackShot(shotIndex, phase);
                    if (shotIndex == 2)
                    {
                        FireRootHazard();
                    }

                    break;
                default:
                    FireAttackShot(shotIndex, phase);
                    break;
            }
        }

        private void FireAttackShot(int shotIndex, BossPhaseConfig phase)
        {
            if (encounterMode)
            {
                var e = config.encounter;
                var geometry = Resources.Load<GardenBossAssets>("Config/GardenBossAssets").potatoGeometry;
                Vector3 mouth = new Vector3(transform.TransformPoint(geometry.mouth).x,
                    config.level.stageFloorY + e.potatoHeights[shotIndex % 3], -0.1f);
                var kind = shotIndex == 1 && attackCycle % 2 == 0 ? BossProjectileType.Pink : BossProjectileType.Normal;
                BossProjectile.Spawn(mouth, Vector2.left * e.potatoSpeed, kind, (BossProjectileLane)(shotIndex % 3));
                MMORPG.Game.Audio.BattleAudio.Play("spit", mouth);
                return;
            }
            int direction = -1;
            if (target != null)
            {
                direction = target.transform.position.x < transform.position.x ? -1 : 1;
            }

            BossProjectileLane lane = (BossProjectileLane)(shotIndex % 3);
            float laneOffsetY = GetLaneOffset(lane);
            Vector3 spawnPosition = transform.position + new Vector3(
                direction * config.boss.projectileSpawnOffsetX,
                laneOffsetY,
                -0.1f);

            BossProjectileType type = UnityEngine.Random.value < phase.pinkChance
                ? BossProjectileType.Pink
                : BossProjectileType.Normal;
            float speed = type == BossProjectileType.Pink
                ? config.projectile.pinkSpeed
                : config.projectile.normalSpeed;

            Vector2 horizontalVelocity = new Vector2(direction * speed, 0f);
            BossProjectile.Spawn(spawnPosition, horizontalVelocity, type, lane);
            MMORPG.Game.Audio.BattleAudio.Play("spit", spawnPosition);
            string projectileName = type == BossProjectileType.Pink ? "紫色可采集子弹" : "普通土块子弹";
            Debug.Log($"阶段 {CurrentPhase} 土豆 Boss 发动{GetGrammarName(currentGrammar)}，第 {shotIndex + 1} 发从{GetLaneName(lane)}位置生成{projectileName}，水平速度 {speed:0.0}。");
        }

        private void FireLobbedSeed()
        {
            if (target == null)
            {
                return;
            }

            int direction = target.transform.position.x < transform.position.x ? -1 : 1;
            Vector3 spawnPosition = transform.position + new Vector3(
                direction * config.boss.projectileSpawnOffsetX * 0.7f,
                config.boss.projectileSpawnOffsetYMiddle * 0.6f,
                -0.1f);
            Vector2 velocity = new Vector2(
                direction * config.boss.seedLobSpeed,
                config.boss.seedArcHeight);
            BossLobbedSeed.Spawn(spawnPosition, velocity, config.level.stageFloorY);
            Debug.Log($"阶段 {CurrentPhase} 土豆 Boss 发动抛物线种子攻击，目标 X={target.transform.position.x:0.00}。");
        }

        private void FireRootHazard()
        {
            if (target == null)
            {
                return;
            }

            int count = Mathf.Max(1, Mathf.Min(config.boss.rootHazardCount, 2));
            for (int index = 0; index < count; index++)
            {
                float offsetX = index == 0 ? 0f : UnityEngine.Random.Range(-1.2f, 1.2f);
                float rootX = Mathf.Clamp(
                    target.transform.position.x + offsetX,
                    config.level.minStageX + 0.6f,
                    config.level.maxStageX - 0.6f);
                BossRootHazard.Spawn(new Vector3(rootX, config.level.stageFloorY, -0.2f));
            }

            Debug.Log($"阶段 {CurrentPhase} 土豆 Boss 发动根须破土，在玩家附近生成 {count} 个根须。");
        }

        private void FireInsectSwarm(BossPhaseConfig phase)
        {
            int direction = target != null && target.transform.position.x < transform.position.x ? -1 : 1;
            int count = Mathf.Max(2, Mathf.Min(phase.insectCount, 4));
            for (int index = 0; index < count; index++)
            {
                Vector3 spawnPosition = transform.position + new Vector3(
                    direction * (0.5f + index * 0.35f),
                    1.1f + index * 0.25f,
                    -0.2f);
                BossInsect.Spawn(spawnPosition, target == null ? null : target.transform);
            }

            Debug.Log($"阶段 {CurrentPhase} 土豆 Boss 召唤 {count} 只昆虫。");
        }

        private BossPhaseConfig GetPhaseConfig(int phase)
        {
            switch (phase)
            {
                case 2:
                    return config.boss.phase2;
                case 3:
                    return config.boss.phase3;
                default:
                    return config.boss.phase1;
            }
        }

        private static int GetPhaseForHealth(int health, int maxHealth)
        {
            float phaseTwoThreshold = maxHealth * 0.70f;
            float phaseThreeThreshold = maxHealth * 0.35f;
            if (health > phaseTwoThreshold)
            {
                return 1;
            }

            return health > phaseThreeThreshold ? 2 : 3;
        }

        private float GetLaneOffset(BossProjectileLane lane)
        {
            switch (lane)
            {
                case BossProjectileLane.Top:
                    return config.boss.projectileSpawnOffsetYTop;
                case BossProjectileLane.Bottom:
                    return config.boss.projectileSpawnOffsetYBottom;
                default:
                    return config.boss.projectileSpawnOffsetYMiddle;
            }
        }

        private static string GetLaneName(BossProjectileLane lane)
        {
            switch (lane)
            {
                case BossProjectileLane.Top:
                    return "上方";
                case BossProjectileLane.Bottom:
                    return "下方";
                default:
                    return "中间";
            }
        }

        private static string GetGrammarName(PotatoBossGrammar grammar)
        {
            switch (grammar)
            {
                case PotatoBossGrammar.LobbedSeed:
                    return "抛物线种子";
                case PotatoBossGrammar.RootBelch:
                    return "根须破土";
                case PotatoBossGrammar.InsectSwarm:
                    return "昆虫群";
                case PotatoBossGrammar.FinalShotgun:
                    return "最终散射";
                default:
                    return "三段散射";
            }
        }
    }
}

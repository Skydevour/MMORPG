using System;
using UnityEngine;

namespace MMORPG.Game.Config
{
    [Serializable]
    public sealed class GameConfig
    {
        public LevelConfig level = new LevelConfig();
        public HeroAnimationConfig animation = new HeroAnimationConfig();
        public BattleCameraConfig camera = new BattleCameraConfig();
        public PlayerConfig player = new PlayerConfig();
        public BossConfig boss = new BossConfig();
        public ProjectileConfig projectile = new ProjectileConfig();
        public SpecialConfig special = new SpecialConfig();
        public BattleVfxConfig vfx = new BattleVfxConfig();
        public EncounterConfig encounter = new EncounterConfig();
        public FeedbackConfig feedback = new FeedbackConfig();
        public BattleAudioConfig audio = new BattleAudioConfig();
        public PresentationConfig presentation = new PresentationConfig();

        public void Normalize()
        {
            EnsureDefaults();
            animation = animation ?? new HeroAnimationConfig();
            animation.hero = animation.hero ?? new HeroAnimationTiming();
            camera = camera ?? new BattleCameraConfig();
            camera.viewHeight = Mathf.Clamp(camera.viewHeight, 7.4f, 9f);
            encounter = encounter ?? new EncounterConfig();
            feedback = feedback ?? new FeedbackConfig();
            audio = audio ?? new BattleAudioConfig();
            presentation = presentation ?? new PresentationConfig();
            encounter.Validate();
            audio.maxVoices = Mathf.Clamp(audio.maxVoices, 1, 24);
            audio.musicCrossfadeDuration = Mathf.Max(0.01f, audio.musicCrossfadeDuration);
            audio.shotDuckDuration = Mathf.Max(0f, audio.shotDuckDuration);
            audio.shotDuckGain = Mathf.Clamp01(audio.shotDuckGain);
            presentation.bossHealthFillDuration = Mathf.Max(0.01f, presentation.bossHealthFillDuration);
            presentation.bossHealthTrailSpeed = Mathf.Max(0.01f, presentation.bossHealthTrailSpeed);
            level.width = Mathf.Max(1f, level.width);
            level.height = Mathf.Max(1f, level.height);
            level.stageFloorY = Mathf.Clamp(level.stageFloorY, -level.height, level.height);
            level.minStageX = Mathf.Min(level.minStageX, level.maxStageX - 0.1f);
            level.maxStageX = Mathf.Max(level.maxStageX, level.minStageX + 0.1f);

            level.playerSpawnX = Mathf.Clamp(level.playerSpawnX, level.minStageX, level.maxStageX);

            level.foregroundCropHeight = Mathf.Clamp01(level.foregroundCropHeight);

            player.maxHealth = Mathf.Max(1, player.maxHealth);
            player.maxEnergy = Mathf.Max(1, player.maxEnergy);
            player.moveSpeed = Mathf.Max(0.1f, player.moveSpeed);
            player.idleCycleDuration = Mathf.Max(0.05f, player.idleCycleDuration);
            player.runCycleDuration = Mathf.Max(0.05f, player.runCycleDuration);
            player.shootCycleDuration = Mathf.Max(0.05f, player.shootCycleDuration);
            player.jumpVelocity = Mathf.Max(0.1f, player.jumpVelocity);
            player.jumpCutBlendDuration = Mathf.Clamp(player.jumpCutBlendDuration, 0.02f, 0.15f);
            player.secondJumpVelocity = Mathf.Max(0.1f, player.secondJumpVelocity);
            player.maxAirJumps = Mathf.Max(0, player.maxAirJumps);
            player.dashSpeed = Mathf.Max(0.1f, player.dashSpeed);
            player.dashDuration = Mathf.Max(0.01f, player.dashDuration);
            player.dashCooldown = Mathf.Max(0.01f, player.dashCooldown);
            player.shotCooldown = Mathf.Max(0.01f, player.shotCooldown);
            player.airFlipDuration = Mathf.Max(0.01f, player.airFlipDuration);
            player.groundCastDistance = Mathf.Max(0.01f, player.groundCastDistance);

            player.damageInvincibilityDuration = Mathf.Max(0.05f, player.damageInvincibilityDuration);

            player.hurtFlashCount = Mathf.Max(2, player.hurtFlashCount);

            player.hitShakeDuration = Mathf.Max(0.01f, player.hitShakeDuration);

            player.hitShakeStrength = Mathf.Max(0f, player.hitShakeStrength);

            player.hurtFlashDuration = Mathf.Max(0.03f, player.hurtFlashDuration);

            player.coyoteTime = Mathf.Max(0f, player.coyoteTime);

            player.jumpBufferTime = Mathf.Max(0f, player.jumpBufferTime);

            player.groundAcceleration = Mathf.Max(0.1f, player.groundAcceleration);

            player.groundDeceleration = Mathf.Max(0.1f, player.groundDeceleration);

            player.airControl = Mathf.Clamp01(player.airControl);

            player.jumpCutMultiplier = Mathf.Clamp(player.jumpCutMultiplier, 0.1f, 1f);

            player.deathActionDuration = Mathf.Max(0.01f, player.deathActionDuration);

            player.ghostRiseDuration = Mathf.Max(0.01f, player.ghostRiseDuration);

            player.ghostRiseSpeed = Mathf.Max(0.01f, player.ghostRiseSpeed);

            player.deathResultDelay = Mathf.Max(0f, player.deathResultDelay);
            player.superKey = string.IsNullOrWhiteSpace(player.superKey) ? "K" : player.superKey.ToUpperInvariant();

            boss.maxHealth = Mathf.Max(1, boss.maxHealth);
            boss.attackAnticipation = Mathf.Max(0.05f, boss.attackAnticipation);
            boss.attackRecovery = Mathf.Max(0.05f, boss.attackRecovery);
            boss.scale = Mathf.Max(0.1f, boss.scale);
            boss.groundEmbedDepth = Mathf.Max(0f, boss.groundEmbedDepth);

            boss.spawnX = Mathf.Clamp(boss.spawnX, level.minStageX, level.maxStageX);
            boss.attackCooldownMin = Mathf.Max(0.1f, boss.attackCooldownMin);
            boss.attackCooldownMax = Mathf.Max(boss.attackCooldownMin, boss.attackCooldownMax);
            boss.attackAnimationFramesPerSecond = Mathf.Max(1f, boss.attackAnimationFramesPerSecond);

            boss.attackShotInterval = Mathf.Max(0.01f, boss.attackShotInterval);

            boss.attackShotCount = Mathf.Max(1, boss.attackShotCount);

            boss.attackDuration = Mathf.Max(boss.attackDuration, boss.attackShotInterval * (boss.attackShotCount - 1) + 0.05f);
            boss.projectileSpawnOffsetX = Mathf.Max(0.1f, boss.projectileSpawnOffsetX);
            boss.projectileSpawnOffsetYTop = Mathf.Max(0f, boss.projectileSpawnOffsetYTop);

            boss.projectileSpawnOffsetYMiddle = Mathf.Max(0f, boss.projectileSpawnOffsetYMiddle);

            boss.projectileSpawnOffsetYBottom = Mathf.Max(0f, boss.projectileSpawnOffsetYBottom);
            boss.phaseTransitionDuration = Mathf.Max(0.1f, boss.phaseTransitionDuration);
            boss.rootHazardCount = Mathf.Clamp(boss.rootHazardCount, 1, 4);
            boss.rootHazardSpeed = Mathf.Max(0.1f, boss.rootHazardSpeed);
            boss.rootHazardLifetime = Mathf.Max(0.1f, boss.rootHazardLifetime);
            boss.rootTellDuration = Mathf.Max(0.05f, boss.rootTellDuration);
            boss.seedLobSpeed = Mathf.Max(0.1f, boss.seedLobSpeed);
            boss.seedArcHeight = Mathf.Max(0.1f, boss.seedArcHeight);
            boss.seedObstacleLifetime = Mathf.Max(0.1f, boss.seedObstacleLifetime);
            boss.insectCount = Mathf.Clamp(boss.insectCount, 1, 5);
            boss.insectSpeed = Mathf.Max(0.1f, boss.insectSpeed);
            boss.insectLifetime = Mathf.Max(0.5f, boss.insectLifetime);
            NormalizePhase(boss.phase1);
            NormalizePhase(boss.phase2);
            NormalizePhase(boss.phase3);

            projectile.normalSpeed = Mathf.Max(0.1f, projectile.normalSpeed);
            projectile.pinkSpeed = Mathf.Max(0.1f, projectile.pinkSpeed);
            projectile.lifetime = Mathf.Max(0.1f, projectile.lifetime);
            projectile.normalDamage = Mathf.Max(1, projectile.normalDamage);
            projectile.pinkEnergyValue = Mathf.Max(1, projectile.pinkEnergyValue);
            projectile.pinkChance = Mathf.Clamp01(projectile.pinkChance);
            projectile.playerBulletSpeed = Mathf.Max(0.1f, projectile.playerBulletSpeed);
            projectile.playerBulletLifetime = Mathf.Max(0.1f, projectile.playerBulletLifetime);

            projectile.pinkPickupEffectDuration = Mathf.Max(0.05f, projectile.pinkPickupEffectDuration);

            special.damage = Mathf.Max(1, special.damage);
            special.duration = Mathf.Max(0.1f, special.duration);
            special.range = Mathf.Max(0.1f, special.range);
            special.height = Mathf.Max(0.1f, special.height);
            special.effectYOffset = Mathf.Max(0f, special.effectYOffset);
            vfx.smokeCount = Mathf.Clamp(vfx.smokeCount, 1, 64);
            vfx.smokeDuration = Mathf.Max(0.05f, vfx.smokeDuration);
            vfx.smokeSize = Mathf.Max(0.01f, vfx.smokeSize);
        }

        private void EnsureDefaults()
        {
            if (level == null) level = new LevelConfig();
            if (player == null) player = new PlayerConfig();
            if (boss == null) boss = new BossConfig();
            if (projectile == null) projectile = new ProjectileConfig();
            if (special == null) special = new SpecialConfig();
            if (vfx == null) vfx = new BattleVfxConfig();
            if (boss.phase1 == null) boss.phase1 = new BossPhaseConfig();
            if (boss.phase2 == null) boss.phase2 = new BossPhaseConfig();
            if (boss.phase3 == null) boss.phase3 = new BossPhaseConfig();
        }

        private static void NormalizePhase(BossPhaseConfig phase)
        {
            phase.attackCooldownMin = Mathf.Max(0.1f, phase.attackCooldownMin);
            phase.attackCooldownMax = Mathf.Max(phase.attackCooldownMin, phase.attackCooldownMax);
            phase.attackShotInterval = Mathf.Max(0.01f, phase.attackShotInterval);
            phase.attackShotCount = Mathf.Max(1, phase.attackShotCount);
            phase.pinkChance = Mathf.Clamp01(phase.pinkChance);
            phase.lobChance = Mathf.Clamp01(phase.lobChance);
            phase.rootChance = Mathf.Clamp01(phase.rootChance);
            phase.insectChance = Mathf.Clamp01(phase.insectChance);
            phase.insectCount = Mathf.Clamp(phase.insectCount, 1, 5);
        }

        public static GameConfig CreateDefault()
        {
            GameConfig config = new GameConfig();
            config.Normalize();
            return config;
        }
    }

    [Serializable]
    public sealed class LevelConfig
    {
        public float backgroundOverscan = 0.04f;
        public float width = 16f;
        public float height = 9f;
        public float stageFloorY = -2.5f;
        public float minStageX = -5.7f;
        public float maxStageX = 6.1f;

        public float playerSpawnX = -4.7f;
        public float stageColliderThickness = 0.8f;

        public float foregroundCropHeight = 0.24f;

        public int foregroundSortingOrder = 48;
    }

    [Serializable]
    public sealed class PlayerConfig
    {
        public float idleCycleDuration = 0.85f;
        public float runCycleDuration = 0.65f;
        public float shootCycleDuration = 0.5f;
        public int maxHealth = 3;
        public int maxEnergy = 3;
        public float moveSpeed = 5.5f;
        public float jumpVelocity = 10.5f;
        public float secondJumpVelocity = 9.2f;
        public int maxAirJumps = 1;
        public float dashSpeed = 13f;
        public float dashDuration = 0.18f;
        public float dashCooldown = 0.35f;
        public float dodgeBurstYOffset = 0.18f;
        public float shotCooldown = 0.16f;
        public float muzzleOffsetX = 0.5f;
        public float muzzleOffsetY = 0.65f;
        public float airFlipDuration = 0.42f;
        public float groundCastDistance = 0.06f;

        public float damageInvincibilityDuration = 3f;

        public float hurtFlashDuration = 0.16f;

        public int hurtFlashCount = 5;

        public float hitShakeDuration = 0.12f;

        public float hitShakeStrength = 0.1f;

        public float coyoteTime = 0.1f;

        public float jumpBufferTime = 0.12f;

        public float groundAcceleration = 45f;

        public float groundDeceleration = 55f;

        public float airControl = 1f;

        public float jumpCutMultiplier = 0.45f;
        public float jumpCutBlendDuration = 0.06f;

        public float deathActionDuration = 0.38f;

        public float ghostRiseDuration = 1.2f;

        public float ghostRiseSpeed = 1.05f;

        public float deathResultDelay = 2f;
        public float colliderOffsetX = 0f;
        public float colliderOffsetY = 0.64f;
        public float colliderSizeX = 0.72f;
        public float colliderSizeY = 1.28f;
        public float visualCenterY = 0.64f;
        public string superKey = "K";
    }

    [Serializable]
    public sealed class BossConfig
    {
        public float attackAnticipation = 0.3f;
        public float attackRecovery = 0.25f;
        public int maxHealth = 100;
        public float scale = 2.2f;
        public float groundEmbedDepth = 0.12f;

        public float spawnX = 4.7f;
        public float attackCooldownMin = 0.8f;
        public float attackCooldownMax = 1.2f;
        public float attackDuration = 0.6f;

        public float attackAnimationFramesPerSecond = 10f;

        public float attackShotInterval = 0.18f;

        public int attackShotCount = 3;
        public float projectileSpawnOffsetX = 0.9f;
        public float projectileSpawnOffsetYTop = 2f;

        public float projectileSpawnOffsetYMiddle = 1.25f;

        public float projectileSpawnOffsetYBottom = 0.55f;
        public float phaseTransitionDuration = 0.6f;
        public BossPhaseConfig phase1 = new BossPhaseConfig
        {
            attackCooldownMin = 1f,
            attackCooldownMax = 1.4f,
            attackShotInterval = 0.18f,
            attackShotCount = 3,
            pinkChance = 0.35f,
            lobChance = 0.45f
        };
        public BossPhaseConfig phase2 = new BossPhaseConfig
        {
            attackCooldownMin = 0.9f,
            attackCooldownMax = 1.2f,
            attackShotInterval = 0.15f,
            attackShotCount = 4,
            pinkChance = 0.45f,
            rootChance = 0.55f
        };
        public BossPhaseConfig phase3 = new BossPhaseConfig
        {
            attackCooldownMin = 0.8f,
            attackCooldownMax = 1.1f,
            attackShotInterval = 0.14f,
            attackShotCount = 5,
            pinkChance = 0.5f,
            insectChance = 0.6f,
            insectCount = 3
        };
        public int rootHazardCount = 2;
        public float rootHazardSpeed = 4.2f;
        public float rootHazardLifetime = 2.2f;
        public float rootTellDuration = 0.35f;
        public float seedLobSpeed = 5f;
        public float seedArcHeight = 2.6f;
        public float seedObstacleLifetime = 3.5f;
        public int insectCount = 3;
        public float insectSpeed = 3.4f;
        public float insectLifetime = 5f;
    }

    [Serializable]
    public sealed class BossPhaseConfig
    {
        public float attackCooldownMin = 1f;
        public float attackCooldownMax = 1.4f;
        public float attackShotInterval = 0.18f;
        public int attackShotCount = 3;
        public float pinkChance = 0.35f;
        public float lobChance = 0.45f;
        public float rootChance = 0.55f;
        public float insectChance = 0.6f;
        public int insectCount = 3;
    }

    [Serializable]
    public sealed class ProjectileConfig
    {
        public float normalRadius = 0.2f, pinkRadius = 0.24f, rainScaleX = 0.7f, rainScaleY = 1.4f;
        public float normalSpeed = 8f;
        public float pinkSpeed = 7.2f;
        public float lifetime = 6f;
        public int normalDamage = 1;
        public int pinkEnergyValue = 1;
        public float pinkChance = 0.35f;
        public float playerBulletSpeed = 11f;
        public float playerBulletLifetime = 1.4f;

        public float pinkPickupEffectDuration = 0.45f;
    }

    [Serializable]
    public sealed class SpecialConfig
    {
        public float range = 14f;
        public float height = 1.1f;
        public int damage = 30;
        public float duration = 0.85f;
        public float effectYOffset = 0.85f;
    }

    [Serializable]
    public sealed class BattleVfxConfig
    {
        public float hitDriftSpeed = 1.4f;
        public float smokeDuration = 0.52f;
        public float smokeDriftSpeed = 0.8f;
        public float smokeRiseSpeed = 0.35f;
        public float smokeSize = 0.6f;
        public int smokeCount = 9;
        public float smokeCenterYOffset = 0.25f;
        public float hitDuration = 0.22f;
        public float hitSize = 0.12f;
        public int hitCount = 6;
        public float deathDuration = 0.65f;
        public float deathSize = 0.45f;
        public int deathCount = 24;
    }

    public static class GameConfigService
    {
        private const string ResourcePath = "Config/GameConfig";
        private static GameConfig current;
        private static bool loaded;

        public static GameConfig Current
        {
            get
            {
                if (!loaded)
                {
                    Load();
                }

                return current;
            }
        }

        public static void Load()
        {
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                current = GameConfig.CreateDefault();
                loaded = true;
                Debug.LogWarning("未找到全局配置 Assets/Resources/Config/GameConfig.json，已使用代码默认配置。");
                return;
            }

            try
            {
                current = JsonUtility.FromJson<GameConfig>(asset.text);
                if (current == null)
                {
                    current = GameConfig.CreateDefault();
                }

                current.Normalize();
            }
            catch (Exception exception)
            {
                current = GameConfig.CreateDefault();
                Debug.LogWarning($"全局 JSON 配置解析或规范化失败，已使用代码默认配置：{exception.Message}");
            }

            loaded = true;
            Debug.Log($"全局配置加载完成：最大能量 {current.player.maxEnergy} 格，Boss 生命 {current.boss.maxHealth}。");
        }
    }
}

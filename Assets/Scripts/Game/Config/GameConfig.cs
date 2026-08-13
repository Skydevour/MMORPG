using System;
using UnityEngine;

namespace MMORPG.Game.Config
{
    [Serializable]
    public sealed class GameConfig
    {
        public LevelConfig level = new LevelConfig();
        public PlayerConfig player = new PlayerConfig();
        public BossConfig boss = new BossConfig();
        public ProjectileConfig projectile = new ProjectileConfig();
        public SpecialConfig special = new SpecialConfig();

        public void Normalize()
        {
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
            player.jumpVelocity = Mathf.Max(0.1f, player.jumpVelocity);
            player.secondJumpVelocity = Mathf.Max(0.1f, player.secondJumpVelocity);
            player.maxAirJumps = Mathf.Max(0, player.maxAirJumps);
            player.dashSpeed = Mathf.Max(0.1f, player.dashSpeed);
            player.dashDuration = Mathf.Max(0.01f, player.dashDuration);
            player.dashCooldown = Mathf.Max(0.01f, player.dashCooldown);
            player.shotCooldown = Mathf.Max(0.01f, player.shotCooldown);
            player.airFlipDuration = Mathf.Max(0.01f, player.airFlipDuration);
            player.groundCastDistance = Mathf.Max(0.01f, player.groundCastDistance);
            player.damageInvincibilityDuration = Mathf.Max(0.05f, player.damageInvincibilityDuration);
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
            special.effectYOffset = Mathf.Max(0f, special.effectYOffset);
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
        public float width = 16f;
        public float height = 9f;
        public float stageFloorY = -2.5f;
        public float minStageX = -7.55f;
        public float maxStageX = 7.55f;
        public float playerSpawnX = -6.2f;
        public float stageColliderThickness = 0.8f;
        public float foregroundCropHeight = 0.24f;
        public int foregroundSortingOrder = 48;
    }

    [Serializable]
    public sealed class PlayerConfig
    {
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
        public float muzzleOffsetX = 0.86f;
        public float muzzleOffsetY = 0.28f;
        public float airFlipDuration = 0.42f;
        public float groundCastDistance = 0.06f;
        public float damageInvincibilityDuration = 0.75f;
        public float hurtFlashDuration = 0.16f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.12f;
        public float groundAcceleration = 45f;
        public float groundDeceleration = 55f;
        public float airControl = 0.85f;
        public float jumpCutMultiplier = 0.45f;
        public float deathActionDuration = 0.38f;
        public float ghostRiseDuration = 1.2f;
        public float ghostRiseSpeed = 1.05f;
        public float deathResultDelay = 2f;
        public float colliderOffsetX = -0.45f;
        public float colliderOffsetY = -0.04f;
        public float colliderSizeX = 0.72f;
        public float colliderSizeY = 1.28f;
        public float visualCenterY = 0.68f;
        public string superKey = "K";
    }

    [Serializable]
    public sealed class BossConfig
    {
        public int maxHealth = 100;
        public float scale = 4.5f;
        public float groundEmbedDepth = 0.42f;
        public float spawnX = 6.3f;
        public float attackCooldownMin = 0.8f;
        public float attackCooldownMax = 1.2f;
        public float attackDuration = 0.6f;
        public float attackAnimationFramesPerSecond = 10f;
        public float attackShotInterval = 0.18f;
        public int attackShotCount = 3;
        public float projectileSpawnOffsetX = 2.4f;
        public float projectileSpawnOffsetYTop = 3.8f;
        public float projectileSpawnOffsetYMiddle = 2.3f;
        public float projectileSpawnOffsetYBottom = 0.8f;
    }

    [Serializable]
    public sealed class ProjectileConfig
    {
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
        public int damage = 30;
        public float duration = 0.85f;
        public float effectYOffset = 0.85f;
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
                current = JsonUtility.FromJson<GameConfig>(asset.text) ?? GameConfig.CreateDefault();
            }
            catch (Exception exception)
            {
                current = GameConfig.CreateDefault();
                Debug.LogWarning($"全局 JSON 配置解析失败，已使用代码默认配置：{exception.Message}");
            }

            current.Normalize();
            loaded = true;
            Debug.Log($"全局配置加载完成：最大能量 {current.player.maxEnergy} 格，Boss 生命 {current.boss.maxHealth}。");
        }
    }
}

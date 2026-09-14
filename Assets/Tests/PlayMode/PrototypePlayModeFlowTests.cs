#if UNITY_INCLUDE_TESTS
using System.Collections;
using System.Linq;
using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Config;
using MMORPG.Game.Core;
using MMORPG.Game.Player;
using MMORPG.Game.Projectiles;
using MMORPG.Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;

namespace MMORPG.Tests.PlayMode
{
    public sealed class PrototypePlayModeFlowTests
    {
        [SetUp] public void LegacySetup() => GameManager.LegacyTestMode = true;
        [TearDown] public void LegacyTeardown() => GameManager.LegacyTestMode = false;
        [UnityTest]
        public IEnumerator MainSceneCanRunBossEnergyAndSuperFlow()
        {
            Random.InitState(20260802);
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(3f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
            PotatoBossController boss = Object.FindFirstObjectByType<PotatoBossController>();
            PlayerEnergyMeter meter = Object.FindFirstObjectByType<PlayerEnergyMeter>();

            Assert.IsNotNull(gameManager, "开始游戏后应存在 GameManager。");
            Assert.IsNotNull(player, "开始游戏后应生成玩家。");
            Assert.IsNotNull(boss, "开始游戏后应生成土豆 Boss。");
            Assert.IsNotNull(meter, "开始游戏后应生成能量 HUD。");
            Assert.IsNotNull(player.Energy, "玩家应挂载能量组件。");
            Assert.AreEqual(3, player.Energy.MaxEnergy, "能量上限应为三格。");

            player.GrantInvincibility(30f);
            player.transform.position = new Vector3(GameConfigService.Current.level.minStageX, GameConfigService.Current.level.stageFloorY, 0f);

            yield return new WaitForSeconds(2f);
            Assert.Greater(boss.transform.position.x, player.transform.position.x, "Boss 应位于玩家右侧。");
            float stageCenterX = (GameConfigService.Current.level.minStageX + GameConfigService.Current.level.maxStageX) * 0.5f;
            Assert.Less(player.transform.position.x, stageCenterX, "玩家应位于画面左侧。");
            Assert.Greater(boss.transform.position.x, stageCenterX, "Boss 应位于画面右侧。");
            Assert.GreaterOrEqual(BossProjectile.TotalSpawned, 3, "Boss 一次攻击应完成至少三颗连射。");
            Assert.Greater(BossProjectile.TotalTopSpawned, 0, "Boss 应生成上方弹道子弹。");
            Assert.Greater(BossProjectile.TotalMiddleSpawned, 0, "Boss 应生成中间弹道子弹。");
            Assert.Greater(BossProjectile.TotalBottomSpawned, 0, "Boss 应生成下方弹道子弹。");
            Assert.That(BossProjectile.LastSpawnedVelocity.y, Is.EqualTo(0f).Within(0.001f), "Boss 子弹应保持水平飞行。");
            Assert.Less(BossProjectile.LastSpawnedVelocity.x, -7f, "Boss 子弹水平速度应明显加快并向玩家方向飞行。");

            yield return new WaitForSeconds(6f);
            Assert.Greater(BossProjectile.TotalNormalSpawned, 0, "Boss 应生成普通土块子弹。");
            Assert.Greater(BossProjectile.TotalPinkSpawned, 0, "Boss 应生成紫色可采集子弹。");

            BossProjectile pink = Object.FindObjectsByType<BossProjectile>(FindObjectsSortMode.None)
                .FirstOrDefault(projectile => projectile.ProjectileType == BossProjectileType.Pink);
            if (pink == null)
            {
                pink = BossProjectile.Spawn(player.transform.position + Vector3.right, Vector2.zero, BossProjectileType.Pink);
            }

            Assert.IsTrue(pink.TryCollectByPlayerAttack(), "玩家攻击命中紫色子弹后应获得能量。");
            player.Energy.AddEnergy(player.Energy.MaxEnergy - player.Energy.CurrentEnergy);
            int healthBeforeSuper = boss.CurrentHealth;
            Assert.IsTrue(player.TryActivateSuper(), "满三格能量后应可以释放大招。");
            Assert.AreEqual(0, player.Energy.CurrentEnergy, "释放大招后能量应清零。");
            Assert.AreEqual(healthBeforeSuper, boss.CurrentHealth, "大招预备期间不应提前造成不可见伤害。");
            yield return new WaitForSeconds(0.2f);
            Assert.Less(boss.CurrentHealth, healthBeforeSuper, "大招应对 Boss 造成伤害。");
        }
        [UnityTest]
        public IEnumerator BossBattleHudShowsHealthAndResultState()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1.5f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            Assert.IsNotNull(EventSystem.current, "战斗 HUD 应该创建可交互的 EventSystem。");
            PlayerController2D player = gameManager.Player;
            PotatoBossController boss = gameManager.Boss;
            boss.SetAiEnabled(false);
            BossProjectile.DespawnAllActive();

            Assert.IsNotNull(gameManager.BattleHud, "战斗 HUD 应该在关卡开始时创建。");
            Assert.AreEqual(1f, gameManager.BattleHud.BossHealthNormalized, 0.001f, "Boss 初始血条应该是满的。");
            Assert.AreEqual(1f, gameManager.BattleHud.PlayerHealthNormalized, 0.001f, "玩家初始生命条应该是满的。");

            boss.TakeDamage(5);
            Assert.AreEqual(95, boss.CurrentHealth, "Boss 受到攻击后生命应该减少。");
            Assert.That(gameManager.BattleHud.BossHealthNormalized, Is.EqualTo(0.95f).Within(0.001f), "Boss 血条应该同步减少。");

            player.TakeDamage(1);
            Assert.AreEqual(player.MaxHealth - 1, player.CurrentHealth, "玩家受击后生命应该减少。");
            Assert.That(gameManager.BattleHud.PlayerHealthNormalized, Is.EqualTo(2f / 3f).Within(0.001f), "玩家生命条应该同步减少。");
            player.TakeDamage(1);
            Assert.AreEqual(player.MaxHealth - 1, player.CurrentHealth, "玩家无敌帧内不应重复扣血。");
            yield return new WaitForSeconds(GameConfigService.Current.player.damageInvincibilityDuration + 0.05f);
            player.TakeDamage(1);
            Assert.AreEqual(player.MaxHealth - 2, player.CurrentHealth, "无敌帧结束后应允许再次扣血。");

            yield return null;
        }

        [UnityTest]
        public IEnumerator BossDeathAndPlayerDeathShowCorrectResult()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            gameManager.Boss.TakeDamage(gameManager.Boss.CurrentHealth);
            yield return null;

            Assert.IsTrue(gameManager.Boss.IsDead, "Boss 生命归零后应该进入死亡状态。");
            Assert.IsTrue(gameManager.BattleHud.IsVictoryShown, "Boss 死亡后应该显示胜利界面。");

            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1f);

            gameManager = Object.FindFirstObjectByType<GameManager>();
            gameManager.Player.TakeDamage(gameManager.Player.CurrentHealth);
            yield return null;

            Assert.IsTrue(gameManager.Player.IsDead, "玩家生命归零后应该进入死亡状态。");
            yield return new WaitForSeconds(GameConfigService.Current.player.deathResultDelay + 0.2f);
            Assert.IsTrue(gameManager.BattleHud.IsDefeatShown, "玩家死亡后应该显示失败界面。");
        }

        [UnityTest]
        public IEnumerator BossAdvancesThroughThreePhasesOnDamageThresholds()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            PotatoBossController boss = gameManager.Boss;

            Assert.AreEqual(1, boss.CurrentPhase, "Boss 初始应处于阶段 1。");

            boss.TakeDamage(30);
            yield return null;
            Assert.AreEqual(70, boss.CurrentHealth, "阶段切换阈值计算基于受伤后血量。");
            boss.TakeDamage(5);
            Assert.AreEqual(70, boss.CurrentHealth, "阶段切换安全窗内 Boss 不应继续受到伤害。");
            Assert.AreEqual(2, boss.CurrentPhase, "累计 30 点伤害后 Boss 应进入阶段 2。");
            Assert.IsTrue(boss.IsInTransition, "阶段切换期间应处于安全窗口。");

            yield return new WaitForSeconds(GameConfigService.Current.boss.phaseTransitionDuration + 0.05f);
            Assert.IsFalse(boss.IsInTransition, "安全窗口结束后应恢复攻击。");

            boss.TakeDamage(35);
            yield return null;
            Assert.AreEqual(35, boss.CurrentHealth, "累计 65 点伤害后 Boss 血量应为 35。");
            Assert.AreEqual(3, boss.CurrentPhase, "累计 65 点伤害后 Boss 应进入阶段 3。");
        }

        [UnityTest]
        public IEnumerator PhaseTransitionClearsBossProjectilesAndBlocksAttacks()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            PotatoBossController boss = gameManager.Boss;

            boss.TakeDamage(30);
            yield return null;

            Assert.AreEqual(0, BossProjectile.ActiveCount, "阶段切换应清空场上 Boss 投射物。");
            int spawnedDuringWindow = BossProjectile.TotalSpawned;
            yield return new WaitForSeconds(GameConfigService.Current.boss.phaseTransitionDuration * 0.5f);
            Assert.AreEqual(spawnedDuringWindow, BossProjectile.TotalSpawned, "安全窗口内 Boss 不应再发射投射物。");

            yield return new WaitForSeconds(GameConfigService.Current.boss.phaseTransitionDuration + 0.3f);
        }
        [UnityTest]
        public IEnumerator AirbornePlayerCanBreakPinkProjectileAndGainEnergy()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            gameManager.Boss.SetBattleLocked(true);
            PlayerController2D player = gameManager.Player;
            player.transform.position += Vector3.up * 1.5f;
            yield return new WaitForFixedUpdate();
            yield return null;

            BossProjectile.DespawnAllActive();
            BossProjectile pink = BossProjectile.Spawn(player.transform.position, Vector2.zero, BossProjectileType.Pink);
            int energyBefore = player.Energy.CurrentEnergy;
            Assert.IsTrue(player.TryBreakPinkProjectile(pink), "玩家处于空中时应该可以跳跃击碎粉色子弹。");
            Assert.AreEqual(energyBefore + 1, player.Energy.CurrentEnergy, "击碎粉色子弹后应该获得一格能量。");
            Assert.AreEqual(0, BossProjectile.ActiveCount, "粉色子弹被跳跃击碎后不应继续留在场景中。");
        }

        [Test]
        public void GameConfigNormalizeRestoresNullSections()
        {
            GameConfig config = new GameConfig
            {
                level = null,
                player = null,
                boss = new BossConfig
                {
                    phase1 = null,
                    phase2 = null,
                    phase3 = null
                },
                projectile = null,
                special = null
            };

            config.Normalize();

            Assert.IsNotNull(config.level, "空配置应补齐关卡默认段。");
            Assert.IsNotNull(config.player, "空配置应补齐玩家默认段。");
            Assert.IsNotNull(config.boss.phase1, "空配置应补齐 Boss 阶段 1 默认段。");
            Assert.IsNotNull(config.boss.phase2, "空配置应补齐 Boss 阶段 2 默认段。");
            Assert.IsNotNull(config.boss.phase3, "空配置应补齐 Boss 阶段 3 默认段。");
            Assert.IsNotNull(config.projectile, "空配置应补齐投射物默认段。");
            Assert.IsNotNull(config.special, "空配置应补齐大招默认段。");
        }

        [UnityTest]
        public IEnumerator PooledBossHazardsCanBeCleared()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            PlayerController2D player = gameManager.Player;
            Vector3 floorPosition = new Vector3(0f, GameConfigService.Current.level.stageFloorY, -0.2f);

            BossProjectile.DespawnAllActive();
            BossRootHazard.DespawnAllActive();
            BossInsect.DespawnAllActive();
            BossLobbedSeed.DespawnAllActive();

            BossProjectile.Spawn(new Vector3(0f, 1f, 0f), Vector2.left, BossProjectileType.Normal);
            BossRootHazard.Spawn(floorPosition);
            BossInsect.Spawn(new Vector3(0f, 1.5f, 0f), player.transform);
            BossLobbedSeed.Spawn(new Vector3(0f, 1.5f, 0f), Vector2.zero, floorPosition.y);

            Assert.Greater(BossProjectile.ActiveCount, 0, "Boss 子弹生成后应登记到活动集合。");
            Assert.Greater(BossRootHazard.ActiveCount, 0, "根须危险物生成后应登记到活动集合。");
            Assert.Greater(BossInsect.ActiveCount, 0, "昆虫生成后应登记到活动集合。");
            Assert.Greater(BossLobbedSeed.ActiveCount, 0, "抛物线种子生成后应登记到活动集合。");

            BossProjectile.DespawnAllActive();
            BossRootHazard.DespawnAllActive();
            BossInsect.DespawnAllActive();
            BossLobbedSeed.DespawnAllActive();

            Assert.AreEqual(0, BossProjectile.ActiveCount, "Boss 子弹清场后不应残留。");
            Assert.AreEqual(0, BossRootHazard.ActiveCount, "根须危险物清场后不应残留。");
            Assert.AreEqual(0, BossInsect.ActiveCount, "昆虫清场后不应残留。");
            Assert.AreEqual(0, BossLobbedSeed.ActiveCount, "抛物线种子清场后不应残留。");
        }

    }
}
#endif

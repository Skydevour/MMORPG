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

namespace MMORPG.Tests.PlayMode
{
    public sealed class PrototypePlayModeFlowTests
    {
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
            Assert.Less(boss.CurrentHealth, healthBeforeSuper, "大招应对 Boss 造成伤害。");
        }
        [UnityTest]
        public IEnumerator BossBattleHudShowsHealthAndResultState()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            yield return new WaitForSeconds(1.5f);

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            PlayerController2D player = gameManager.Player;
            PotatoBossController boss = gameManager.Boss;

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
            Assert.IsTrue(gameManager.BattleHud.IsDefeatShown, "玩家死亡后应该显示失败界面。");
        }
    }
}
#endif

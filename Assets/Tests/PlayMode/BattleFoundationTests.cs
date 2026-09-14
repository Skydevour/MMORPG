#if UNITY_INCLUDE_TESTS
using System.Collections;
using System.IO;
using System.Linq;
using MMORPG.Framework.Animation;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Core;
using MMORPG.Game.Projectiles;
using MMORPG.Game.VFX;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MMORPG.Tests.PlayMode
{
    public sealed class BattleFoundationTests
    {
        private Keyboard keyboard;
        private GameManager manager;
        private InputSettings.BackgroundBehavior oldBackground;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
#endif

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;
            oldBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>();
            GameManager.LegacyTestMode = true;
            SceneManager.LoadScene("MainScene");
            yield return null;
            manager = Object.FindFirstObjectByType<GameManager>();
            manager.Boss.SetAiEnabled(false);
            yield return new WaitForSeconds(0.15f);
        }

        [TearDown]
        public void TearDown()
        {
            GameManager.LegacyTestMode = false;
            Time.timeScale = 1f;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
#endif
        }

        private IEnumerator Keys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            yield return null;
            yield return null;
            if (Time.timeScale > 0f) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator KeyboardMovementJumpAndDashRemainResponsive()
        {
            var player = manager.Player;
            var body = player.GetComponent<Rigidbody2D>();
            yield return Keys(Key.D, Key.J);
            Assert.That(body.linearVelocity.x, Is.EqualTo(GameConfigService.Current.player.moveSpeed).Within(0.01f), "按下移动即达到目标速度。");
            Assert.AreEqual("run", player.GetComponentInChildren<FrameAnimator>().CurrentClipName, "跑射不能覆盖跑动动作。");
            yield return Keys();
            Assert.That(body.linearVelocity.x, Is.EqualTo(0f).Within(0.01f), "松手立即停止。");
            yield return Keys(Key.Space);
            Assert.Greater(body.linearVelocity.y, 0f, "真实空格输入触发跳跃。");
            yield return Keys();
            yield return Keys(Key.Space);
            Assert.AreEqual(2, player.JumpSequence, "第二次空格触发二段跳。");
            yield return Keys();
            yield return Keys(Key.Space);
            Assert.AreEqual(2, player.JumpSequence, "不能获得第三段跳。");
            yield return Keys(Key.D, Key.L);
            Assert.IsTrue(player.IsDashing, "L 触发闪避。");
            Assert.IsTrue(player.IsInvincible, "闪避期间无敌。");
            yield return Keys(Key.A);
            Assert.Greater(body.linearVelocity.x, 0f, "闪避期间方向固定。");
        }

        [UnityTest]
        public IEnumerator PauseButtonResumesAndBlocksSuperConsumption()
        {
            manager.Player.Energy.AddEnergy(3);
            manager.SetPaused(true);
            Assert.IsFalse(manager.Player.TryActivateSuper(), "暂停中不能释放大招。");
            Assert.AreEqual(3, manager.Player.Energy.CurrentEnergy);
            var button = Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(x => x.name == "ResumeButton");
            button.onClick.Invoke();
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(manager.BattleHud.IsPaused);
            yield return Keys(Key.Escape);
            Assert.IsTrue(manager.BattleHud.IsPaused);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            yield return null;
            yield return null;
            Assert.IsFalse(manager.BattleHud.IsPaused);
        }

        [UnityTest]
        public IEnumerator PinkCollectionAndDirectionalSuperAreSingleUse()
        {
            var pink = BossProjectile.Spawn(manager.Player.transform.position + Vector3.up * 3f, Vector2.zero, BossProjectileType.Pink);
            Assert.IsTrue(pink.TryCollectByPlayerAttack());
            Assert.IsFalse(pink.TryCollectByJump());
            Assert.IsFalse(pink.TryCollectByPlayerAttack());
            Assert.AreEqual(1, manager.Player.Energy.CurrentEnergy);
            manager.Player.Energy.AddEnergy(2);
            yield return Keys(Key.A);
            int hp = manager.Boss.CurrentHealth;
            Assert.IsTrue(manager.Player.TryActivateSuper());
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual(hp, manager.Boss.CurrentHealth, "背向大招不得扣右侧 Boss 生命。");
            yield return Keys(Key.D);
            yield return Keys();
            manager.Player.Energy.AddEnergy(3);
            Assert.IsTrue(manager.Player.TryActivateSuper());
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual(hp - 30, manager.Boss.CurrentHealth, "范围命中且全程仅扣一次伤害。");
        }

        [UnityTest]
        public IEnumerator ResourcesDamageAndShakeMatchVisibleState()
        {
            var sprite = manager.Player.GetComponentInChildren<SpriteRenderer>().sprite;
            Assert.That(sprite.pivot.x, Is.EqualTo(99f).Within(0.1f));
            Assert.That(sprite.pivot.y, Is.EqualTo(1f).Within(0.1f));
            Assert.Greater(manager.Boss.transform.position.x, GameConfigService.Current.camera.centerX);
            Assert.Less(manager.Boss.GetComponent<SpriteRenderer>().bounds.max.x, GameConfigService.Current.camera.Right);
            manager.Player.TakeDamage(1);
            manager.Player.TakeDamage(1);
            Assert.AreEqual(2, manager.Player.CurrentHealth);
            var properties = new MaterialPropertyBlock();
            manager.Player.GetComponentInChildren<SpriteRenderer>().GetPropertyBlock(properties);
            Assert.Greater(properties.GetFloat("_FlashAmount"), 0f, "受击写入真实闪白材质参数。");
            Vector3 origin = Camera.main.transform.localPosition;
            yield return new WaitForSeconds(0.2f);
            origin = Camera.main.transform.localPosition;
            for (int i = 0; i < 100; i++) { ScreenShakeEffect.Shake(0.04f, 0.05f); yield return null; }
            yield return new WaitForSeconds(0.2f);
            Assert.That(Vector3.Distance(origin, Camera.main.transform.localPosition), Is.LessThan(0.001f));
            yield return new WaitForSeconds(3.05f);
            manager.Player.TakeDamage(1);
            Assert.AreEqual(1, manager.Player.CurrentHealth);
            Capture("battle", 1280, 720);
        }

        [UnityTest]
        public IEnumerator ActualProjectileContactsDealDamageAndCollectPink()
        {
            var body = manager.Player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(0f, 0f);
            body.gravityScale = 0f;
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(manager.Player.IsGrounded);
            BossProjectile.Spawn(manager.Player.GetComponent<Collider2D>().bounds.center, Vector2.zero, BossProjectileType.Pink);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(1, manager.Player.Energy.CurrentEnergy, "真实触发碰撞通过空中接触采集粉色弹。");
            Assert.AreEqual(3, manager.Player.CurrentHealth);
            var bossBounds = manager.Boss.GetComponent<Collider2D>().bounds;
            int health = manager.Boss.CurrentHealth;
            PlayerProjectile.Spawn(new Vector3(bossBounds.min.x - 0.25f, bossBounds.center.y, 0f), 1);
            yield return new WaitForSeconds(0.12f);
            Assert.AreEqual(health - 1, manager.Boss.CurrentHealth, "真实玩家弹碰撞应扣 Boss 生命。");
        }

        [UnityTest]
        public IEnumerator EffectsAndTenRetriesDoNotAccumulate()
        {
            Vector3 smokePosition = new Vector3(3f, 2f, 0f);
            DodgeSmokeEffect.Spawn(smokePosition, 1);
            var emitted = new ParticleSystem.Particle[64];
            var emitter = Object.FindFirstObjectByType<PooledBattleEffect>().GetComponent<ParticleSystem>();
            Assert.Greater(emitter.GetParticles(emitted), 0);
            Assert.Less(Vector3.Distance(emitted[0].position, smokePosition + Vector3.up * 0.25f), 0.01f, "世界空间粒子必须在指定爆烟位置生成。");
            yield return new WaitForSeconds(0.6f);
            int materials = Resources.FindObjectsOfTypeAll<Material>().Length;
            for (int i = 0; i < 100; i++)
            {
                DodgeSmokeEffect.Spawn(Vector3.zero, 1);
                yield return new WaitForSeconds(0.02f);
            }
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(materials, Resources.FindObjectsOfTypeAll<Material>().Length, "粒子复用共享材质。");
            Assert.AreEqual(0, Object.FindObjectsByType<PooledBattleEffect>(FindObjectsSortMode.None).Length);
            manager.Player.TakeDamage(99);
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(manager.Player.IsGhost);
            Assert.AreEqual("ghost", manager.Player.GetComponentInChildren<FrameAnimator>().CurrentClipName);
            Capture("ghost", 1280, 720);
            yield return new WaitForSeconds(1.85f);
            Assert.IsTrue(manager.BattleHud.IsDefeatShown);
            Capture("defeat", 1280, 720);
            for (int i = 0; i < 10; i++)
            {
                manager.RestartBattle();
                yield return null;
                yield return null;
                manager = Object.FindFirstObjectByType<GameManager>();
                manager.Boss.SetAiEnabled(false);
                Assert.AreEqual(3, manager.Player.CurrentHealth);
                Assert.AreEqual(0, manager.Player.Energy.CurrentEnergy);
                Assert.AreEqual(1f, Time.timeScale);
                Assert.AreEqual(1, Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length);
            }
        }

        private static void Capture(string name, int width, int height)
        {
            Directory.CreateDirectory("Plan/Validation/DEV-006");
            Canvas.ForceUpdateCanvases();
            Camera camera = Camera.main;
            var target = new RenderTexture(width, height, 24);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            Rect previousRect = camera.rect;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes($"Plan/Validation/DEV-006/{name}.png", image.EncodeToPNG());
            camera.targetTexture = previousTarget;
            camera.rect = previousRect;
            RenderTexture.active = previousActive;
            Object.Destroy(image);
            Object.Destroy(target);
        }
    }
}
#endif

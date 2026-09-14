#if UNITY_INCLUDE_TESTS
using System.Collections;
using MMORPG.Framework.Timing;
using MMORPG.Game.Audio;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Core;
using MMORPG.Game.Projectiles;
using MMORPG.Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MMORPG.Tests.PlayMode
{
    public sealed class GardenEncounterTests
    {
        private GameManager manager;
        private Keyboard keyboard;
        private InputSettings.BackgroundBehavior oldBackground;
        private InputSettings.EditorInputBehaviorInPlayMode oldInput;
        [UnitySetUp] public IEnumerator Setup()
        {
            GameManager.LegacyTestMode = false;
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            SceneManager.LoadScene("MainScene"); yield return null; yield return null;
            manager = Object.FindFirstObjectByType<GameManager>();
        }
        [TearDown] public void Teardown()
        {
            BattleClock.ResetSession();
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldInput;
        }
        private IEnumerator Begin()
        {
            manager.Encounter.Begin();
            var title = Object.FindFirstObjectByType<BattleTitleView>(); if (title != null) Object.Destroy(title.gameObject);
            yield return new WaitForSeconds(GameConfigService.Current.encounter.introDuration + 0.1f);
            manager.Boss.SetAiEnabled(false);
        }
        [UnityTest] public IEnumerator AirbornePresentationUsesVelocityAndKeepsFlipMirrorStable()
        {
            yield return Begin();
            var player = manager.Player;
            var animator = player.GetComponentInChildren<MMORPG.Framework.Animation.FrameAnimator>();
            var body = player.GetComponent<Rigidbody2D>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(0.14f);
            Assert.AreEqual("rise", animator.CurrentClipName);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space, Key.D));
            yield return new WaitForSeconds(0.08f);
            Assert.IsTrue(player.IsAirFlipping);
            var mirror = player.transform.Find("VisualPivot/VisualMirror");
            float mirrored = mirror.localScale.x;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space, Key.A));
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(-1, player.FacingDirection);
            Assert.AreEqual(mirrored, mirror.localScale.x);
            Assert.Less(body.linearVelocity.x, 0f);
            Assert.AreEqual(Quaternion.identity, player.transform.rotation);
            yield return new WaitForSeconds(0.25f);
            Assert.AreEqual("fall", animator.CurrentClipName);
            Capture("airborne_fall");
        }

        [UnityTest] public IEnumerator DashDistanceUsesPhysicsTimeAndDoesNotOvershootOnUnevenSteps()
        {
            yield return Begin();
            float oldStep = Time.fixedDeltaTime;
            var body = manager.Player.GetComponent<Rigidbody2D>();
            try
            {
                foreach (float step in new[] { 0.02f, 0.025f, 1f / 60f })
                {
                    Time.fixedDeltaTime = step;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    yield return new WaitForSeconds(0.4f);
                    body.position = new Vector2(-4f, GameConfigService.Current.level.stageFloorY);
                    body.linearVelocity = Vector2.zero;
                    yield return new WaitForFixedUpdate();
                    float start = body.position.x;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.L));
                    yield return new WaitForSeconds(0.3f);
                    float expected = GameConfigService.Current.player.dashSpeed * GameConfigService.Current.player.dashDuration;
                    Assert.That(body.position.x - start, Is.EqualTo(expected).Within(0.025f), "闪避位移必须由配置时长和物理步决定。");
                    Assert.IsFalse(manager.Player.IsDashing);
                    Assert.IsFalse(manager.Player.IsInvincible, "闪避结束后不能残留额外无敌。");
                }
            }
            finally { Time.fixedDeltaTime = oldStep; }
        }

        [UnityTest] public IEnumerator ShortJumpReleaseDeceleratesBeforeReachingItsTarget()
        {
            yield return Begin();
            var body = manager.Player.GetComponent<Rigidbody2D>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSeconds(0.06f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            float before = body.linearVelocity.y;
            yield return new WaitForFixedUpdate();
            float after = body.linearVelocity.y;
            Assert.LessOrEqual(after, before, "短跳不能增加向上速度。");
            Assert.Greater(after, GameConfigService.Current.player.jumpVelocity * GameConfigService.Current.player.jumpCutMultiplier,
                "松键不应在首个物理步直接截到目标速度。");
            yield return new WaitForSeconds(0.08f);
            Assert.LessOrEqual(body.linearVelocity.y, GameConfigService.Current.player.jumpVelocity * GameConfigService.Current.player.jumpCutMultiplier);
        }
        [UnityTest] public IEnumerator JumpDuringHurtKeepsPriorityAndTransitionSettlesRotation()
        {
            yield return Begin();
            yield return Press(Key.Space); yield return Press();
            manager.Player.ReceiveHit(new HitContext(1, manager.Player.FootPosition));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return new WaitForSecondsRealtime(0.08f);
            Assert.IsTrue(manager.Player.IsHurt);
            Assert.AreEqual("hurt", manager.Player.GetComponentInChildren<MMORPG.Framework.Animation.FrameAnimator>().CurrentClipName);
            yield return new WaitForSeconds(0.8f);
            yield return Press(); yield return Press(Key.Space); yield return Press(); yield return Press(Key.Space);
            yield return new WaitForSeconds(0.08f);
            Assert.IsTrue(manager.Player.IsAirFlipping);
            manager.Boss.TakeDamage(999);
            yield return new WaitForSeconds(0.12f);
            Assert.AreEqual(EncounterState.Transition, manager.Encounter.State);
            Assert.Less(Quaternion.Angle(Quaternion.identity, manager.Player.transform.Find("VisualPivot").localRotation), 1f);
        }

        [UnityTest] public IEnumerator UiHealthEnergyAndDefeatBindingsSurviveRetry()
        {
            yield return Begin();
            manager.Player.Energy.AddEnergy(3);
            manager.Player.ReceiveHit(new HitContext(1, manager.Player.FootPosition));
            yield return new WaitForSecondsRealtime(0.35f);
            Assert.AreEqual(2, manager.BattleHud.GetComponentInChildren<PlayerHealthCardView>().CurrentHealth);
            var meter = manager.BattleHud.GetComponentInChildren<PlayerEnergyMeter>();
            Assert.IsNotNull(meter);
            foreach (var slot in meter.slots) Assert.That(slot.transform.localScale.x, Is.EqualTo(1f).Within(0.01f));
            Capture("hud_hp2_energy3");
            manager.SetPaused(true); yield return new WaitForSecondsRealtime(0.2f); Capture("pause");
            manager.SetPaused(false); yield return new WaitForSecondsRealtime(0.2f);
            yield return new WaitForSeconds(3.1f);
            manager.Player.ReceiveHit(new HitContext(999, manager.Player.FootPosition));
            yield return new WaitForSecondsRealtime(2.4f);
            Assert.IsTrue(manager.BattleHud.IsDefeatShown); Capture("defeat");
            var retry = System.Array.Find(manager.BattleHud.GetComponentsInChildren<UnityEngine.UI.Button>(), button => button.name == "RetryButton");
            Assert.IsTrue(retry.IsInteractable()); retry.onClick.Invoke(); yield return null; yield return null;
            manager = Object.FindFirstObjectByType<GameManager>();
            Assert.AreEqual(3, manager.Player.CurrentHealth);
            Assert.AreEqual(0, manager.Player.Energy.CurrentEnergy);
        }
        [UnityTest] public IEnumerator FastSettingsEntryRestoresOpaquePauseAndResultClearsEnergyTrail()
        {
            yield return Begin();
            manager.SetPaused(true);
            BattleTitleView.Create(manager, true);
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.AreEqual(0f, manager.BattleHud.PauseGroup.alpha);
            var settings = Object.FindFirstObjectByType<BattleTitleView>().transform.Find("Settings").GetComponent<GardenUiScreen>();
            settings.Get<UnityEngine.UI.Button>("back").onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.AreEqual(1f, manager.BattleHud.PauseGroup.alpha);
            manager.SetPaused(false);
            manager.Player.Energy.AddEnergy(1); PlayerEnergyMeter.ShowCollection(manager.Player.FootPosition);
            Assert.IsNotEmpty(Object.FindObjectsByType<EnergyTrailView>(FindObjectsSortMode.None));
            manager.BattleHud.ShowVictory();
            Assert.IsEmpty(Object.FindObjectsByType<EnergyTrailView>(FindObjectsSortMode.None));
        }
        [UnityTest] public IEnumerator SettingsSliderFillStaysInsideTrackAtAllValues()
        {
            yield return Press(Key.S); yield return Press(Key.Enter);
            var title = Object.FindFirstObjectByType<BattleTitleView>();
            foreach (var slider in title.GetComponentsInChildren<UnityEngine.UI.Slider>())
            {
                foreach (float value in new[] { 0f, 0.5f, 1f })
                {
                    slider.SetValueWithoutNotify(value); Canvas.ForceUpdateCanvases();
                    var rail = (RectTransform)slider.transform;
                    Assert.That(slider.fillRect.rect.width, Is.EqualTo(rail.rect.width * value).Within(0.1f));
                    Assert.That(slider.fillRect.rect.height, Is.EqualTo(rail.rect.height).Within(0.1f));
                }
            }
        }
        [UnityTest] public IEnumerator TitlePreventsAttacksAndThreeFormsOnlyWinAtEnd()
        {
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(EncounterState.Title, manager.Encounter.State);
            Capture("title");
            Assert.IsFalse(manager.Player.GetComponentInChildren<SpriteRenderer>().enabled);
            Assert.IsFalse(manager.Boss.GetComponent<SpriteRenderer>().enabled);
            Assert.AreEqual(0, BossProjectile.ActiveCount);
            yield return Begin();
            Assert.IsTrue(manager.Player.GetComponentInChildren<SpriteRenderer>().enabled);
            Assert.IsTrue(manager.Boss.GetComponent<SpriteRenderer>().enabled);
            manager.Player.Energy.AddEnergy(2);
            int hp = manager.Player.CurrentHealth;
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, manager.Encounter.FormIndex);
                Capture("form_" + i);
                manager.Encounter.CurrentBoss.TakeDamage(999);
                yield return null;
                if (i < 2)
                {
                    Assert.IsFalse(manager.BattleHud.IsVictoryShown);
                    yield return new WaitForSeconds(GameConfigService.Current.encounter.transitionDuration + 0.1f);
                    Assert.AreEqual(hp, manager.Player.CurrentHealth);
                    Assert.AreEqual(2, manager.Player.Energy.CurrentEnergy);
                    Assert.AreEqual(0, BossProjectile.ActiveCount);
                }
            }
            Assert.AreEqual(EncounterState.Victory, manager.Encounter.State);
            Assert.IsTrue(manager.BattleHud.IsVictoryShown);
            yield return new WaitForSecondsRealtime(0.4f); Capture("victory");
        }
        [UnityTest] public IEnumerator DashContactKeepsBothProjectileKindsAndTheirLease()
        {
            yield return Begin();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.L));
            yield return null; yield return null;
            Assert.IsTrue(manager.Player.IsDashing);
            int energy = manager.Player.Energy.CurrentEnergy;
            foreach (var kind in new[] { BossProjectileType.Normal, BossProjectileType.Pink })
            {
                var bullet = BossProjectile.Spawn(manager.Player.GetComponent<Collider2D>().bounds.center, Vector2.left * 8f, kind);
                int lease = bullet.Generation;
                yield return new WaitForFixedUpdate();
                Assert.IsTrue(bullet.gameObject.activeSelf);
                Assert.AreEqual(lease, bullet.Generation);
                Assert.AreEqual(Vector2.left * 8f, bullet.Velocity);
            }
            Assert.AreEqual(3, manager.Player.CurrentHealth); Assert.AreEqual(energy, manager.Player.Energy.CurrentEnergy);
        }
        [UnityTest] public IEnumerator ImmunityRejectsDamageWithoutConsumingProjectile()
        {
            yield return Begin();
            manager.Player.GrantInvincibility(3f);
            var bullet = BossProjectile.Spawn(manager.Player.GetComponent<Collider2D>().bounds.center, Vector2.left, BossProjectileType.Normal);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(bullet.gameObject.activeSelf);
            Assert.AreEqual(HitResult.Invulnerable, manager.Player.ReceiveHit(new HitContext(1, manager.Player.transform.position)));
            Assert.AreEqual(3, manager.Player.CurrentHealth);
        }
        [UnityTest] public IEnumerator PinkParryBouncesOnceAndCapsEnergy()
        {
            yield return Begin();
            var body = manager.Player.GetComponent<Rigidbody2D>(); body.position = new Vector2(-4f, 0f);
            yield return null;
            manager.Player.Energy.AddEnergy(3);
            var bullet = BossProjectile.Spawn(body.position + Vector2.right * 2f, Vector2.zero, BossProjectileType.Pink);
            int sequence = manager.Player.JumpSequence;
            Assert.IsTrue(manager.Player.TryBreakPinkProjectile(bullet));
            Assert.IsFalse(manager.Player.TryBreakPinkProjectile(bullet));
            Assert.IsTrue(manager.Player.IsParrying);
            Assert.That(body.linearVelocity.y, Is.EqualTo(GameConfigService.Current.feedback.parryBounce).Within(0.001f));
            Assert.AreEqual(3, manager.Player.Energy.CurrentEnergy); Assert.AreEqual(sequence, manager.Player.JumpSequence);
            Assert.IsTrue(BattleClock.HitStopped);
        }
        [UnityTest] public IEnumerator PauseCannotBeReleasedByHitStop()
        {
            yield return Begin();
            BattleClock.Stop(0.05f); manager.SetPaused(true);
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.AreEqual(0f, Time.timeScale); Assert.IsTrue(manager.BattleHud.IsPaused);
            manager.SetPaused(false); yield return new WaitForSecondsRealtime(0.1f);
            Assert.AreEqual(1f, Time.timeScale);
        }
        [UnityTest] public IEnumerator SimultaneousDeathsPreferDefeat()
        {
            yield return Begin();
            manager.Boss.TakeDamage(999); manager.Player.TakeDamage(999);
            yield return null;
            Assert.AreEqual(EncounterState.Defeat, manager.Encounter.State);
            yield return new WaitForSecondsRealtime(2.2f);
            Assert.IsTrue(manager.BattleHud.IsDefeatShown); Assert.IsFalse(manager.BattleHud.IsVictoryShown);
            yield return new WaitForSecondsRealtime(0.35f); Capture("defeat");
        }
        [UnityTest] public IEnumerator AudioBudgetAndBeamLifetimeRemainBounded()
        {
            yield return Begin();
            for (int i = 0; i < 80; i++) BattleAudio.Play(i % 2 == 0 ? "shot" : "parry_success");
            Assert.LessOrEqual(BattleAudio.ActiveVoices, 24);
            var beam = GardenHazard.SpawnBeam(new Vector3(5, 0, 0), Vector2.left);
            Assert.IsFalse(beam.IsDangerous);
            yield return new WaitForSeconds(0.58f);
            Assert.IsTrue(beam.IsDangerous);
            Assert.AreEqual(Vector2.left, beam.Direction);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0, GardenHazard.ActiveCount);
        }

        [UnityTest] public IEnumerator PersistentHazardCanDamageAfterImmunityExpires()
        {
            yield return Begin();
            manager.Player.GrantInvincibility(0.15f);
            var settings = GameConfigService.Current.encounter;
            float oldActive = settings.beamActive, oldTell = settings.beamTell;
            settings.beamTell = 0.02f; settings.beamActive = 0.6f;
            Vector3 center = manager.Player.GetComponent<Collider2D>().bounds.center;
            var beam = GardenHazard.SpawnBeam(center + Vector3.right * 2f, Vector2.left);
            yield return new WaitForSeconds(0.1f); Assert.AreEqual(3, manager.Player.CurrentHealth);
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(2, manager.Player.CurrentHealth); Assert.IsTrue(beam.gameObject.activeSelf);
            settings.beamActive = oldActive; settings.beamTell = oldTell;
        }

        private static void Capture(string name)
        {
            const int width = 1280, height = 720;
            System.IO.Directory.CreateDirectory("Plan/Validation/DEV-009");
            Canvas.ForceUpdateCanvases();
            var camera = Camera.main; var previous = camera.targetTexture; var active = RenderTexture.active; var previousRect = camera.rect;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            var target = new RenderTexture(width, height, 24); camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
            System.IO.File.WriteAllBytes("Plan/Validation/DEV-009/" + name + ".png", texture.EncodeToPNG());
            camera.targetTexture = previous; camera.rect = previousRect; RenderTexture.active = active; Object.Destroy(texture); Object.Destroy(target);
        }

        [UnityTest] public IEnumerator OnionAndCarrotExecuteTheirOwnAttacks()
        {
            yield return Begin(); manager.Player.GrantInvincibility(30f);
            manager.Boss.TakeDamage(999); yield return null;
            yield return new WaitForSeconds(GameConfigService.Current.encounter.transitionDuration + 0.1f);
            int spawnedBefore = BossProjectile.TotalSpawned;
            yield return new WaitForSeconds(2f); Capture("onion_rain");
            yield return new WaitForSeconds(2f);
            Assert.Greater(BossProjectile.TotalSpawned, spawnedBefore, "洋葱需要实际执行预警和泪雨生成。");
            Assert.AreEqual(0, BossRootHazard.ActiveCount, "新形态不能叠加旧土豆根须。");
            manager.Encounter.CurrentBoss.TakeDamage(999); yield return null;
            yield return new WaitForSeconds(GameConfigService.Current.encounter.transitionDuration + 0.1f);
            bool sawBeam = false, sawSeeker = false;
            float elapsed = 0f;
            while (elapsed < 7f)
            {
                yield return null; elapsed += Time.deltaTime;
                if (!sawBeam && GardenHazard.ActiveCount > 0) { sawBeam = true; Capture("carrot_tell"); }
                foreach (var projectile in Object.FindObjectsByType<BossProjectile>(FindObjectsSortMode.None))
                    if (projectile.IsSeeker) sawSeeker = true;
                Assert.LessOrEqual(BossProjectile.ActiveCount, 2, "胡萝卜追踪弹不能超过两颗。");
            }
            Assert.IsTrue(sawBeam); Assert.IsTrue(sawSeeker);
        }

        [UnityTest] public IEnumerator EnergyTrailUsesBoundCameraDuringOffscreenRendering()
        {
            yield return Begin();
            var camera = Camera.main; camera.enabled = false;
            manager.Player.Energy.AddEnergy(1);
            PlayerEnergyMeter.ShowCollection(manager.Player.transform.position);
            Assert.AreEqual(1, Object.FindObjectsByType<EnergyTrailView>(FindObjectsSortMode.None).Length);
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.AreEqual(0, Object.FindObjectsByType<EnergyTrailView>(FindObjectsSortMode.None).Length);
            camera.enabled = true;
        }

        private IEnumerator Press(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null; yield return null;
        }

        [UnityTest] public IEnumerator KeyboardMenusUseOneInputSystemAndDoNotLeakActions()
        {
            var events = UnityEngine.EventSystems.EventSystem.current;
            Assert.IsInstanceOf<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(events.currentInputModule);
            yield return Press(Key.S);
            Assert.AreEqual(GameConfigService.Current.presentation.settings, events.currentSelectedGameObject.name);
            yield return Press(Key.Enter);
            Assert.IsNotNull(events.currentSelectedGameObject.GetComponent<UnityEngine.UI.Slider>());
            yield return Press(Key.Escape);
            Assert.AreEqual(EncounterState.Title, manager.Encounter.State);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Press(Key.UpArrow);
            yield return Press(Key.Enter);
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.AreEqual(EncounterState.Intro, manager.Encounter.State);
            yield return new WaitForSeconds(1.4f);
            manager.Boss.SetAiEnabled(false);
            yield return Press(Key.Escape); Assert.IsTrue(manager.BattleHud.IsPaused);
            yield return Press(Key.DownArrow); yield return Press(Key.DownArrow); yield return Press(Key.Enter);
            Assert.IsTrue(BattleTitleView.IsOpen);
            yield return Press(Key.Escape);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.IsTrue(manager.BattleHud.IsPaused, "关闭设置不能同时取消暂停。");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J, Key.Escape));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
            yield return null; yield return null;
            Assert.IsFalse(manager.BattleHud.IsPaused);
            Assert.IsFalse(manager.Player.IsShooting, "菜单中按住攻击不能在恢复战斗时自动开火。");
            yield return Press();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
            yield return null; yield return null;
            Assert.IsTrue(manager.Player.IsShooting);
        }

        [UnityTest] public IEnumerator HitStopBuffersDashAndMultipleCollidersCollectOnlyOnce()
        {
            yield return Begin();
            manager.Player.GetComponent<Rigidbody2D>().position = new Vector2(-4f, 0f);
            var extra = manager.Player.gameObject.AddComponent<CircleCollider2D>(); extra.isTrigger = true; extra.radius = 0.3f;
            yield return null;
            var pink = BossProjectile.Spawn(manager.Player.GetComponent<Collider2D>().bounds.center, Vector2.zero, BossProjectileType.Pink);
            yield return new WaitForFixedUpdate(); yield return null;
            Assert.AreEqual(1, manager.Player.Energy.CurrentEnergy);
            Assert.IsFalse(pink.gameObject.activeSelf);
            BattleClock.Stop(0.15f);
            yield return Press(Key.L);
            yield return new WaitForSecondsRealtime(0.17f);
            Assert.IsTrue(manager.Player.IsDashing, "停顿中短按 L 应在恢复模拟后执行一次。");
            Assert.AreEqual(1, manager.Player.Energy.CurrentEnergy);
        }

        [UnityTest] public IEnumerator NewEncounterRetriesTenTimesWithoutOldHazardsOrListeners()
        {
            yield return Begin();
            for (int retry = 0; retry < 10; retry++)
            {
                if (retry % 2 == 0)
                {
                    manager.Encounter.CurrentBoss.TakeDamage(999); yield return null;
                    yield return new WaitForSeconds(1.4f);
                    Assert.AreEqual(1, manager.Encounter.FormIndex);
                }
                manager.Player.Energy.AddEnergy(2);
                GardenHazard.SpawnBeam(new Vector3(5, 2, 0), Vector2.left);
                var previous = manager;
                previous.RestartBattle(); previous.RestartBattle();
                yield return null; yield return null;
                manager = Object.FindFirstObjectByType<GameManager>();
                Assert.AreEqual(0, manager.Encounter.FormIndex);
                Assert.AreEqual(3, manager.Player.CurrentHealth); Assert.AreEqual(0, manager.Player.Energy.CurrentEnergy);
                Assert.AreEqual(0, GardenHazard.ActiveCount);
                Assert.AreEqual(1, Object.FindObjectsByType<EncounterDirector>(FindObjectsSortMode.None).Length);
                Assert.AreEqual(1, Object.FindObjectsByType<BattleAudio>(FindObjectsSortMode.None).Length);
                yield return new WaitForSeconds(1.4f); manager.Boss.SetAiEnabled(false);
            }
            manager.ReturnToTitle(); yield return null; yield return null;
            manager = Object.FindFirstObjectByType<GameManager>();
            Assert.AreEqual(EncounterState.Title, manager.Encounter.State);
            Assert.IsTrue(BattleTitleView.IsOpen);
        }

        [UnityTest] public IEnumerator BeamLocksDirectionAndSeekerObeysTurnAndLifetimeLimits()
        {
            yield return Begin();
            var beam = GardenHazard.SpawnBeam(new Vector3(5, 2, 0), Vector2.left);
            yield return new WaitForSeconds(0.4f);
            Assert.IsTrue(beam.IsAimLocked);
            Vector2 locked = beam.Direction;
            beam.Aim(Vector2.up);
            Assert.AreEqual(locked, beam.Direction, "激光锁定后不得改变方向。");
            GardenHazard.ClearAll();
            manager.Player.GrantInvincibility(10f);
            var seeker = BossProjectile.SpawnSeeker(new Vector3(4, 2, 0), manager.Player.transform, false);
            int lease = seeker.Generation;
            Vector2 initial = seeker.Velocity;
            manager.Player.GetComponent<Rigidbody2D>().position = new Vector2(5, 0);
            yield return new WaitForSeconds(0.16f);
            Assert.That(Vector2.Angle(initial, seeker.Velocity), Is.LessThan(0.01f));
            yield return new WaitForSeconds(0.14f);
            float deadline = Time.time + 0.7f;
            while (Time.time < deadline)
            {
                Vector2 previous = seeker.Velocity;
                yield return new WaitForFixedUpdate();
                Assert.LessOrEqual(Vector2.Angle(previous, seeker.Velocity), GameConfigService.Current.encounter.seekerTurnRate * Time.fixedDeltaTime + 0.02f);
                Assert.That(seeker.Velocity.magnitude, Is.EqualTo(GameConfigService.Current.encounter.seekerSpeed).Within(0.01f));
            }
            yield return new WaitForSeconds(GameConfigService.Current.encounter.seekerLifetime);
            Assert.IsFalse(seeker.gameObject.activeSelf);
            Assert.AreEqual(lease, seeker.Generation);
        }

        [UnityTest] public IEnumerator OnionCorridorsAreReachableAndPinkFallsOutsideBoss()
        {
            yield return Begin();
            var config = GameConfigService.Current;
            manager.Player.GrantInvincibility(10f);
            manager.Boss.TakeDamage(999); yield return null;
            yield return new WaitForSeconds(config.encounter.transitionDuration + 0.1f);
            float rightEdge = manager.Player.CombatRightEdge;
            float cell = (rightEdge - config.level.minStageX) / config.encounter.rainColumns;
            float halfPlayer = manager.Player.GetComponent<Collider2D>().bounds.extents.x;
            int span = MMORPG.Game.Bosses.Onion.OnionBossController.CalculateSafeSpan(rightEdge);
            Assert.GreaterOrEqual(cell * span - halfPlayer * 2f - BossProjectile.RainDiameter, 1.6f);
            var probe = BossProjectile.Spawn(new Vector3(0, 8, 0), Vector2.zero, BossProjectileType.Normal);
            probe.SetRain();
            float rainRadius = probe.GetComponent<Collider2D>().bounds.extents.x;
            BossProjectile.DespawnAllActive();
            for (float x = config.level.minStageX; x <= rightEdge; x += 0.25f)
            {
                int last = -1;
                for (int cycle = 1; cycle <= 2; cycle++)
                {
                    int safe = MMORPG.Game.Bosses.Onion.OnionBossController.ChooseSafeColumn(x, cycle, rightEdge);
                    float left = config.level.minStageX + safe * cell + halfPlayer;
                    float right = config.level.minStageX + (safe + span) * cell - halfPlayer;
                    float distance = Mathf.Abs(Mathf.Clamp(x, left, right) - x);
                    Assert.Less(distance / config.player.moveSpeed, config.encounter.onionTell + config.encounter.rainWarning);
                    if (last >= 0) Assert.AreNotEqual(last, safe, "固定站位不能一直享有同一安全区。");
                    last = safe;
                }
                float nearest = float.MaxValue;
                for (int cycle = 1; cycle <= 6; cycle++)
                {
                    int safe = MMORPG.Game.Bosses.Onion.OnionBossController.ChooseSafeColumn(x, cycle, rightEdge);
                    for (int column = 0; column < config.encounter.rainColumns; column++)
                    {
                        if (column >= safe && column < safe + span) continue;
                        float rainX = MMORPG.Game.Bosses.Onion.OnionBossController.RainPosition(column, cycle, rightEdge);
                        nearest = Mathf.Min(nearest, Mathf.Abs(rainX - x));
                    }
                }
                Assert.LessOrEqual(nearest, halfPlayer + rainRadius, $"站位 {x} 在六轮变化中仍是永久安全缝。");
            }
            yield return new WaitForSeconds(2.7f);
            bool pinkSeen = false;
            foreach (var projectile in Object.FindObjectsByType<BossProjectile>(FindObjectsSortMode.None))
            {
                if (projectile.ProjectileType != BossProjectileType.Pink) continue;
                pinkSeen = true;
                Assert.Less(projectile.transform.position.x, manager.Encounter.CurrentBoss.Root.GetComponent<Collider2D>().bounds.min.x - 0.49f);
            }
            Assert.IsTrue(pinkSeen, "第二组粉色泪滴必须实际生成。");
        }

        private IEnumerator BeamDodgeTrial(float lead, bool survives)
        {
            float previousStep = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 120f;
            try
            {
            yield return Begin();
            Vector3 center = manager.Player.GetComponent<Collider2D>().bounds.center;
            var beam = GardenHazard.SpawnBeam(center + Vector3.right * 10f, Vector2.left);
            while (beam.TimeUntilDanger > lead) yield return null;
            yield return Press(Key.L);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(survives ? 3 : 2, manager.Player.CurrentHealth, $"闪避提前量 {lead} 的血量结果不符合预期。");
            }
            finally { Time.captureDeltaTime = previousStep; }
        }

        [UnityTest] public IEnumerator EarlyBeamDodgeExpiresBeforeDamage() { yield return BeamDodgeTrial(0.3f, false); }
        [UnityTest] public IEnumerator TimelyBeamDodgeAvoidsDamage() { yield return BeamDodgeTrial(0.01f, true); }
        [UnityTest] public IEnumerator LateBeamDodgeCannotUndoDamage()
        {
            yield return Begin();
            Vector3 center = manager.Player.GetComponent<Collider2D>().bounds.center;
            GardenHazard.SpawnBeam(center + Vector3.right * 10f, Vector2.left);
            yield return new WaitForSeconds(GameConfigService.Current.encounter.beamTell + 0.07f);
            Assert.AreEqual(2, manager.Player.CurrentHealth);
            yield return Press(Key.L);
            Assert.AreEqual(2, manager.Player.CurrentHealth);
        }

        [UnityTest] public IEnumerator SettingsKeepKeyboardFocusInsideModalAndPauseCanReturnToTitle()
        {
            yield return Press(Key.S); yield return Press(Key.Enter);
            var title = Object.FindFirstObjectByType<BattleTitleView>();
            var settings = title.transform.Find("Settings");
            for (int i = 0; i < 9; i++)
            {
                yield return Press(Key.DownArrow); yield return Press(Key.LeftArrow);
                Assert.IsTrue(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.IsChildOf(settings));
                Assert.AreEqual(EncounterState.Title, manager.Encounter.State);
            }
            yield return Press(Key.Escape); yield return new WaitForSecondsRealtime(0.2f);
            Assert.AreEqual("设置", UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
            yield return Press(Key.UpArrow); yield return Press(Key.Enter);
            yield return new WaitForSeconds(1.6f);
            manager.Boss.TakeDamage(999); yield return null;
            yield return Press(Key.Escape);
            var titleButton = manager.BattleHud.transform.Find("BattleHudRoot/PausePanel/ReturnTitleButton").GetComponent<UnityEngine.UI.Button>();
            titleButton.Select(); yield return Press(Key.Enter);
            yield return null; yield return null;
            manager = Object.FindFirstObjectByType<GameManager>();
            Assert.AreEqual(EncounterState.Title, manager.Encounter.State);
            Assert.AreEqual(0, GardenHazard.ActiveCount);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest] public IEnumerator BossBoundaryStopsRunningAndDashingPastTheOpponent()
        {
            yield return Begin();
            var collider = manager.Player.GetComponent<Collider2D>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return new WaitForSeconds(2.4f);
            yield return Press(Key.D, Key.L);
            yield return new WaitForSeconds(0.25f);
            Assert.LessOrEqual(collider.bounds.max.x, manager.Boss.GetComponent<Collider2D>().bounds.min.x - GameConfigService.Current.encounter.bossSeparation + 0.05f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.LessOrEqual(collider.bounds.max.x, manager.Player.CombatRightEdge + 0.01f, "持续顶住边界不得往返穿入 Boss。");
            }
            Assert.That(manager.Boss.transform.position.x, Is.InRange(3.8f, 4.2f));
            var geometry = Resources.Load<MMORPG.Game.Bosses.GardenBossAssets>("Config/GardenBossAssets").potatoGeometry;
            Assert.That(geometry.idleBounds.height * manager.Boss.transform.localScale.y, Is.EqualTo(GameConfigService.Current.encounter.forms[0].visibleHeight).Within(0.03f));
        }

        [UnityTest] public IEnumerator TransitionClearsOldJumpBufferAndHeldActions()
        {
            yield return Begin();
            yield return Press(Key.Space);
            yield return Press(Key.Space);
            yield return Press(Key.Space);
            Assert.IsFalse(manager.Player.IsGrounded);
            manager.Boss.TakeDamage(999); yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
            yield return new WaitForSeconds(GameConfigService.Current.encounter.transitionDuration + 0.15f);
            Assert.AreEqual(1, manager.Encounter.FormIndex);
            Assert.IsTrue(manager.Player.IsGrounded, "旧的落地跳跃缓冲不能跨形态保留。");
            Assert.IsFalse(manager.Player.IsShooting, "过渡中按住的攻击键需要释放后才能重新开火。");
            yield return Press();
            yield return Press(Key.J);
        }

        [UnityTest] public IEnumerator KeyboardPracticeCompletesThreeFormsWithNormalDamage()
        {
            Random.InitState(914);
            yield return Begin(); manager.Boss.SetAiEnabled(true);
            float elapsed = 0f, jumpUntil = 0f;
            int lastForm = -1;
            var keys = new System.Collections.Generic.List<Key>();
            while (elapsed < 180f && manager.Encounter.State != EncounterState.Victory && manager.Encounter.State != EncounterState.Defeat)
            {
                keys.Clear();
                if (manager.Encounter.State == EncounterState.Fighting)
                {
                    keys.Add(Key.J);
                    if (manager.Player.Energy.IsFull) keys.Add(Key.K);
                    int form = manager.Encounter.FormIndex;
                    if (form != lastForm) { Capture("practice_form_" + form); lastForm = form; }
                    float targetX = -4.5f;
                    if (form == 1)
                    {
                        var onion = (MMORPG.Game.Bosses.Onion.OnionBossController)manager.Encounter.CurrentBoss;
                        var level = GameConfigService.Current.level;
                        float cell = (manager.Player.CombatRightEdge - level.minStageX) / GameConfigService.Current.encounter.rainColumns;
                        targetX = level.minStageX + (onion.SafeColumn + onion.SafeSpan * 0.5f) * cell;
                    }
                    float difference = targetX - manager.Player.FootPosition.x;
                    if (Mathf.Abs(difference) > 0.12f) keys.Add(difference > 0 ? Key.D : Key.A);
                    else if (manager.Player.FacingDirection < 0) keys.Add(Key.D);
                    bool jump = false;
                    if (form == 0)
                    {
                        foreach (var p in Object.FindObjectsByType<BossProjectile>(FindObjectsSortMode.None))
                        {
                            float distance = p.transform.position.x - manager.Player.FootPosition.x;
                            if (distance > 0 && distance < 1.45f && p.ProjectileLane != BossProjectileLane.Top)
                            {
                                keys.Remove(Key.A);
                                if (!keys.Contains(Key.D)) keys.Add(Key.D);
                                keys.Add(Key.L);
                                break;
                            }
                        }
                    }
                    if (form == 2)
                        foreach (var beam in Object.FindObjectsByType<GardenHazard>(FindObjectsSortMode.None))
                            if (beam.IsAimLocked && beam.TimeUntilDanger > 0.02f) jump = true;
                    if (jump && manager.Player.IsGrounded) jumpUntil = Time.time + 0.28f;
                    if (Time.time < jumpUntil) keys.Add(Key.Space);
                }
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys.ToArray()));
                yield return null; elapsed += Time.deltaTime;
            }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            string report = $"固定种子 914，实际键盘自动驾驶；未修改血量/伤害，未授予无敌，未直接击杀 Boss。\n终态：{manager.Encounter.State}\n形态：{manager.Encounter.FormIndex + 1}\n剩余生命：{manager.Player.CurrentHealth}\n{BattleStats.Active.BuildSummaryText()}";
            System.IO.Directory.CreateDirectory("Plan/Validation/DEV-009");
            System.IO.File.WriteAllText("Plan/Validation/DEV-009/keyboard_practice.txt", report);
            Capture("practice_end");
            Assert.AreEqual(EncounterState.Victory, manager.Encounter.State, report);
        }
    }
}
#endif

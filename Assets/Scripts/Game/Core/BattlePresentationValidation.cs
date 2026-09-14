using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace MMORPG.Game.Core
{
    public sealed class BattlePresentationValidation : MonoBehaviour
    {
        private string directory;
        private GameManager manager;
        private readonly StringBuilder report = new StringBuilder();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-battleReview") < 0) return;
            var obj = new GameObject("画面验收记录器");
            DontDestroyOnLoad(obj); obj.AddComponent<BattlePresentationValidation>();
#endif
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            yield return new WaitForSecondsRealtime(0.7f);
            directory = Path.Combine(Application.dataPath, "..", "Review", Screen.width + "x" + Screen.height);
            Directory.CreateDirectory(directory);
            report.AppendLine($"硬件：{SystemInfo.processorType}；显卡：{SystemInfo.graphicsDeviceName}；分辨率：{Screen.width}x{Screen.height}");
            report.AppendLine("此记录使用合成键盘输入、关闭 Boss AI 并直接推进结算以覆盖画面，不代表正常挑战通关或物理键盘至屏幕延迟。");
            manager = FindFirstObjectByType<GameManager>();
            yield return Capture("01_标题");
            ActiveScreen("TitleScreen").Get<UnityEngine.UI.Button>("settings").onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.25f); yield return Capture("02_设置");
            ActiveScreen("Settings").Get<UnityEngine.UI.Button>("back").onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.2f);
            ActiveScreen("TitleScreen").Get<UnityEngine.UI.Button>("start").onClick.Invoke();
            yield return new WaitForSeconds(GameConfigService.Current.encounter.introDuration + 0.4f);
            manager.Boss.SetAiEnabled(false);
            yield return Capture("03_土豆战斗");
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-battleReviewQuick") < 0)
                foreach (int rate in new[] { 30, 60, 120 }) yield return ProbeMotion(rate);
            manager.SetPaused(true);
            yield return new WaitForSecondsRealtime(0.25f); yield return Capture("04_暂停");
            manager.SetPaused(false); yield return new WaitForSecondsRealtime(0.2f);
            manager.Player.ReceiveHit(new HitContext(999, manager.Player.FootPosition));
            yield return new WaitForSecondsRealtime(2.4f); yield return Capture("05_失败");
            ActiveScreen("DefeatScreen").Get<UnityEngine.UI.Button>("retry").onClick.Invoke();
            yield return null; yield return null;
            manager = FindFirstObjectByType<GameManager>();
            yield return new WaitForSeconds(GameConfigService.Current.encounter.introDuration + 0.2f);
            for (int i = 0; i < 3; i++)
            {
                if (i > 0) yield return Capture(i == 1 ? "06_洋葱" : "07_胡萝卜");
                manager.Encounter.CurrentBoss.TakeDamage(999);
                yield return null;
                yield return new WaitForSeconds(i < 2 ? GameConfigService.Current.encounter.transitionDuration + 0.15f : 0.6f);
            }
            yield return Capture("08_胜利");
            report.AppendLine("标题、设置、战斗、暂停、失败、重试、三形态、胜利画面流程执行完毕。");
            File.WriteAllText(Path.Combine(directory, "画面验收记录.txt"), report.ToString());
            Debug.Log(report.ToString()); Application.Quit();
        }

        private static GardenUiScreen ActiveScreen(string name)
        {
            foreach (var screen in FindObjectsByType<GardenUiScreen>(FindObjectsSortMode.None)) if (screen.name == name) return screen;
            throw new InvalidOperationException($"画面验收没有找到活动界面：{name}");
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), texture.EncodeToPNG());
            Destroy(texture);
        }

        private IEnumerator ProbeMotion(int rate)
        {
            int oldRate = Application.targetFrameRate, oldSync = QualitySettings.vSyncCount;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var frames = new List<float>();
            var csv = new StringBuilder("入队帧,响应采样帧,输入状态,已过秒,实际帧毫秒,位置X,位置Y,速度X,速度Y,动画,姿态帧,旋转角\n");
            var player = manager.Player; var body = player.GetComponent<Rigidbody2D>();
            var animator = player.GetComponentInChildren<MMORPG.Framework.Animation.FrameAnimator>();
            var pivot = player.transform.Find("VisualPivot");
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = rate;
            body.position = new Vector2(GameConfigService.Current.level.playerSpawnX, GameConfigService.Current.level.stageFloorY);
            yield return new WaitForSeconds(0.3f);
            float start = Time.realtimeSinceStartup;
            try
            {
                while (Time.realtimeSinceStartup - start < 3f)
                {
                    float t = Time.realtimeSinceStartup - start;
                    Key[] keys = t < 0.25f ? new[] { Key.D, Key.J }
                        : t < 0.4f ? new[] { Key.D, Key.J, Key.Space }
                        : t < 0.46f ? new[] { Key.D, Key.J }
                        : t < 0.58f ? new[] { Key.D, Key.J, Key.Space }
                        : t < 0.8f ? new[] { Key.A, Key.J, Key.Space }
                        : t < 1.1f ? new[] { Key.A, Key.J }
                        : t < 1.18f ? new[] { Key.D, Key.L } : Array.Empty<Key>();
                    int queuedFrame = Time.frameCount;
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
                    yield return null;
                    frames.Add(Time.unscaledDeltaTime * 1000f);
                    csv.AppendLine(FormattableString.Invariant($"{queuedFrame},{Time.frameCount},{string.Join("+", keys)},{t:F4},{Time.unscaledDeltaTime * 1000:F3},{body.position.x:F3},{body.position.y:F3},{body.linearVelocity.x:F3},{body.linearVelocity.y:F3},{animator.CurrentClipName},{animator.CurrentFrameIndex},{pivot.localEulerAngles.z:F2}"));
                }
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard); Application.targetFrameRate = oldRate; QualitySettings.vSyncCount = oldSync;
            }
            frames.Sort(); double total = 0; foreach (float frame in frames) total += frame;
            report.AppendLine($"目标 {rate} FPS，垂直同步关闭；实际平均 {1000 * frames.Count / total:F1} FPS，P95 {frames[(int)(frames.Count * 0.95f)]:F2} ms；CPU/GPU独立计时及物理键盘延迟未测。");
            File.WriteAllText(Path.Combine(directory, $"动作采样_{rate}.csv"), csv.ToString());
        }
    }
}

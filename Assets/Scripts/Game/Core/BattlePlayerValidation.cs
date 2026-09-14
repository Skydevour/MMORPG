using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MMORPG.Game.Projectiles;
using UnityEngine;

namespace MMORPG.Game.Core
{
    public sealed class BattlePlayerValidation : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-battleValidation") >= 0)
            {
                Application.runInBackground = true;
                MMORPG.Game.Config.GameConfigService.Current.boss.maxHealth = 10000;
                new GameObject("BattlePlayerValidation").AddComponent<BattlePlayerValidation>();
            }
#endif
        }

        private IEnumerator Start()
        {
            string directory = Path.Combine(Application.dataPath, "..", "Validation", Screen.width + "x" + Screen.height);
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(1f);
            var manager = FindFirstObjectByType<GameManager>();
            if (manager.Encounter != null)
            {
                manager.Encounter.Begin();
                var title = FindFirstObjectByType<MMORPG.Game.UI.BattleTitleView>(); if (title != null) Destroy(title.gameObject);
                yield return new WaitForSeconds(1.4f);
            }
            Camera camera = Camera.main;
            var renderTarget = new RenderTexture(Screen.width, Screen.height, 24);
            camera.targetTexture = renderTarget;
            camera.enabled = false;
            manager.Boss.SetAiEnabled(false);
            yield return new WaitForSeconds(0.5f);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = renderTarget;
            var screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();
            File.WriteAllBytes(Path.Combine(directory, "battle.png"), screenshot.EncodeToPNG());
            RenderTexture.active = null;
            Destroy(screenshot);
            yield return new WaitForSeconds(0.1f);
            manager.Boss.SetAiEnabled(true);
            manager.Player.GrantInvincibility(120f);
            var frames = new List<float>(10000);
            float elapsed = 0f;
            float shot = 0f;
            float sampleDuration = Array.IndexOf(Environment.GetCommandLineArgs(), "-battleValidationQuick") >= 0 ? 3f : 60f;
            while (elapsed < sampleDuration)
            {
                yield return null;
                camera.Render();
                elapsed += Time.unscaledDeltaTime;
                shot -= Time.deltaTime;
                frames.Add(Time.unscaledDeltaTime * 1000f);
                bool bossAlive = manager.Encounter != null ? !manager.Encounter.CurrentBoss.IsDead : manager.Boss != null && !manager.Boss.IsDead;
                if (shot <= 0f && bossAlive)
                {
                    PlayerProjectile.Spawn(manager.Player.MuzzlePosition, 1);
                    shot = 0.16f;
                }
            }
            frames.Sort();
            double sum = 0;
            foreach (float frame in frames) sum += frame;
            int bossHealth = manager.Encounter != null ? manager.Encounter.CurrentBoss.CurrentHealth : manager.Boss.CurrentHealth;
            string result = $"离屏播放器采样（不代表输入延迟或人工体验）：{Screen.width}×{Screen.height}\n采样时长：{elapsed:0.00} 秒\n帧数：{frames.Count}\n平均帧时间：{sum / frames.Count:0.00} 毫秒\nP95 帧时间：{frames[(int)(frames.Count * 0.95f)]:0.00} 毫秒\nBoss 剩余生命：{bossHealth}\n";
            File.WriteAllText(Path.Combine(directory, "性能记录.txt"), result);
            Debug.Log(result);
            camera.targetTexture = null;
            Destroy(renderTarget);
            Application.Quit();
        }
    }
}

using System;
using System.Collections;
using MMORPG.Game.Bosses;
using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Combat;
using MMORPG.Game.Core;
using MMORPG.Game.Player;
using UnityEngine;
using UnityEngine.UI;
namespace MMORPG.Game.UI
{
    // Compatibility facade: gameplay owns outcome and pause; individual screens only present them.
    public sealed class BossBattleHud : MonoBehaviour
    {
        private GardenUiScreen hud, pause;
        private HealthBarView health;
        private PlayerController2D player;
        private PotatoBossController legacyBoss;
        private EncounterDirector encounter;
        private Action retryAction;
        private bool resultShown, defeatPending;
        private int resultForm;
        private float resultProgress;
        private string summary, details;
        public event Action TitleRequested;
        public event Action ResumeRequested;
        public bool IsVictoryShown { get; private set; }
        public bool IsDefeatShown { get; private set; }
        public bool IsPaused { get; private set; }
        public float BossHealthNormalized => health == null ? 0f : health.NormalizedAmount;
        public float PlayerHealthNormalized => player == null ? 0f : (float)player.CurrentHealth / Mathf.Max(1, player.MaxHealth);
        public CanvasGroup PauseGroup => pause.group;
        public void BindEncounter(EncounterDirector value) => encounter = value;
        public void FocusResume() => pause.Get<Button>("resume").Select();
        public static BossBattleHud Create(PlayerController2D player, PotatoBossController boss, Action retry)
        {
            var root = GardenUiRoot.Create("BossBattleHudCanvas", 110);
            var value = root.gameObject.AddComponent<BossBattleHud>();
            value.player = player; value.legacyBoss = boss; value.retryAction = retry;
            var catalog = GardenUiCatalog.Load();
            var container = new GameObject("BattleHudRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            container.SetParent(root, false); container.anchorMin = Vector2.zero; container.anchorMax = Vector2.one; container.offsetMin = container.offsetMax = Vector2.zero;
            value.hud = GardenUiScreen.Spawn(catalog.hud, container);
            value.health = value.hud.Get<HealthBarView>("health");
            value.hud.Get<PlayerHealthCardView>("cards").Bind(player);
            PlayerEnergyMeter.AttachTo(value.hud.transform);
            value.BindBoss(boss, 0);
            value.pause = GardenUiScreen.Spawn(catalog.pause, container);
            value.pause.Get<Button>("resume").onClick.AddListener(() => value.ResumeRequested?.Invoke());
            value.pause.Get<Button>("retry").onClick.AddListener(() => retry?.Invoke());
            value.pause.Get<Button>("settings").onClick.AddListener(() => BattleTitleView.Create(FindFirstObjectByType<GameManager>(), true));
            value.pause.Get<Button>("title").onClick.AddListener(() => value.TitleRequested?.Invoke());
            value.pause.gameObject.SetActive(false);
            if (!MMORPG.Game.Config.GameConfigService.Current.encounter.enabled) boss.Died += value.ShowVictory;
            return value;
        }
        public void BindBoss(IBossActor actor, int index)
        {
            health.Bind(actor); hud.Get<Text>("name").text = actor.DisplayName;
            hud.Get<Text>("phase").text = $"{index + 1} / 3";
            for (int i = 0; i < 3; i++)
            {
                hud.Get<Image>("stage" + i).color = GardenUiCatalog.Color(i <= index ? GardenUiCatalog.Rules.focus : GardenUiCatalog.Rules.paper);
                hud.Get<Text>("stageLabel" + i).text = i < index ? "✓" : (i + 1).ToString();
            }
        }
        public void SetPaused(bool value)
        {
            if (resultShown || IsPaused == value) return;
            IsPaused = value;
            if (value)
            {
                pause.gameObject.SetActive(true); pause.group.alpha = 0f; pause.SetInteractive(true);
                Motion(pause).Fade(pause.group, 1f, GardenUiCatalog.Rules.menuEnter); FocusResume();
            }
            else
            {
                pause.SetInteractive(false);
                Motion(pause).Fade(pause.group, 0f, GardenUiCatalog.Rules.menuExit, () => pause.gameObject.SetActive(false));
            }
        }
        public void SetEncounterResult(int form, float progress)
        {
            resultForm = form; resultProgress = Mathf.Clamp01(progress);
            CaptureStats();
        }
        private void CaptureStats()
        {
            var stats = BattleStats.Active;
            summary = stats == null ? "" : $"用时 {stats.Duration:0.0} 秒    受伤 {stats.HitsTaken} 次    粉弹收集 {stats.PinkCollected}";
            details = stats == null ? "暂无记录" : stats.BuildSummaryText();
        }
        public void ShowVictory() => ShowResult(true);
        public void ShowDefeat() => ShowResult(false);
        public void ShowDefeatAfterDelay(float delay)
        {
            if (resultShown || defeatPending) return;
            defeatPending = true; StartCoroutine(DefeatDelay(delay));
        }
        private IEnumerator DefeatDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay));
            defeatPending = false; ShowDefeat();
        }
        private void ShowResult(bool won)
        {
            if (resultShown) return;
            resultShown = true; IsVictoryShown = won; IsDefeatShown = !won; IsPaused = false;
            pause.gameObject.SetActive(false);
            if (summary == null) CaptureStats();
            EnergyTrailView.ClearAll();
            if (encounter == null) { resultForm = won ? 2 : 0; resultProgress = won ? 1f : 0f; }
            hud.SetInteractive(false); Motion(hud).Fade(hud.group, 0f, GardenUiCatalog.Rules.hudExit);
            var screen = GardenUiScreen.Spawn(won ? GardenUiCatalog.Load().victory : GardenUiCatalog.Load().defeat, transform);
            screen.gameObject.AddComponent<GardenResultView>().Initialize(screen, won, resultForm, resultProgress, won ? summary : "", details, retryAction, () => TitleRequested?.Invoke());
            Debug.Log(won ? "花园剧场胜利界面已显示。" : "花园剧场失败票券已显示。");
        }
        private static GardenUiMotion Motion(GardenUiScreen screen)
        {
            var motion = screen.GetComponent<GardenUiMotion>(); return motion != null ? motion : screen.gameObject.AddComponent<GardenUiMotion>();
        }
        private void OnDestroy() { if (legacyBoss != null) legacyBoss.Died -= ShowVictory; StopAllCoroutines(); }
    }
}

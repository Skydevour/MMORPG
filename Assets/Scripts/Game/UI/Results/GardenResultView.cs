using System;
using MMORPG.Game.Combat;
using UnityEngine;
using UnityEngine.UI;
namespace MMORPG.Game.UI
{
    public sealed class GardenResultView : MonoBehaviour
    {
        private GardenUiScreen screen;
        private float age;
        private bool victory, ready, submitted;
        private Image[] stamps;
        public void Initialize(GardenUiScreen view, bool won, int form, float progress, string summary, string details, Action retry, Action title)
        {
            screen = view; victory = won; screen.group.alpha = 0f; screen.SetInteractive(false);
            screen.Get<Text>("summary").text = summary;
            screen.Get<Button>("retry").onClick.AddListener(() => Submit(retry));
            screen.Get<Button>("title").onClick.AddListener(() => Submit(title));
            stamps = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                stamps[i] = screen.Get<Image>("stamp" + i);
                stamps[i].sprite = won || i < form ? GardenUiCatalog.Load().stamp : GardenUiCatalog.Load().stampPending;
                stamps[i].color = won || i < form ? Color.white : new Color(0.55f, 0.58f, 0.55f, 0.7f);
                screen.Get<Text>("node" + i).text = new[] { "土豆", "洋葱", "胡萝卜" }[i] + (won || i < form ? "  完成" : i == form ? "  到达" : "  未到达");
            }
            if (!won)
            {
                screen.Get<Text>("progress").text = $"挑战进度 {progress:P0}";
                var fill = screen.Get<Image>("progressFill"); fill.rectTransform.anchorMax = new Vector2(progress, 1f);
                var body = screen.Get<Text>("details"); body.text = details; body.gameObject.SetActive(false);
                screen.Get<Button>("detailsButton").onClick.AddListener(() => body.gameObject.SetActive(!body.gameObject.activeSelf));
            }
        }
        private void Submit(Action action)
        {
            if (submitted || !ready) return;
            submitted = true; screen.SetInteractive(false); action?.Invoke();
        }
        private void Update()
        {
            if (screen == null) return;
            age += Time.unscaledDeltaTime;
            var c = GardenUiCatalog.Rules;
            float duration = victory ? c.victoryEnter : c.defeatEnter;
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, duration));
            screen.group.alpha = t;
            var heading = screen.Get<RectTransform>("heading");
            heading.localScale = Vector3.one * (GardenUiCatalog.ReducedMotion ? 1f : Mathf.Lerp(0.97f, 1f, Mathf.SmoothStep(0f, 1f, t)));
            for (int i = 0; i < stamps.Length; i++)
            {
                float a = Mathf.Clamp01((age - i * c.stampInterval) / 0.12f);
                var color = stamps[i].color; color.a = a; stamps[i].color = color;
                stamps[i].transform.localScale = Vector3.one * (GardenUiCatalog.ReducedMotion ? 1f : Mathf.Lerp(0.9f, 1f, a));
            }
            if (!ready && age >= (victory ? 0.44f : 0.3f))
            {
                ready = true; screen.SetInteractive(true); screen.Get<Button>("retry").Select();
            }
        }
    }
}

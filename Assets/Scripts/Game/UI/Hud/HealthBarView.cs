using MMORPG.Game.Combat;
using UnityEngine;
using UnityEngine.UI;
namespace MMORPG.Game.UI
{
    public sealed class HealthBarView : MonoBehaviour
    {
        public Image fill, delayedFill;
        private IHealthSource source;
        private float target, shown, trail, trailFrom, elapsed, wait;
        private bool introducing;
        public float NormalizedAmount => target;
        public int CurrentHealth => source == null ? 0 : source.CurrentHealth;
        public int MaxHealth => source == null ? 0 : source.MaxHealth;
        public void Bind(IHealthSource value)
        {
            if (source != null) source.HealthChanged -= Refresh;
            source = value;
            if (source != null) source.HealthChanged += Refresh;
            target = source == null ? 0f : (float)source.CurrentHealth / Mathf.Max(1, source.MaxHealth);
            shown = trail = 0f; elapsed = 0f; introducing = source != null; Apply();
        }
        private void Refresh(int current, int maximum)
        {
            float previous = target;
            target = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0f;
            // Damage always wins over the introductory display animation.
            if (target < previous) introducing = false;
            if (!introducing) shown = target;
            trailFrom = Mathf.Max(trail, target); trail = trailFrom;
            elapsed = 0f; wait = GardenUiCatalog.Rules.healthTrailDelay; Apply();
        }
        private void Update()
        {
            var c = GardenUiCatalog.Rules;
            if (introducing)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, c.bossFill));
                shown = trail = target * Mathf.SmoothStep(0f, 1f, t);
                if (t >= 1f) { introducing = false; elapsed = 0f; trailFrom = trail; }
            }
            else if (wait > 0f) wait -= Time.unscaledDeltaTime;
            else
            {
                elapsed += Time.unscaledDeltaTime;
                trail = Mathf.Lerp(trailFrom, target, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, c.healthTrailDuration)));
            }
            Apply();
        }
        private void Apply() { Width(fill, shown); Width(delayedFill, trail); }
        private static void Width(Image image, float amount)
        {
            if (image == null) return;
            image.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
            image.enabled = amount > 0f;
        }
        private void OnDestroy() { if (source != null) source.HealthChanged -= Refresh; }
    }
}

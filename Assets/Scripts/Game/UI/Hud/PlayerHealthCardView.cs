using MMORPG.Game.Player;
using UnityEngine;
using UnityEngine.UI;
namespace MMORPG.Game.UI
{
    public sealed class PlayerHealthCardView : MonoBehaviour
    {
        public Image[] cards;
        private PlayerController2D player;
        private float[] ages;
        private int previous;
        public int CurrentHealth { get; private set; }
        public void Bind(PlayerController2D value)
        {
            if (player != null) player.HealthChanged -= Refresh;
            player = value; ages = new float[cards.Length];
            for (int i = 0; i < ages.Length; i++) ages[i] = -1f;
            previous = CurrentHealth = value.CurrentHealth;
            player.HealthChanged += Refresh; Refresh(value.CurrentHealth, value.MaxHealth);
        }
        private void Refresh(int current, int maximum)
        {
            CurrentHealth = Mathf.Clamp(current, 0, maximum);
            for (int i = CurrentHealth; i < previous && i < ages.Length; i++) ages[i] = 0f;
            previous = CurrentHealth; Paint();
        }
        private void Paint()
        {
            var art = GardenUiCatalog.Load();
            for (int i = 0; i < cards.Length; i++)
            {
                bool lost = ages[i] >= 0f && ages[i] < GardenUiCatalog.Rules.healthFlip * 0.5f;
                cards[i].sprite = i < CurrentHealth || lost ? art.healthFull : art.healthEmpty;
            }
        }
        private void Update()
        {
            if (ages == null) return;
            bool changed = false;
            for (int i = 0; i < ages.Length; i++)
            {
                if (ages[i] < 0f) continue;
                changed = true; ages[i] += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(ages[i] / Mathf.Max(0.01f, GardenUiCatalog.Rules.healthFlip));
                cards[i].transform.localScale = GardenUiCatalog.ReducedMotion ? Vector3.one : new Vector3(Mathf.Max(0.06f, Mathf.Abs(Mathf.Cos(t * Mathf.PI))), 1, 1);
                if (t >= 1f) { ages[i] = -1f; cards[i].transform.localScale = Vector3.one; }
            }
            if (changed) Paint();
        }
        private void OnDestroy() { if (player != null) player.HealthChanged -= Refresh; }
    }
}

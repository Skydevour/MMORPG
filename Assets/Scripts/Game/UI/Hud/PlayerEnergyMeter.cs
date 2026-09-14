using MMORPG.Game.Player;
using UnityEngine;
using UnityEngine.UI;
namespace MMORPG.Game.UI
{
    public sealed class PlayerEnergyMeter : MonoBehaviour
    {
        public Image[] slots;
        private static PlayerEnergyMeter activeMeter;
        private PlayerEnergyController energy;
        private float pulseAge = -1f;
        private int previous;
        private Transform canvasRoot;
        public static void ShowCollection(Vector3 point)
        {
            if (activeMeter == null || !activeMeter.gameObject.activeInHierarchy) return;
            int index = Mathf.Clamp(activeMeter.energy.CurrentEnergy - 1, 0, activeMeter.slots.Length - 1);
            EnergyTrailView.Spawn(point, activeMeter.slots[index].rectTransform, activeMeter.GetComponentInParent<Canvas>());
        }
        public static PlayerEnergyMeter Create(PlayerEnergyController target)
        {
            if (activeMeter != null) Destroy(activeMeter.gameObject);
            var root = GardenUiRoot.Create("PlayerHudCanvas", 100);
            var view = GardenUiScreen.Spawn(GardenUiCatalog.Load().energy, root);
            activeMeter = view.Get<PlayerEnergyMeter>("energy");
            activeMeter.canvasRoot = root; activeMeter.energy = target;
            target.EnergyChanged += activeMeter.Refresh;
            activeMeter.previous = target.CurrentEnergy; activeMeter.Refresh(target.CurrentEnergy, target.MaxEnergy);
            return activeMeter;
        }
        public static void AttachTo(Transform hud)
        {
            if (activeMeter == null) return;
            var old = activeMeter.canvasRoot;
            activeMeter.transform.SetParent(hud, false); activeMeter.canvasRoot = hud;
            if (old != null && old != hud) Destroy(old.gameObject);
        }
        private void Refresh(int current, int maximum)
        {
            var art = GardenUiCatalog.Load();
            for (int i = 0; i < slots.Length; i++) slots[i].sprite = i < current ? art.energyFull : art.energyEmpty;
            if (current >= maximum && previous < maximum) pulseAge = 0f;
            previous = current;
        }
        private void Update()
        {
            if (pulseAge < 0f) return;
            var c = GardenUiCatalog.Rules; pulseAge += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(pulseAge / Mathf.Max(0.01f, c.energyPulse));
            float scale = GardenUiCatalog.ReducedMotion ? 1f : 1f + Mathf.Sin(t * Mathf.PI) * (Mathf.Min(1.08f, c.energyPulseScale) - 1f);
            foreach (var slot in slots) slot.transform.localScale = Vector3.one * scale;
            if (t >= 1f) pulseAge = -1f;
        }
        private void OnDestroy()
        {
            if (energy != null) energy.EnergyChanged -= Refresh;
            if (activeMeter == this) activeMeter = null;
        }
    }
}

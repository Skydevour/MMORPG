using MMORPG.Framework.Pooling;
using MMORPG.Framework.Timing;
using UnityEngine;
using UnityEngine.UI;

namespace MMORPG.Game.UI
{
    public sealed class EnergyTrailView : MonoBehaviour, IPoolable
    {
        private static Transform root;
        private static ComponentObjectPool<EnergyTrailView> pool;
        private RectTransform rect;
        private Vector2 from, to;
        private float age;
        public static void ClearAll()
        {
            if (root == null) return;
            foreach (var effect in root.GetComponentsInChildren<EnergyTrailView>(true))
                if (effect.gameObject.activeSelf) pool.Release(effect);
        }
        public static void Spawn(Vector3 world, RectTransform slot, Canvas canvas)
        {
            if (slot == null || canvas == null) return;
            Camera camera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            if (camera == null) return;
            if (root == null || root != canvas.transform)
            {
                if (pool != null) pool.Clear();
                root = canvas.transform;
                pool = new ComponentObjectPool<EnergyTrailView>(Create, root, 4);
            }
            var effect = pool.Get(Vector3.zero, Quaternion.identity, root);
            var canvasRect = (RectTransform)canvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, camera.WorldToScreenPoint(world), canvas.worldCamera, out effect.from);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, slot.position), canvas.worldCamera, out effect.to);
            effect.rect.anchoredPosition = effect.from;
        }
        private static EnergyTrailView Create()
        {
            var obj = new GameObject("EnergyArrival", typeof(RectTransform), typeof(Image));
            var effect = obj.AddComponent<EnergyTrailView>(); effect.rect = (RectTransform)obj.transform;
            effect.rect.sizeDelta = new Vector2(16, 22);
            obj.GetComponent<Image>().color = new Color(1f, 0.35f, 0.76f);
            obj.GetComponent<Image>().raycastTarget = false; return effect;
        }
        private void Update()
        {
            if (BattleClock.Paused) return;
            age += Time.unscaledDeltaTime; float t = Mathf.Clamp01(age / Mathf.Max(0.01f, MMORPG.Game.Config.GameConfigService.Current.feedback.energyTravelDuration));
            rect.anchoredPosition = Vector2.Lerp(from, to, t * t) + Vector2.up * (Mathf.Sin(t * Mathf.PI) * 70f);
            rect.localRotation = GardenUiCatalog.ReducedMotion ? Quaternion.identity : Quaternion.Euler(0, 0, t * 360f);
            if (t >= 1f) { MMORPG.Game.Audio.BattleAudio.Play("pickup"); pool.Release(this); }
        }
        public void OnSpawnedFromPool() { age = 0f; }
        public void OnDespawnedToPool() { }
    }
}

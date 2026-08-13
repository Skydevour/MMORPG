using MMORPG.Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace MMORPG.Game.UI
{
    public sealed class PlayerEnergyMeter : MonoBehaviour
    {
        private const float SlotWidth = 28f;
        private const float SlotHeight = 24f;
        private const float SlotGap = 4f;

        private static PlayerEnergyMeter activeMeter;

        private Image[] slots;
        private PlayerEnergyController energy;

        public static PlayerEnergyMeter Create(PlayerEnergyController target)
        {
            if (activeMeter != null)
            {
                Destroy(activeMeter.gameObject);
            }

            GameObject canvasObject = new GameObject("PlayerHudCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            Camera targetCamera = Camera.main;
            canvas.renderMode = targetCamera == null ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = targetCamera;
            canvas.planeDistance = 1f;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject meterObject = new GameObject("EnergyMeter");
            meterObject.transform.SetParent(canvasObject.transform, false);
            RectTransform meterRect = meterObject.AddComponent<RectTransform>();
            meterRect.anchorMin = new Vector2(0f, 0f);
            meterRect.anchorMax = new Vector2(0f, 0f);
            meterRect.pivot = new Vector2(0f, 0f);
            meterRect.anchoredPosition = new Vector2(158f, 54f);
            int maxEnergy = target != null ? Mathf.Max(1, target.MaxEnergy) : 1;
            meterRect.sizeDelta = new Vector2(maxEnergy * SlotWidth + (maxEnergy - 1) * SlotGap, SlotHeight);

            activeMeter = meterObject.AddComponent<PlayerEnergyMeter>();
            activeMeter.CreateSlots(meterObject.transform, maxEnergy);
            activeMeter.Bind(target);
            return activeMeter;
        }

        private void CreateSlots(Transform parent, int maxEnergy)
        {
            slots = new Image[maxEnergy];

            for (int index = 0; index < slots.Length; index++)
            {
                GameObject slotObject = new GameObject($"EnergySlot_{index + 1}");
                slotObject.transform.SetParent(parent, false);
                RectTransform rect = slotObject.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(index * (SlotWidth + SlotGap), 0f);
                rect.sizeDelta = new Vector2(SlotWidth, SlotHeight);

                Image image = slotObject.AddComponent<Image>();
                image.color = new Color(0.16f, 0.08f, 0.2f, 0.92f);
                slots[index] = image;
            }
        }

        private void Bind(PlayerEnergyController target)
        {
            energy = target;
            if (energy == null)
            {
                Debug.LogWarning("能量 UI 没有绑定玩家能量组件。");
                return;
            }

            energy.EnergyChanged += Refresh;
            Refresh(energy.CurrentEnergy, energy.MaxEnergy);
        }

        private void OnDestroy()
        {
            if (energy != null)
            {
                energy.EnergyChanged -= Refresh;
            }

            if (activeMeter == this)
            {
                activeMeter = null;
            }
        }

        private void Refresh(int current, int maximum)
        {
            if (slots == null)
            {
                return;
            }

            Color filled = new Color(1f, 0.25f, 0.82f, 1f);
            Color empty = new Color(0.16f, 0.08f, 0.2f, 0.92f);
            for (int index = 0; index < slots.Length; index++)
            {
                slots[index].color = index < current ? filled : empty;
            }
        }
    }
}

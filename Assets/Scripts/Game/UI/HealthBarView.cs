using MMORPG.Game.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace MMORPG.Game.UI
{
    public sealed class HealthBarView : MonoBehaviour
    {
        private Image fill;
        private Image delayedFill;
        private IHealthSource source;
        private float targetAmount;

        public float NormalizedAmount => targetAmount;
        public int CurrentHealth => source == null ? 0 : source.CurrentHealth;
        public int MaxHealth => source == null ? 0 : source.MaxHealth;

        public static HealthBarView Create(
            Transform parent,
            string objectName,
            Vector2 anchoredPosition,
            Vector2 size,
            Color fillColor,
            IHealthSource target)
        {
            return Create(parent, objectName, new Vector2(0.5f, 1f), anchoredPosition, size, fillColor, target);
        }

        public static HealthBarView Create(
            Transform parent,
            string objectName,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 size,
            Color fillColor,
            IHealthSource target)
        {
            GameObject rootObject = new GameObject(objectName);
            rootObject.transform.SetParent(parent, false);
            RectTransform rootRect = rootObject.AddComponent<RectTransform>();
            rootRect.anchorMin = anchor;
            rootRect.anchorMax = anchor;
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = size;

            Image background = rootObject.AddComponent<Image>();
            background.color = new Color(0.035f, 0.025f, 0.035f, 0.94f);
            background.raycastTarget = false;

            HealthBarView view = rootObject.AddComponent<HealthBarView>();
            view.CreateFill(rootObject.transform, fillColor);
            view.Bind(target);
            return view;
        }

        public void Bind(IHealthSource target)
        {
            if (source != null)
            {
                source.HealthChanged -= Refresh;
            }

            source = target;
            if (source != null)
            {
                source.HealthChanged += Refresh;
                Refresh(source.CurrentHealth, source.MaxHealth);
            }
            else
            {
                Refresh(0, 1);
            }
        }

        private void OnDestroy()
        {
            if (source != null)
            {
                source.HealthChanged -= Refresh;
            }
        }

        private void Update()
        {
            if (delayedFill == null)
            {
                return;
            }

            delayedFill.fillAmount = Mathf.MoveTowards(
                delayedFill.fillAmount,
                targetAmount,
                Time.unscaledDeltaTime * 1.8f);
        }

        private void CreateFill(Transform parent, Color fillColor)
        {
            GameObject delayedObject = CreateChild(parent, "DelayedFill");
            delayedFill = delayedObject.AddComponent<Image>();
            delayedFill.color = new Color(fillColor.r * 0.45f, fillColor.g * 0.45f, fillColor.b * 0.45f, 1f);
            ConfigureFill(delayedFill);

            GameObject fillObject = CreateChild(parent, "Fill");
            fill = fillObject.AddComponent<Image>();
            fill.color = fillColor;
            ConfigureFill(fill);
        }

        private void Refresh(int current, int maximum)
        {
            targetAmount = maximum <= 0 ? 0f : Mathf.Clamp01((float)current / maximum);
            if (fill != null)
            {
                fill.fillAmount = targetAmount;
            }

            if (delayedFill != null && delayedFill.fillAmount < targetAmount)
            {
                delayedFill.fillAmount = targetAmount;
            }
        }

        private static GameObject CreateChild(Transform parent, string objectName)
        {
            GameObject child = new GameObject(objectName);
            child.transform.SetParent(parent, false);
            RectTransform rect = child.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(4f, 4f);
            rect.offsetMax = new Vector2(-4f, -4f);
            return child;
        }

        private static void ConfigureFill(Image image)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = 0;
            image.fillAmount = 1f;
            image.raycastTarget = false;
        }
    }
}
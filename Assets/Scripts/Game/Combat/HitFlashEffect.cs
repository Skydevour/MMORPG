using UnityEngine;

namespace MMORPG.Game.Combat
{
    [DisallowMultipleComponent]
    public sealed class HitFlashEffect : MonoBehaviour
    {
        private SpriteRenderer[] renderers;
        private Color[] baseColors;
        private float timer;
        private float duration;
        private float interval;
        private Color flashColor;
        private bool flashing;

        public static void Play(GameObject target, float effectDuration = 0.14f, int flashCount = 3, Color? color = null)
        {
            if (target == null)
            {
                return;
            }

            HitFlashEffect effect = target.GetComponent<HitFlashEffect>();
            if (effect == null)
            {
                effect = target.AddComponent<HitFlashEffect>();
            }

            effect.Begin(effectDuration, flashCount, color ?? Color.white);
        }

        private void Awake()
        {
            CacheRenderers();
        }

        private void Update()
        {
            if (!flashing)
            {
                return;
            }

            timer += Time.unscaledDeltaTime;
            bool visible = interval <= 0f || Mathf.FloorToInt(timer / interval) % 2 == 0;
            ApplyColor(visible ? flashColor : baseColors[0]);

            if (timer < duration)
            {
                return;
            }

            flashing = false;
            RestoreColors();
        }

        private void Begin(float effectDuration, int flashCount, Color color)
        {
            CacheRenderers();
            if (renderers.Length == 0)
            {
                return;
            }

            duration = Mathf.Max(0.03f, effectDuration);
            interval = duration / Mathf.Max(2, flashCount * 2);
            timer = 0f;
            flashColor = color;
            flashing = true;
            ApplyColor(flashColor);
        }

        private void CacheRenderers()
        {
            if (renderers != null)
            {
                return;
            }

            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            baseColors = new Color[renderers.Length];
            for (int index = 0; index < renderers.Length; index++)
            {
                baseColors[index] = renderers[index].color;
            }
        }

        private void ApplyColor(Color color)
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                renderers[index].color = color;
            }
        }

        private void RestoreColors()
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                renderers[index].color = baseColors[index];
            }
        }
    }
}
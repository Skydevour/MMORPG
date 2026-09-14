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
        private MaterialPropertyBlock properties;
        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

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

            timer += Time.deltaTime;
            bool visible = interval <= 0f || Mathf.FloorToInt(timer / interval) % 2 == 0;
            if (visible)
            {
                ApplyColor(flashColor);
            }
            else
            {
                RestoreColors();
            }

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
            properties = new MaterialPropertyBlock();
            var catalog = Resources.Load<MMORPG.Game.Core.PrototypeSpriteCatalog>("Config/PrototypeSpriteCatalog");
            for (int index = 0; index < renderers.Length; index++)
            {
                if (catalog != null && catalog.flashMaterial != null) renderers[index].sharedMaterial = catalog.flashMaterial;
                baseColors[index] = renderers[index].color;
            }
        }

        private void ApplyColor(Color color)
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                renderers[index].GetPropertyBlock(properties);
                properties.SetFloat(FlashAmount, 1f);
                renderers[index].SetPropertyBlock(properties);
            }
        }

        private void RestoreColors()
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                renderers[index].GetPropertyBlock(properties);
                properties.SetFloat(FlashAmount, 0f);
                renderers[index].SetPropertyBlock(properties);
            }
        }
        private void OnDisable()
        {
            flashing = false;
            if (renderers != null) RestoreColors();
        }
    }
}

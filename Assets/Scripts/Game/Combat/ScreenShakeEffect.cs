using UnityEngine;

namespace MMORPG.Game.Combat
{
    [DisallowMultipleComponent]
    public sealed class ScreenShakeEffect : MonoBehaviour
    {
        private Vector3 basePosition;
        private float timer;
        private float duration;
        private float strength;

        public static void Shake(float effectDuration, float effectStrength)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            ScreenShakeEffect shake = mainCamera.GetComponent<ScreenShakeEffect>();
            if (shake == null)
            {
                shake = mainCamera.gameObject.AddComponent<ScreenShakeEffect>();
            }

            shake.Begin(effectDuration, effectStrength);
        }

        private void OnEnable()
        {
            basePosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (duration <= 0f)
            {
                transform.localPosition = basePosition;
                return;
            }

            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float falloff = 1f - progress;
            Vector2 offset = Random.insideUnitCircle * strength * falloff;
            transform.localPosition = basePosition + new Vector3(offset.x, offset.y, 0f);
            if (progress >= 1f)
            {
                duration = 0f;
                transform.localPosition = basePosition;
            }
        }

        private void Begin(float effectDuration, float effectStrength)
        {
            basePosition = transform.localPosition;
            timer = 0f;
            duration = Mathf.Max(duration, Mathf.Max(0.01f, effectDuration));
            strength = Mathf.Max(strength, Mathf.Max(0f, effectStrength));
        }
    }
}
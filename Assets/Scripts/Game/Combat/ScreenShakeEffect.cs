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
        private Vector3 lastOffset;
        private readonly System.Random random = new System.Random();

        public static void Shake(float effectDuration, float effectStrength)
        {
            int mode = PlayerPrefs.GetInt("Camera.Shake", 0);
            if (mode == 2) return;
            if (mode == 1) effectStrength *= 0.35f;
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
            if (Time.timeScale <= 0f) return;
            transform.localPosition -= lastOffset;
            lastOffset = Vector3.zero;
            if (duration <= 0f)
            {
                return;
            }

            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float falloff = 1f - progress;
            basePosition = transform.localPosition;
            lastOffset = new Vector3((float)random.NextDouble() * 2f - 1f, (float)random.NextDouble() * 2f - 1f, 0f) * strength * falloff;
            transform.localPosition += lastOffset;
            if (progress >= 1f)
            {
                duration = 0f;
                strength = 0f;
                transform.localPosition = basePosition;
                lastOffset = Vector3.zero;
            }
        }

        private void Begin(float effectDuration, float effectStrength)
        {
            timer = 0f;
            duration = Mathf.Max(duration, Mathf.Max(0.01f, effectDuration));
            strength = Mathf.Max(strength, Mathf.Max(0f, effectStrength));
        }

        public void ClearShake()
        {
            transform.localPosition -= lastOffset;
            lastOffset = Vector3.zero;
            duration = strength = timer = 0f;
        }

        private void OnDisable() => ClearShake();
    }
}

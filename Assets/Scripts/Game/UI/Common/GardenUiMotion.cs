using System;
using System.Collections;
using UnityEngine;
namespace MMORPG.Game.UI
{
    public sealed class GardenUiMotion : MonoBehaviour
    {
        public void Cancel() => StopAllCoroutines();
        public void Fade(CanvasGroup group, float target, float duration, Action complete = null)
        {
            StopAllCoroutines(); StartCoroutine(Animate(group, target, duration, complete));
        }
        private IEnumerator Animate(CanvasGroup group, float target, float duration, Action complete)
        {
            float from = group.alpha, elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(0.01f, duration)));
                yield return null;
            }
            group.alpha = target; complete?.Invoke();
        }
        private void OnDisable() => StopAllCoroutines();
    }
}

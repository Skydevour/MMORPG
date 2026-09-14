using UnityEngine;

namespace MMORPG.Framework.Timing
{
    public sealed class BattleClock : MonoBehaviour
    {
        private static BattleClock active;
        private float stopRemaining;
        public static bool Paused { get; private set; }
        public static bool HitStopped => active != null && active.stopRemaining > 0f;

        public static void ResetSession()
        {
            if (active == null) active = new GameObject("BattleClock").AddComponent<BattleClock>();
            active.stopRemaining = 0f;
            Paused = false;
            Time.timeScale = 1f;
        }

        public static void SetPaused(bool value)
        {
            Paused = value;
            Apply();
        }

        public static void Stop(float duration)
        {
            if (active == null) ResetSession();
            active.stopRemaining = Mathf.Max(active.stopRemaining, duration);
            Apply();
        }

        private void Update()
        {
            if (!Paused) stopRemaining = Mathf.Max(0f, stopRemaining - Time.unscaledDeltaTime);
            Apply();
        }

        private static void Apply() => Time.timeScale = Paused || HitStopped ? 0f : 1f;
        private void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Paused = false;
            Time.timeScale = 1f;
        }
    }
}

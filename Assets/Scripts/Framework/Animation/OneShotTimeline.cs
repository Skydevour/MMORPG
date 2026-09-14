using System;

namespace MMORPG.Framework.Animation
{
    // 非循环序列：标记按时间严格递增，索引就是本轮唯一事件编号。
    public sealed class OneShotTimeline
    {
        private readonly float[] markers;
        private int nextMarker;
        public float Time { get; private set; }
        public float Duration { get; }
        public bool IsCancelled { get; private set; }
        public bool IsComplete => !IsCancelled && Time >= Duration && nextMarker == markers.Length;
        public bool IsActive => !IsCancelled && !IsComplete;
        public float DiscardedTime { get; private set; }

        public OneShotTimeline(float duration, params float[] markerTimes)
        {
            if (!Finite(duration) || duration < 0f || markerTimes == null)
                throw new ArgumentException("时间轴时长必须为有限非负数，标记不可为空引用。");
            markers = (float[])markerTimes.Clone();
            for (int i = 0; i < markers.Length; i++)
                if (!Finite(markers[i]) || markers[i] < 0f || markers[i] > duration ||
                    (i > 0 && markers[i] <= markers[i - 1]))
                    throw new ArgumentException("时间标记必须在时长内严格递增。");
            Duration = duration;
        }

        // 枚举本次区间内最早的未消费标记（包含右端点）。到标记即停，
        // 舍弃余量而不积压补发；每次最多一个事件，后续标记仍保留。
        // 零增量表示暂停，包括时间零的标记也不派发；取消后永久静默。
        public int Advance(float deltaTime)
        {
            if (!Finite(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "时间增量必须为有限非负数。");
            DiscardedTime = 0f;
            if (!IsActive || deltaTime == 0f) return -1;
            float destination = Math.Min(Duration, Time + deltaTime);
            if (nextMarker < markers.Length && markers[nextMarker] <= destination)
            {
                float consumed = markers[nextMarker] - Time;
                Time = markers[nextMarker];
                DiscardedTime = Math.Max(0f, deltaTime - consumed);
                return nextMarker++;
            }
            Time = destination;
            return -1;
        }

        public void Cancel() { IsCancelled = true; DiscardedTime = 0f; }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

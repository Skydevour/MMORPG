using MMORPG.Framework.Animation;

namespace MMORPG.Game.Bosses.Potato
{
    public sealed class PotatoAttackSequence
    {
        private readonly float tell;
        private readonly float lastRelease;
        private readonly float recovery;
        public OneShotTimeline Clock { get; }

        public PotatoAttackSequence(float anticipation, float interval, int count, float recover)
        {
            if (count < 1 || !Finite(interval) || interval <= 0f ||
                !Finite(anticipation) || anticipation < 0f || !Finite(recover) || recover < 0f)
                throw new System.ArgumentException("土豆攻击间隔和弹数必须为正数，预备和恢复必须为有限非负数。");
            tell = anticipation;
            recovery = recover;
            float[] markers = new float[count];
            for (int i = 0; i < count; i++) markers[i] = anticipation + i * interval;
            lastRelease = markers[count - 1];
            Clock = new OneShotTimeline(lastRelease + recovery, markers);
        }

        // 现有六帧只向前采样：预备为第1至2帧，释放区为第3至4帧，最后恢复。
        // 三路独立嘴型尚无正式资源，不能用回跳或移动嘴位伪造。
        public float VisualProgress
        {
            get
            {
                float time = Clock.Time;
                if (tell > 0f && time < tell) return time / tell * (2f / 6f);
                if (time <= lastRelease)
                    return (2f + (lastRelease > tell ? (time - tell) / (lastRelease - tell) : 0f)) / 6f;
                return recovery > 0f ? (3f + 3f * (time - lastRelease) / recovery) / 6f : 1f;
            }
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

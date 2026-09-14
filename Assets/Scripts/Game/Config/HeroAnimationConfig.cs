using System;

namespace MMORPG.Game.Config
{
    [Serializable] public sealed class HeroAnimationConfig
    {
        public HeroAnimationTiming hero = new HeroAnimationTiming();
    }

    [Serializable] public sealed class HeroAnimationTiming
    {
        public float takeoff = 0.075f, rise = 0.12f, apex = 0.05f, fall = 0.12f, land = 0.08f;
        public float uprightDuration = 0.06f, apexVelocity = 0.35f;
    }
}

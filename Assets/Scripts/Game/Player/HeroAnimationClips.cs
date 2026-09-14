using MMORPG.Framework.Animation;
using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.Player
{
    public static class HeroAnimationClips
    {
        public static void RegisterAirborne(FrameAnimator animator, Sprite[] frames)
        {
            if (frames == null || frames.Length == 0) return;
            var timing = GameConfigService.Current.animation.hero;
            animator.SetClipDuration("takeoff", Select(frames, 0, 1), timing.takeoff, false);
            animator.SetClipDuration("rise", Select(frames, 2, 3), timing.rise, false);
            animator.SetClipDuration("apex", Select(frames, 4), timing.apex, false);
            animator.SetClipDuration("fall", Select(frames, 5, 6), timing.fall, false);
            animator.SetClipDuration("land", Select(frames, 7, 8), timing.land, false);
            animator.SetClipDuration("air_flip", Select(frames, 3, 4, 5), GameConfigService.Current.player.airFlipDuration, false);
        }

        private static Sprite[] Select(Sprite[] frames, params int[] indices)
        {
            var result = new Sprite[indices.Length];
            for (int i = 0; i < indices.Length; i++) result[i] = frames[Mathf.Min(indices[i], frames.Length - 1)];
            return result;
        }
    }
}

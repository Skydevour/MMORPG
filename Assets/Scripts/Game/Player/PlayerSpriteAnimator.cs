using MMORPG.Framework.Animation;
using UnityEngine;

namespace MMORPG.Game.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(FrameAnimator))]
    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        private FrameAnimator frameAnimator;

        private void Awake()
        {
            frameAnimator = GetComponent<FrameAnimator>();
        }

        public void SetClip(string clipName, Sprite[] sprites, float framesPerSecond = 12f, bool loop = true)
        {
            frameAnimator.SetClip(clipName, sprites, framesPerSecond, loop);
            if (string.IsNullOrEmpty(frameAnimator.CurrentClipName) && sprites != null && sprites.Length > 0)
            {
                Play(clipName, true);
            }
        }

        public void Play(string clipName, bool restart = false)
        {
            frameAnimator.Play(clipName, restart);
        }
    }
}

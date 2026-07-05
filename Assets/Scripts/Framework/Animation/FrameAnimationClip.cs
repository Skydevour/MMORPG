using System;
using UnityEngine;

namespace MMORPG.Framework.Animation
{
    [Serializable]
    public sealed class FrameAnimationClip
    {
        [SerializeField] private string clipName;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 12f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool holdLastFrame = true;

        public FrameAnimationClip(string clipName, Sprite[] frames, float framesPerSecond, bool loop)
        {
            this.clipName = clipName;
            this.frames = frames ?? Array.Empty<Sprite>();
            this.framesPerSecond = Mathf.Max(1f, framesPerSecond);
            this.loop = loop;
            holdLastFrame = true;
        }

        public string ClipName => clipName;

        public Sprite[] Frames => frames ?? Array.Empty<Sprite>();

        public float FramesPerSecond => Mathf.Max(1f, framesPerSecond);

        public bool Loop => loop;

        public bool HoldLastFrame => holdLastFrame;
    }
}

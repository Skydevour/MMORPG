using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMORPG.Framework.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DefaultExecutionOrder(100)]
    public sealed class FrameAnimator : MonoBehaviour
    {
        private readonly Dictionary<string, FrameAnimationClip> clips = new Dictionary<string, FrameAnimationClip>();

        [SerializeField] private SpriteRenderer targetRenderer;

        private FrameAnimationClip currentClip;
        private float timer;
        private int frameIndex;
        private bool playing;

        public event Action<string> ClipCompleted;

        public string CurrentClipName => currentClip?.ClipName;

        public int CurrentFrameIndex => frameIndex;

        public bool IsPlaying => playing;
        public bool ExternalClock { get; set; }

        public void SetClipDuration(string clipName, Sprite[] frames, float duration, bool loop = true)
        {
            SetClip(clipName, frames, Mathf.Max(1, frames == null ? 0 : frames.Length) / Mathf.Max(0.01f, duration), loop);
        }

        private void Awake()
        {
            targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!ExternalClock) Tick(Time.deltaTime);
        }

        public void SampleNormalized(float progress)
        {
            if (currentClip == null || currentClip.Frames.Length == 0) return;
            frameIndex = Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(progress) * currentClip.Frames.Length), 0, currentClip.Frames.Length - 1);
            ApplyFrame();
        }

        public void SetClip(string clipName, Sprite[] frames, float framesPerSecond = 12f, bool loop = true)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogWarning("序列帧动画名称为空，已跳过注册。");
                return;
            }

            clips[clipName] = new FrameAnimationClip(clipName, frames, framesPerSecond, loop);
        }

        public bool Play(string clipName, bool restart = false)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                return false;
            }

            if (!restart && currentClip != null && currentClip.ClipName == clipName)
            {
                return true;
            }

            if (!clips.TryGetValue(clipName, out FrameAnimationClip nextClip) || nextClip.Frames.Length == 0)
            {
                Debug.LogWarning($"未找到可播放的序列帧动画：{clipName}");
                return false;
            }

            currentClip = nextClip;
            frameIndex = 0;
            timer = 0f;
            playing = true;
            ApplyFrame();
            return true;
        }

        public void Stop(bool clearSprite = false)
        {
            playing = false;
            timer = 0f;

            if (clearSprite && targetRenderer != null)
            {
                targetRenderer.sprite = null;
            }
        }

        public void Tick(float deltaTime)
        {
            if (!playing || currentClip == null || currentClip.Frames.Length == 0)
            {
                return;
            }

            timer += deltaTime;
            float frameDuration = 1f / currentClip.FramesPerSecond;
            while (timer >= frameDuration)
            {
                timer -= frameDuration;
                if (!AdvanceFrame())
                {
                    break;
                }
            }
        }

        private bool AdvanceFrame()
        {
            int nextIndex = frameIndex + 1;
            if (nextIndex >= currentClip.Frames.Length)
            {
                if (!currentClip.Loop)
                {
                    playing = false;
                    if (currentClip.HoldLastFrame)
                    {
                        frameIndex = currentClip.Frames.Length - 1;
                        ApplyFrame();
                    }

                    ClipCompleted?.Invoke(currentClip.ClipName);
                    return false;
                }

                nextIndex = 0;
            }

            frameIndex = nextIndex;
            ApplyFrame();
            return true;
        }

        private void ApplyFrame()
        {
            if (targetRenderer == null || currentClip == null || currentClip.Frames.Length == 0)
            {
                return;
            }

            targetRenderer.sprite = currentClip.Frames[Mathf.Clamp(frameIndex, 0, currentClip.Frames.Length - 1)];
        }
    }
}

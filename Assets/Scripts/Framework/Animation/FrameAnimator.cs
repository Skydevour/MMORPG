using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMORPG.Framework.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
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

        private void Awake()
        {
            targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
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
                playing = true;
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

using System;
using UnityEngine;

namespace MMORPG.Game.Bosses
{
    [Serializable]
    public sealed class BossEntranceClip
    {
        public Sprite[] frames = Array.Empty<Sprite>();
        public float framesPerSecond = 12f;
        public bool HasFrames => frames != null && frames.Length > 0;
        public float Duration => HasFrames ? frames.Length / Mathf.Max(1f, framesPerSecond) : 0f;
    }

    // 入场只替换视觉表现，根节点、碰撞体和战斗站位始终保持稳定。
    public sealed class BossEntrancePlayer : MonoBehaviour
    {
        private SpriteRenderer body;
        private SpriteRenderer entrance;
        private BossEntranceClip clip;
        private float elapsed;
        private float duration;
        private bool playing;
        private bool bodyWasVisible;
        public bool IsComplete { get; private set; }
        public bool IsCancelled { get; private set; }
        public bool OwnsBodyRenderer => playing && clip != null && clip.HasFrames;

        public void Begin(SpriteRenderer bodyRenderer, BossEntranceClip sequence, float fallbackDuration)
        {
            Cancel();
            body = bodyRenderer;
            // 标题页可能刚隐藏过身体，入场完成后必须恢复战斗角色。
            bodyWasVisible = true;
            clip = sequence;
            if (clip != null && clip.HasFrames)
            {
                if (float.IsNaN(clip.framesPerSecond) || float.IsInfinity(clip.framesPerSecond) || clip.framesPerSecond < 1f)
                    throw new ArgumentException("入场序列帧率必须为有限正数且不低于1。");
                foreach (var frame in clip.frames)
                    if (frame == null) throw new ArgumentException("入场序列包含缺失帧，请检查资源目录。");
            }
            duration = clip != null && clip.HasFrames ? clip.Duration : Mathf.Max(0f, fallbackDuration);
            elapsed = 0f;
            playing = duration > 0f;
            IsComplete = !playing;
            IsCancelled = false;
            if (clip == null || !clip.HasFrames)
            {
                body.enabled = true;
                Debug.LogWarning($"{name} 缺少正式入场序列，暂保留已有待机等待；不会以根节点缩放替代入场动作。");
                return;
            }
            var visual = new GameObject("EntranceVisual");
            visual.transform.SetParent(transform, false);
            entrance = visual.AddComponent<SpriteRenderer>();
            entrance.sortingLayerID = body.sortingLayerID;
            entrance.sortingOrder = body.sortingOrder;
            entrance.sharedMaterial = body.sharedMaterial;
            entrance.color = body.color;
            entrance.flipX = body.flipX;
            entrance.sprite = clip.frames[0];
            body.enabled = false;
        }

        public void Tick(float deltaTime)
        {
            if (!playing || !isActiveAndEnabled) return;
            if (entrance != null) { body.enabled = false; entrance.enabled = true; }
            if (deltaTime <= 0f) return;
            elapsed += deltaTime;
            if (elapsed >= duration) { Cancel(); IsCancelled = false; IsComplete = true; return; }
            if (entrance == null) return;
            body.enabled = false;
            entrance.sprite = clip.frames[Mathf.Min((int)(elapsed * clip.framesPerSecond), clip.frames.Length - 1)];
        }

        public void Cancel()
        {
            playing = false;
            IsCancelled = true;
            IsComplete = false;
            if (body != null) body.enabled = bodyWasVisible;
            if (entrance != null)
            {
                entrance.enabled = false;
                Destroy(entrance.gameObject);
                entrance = null;
                if (body != null) body.enabled = bodyWasVisible;
            }
        }

        private void OnDisable() { if (entrance != null) entrance.enabled = false; }
        private void OnDestroy() => Cancel();
    }
}

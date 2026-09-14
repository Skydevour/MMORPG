using System;
using UnityEngine;

namespace MMORPG.Game.Config
{
    [Serializable] public sealed class BattleCameraConfig
    {
        public float viewHeight = 7.6f, centerX = 0.2f, centerY = -0.6f;
        public float safetyPixels = 32f, rainTopInset = 0.1f;
        public float Top => centerY + viewHeight * 0.5f;
        public float Bottom => centerY - viewHeight * 0.5f;
        public float Left => centerX - viewHeight * 8f / 9f;
        public float Right => centerX + viewHeight * 8f / 9f;
        public float Margin => safetyPixels / 720f * viewHeight;
        public bool Outside(Vector2 point, float padding) => point.x < Left - padding || point.x > Right + padding || point.y < Bottom - padding || point.y > Top + padding;
    }
}

using System;
using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.Bosses
{
    [Serializable] public sealed class BossSpriteGeometry
    {
        public Rect idleBounds, actionBounds;
        public Rect bodyHitBounds;
        public Vector2 feet, bodyCenter, mouth, eyes;

        public float Apply(Transform target, BoxCollider2D collider, BossFormConfig form)
        {
            if (idleBounds.height <= 0f) throw new InvalidOperationException($"Boss {form.id} 缺少离线轮廓元数据，请执行资源准备。");
            if (!Valid(idleBounds) || !Valid(actionBounds) || !Valid(bodyHitBounds))
                throw new InvalidOperationException($"Boss {form.id} 的轮廓或受击区域无效。");
            float scale = form.visibleHeight / idleBounds.height;
            target.localScale = Vector3.one * scale;
            var camera = GameConfigService.Current.camera;
            float minX = camera.Left + camera.Margin - actionBounds.xMin * scale;
            float maxX = camera.Right - camera.Margin - actionBounds.xMax * scale;
            if (minX > maxX) throw new InvalidOperationException($"Boss {form.id} 全动作轮廓无法容纳于当前镜头。");
            var position = target.position;
            position.x = Mathf.Clamp(form.spawnX, minX, maxX);
            position.y = GameConfigService.Current.level.stageFloorY - form.embedDepth - feet.y * scale;
            target.position = position;
            collider.size = bodyHitBounds.size;
            collider.offset = bodyHitBounds.center;
            return scale;
        }

        private static bool Valid(Rect value) => value.width > 0f && value.height > 0f
            && !float.IsNaN(value.xMin + value.yMin + value.width + value.height)
            && !float.IsInfinity(value.xMin + value.yMin + value.width + value.height);
    }
}

#if UNITY_EDITOR
using System.IO;
using MMORPG.Game.Bosses;
using UnityEditor;
using UnityEngine;

namespace MMORPG.EditorTools.Import
{
    public static class BossGeometryPreparation
    {
        public static BossSpriteGeometry Measure(string actor)
        {
            string root = $"Assets/Res/Bosses/{actor}/Frames";
            Rect all = default, idle = default;
            bool hasAll = false, hasIdle = false;
            foreach (string path in Directory.GetFiles(root, "*.png", SearchOption.AllDirectories))
            {
                if (path.Contains("_tween")) continue;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) continue;
                var texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(path));
                var pixels = texture.GetPixels32();
                int left = texture.width, bottom = texture.height, right = -1, top = -1;
                for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                {
                    if (pixels[y * texture.width + x].a < 16) continue;
                    left = Mathf.Min(left, x); right = Mathf.Max(right, x);
                    bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
                }
                Object.DestroyImmediate(texture);
                if (right < left) continue;
                var bounds = Rect.MinMaxRect((left - sprite.pivot.x) / sprite.pixelsPerUnit,
                    (bottom - sprite.pivot.y) / sprite.pixelsPerUnit,
                    (right + 1 - sprite.pivot.x) / sprite.pixelsPerUnit, (top + 1 - sprite.pivot.y) / sprite.pixelsPerUnit);
                all = hasAll ? Union(all, bounds) : bounds; hasAll = true;
                if (Path.GetFileName(Path.GetDirectoryName(path)) == "idle")
                { idle = hasIdle ? Union(idle, bounds) : bounds; hasIdle = true; }
            }
            if (!hasIdle) throw new System.InvalidOperationException($"{actor} 没有有效待机轮廓。");
            // 当前原画的人工标定身体区域；不随烟尘、叶片或手臂伸展自动扩大碰撞体。
            Rect body = actor == "Potato" ? new Rect(-0.6f, 0.1f, 1.15f, 1.25f)
                : actor == "Onion" ? new Rect(-0.65f, 0.04f, 1.3f, 2.02f) : new Rect(-0.52f, 0.04f, 1.04f, 2.03f);
            return new BossSpriteGeometry { idleBounds = idle, actionBounds = all, bodyHitBounds = body, feet = Vector2.zero,
                bodyCenter = body.center, mouth = actor == "Potato" ? new Vector2(-0.3f, 0.5f) : new Vector2(0f, 0.9f),
                eyes = actor == "Carrot" ? new Vector2(0f, 1.6f) : new Vector2(0f, 1.43f) };
        }
        private static Rect Union(Rect a, Rect b) => Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));
    }
}
#endif

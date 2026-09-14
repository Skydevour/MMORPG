using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMORPG.Game.Core
{
    public static class PrototypeAssetLoader
    {
        private const float DefaultPixelsPerUnit = 128f;
        private const string RuntimeCatalogResourcePath = "Config/PrototypeSpriteCatalog";
        private static readonly Vector2 HeroFramePivot = new Vector2(99f / 307f, 1f / 167f);
        private static readonly Vector2 PotatoBossFramePivot = new Vector2(0.5f, 0f);
        private static readonly HashSet<string> MissingRuntimeAssetWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static PrototypeSpriteCatalog runtimeCatalog;
        private static bool runtimeCatalogLoadAttempted;

        public static Sprite LoadSprite(string assetPath, float pixelsPerUnit = DefaultPixelsPerUnit)
        {
#if UNITY_EDITOR
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(NormalizePath(assetPath));
            if (sprite == null)
            {
                Debug.LogWarning($"无法读取 Sprite 资源：{NormalizePath(assetPath)}");
            }

            return sprite;
#else
            if (!TryGetRuntimeCatalog(out PrototypeSpriteCatalog catalog))
            {
                return null;
            }

            if (catalog.TryGetSprite(assetPath, out Sprite sprite))
            {
                return sprite;
            }

            WarnMissingRuntimeAsset(assetPath);
            return null;
#endif
        }

        public static Sprite[] LoadSpritesInFolder(string assetFolder, float pixelsPerUnit = DefaultPixelsPerUnit)
        {
#if UNITY_EDITOR
            string normalizedFolder = NormalizePath(assetFolder);
            if (!Directory.Exists(normalizedFolder))
            {
                Debug.LogWarning($"序列帧目录不存在：{normalizedFolder}");
                return Array.Empty<Sprite>();
            }

            List<Sprite> sprites = new List<Sprite>();
            foreach (string file in Directory.GetFiles(normalizedFolder, "*.png").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                if (file.EndsWith("_tween.png", StringComparison.OrdinalIgnoreCase)) continue;
                Sprite sprite = LoadSprite(file, pixelsPerUnit);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            SortSprites(sprites);
            return sprites.ToArray();
#else
            if (!TryGetRuntimeCatalog(out PrototypeSpriteCatalog catalog) ||
                !catalog.TryGetSpritesInFolder(assetFolder, out Sprite[] catalogSprites))
            {
                WarnMissingRuntimeAsset(assetFolder);
                return Array.Empty<Sprite>();
            }

            List<Sprite> sprites = catalogSprites
                .Where(sprite => sprite != null && !sprite.name.EndsWith("_tween", StringComparison.OrdinalIgnoreCase))
                .ToList();
            SortSprites(sprites);
            return sprites.ToArray();
#endif
        }

#if !UNITY_EDITOR
        private static bool TryGetRuntimeCatalog(out PrototypeSpriteCatalog catalog)
        {
            if (!runtimeCatalogLoadAttempted)
            {
                runtimeCatalogLoadAttempted = true;
                runtimeCatalog = Resources.Load<PrototypeSpriteCatalog>(RuntimeCatalogResourcePath);
                if (runtimeCatalog == null)
                {
                    Debug.LogError($"运行时资源目录加载失败：Resources/{RuntimeCatalogResourcePath}.asset。请执行 MMORPG/Build Runtime Sprite Catalog。");
                }
            }

            catalog = runtimeCatalog;
            return catalog != null;
        }

        private static void WarnMissingRuntimeAsset(string path)
        {
            string normalizedPath = NormalizePath(path);
            if (MissingRuntimeAssetWarnings.Add(normalizedPath))
            {
                Debug.LogWarning($"运行时资源目录中缺少资源：{normalizedPath}");
            }
        }
#endif

        private static void SortSprites(List<Sprite> sprites)
        {
            sprites.Sort(CompareSprites);
        }

        private static int CompareSprites(Sprite left, Sprite right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            string leftName = left.name ?? string.Empty;
            string rightName = right.name ?? string.Empty;
            string leftBaseName = RemoveTweenSuffix(leftName);
            string rightBaseName = RemoveTweenSuffix(rightName);

            int leftFrameNumber = GetFrameNumber(leftBaseName);
            int rightFrameNumber = GetFrameNumber(rightBaseName);
            if (leftFrameNumber >= 0 && rightFrameNumber >= 0 && leftFrameNumber != rightFrameNumber)
            {
                return leftFrameNumber.CompareTo(rightFrameNumber);
            }

            int baseNameComparison = string.Compare(leftBaseName, rightBaseName, StringComparison.OrdinalIgnoreCase);
            if (baseNameComparison != 0)
            {
                return baseNameComparison;
            }

            bool leftIsTween = !string.Equals(leftName, leftBaseName, StringComparison.OrdinalIgnoreCase);
            bool rightIsTween = !string.Equals(rightName, rightBaseName, StringComparison.OrdinalIgnoreCase);
            if (leftIsTween != rightIsTween)
            {
                return leftIsTween ? 1 : -1;
            }

            return string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
        }

        private static string RemoveTweenSuffix(string name)
        {
            const string tweenSuffix = "_tween";
            return name.EndsWith(tweenSuffix, StringComparison.OrdinalIgnoreCase)
                ? name.Substring(0, name.Length - tweenSuffix.Length)
                : name;
        }

        private static int GetFrameNumber(string name)
        {
            int separatorIndex = name.LastIndexOf('_');
            if (separatorIndex < 0 || separatorIndex >= name.Length - 1)
            {
                return -1;
            }

            return int.TryParse(name.Substring(separatorIndex + 1), out int frameNumber)
                ? frameNumber
                : -1;
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

    }
}

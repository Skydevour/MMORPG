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
        private static readonly Vector2 HeroFramePivot = new Vector2(99f / 307f, 1f / 167f);
        private static readonly Vector2 PotatoBossFramePivot = new Vector2(0.5f, 0f);

        public static Sprite LoadSprite(string assetPath, float pixelsPerUnit = DefaultPixelsPerUnit)
        {
#if UNITY_EDITOR
            ConfigureTexture(assetPath, pixelsPerUnit);
            return AssetDatabase.LoadAssetAtPath<Sprite>(NormalizePath(assetPath));
#else
            string resourcePath = Path.ChangeExtension(assetPath, null);
            return Resources.Load<Sprite>(resourcePath);
#endif
        }

        public static Sprite[] LoadSpritesInFolder(string assetFolder, float pixelsPerUnit = DefaultPixelsPerUnit)
        {
#if UNITY_EDITOR
            string normalizedFolder = NormalizePath(assetFolder);
            if (!Directory.Exists(normalizedFolder))
            {
                Debug.LogWarning($"序列帧目录不存在：{normalizedFolder}");
                return new Sprite[0];
            }

            List<Sprite> sprites = new List<Sprite>();
            foreach (string file in Directory.GetFiles(normalizedFolder, "*.png").OrderBy(path => path))
            {
                Sprite sprite = LoadSprite(file, pixelsPerUnit);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites.ToArray();
#else
            return Resources.LoadAll<Sprite>(assetFolder).OrderBy(sprite => sprite.name).ToArray();
#endif
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

#if UNITY_EDITOR
        private static void ConfigureTexture(string assetPath, float pixelsPerUnit)
        {
            string normalizedPath = NormalizePath(assetPath);
            TextureImporter importer = AssetImporter.GetAtPath(normalizedPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool dirty = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                dirty = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                dirty = true;
            }

            if (normalizedPath.Contains("/Hero/Frames/"))
            {
                if (importer.spritePivot != HeroFramePivot)
                {
                    importer.spritePivot = HeroFramePivot;
                    dirty = true;
                }
            }
            else if (normalizedPath.Contains("/Bosses/Potato/Frames/"))
            {
                if (importer.spritePivot != PotatoBossFramePivot)
                {
                    importer.spritePivot = PotatoBossFramePivot;
                    dirty = true;
                }
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                dirty = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                dirty = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }
#endif
    }
}

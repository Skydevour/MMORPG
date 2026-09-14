#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MMORPG.Game.Core;
using UnityEditor;
using UnityEngine;

namespace MMORPG.EditorTools.Import
{
    public static class PrototypeSpriteCatalogBuilder
    {
        private const string CatalogAssetPath = "Assets/Resources/Config/PrototypeSpriteCatalog.asset";
        private const string BackgroundPath = "Assets/Res/Level01_Garden/Background/garden_background_wide.png";

        private static readonly string[] RuntimeFolders =
        {
            "Assets/Res/Hero/Frames/idle",
            "Assets/Res/Hero/Frames/run",
            "Assets/Res/Hero/Frames/jump",
            "Assets/Res/Hero/Frames/dash",
            "Assets/Res/Hero/Frames/shoot",
            "Assets/Res/Hero/Frames/dead",
            "Assets/Res/Hero/Frames/ghost",
            "Assets/Res/Bosses/Potato/Frames/idle",
            "Assets/Res/Bosses/Potato/Frames/attack_spit",
            "Assets/Res/Bosses/Potato/Frames/hurt",
            "Assets/Res/Bosses/Potato/Frames/angry"
        };

        [MenuItem("MMORPG/Build Runtime Sprite Catalog")]
        public static void BuildRuntimeSpriteCatalog()
        {
            EnsureParentDirectory();
            PrototypeSpriteCatalog catalog = AssetDatabase.LoadAssetAtPath<PrototypeSpriteCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PrototypeSpriteCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            }

            List<Sprite[]> folderSprites = new List<Sprite[]>();
            int loadedSpriteCount = 0;
            foreach (string folderPath in RuntimeFolders)
            {
                List<Sprite> sprites = new List<Sprite>();
                if (Directory.Exists(folderPath))
                {
                    foreach (string filePath in Directory.GetFiles(folderPath, "*.png")
                        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                    {
                        if (filePath.EndsWith("_tween.png", StringComparison.OrdinalIgnoreCase)) continue;
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath.Replace('\\', '/'));
                        if (sprite == null)
                        {
                            throw new UnityEditor.Build.BuildFailedException($"运行时资源目录构建时无法读取 Sprite：{filePath}");
                        }

                        sprites.Add(sprite);
                    }
                }
                else
                {
                    throw new UnityEditor.Build.BuildFailedException($"运行时资源目录构建时缺少文件夹：{folderPath}");
                }

                loadedSpriteCount += sprites.Count;
                if (sprites.Count == 0) throw new UnityEditor.Build.BuildFailedException($"动作帧为空：{folderPath}");
                folderSprites.Add(sprites.ToArray());
            }

            Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            if (background == null)
            {
                throw new UnityEditor.Build.BuildFailedException($"运行时资源目录构建时无法读取关卡背景：{BackgroundPath}");
            }

            catalog.SetEditorEntries(
                RuntimeFolders,
                folderSprites.ToArray(),
                new[] { BackgroundPath },
                new[] { background });
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"运行时 Sprite 目录构建完成：{loadedSpriteCount} 张序列帧，1 张关卡背景，资产路径 {CatalogAssetPath}。");
        }

        private static void EnsureParentDirectory()
        {
            string directory = Path.GetDirectoryName(CatalogAssetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(directory) || AssetDatabase.IsValidFolder(directory))
            {
                return;
            }

            string[] parts = directory.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }
    }
}
#endif

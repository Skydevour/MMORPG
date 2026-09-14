#if UNITY_EDITOR
using System.IO;
using System.Security.Cryptography;
using MMORPG.EditorTools.Import;
using UnityEditor;
using UnityEngine;

namespace MMORPG.EditorTools.Validation
{
    public static class PresentationAssetValidator
    {
        public static void PrepareAndVerify()
        {
            string[] paths = {
                "Assets/Res/VFX/BulletImpact/Textures/shard.png",
                "Assets/Res/UI/Results/paper.png",
                "Assets/Res/Bosses/Onion/Frames/idle/01.png",
                "Assets/Res/Bosses/Carrot/Frames/idle/01.png"
            };
            var hashes = new string[paths.Length];
            var pivots = new Vector2[paths.Length];
            var units = new float[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                hashes[i] = Hash(paths[i]);
                var importer = (TextureImporter)AssetImporter.GetAtPath(paths[i]);
                pivots[i] = importer.spritePivot;
                units[i] = importer.spritePixelsPerUnit;
            }
            GardenAssetPreparation.Prepare();
            for (int i = 0; i < paths.Length; i++)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(paths[i]);
                if (Hash(paths[i]) != hashes[i] || importer.spritePivot != pivots[i] || importer.spritePixelsPerUnit != units[i])
                    throw new UnityEditor.Build.BuildFailedException($"资源准备覆盖了原图或几何配置：{paths[i]}");
            }
            Debug.Log("DEV-010资源准备验证通过：原图内容、已有锚点及像素单位保持一致。");
        }

        private static string Hash(string path)
        {
            using (var algorithm = SHA256.Create())
                return System.Convert.ToBase64String(algorithm.ComputeHash(File.ReadAllBytes(path)));
        }
    }
}
#endif

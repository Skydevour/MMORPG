#if UNITY_EDITOR
using System.IO;
using MMORPG.Game.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MMORPG.EditorTools.Import
{
    public sealed class BattleAssetPreparation : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => Prepare();

        [MenuItem("MMORPG/Prepare Battle Assets")]
        public static void Prepare()
        {
            string common = "Assets/Res/VFX/Common";
            Directory.CreateDirectory(common);
            string smokePath = "Assets/Res/VFX/DodgeSmoke/Textures/smoke.png";
            if (!File.Exists(smokePath))
            {
                Texture2D texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                {
                    Vector2 p = new Vector2((x - 63.5f) / 63.5f, (y - 63.5f) / 63.5f);
                    float edge = 0.66f + 0.095f * Mathf.Cos(Mathf.Atan2(p.y, p.x) * 7f);
                    float d = p.magnitude;
                    Color color = d > edge ? Color.clear : d > edge - 0.055f
                        ? new Color(0.22f, 0.24f, 0.27f, 1f) : Color.Lerp(new Color(0.76f, 0.81f, 0.83f), Color.white, (p.y + 1f) * 0.5f);
                    texture.SetPixel(x, y, color);
                }
                WritePng(texture, smokePath);
            }
            PrepareDeathFrames();
            AssetDatabase.Refresh();
            foreach (string path in Directory.GetFiles("Assets/Res/Hero/Frames", "*.png", SearchOption.AllDirectories))
                AssetDatabase.ImportAsset(path.Replace('\\', '/'), ImportAssetOptions.ForceUpdate);
            foreach (string path in Directory.GetFiles("Assets/Res/Bosses/Potato/Frames", "*.png", SearchOption.AllDirectories))
                AssetDatabase.ImportAsset(path.Replace('\\', '/'), ImportAssetOptions.ForceUpdate);

            Material flash = EnsureMaterial(common + "/SpriteFlash.mat", Shader.Find("MMORPG/SpriteFlash"));
            Material particles = EnsureMaterial(common + "/Particle.mat", Shader.Find("Sprites/Default"));
            particles.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(smokePath);
            EditorUtility.SetDirty(particles);
            PrototypeSpriteCatalogBuilder.BuildRuntimeSpriteCatalog();
            var catalog = AssetDatabase.LoadAssetAtPath<PrototypeSpriteCatalog>("Assets/Resources/Config/PrototypeSpriteCatalog.asset");
            catalog.flashMaterial = flash;
            catalog.particleMaterial = particles;
            string physicsPath = "Assets/Res/Hero/Player.physicsMaterial2D";
            var physics = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(physicsPath);
            if (physics == null)
            {
                physics = new PhysicsMaterial2D("Player") { friction = 0f, bounciness = 0f };
                AssetDatabase.CreateAsset(physics, physicsPath);
            }
            catalog.playerPhysicsMaterial = physics;
            string prefabPath = common + "/Prefabs/BattleParticleBurst.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));
            AssetDatabase.Refresh();
            var template = MMORPG.Game.VFX.PooledBattleEffect.CreateTemplate();
            catalog.battleParticlePrefab = PrefabUtility.SaveAsPrefabAsset(template.gameObject, prefabPath);
            Object.DestroyImmediate(template.gameObject);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("战斗资源预处理完成：锚点、死亡幽灵、描边烟雾、透明材质和发布目录已更新。");
        }

        private static Material EnsureMaterial(string path, Shader shader)
        {
            if (shader == null) throw new BuildFailedException("战斗效果 Shader 缺失：" + path);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            return material;
        }

        private static void WritePng(Texture2D texture, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void PrepareDeathFrames()
        {
            Texture2D source = new Texture2D(2, 2);
            source.LoadImage(File.ReadAllBytes("Assets/Res/Hero/Frames/idle/idle_01.png"));
            for (int frame = 0; frame < 6; frame++)
            {
                string path = $"Assets/Res/Hero/Frames/dead/dead_{frame + 1:00}.png";
                if (File.Exists(path)) continue;
                var texture = new Texture2D(307, 167, TextureFormat.RGBA32, false);
                float angle = Mathf.Sin(frame / 5f * Mathf.PI * 0.5f) * 25f * Mathf.Deg2Rad;
                for (int y = 0; y < 167; y++) for (int x = 0; x < 307; x++)
                {
                    float dx = x - 99f, dy = y - 80f;
                    int sx = Mathf.RoundToInt(99f + dx * Mathf.Cos(angle) - dy * Mathf.Sin(angle));
                    int sy = Mathf.RoundToInt(80f + dx * Mathf.Sin(angle) + dy * Mathf.Cos(angle));
                    Color c = sx >= 0 && sx < source.width && sy >= 0 && sy < source.height ? source.GetPixel(sx, sy) : Color.clear;
                    texture.SetPixel(x, y, c);
                }
                WritePng(texture, path);
            }
            Object.DestroyImmediate(source);
            string ghostPath = "Assets/Res/Hero/Frames/ghost/ghost_01.png";
            if (File.Exists(ghostPath)) return;
            var ghost = new Texture2D(307, 167, TextureFormat.RGBA32, false);
            for (int y = 0; y < 167; y++) for (int x = 0; x < 307; x++)
            {
                float dx = (x - 99f) / 37f;
                float dy = (y - 88f) / 57f;
                bool inside = dx * dx + dy * dy < 1f && y > 36f + 5f * Mathf.Cos(x * 0.18f);
                Color c = inside ? new Color(0.82f, 0.96f, 1f, 0.94f) : Color.clear;
                if (inside && (dx * dx + dy * dy > 0.87f)) c = new Color(0.22f, 0.36f, 0.44f, 1f);
                if (inside && ((Mathf.Abs(x - 86) < 4 || Mathf.Abs(x - 109) < 4) && y > 88 && y < 105)) c = new Color(0.12f, 0.2f, 0.24f);
                if (inside && Mathf.Abs(x - 99) < 4 && y > 69 && y < 79) c = new Color(0.12f, 0.2f, 0.24f);
                ghost.SetPixel(x, y, c);
            }
            WritePng(ghost, ghostPath);
        }
    }
}
#endif

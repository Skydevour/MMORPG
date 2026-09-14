#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MMORPG.Game.Audio;
using MMORPG.Game.Bosses;
using UnityEditor;
using UnityEngine;

namespace MMORPG.EditorTools.Import
{
    public static class GardenAssetPreparation
    {
        [MenuItem("MMORPG/Prepare Garden Encounter")]
        public static void Prepare()
        {
            // Check required source files before any preparation changes assets.
            RequireFile(ShardPath);
            RequireFile(PaperPath);
            FramePaths("Onion", false); FramePaths("Onion", true);
            FramePaths("Carrot", false); FramePaths("Carrot", true);
            BattleAssetPreparation.Prepare();
            var assets = LoadOrCreate<GardenBossAssets>("Assets/Resources/Config/GardenBossAssets.asset");
            assets.onionIdle = Frames("Onion", false); assets.onionAttack = Frames("Onion", true);
            assets.carrotIdle = Frames("Carrot", false); assets.carrotAttack = Frames("Carrot", true);
            assets.potatoGeometry = BossGeometryPreparation.Measure("Potato");
            assets.onionGeometry = BossGeometryPreparation.Measure("Onion");
            assets.carrotGeometry = BossGeometryPreparation.Measure("Carrot");
            string materialPath = "Assets/Res/VFX/ParryStar/Materials/Line.mat";
            Directory.CreateDirectory(Path.GetDirectoryName(materialPath)); AssetDatabase.Refresh();
            assets.lineMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (assets.lineMaterial == null)
            {
                assets.lineMaterial = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(assets.lineMaterial, materialPath);
            }
            EditorUtility.SetDirty(assets);
            PrepareVisuals(assets);
            PrepareAudio(); AssetDatabase.SaveAssets();
            GardenUiPreparation.Prepare();
            MMORPG.Game.Config.GameConfigService.Load();
            var config = MMORPG.Game.Config.GameConfigService.Current;
            var rules = new List<MMORPG.Game.Config.AudioEventConfig>(config.audio.events ?? System.Array.Empty<MMORPG.Game.Config.AudioEventConfig>());
            foreach (var entry in Resources.Load<BattleAudioCatalog>("Config/BattleAudioCatalog").entries)
                if (!rules.Exists(rule => rule.eventId == entry.id)) rules.Add(new MMORPG.Game.Config.AudioEventConfig
                { eventId = entry.id, priority = entry.priority, maxVoices = entry.maxVoices, gain = entry.gain, minInterval = entry.minInterval });
            config.audio.events = rules.ToArray();
            File.WriteAllText("Assets/Resources/Config/GameConfig.json", JsonUtility.ToJson(MMORPG.Game.Config.GameConfigService.Current, true));
            Debug.Log("三形态资源导入、绑定和音频准备完成；现有图片已保留，美术质量仍待验收。");
        }
        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        private const string ShardPath = "Assets/Res/VFX/BulletImpact/Textures/shard.png";
        private const string PaperPath = "Assets/Res/UI/Results/paper.png";

        private static void RequireFile(string path)
        {
            if (!File.Exists(path))
                throw new UnityEditor.Build.BuildFailedException($"花园资源准备失败，缺少资源文件：{path}。请接入资源后重试，不会自动生成占位图。");
        }

        private static string[] FramePaths(string actor, bool attack)
        {
            string folder = $"Assets/Res/Bosses/{actor}/Frames/{(attack ? "attack" : "idle")}";
            if (!Directory.Exists(folder))
                throw new UnityEditor.Build.BuildFailedException($"花园资源准备失败，缺少 Boss 动作目录：{folder}。");
            var paths = new List<string>();
            foreach (string path in Directory.GetFiles(folder))
                if (string.Equals(Path.GetExtension(path), ".png", System.StringComparison.OrdinalIgnoreCase))
                    paths.Add(path.Replace('\\', '/'));
            if (paths.Count == 0)
                throw new UnityEditor.Build.BuildFailedException($"花园资源准备失败，Boss 动作目录中没有 PNG 帧：{folder}。");
            paths.Sort((left, right) =>
            {
                int order = EditorUtility.NaturalCompare(Path.GetFileName(left), Path.GetFileName(right));
                return order != 0 ? order : System.StringComparer.Ordinal.Compare(left, right);
            });
            return paths.ToArray();
        }

        private static T ImportVisual<T>(string path) where T : Object
        {
            RequireFile(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new UnityEditor.Build.BuildFailedException($"花园资源准备失败，无法导入 {typeof(T).Name}：{path}。请检查图片及导入设置。");
            return asset;
        }

        private static void PrepareVisuals(GardenBossAssets assets)
        {
            var shard = ImportVisual<Texture2D>(ShardPath);
            var paper = ImportVisual<Sprite>(PaperPath);
            string materialPath = "Assets/Res/VFX/BulletImpact/Impact.mat";
            assets.impactMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (assets.impactMaterial == null) { assets.impactMaterial = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(assets.impactMaterial, materialPath); }
            assets.impactMaterial.mainTexture = shard; EditorUtility.SetDirty(assets.impactMaterial);
            assets.resultPaper = paper;
            EditorUtility.SetDirty(assets);
        }

        private static Sprite[] Frames(string actor, bool attack)
        {
            string[] paths = FramePaths(actor, attack);
            var sprites = new Sprite[paths.Length];
            for (int frame = 0; frame < paths.Length; frame++)
            {
                string path = paths[frame];
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    throw new UnityEditor.Build.BuildFailedException($"花园资源准备失败，无法读取 Boss 帧纹理导入器：{path}。");
                bool newSprite = importer.textureType != TextureImporterType.Sprite;
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                if (newSprite)
                {
                    importer.spritePixelsPerUnit = 100f;
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = new Vector2(0.5f, 0f);
                }
                importer.SetTextureSettings(settings); importer.SaveAndReimport();
                sprites[frame] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[frame] == null)
                    throw new UnityEditor.Build.BuildFailedException($"花园资源准备失败，无法读取 Boss 帧 Sprite：{path}。");
            }
            return sprites;
        }
        private static void PrepareAudio()
        {
            var catalog = LoadOrCreate<BattleAudioCatalog>("Assets/Resources/Config/BattleAudioCatalog.asset");
            string[] ids = { "shot", "jump", "double_jump", "dash_start", "dash_end", "hurt", "death", "parry_success", "pickup", "energy_full", "charge", "release", "super_end", "emerge", "inhale", "spit", "boss_hit", "boss_defeat", "sob_tell", "tear_fall", "tear_splash", "wipe", "psychic_charge", "lock", "beam", "seeker", "stun", "ui_focus", "ui_confirm", "ui_cancel", "ui_start", "ui_defeat", "ui_victory", "super_loop" };
            var entries = new List<BattleAudioCatalog.Entry>();
            for (int index = 0; index < ids.Length; index++)
            {
                string id = ids[index]; int variants = id == "shot" ? 3 : id == "parry_success" || id == "spit" ? 2 : 1;
                var clips = new AudioClip[variants];
                for (int v = 0; v < variants; v++) clips[v] = MakeWave($"Assets/Res/Audio/Combat/{id}_{v + 1}.wav", index, v, false);
                entries.Add(new BattleAudioCatalog.Entry { id = id, clips = clips, priority = id == "shot" || id == "boss_hit" ? 10 : 80, maxVoices = id == "shot" ? 4 : 3, gain = id == "shot" ? 0.35f : 0.65f });
            }
            catalog.entries = entries.ToArray(); catalog.music = new AudioClip[3];
            for (int i = 0; i < 3; i++) catalog.music[i] = MakeWave($"Assets/Res/Audio/Music/garden_{i + 1}.wav", i, 0, true);
            PrepareMixer(catalog);
            EditorUtility.SetDirty(catalog);
        }

        private static void PrepareMixer(BattleAudioCatalog catalog)
        {
            const string path = "Assets/Res/Audio/Garden.mixer";
            var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance;
            System.Type type = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                type = type ?? assembly.GetType("UnityEditor.Audio.AudioMixerController");
            if (type == null) throw new System.InvalidOperationException("无法找到当前 Unity 的混音器编辑器类型。");
            object mixer = AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>(path);
            if (mixer == null) mixer = type.GetMethod("CreateMixerControllerAtPath", flags).Invoke(null, new object[] { path });
            catalog.mixer = (UnityEngine.Audio.AudioMixer)mixer;
            var master = type.GetProperty("masterGroup", flags).GetValue(mixer);
            var groupType = master.GetType();
            var childrenProperty = groupType.GetProperty("children", flags);
            var children = new List<object>();
            foreach (var child in (System.Array)childrenProperty.GetValue(master)) children.Add(child);
            string[] names = { "Music", "Player", "Enemy", "Impact", "UI" };
            foreach (string name in names)
            {
                if (catalog.mixer.FindMatchingGroups(name).Length > 0) continue;
                children.Add(type.GetMethod("CreateNewGroup", flags).Invoke(mixer, new object[] { name, false }));
            }
            var array = System.Array.CreateInstance(groupType, children.Count);
            for (int i = 0; i < children.Count; i++) array.SetValue(children[i], i);
            childrenProperty.SetValue(master, array);
            catalog.groups = new UnityEngine.Audio.AudioMixerGroup[names.Length];
            for (int i = 0; i < names.Length; i++) catalog.groups[i] = catalog.mixer.FindMatchingGroups(names[i])[0];
            EditorUtility.SetDirty((Object)master); EditorUtility.SetDirty(catalog.mixer);
        }

        private static AudioClip MakeWave(string path, int seed, int variant, bool music)
        {
            string arranged = path.Replace("Assets/Res/Audio/", "Assets/Res/Audio/Arranged/");
            if (File.Exists(arranged)) path = arranged;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path))
            {
                const int rate = 48000;
                int samples = (int)(rate * (music ? 8f : seed == 0 ? 0.09f : 0.24f));
                using (var writer = new BinaryWriter(File.Create(path)))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + samples * 2);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
                    writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples * 2);
                    var random = new System.Random(seed * 31 + variant);
                    int[] notes = { 0, 3, 7, 10, 7, 3, 5, 2 };
                    for (int i = 0; i < samples; i++)
                    {
                        float t = (float)i / rate;
                        float frequency = music ? 130.81f * Mathf.Pow(2f, (notes[(int)(t * 4f) % notes.Length] + seed * 2) / 12f) : (seed == 7 ? 1100f : 180f + seed * 29f + variant * 30f);
                        float envelope = music ? Mathf.Exp(-Mathf.Repeat(t, 0.25f) * 12f) : Mathf.Sin(Mathf.PI * i / samples) * Mathf.Exp(-t * 12f);
                        float wave = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.55f + Mathf.Sin(4f * Mathf.PI * frequency * t) * 0.15f;
                        if (!music && (seed == 0 || seed == 5 || seed == 20)) wave += ((float)random.NextDouble() * 2f - 1f) * 0.35f;
                        writer.Write((short)(Mathf.Clamp(wave * envelope * 0.5f, -1f, 1f) * 32767));
                    }
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings; settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
#endif

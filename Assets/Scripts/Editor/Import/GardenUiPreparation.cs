#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MMORPG.Game.UI;
using MMORPG.Game.Config;
using MMORPG.Game.Bosses;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MMORPG.EditorTools.Import
{
    public static class GardenUiPreparation
    {
        private const string ArtRoot = "Assets/Res/UI/GardenTheatre";
        private static GardenUiCatalog art;
        private static GardenUiConfig rules;
        private static GardenUiScreen screen;
        private static List<GardenUiScreen.Entry> entries;
        private static Color Ink => GardenUiCatalog.Color(rules.ink);
        private static Color Paper => GardenUiCatalog.Color(rules.paper);
        private static Color Green => GardenUiCatalog.Color(rules.green);
        private static Color Red => GardenUiCatalog.Color(rules.red);

        [MenuItem("MMORPG/Prepare Garden UI")]
        public static void Prepare()
        {
            GameConfigService.Load(); rules = GameConfigService.Current.presentation.ui ?? new GardenUiConfig();
            GameConfigService.Current.presentation.ui = rules;
            art = AssetDatabase.LoadAssetAtPath<GardenUiCatalog>("Assets/Resources/Config/GardenUiCatalog.asset");
            if (art == null) { art = ScriptableObject.CreateInstance<GardenUiCatalog>(); AssetDatabase.CreateAsset(art, "Assets/Resources/Config/GardenUiCatalog.asset"); }
            art.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            art.ticket = Sprite("Common", "ticket", 24); art.programme = Sprite("Pause", "programme", 32);
            art.defeatPaper = Sprite("Defeat", "torn_ticket", 0);
            art.healthFull = Sprite("HUD", "health_full", 0); art.healthEmpty = Sprite("HUD", "health_empty", 0);
            art.energyFull = Sprite("HUD", "energy_full", 0); art.energyEmpty = Sprite("HUD", "energy_empty", 0);
            art.stamp = Sprite("Victory", "stamp", 0); art.sprig = Sprite("Common", "sprig", 0);
            art.stampPending = Sprite("Victory", "stamp_pending", 0);
            BuildHud(); BuildEnergy(); BuildTitle(); BuildPause(); BuildSettings(); BuildResults(false); BuildResults(true);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            // Serialize only the expanded config object; existing A/B values remain as loaded.
            File.WriteAllText("Assets/Resources/Config/GameConfig.json", JsonUtility.ToJson(GameConfigService.Current, true));
            AssetDatabase.ImportAsset("Assets/Resources/Config/GameConfig.json");
            Validate();
            RenderPreviews();
            Debug.Log("DEV-009 C/D：七个 UI Prefab、资源目录和统一主题准备完成；字体及正式题字仍为技术占位。");
        }
        private static Sprite Sprite(string group, string name, int border)
        {
            string path = $"{ArtRoot}/{group}/Textures/{name}.png";
            if (!File.Exists(path)) throw new System.InvalidOperationException($"UI 原创图缺失：{path}，请先运行 generate_art.py。");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.spriteBorder = Vector4.one * border;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void Begin(string name)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(GardenUiScreen));
            var rect = (RectTransform)obj.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            screen = obj.GetComponent<GardenUiScreen>(); screen.group = obj.GetComponent<CanvasGroup>(); entries = new List<GardenUiScreen.Entry>();
        }
        private static GardenUiScreen Save(string category)
        {
            screen.bindings = entries.ToArray();
            string folder = $"{ArtRoot}/{category}/Prefabs"; Directory.CreateDirectory(folder);
            string path = $"{folder}/{screen.name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(screen.gameObject, path).GetComponent<GardenUiScreen>();
            Object.DestroyImmediate(screen.gameObject); return prefab;
        }
        private static T Bind<T>(string id, T value) where T : Component { entries.Add(new GardenUiScreen.Entry { id = id, value = value }); return value; }
        private static RectTransform Rect(Transform parent, string name, Rect box)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(box.x + box.width * 0.5f, -box.y - box.height * 0.5f); r.sizeDelta = box.size; return r;
        }
        private static Image Picture(Transform parent, string name, Rect box, Sprite sprite, Color color, bool sliced = false)
        {
            var image = Rect(parent, name, box).gameObject.AddComponent<Image>(); image.sprite = sprite; image.color = color;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple; image.raycastTarget = false; return image;
        }
        private static Text Label(Transform parent, string name, string content, Rect box, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var text = Rect(parent, name, box).gameObject.AddComponent<Text>(); text.font = art.font; text.text = content;
            text.fontSize = size; text.color = color; text.alignment = align; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }
        private static Button Button(string id, string name, string caption, Rect box)
        {
            var image = Picture(screen.transform, name, box, art.ticket, Paper, true); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.normalColor = Color.white; colors.selectedColor = GardenUiCatalog.Color(rules.focus);
            colors.highlightedColor = colors.selectedColor; colors.pressedColor = new Color(0.87f, 0.8f, 0.5f); colors.fadeDuration = rules.focusDuration; button.colors = colors;
            Label(image.transform, "Label", caption, new Rect(26, 0, box.width - 52, box.height), rules.menuFont, Ink);
            var mark = Label(image.transform, "FocusArrow", "›", new Rect(8, 0, 22, box.height), 34, new Color(Ink.r, Ink.g, Ink.b, 0));
            image.gameObject.AddComponent<BattleMenuFeedback>().focusMark = mark;
            return Bind(id, button);
        }
        private static void Nav(params Selectable[] controls)
        {
            for (int i = 0; i < controls.Length; i++) controls[i].navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnUp = controls[(i + controls.Length - 1) % controls.Length], selectOnDown = controls[(i + 1) % controls.Length],
                selectOnLeft = controls[i] is Slider ? null : controls[(i + controls.Length - 1) % controls.Length],
                selectOnRight = controls[i] is Slider ? null : controls[(i + 1) % controls.Length] };
        }
        private static void Shade(float alpha)
        {
            var image = Picture(screen.transform, "StageShade", new Rect(0, 0, 1280, 720), null, new Color(Ink.r, Ink.g, Ink.b, alpha));
            image.raycastTarget = true;
        }
        private static void BuildHud()
        {
            Begin("HudScreen");
            Bind("name", Label(screen.transform, "BossTitle", "土豆", rules.bossName, 26, Ink));
            Bind("phase", Label(screen.transform, "BossPhase", "1 / 3", new Rect(830, 24, 90, 32), 20, Ink));
            var bg = Picture(screen.transform, "BossHealthBar", rules.bossBar, null, Ink);
            var inset = Rect(bg.transform, "Inset", new Rect(2, 2, rules.bossBar.width - 4, rules.bossBar.height - 4));
            Image Fill(string name, Color color)
            {
                var img = Picture(inset, name, new Rect(0, 0, inset.rect.width, inset.rect.height), null, color);
                img.rectTransform.anchorMin = Vector2.zero; img.rectTransform.anchorMax = Vector2.one; img.rectTransform.offsetMin = img.rectTransform.offsetMax = Vector2.zero; return img;
            }
            var delayed = Fill("DelayedFill", GardenUiCatalog.Color(rules.focus)); var fill = Fill("Fill", Red);
            var bar = bg.gameObject.AddComponent<HealthBarView>(); bar.fill = fill; bar.delayedFill = delayed; Bind("health", bar);
            for (int i = 0; i < 3; i++)
            {
                Bind("stage" + i, Picture(screen.transform, "Stage" + i, new Rect(576 + i * 44, 96, 32, 24), art.ticket, Paper));
                Bind("stageLabel" + i, Label(screen.transform, "StageLabel" + i, (i + 1).ToString(), new Rect(576 + i * 44, 96, 32, 24), 18, Ink));
            }
            var cards = Rect(screen.transform, "PlayerHealthCards", rules.healthCards).gameObject.AddComponent<PlayerHealthCardView>(); cards.cards = new Image[GameConfigService.Current.player.maxHealth];
            for (int i = 0; i < cards.cards.Length; i++) cards.cards[i] = Picture(cards.transform, "HealthCard_" + (i + 1), new Rect(i * 52, 0, 44, 64), art.healthFull, Color.white);
            Bind("cards", cards); art.hud = Save("HUD");
        }
        private static void BuildEnergy()
        {
            Begin("EnergyMeter"); var energy = screen.gameObject.AddComponent<PlayerEnergyMeter>(); energy.slots = new Image[GameConfigService.Current.player.maxEnergy];
            float start = Mathf.Max(rules.energyCards.x, rules.healthCards.x + GameConfigService.Current.player.maxHealth * 52 + 20);
            for (int i = 0; i < energy.slots.Length; i++) energy.slots[i] = Picture(screen.transform, "EnergySlot_" + (i + 1), new Rect(start + i * 44, rules.energyCards.y, 36, 48), art.energyEmpty, Color.white);
            Bind("energy", energy); art.energy = Save("HUD");
        }
        private static void BuildTitle()
        {
            Begin("TitleScreen");
            Label(screen.transform, "Title", "菜园恶战", rules.title, 64, Ink);
            Picture(screen.transform, "TitleSprig", new Rect(126, 272, 400, 62), art.sprig, Color.white);
            var a = Button("start", "开始挑战", "开始挑战", new Rect(112, 350, 360, 64));
            var b = Button("settings", "设置", "设置", new Rect(112, 434, 360, 56));
            var c = Button("quit", "退出", "退出", new Rect(112, 498, 360, 56)); Nav(a, b, c);
            Sprite[] cast = { Sprite("Title", "potato_portrait", 0), Sprite("Title", "onion_portrait", 0), Sprite("Title", "carrot_portrait", 0) };
            Rect[] castRects = { new Rect(720, 292, 240, 240), new Rect(900, 230, 185, 302), new Rect(1050, 172, 130, 360) };
            for (int i = 0; i < cast.Length; i++)
            {
                var portrait = Picture(screen.transform, "Cast" + i, castRects[i], cast[i], Color.white);
                portrait.preserveAspect = true;
                Label(screen.transform, "CastName" + i, new[] { "土豆", "洋葱", "胡萝卜" }[i], new Rect(castRects[i].center.x - 75, 540, 150, 32), 24, Ink);
            }
            art.title = Save("Title");
        }
        private static void BuildPause()
        {
            Begin("PausePanel"); Shade(rules.shadeAlpha); Picture(screen.transform, "Programme", rules.programme, art.programme, Color.white, true);
            Label(screen.transform, "PauseText", "幕间休息", new Rect(440, 150, 400, 64), 36, Green);
            var a = Button("resume", "ResumeButton", "继续挑战", new Rect(480, 244, 320, 56));
            var b = Button("retry", "RestartButton", "重新挑战", new Rect(480, 308, 320, 56));
            var c = Button("settings", "SettingsButton", "设置", new Rect(480, 372, 320, 56));
            var d = Button("title", "ReturnTitleButton", "返回标题", new Rect(480, 436, 320, 56)); Nav(a, b, c, d);
            art.pause = Save("Pause");
        }
        private static Slider Slider(string id, string title, float y)
        {
            Label(screen.transform, id + "Label", title, new Rect(440, y, 135, 42), 24, Ink, TextAnchor.MiddleLeft);
            var rail = Picture(screen.transform, title, new Rect(584, y + 12, 244, 18), null, Ink); rail.raycastTarget = true;
            var slider = rail.gameObject.AddComponent<Slider>();
            var fill = Picture(rail.transform, "Fill", new Rect(0, 0, 244, 18), null, Green);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            slider.fillRect = fill.rectTransform; slider.targetGraphic = fill;
            var colors = slider.colors; colors.selectedColor = GardenUiCatalog.Color(rules.focus); colors.highlightedColor = colors.selectedColor; slider.colors = colors;
            var mark = Label(rail.transform, "FocusArrow", "›", new Rect(-24, -12, 22, 42), 28, new Color(Ink.r, Ink.g, Ink.b, 0));
            rail.gameObject.AddComponent<BattleMenuFeedback>().focusMark = mark;
            return Bind(id, slider);
        }
        private static void BuildSettings()
        {
            Begin("Settings"); Shade(rules.shadeAlpha); Picture(screen.transform, "Programme", rules.programme, art.programme, Color.white, true);
            Label(screen.transform, "Heading", "演出设置", new Rect(440, 144, 400, 60), 36, Green);
            var a = Slider("master", "主音量", 238); var b = Slider("music", "音乐", 304); var c = Slider("effects", "音效", 370);
            var d = Button("motion", "MotionButton", "动态与震动：标准", new Rect(450, 444, 380, 56)); Bind("motionLabel", d.GetComponentInChildren<Text>());
            var e = Button("back", "BackButton", "返回", new Rect(480, 516, 320, 56)); Nav(a, b, c, d, e);
            art.settings = Save("Pause");
        }
        private static void BuildResults(bool won)
        {
            Begin(won ? "VictoryScreen" : "DefeatScreen"); Shade(rules.shadeAlpha);
            if (!won) Picture(screen.transform, "TornTicket", rules.defeatTicket, art.defeatPaper, Color.white);
            else Picture(screen.transform, "VictorySprig", new Rect(340, 238, 600, 170), art.sprig, Color.white);
            var heading = Label(screen.transform, "ResultText", won ? "满园喝彩" : "YOU LOSE", won ? new Rect(320, 110, 640, 128) : new Rect(360, 170, 560, 84), won ? 64 : 54, won ? Paper : Red);
            Bind("heading", heading.rectTransform);
            for (int i = 0; i < 3; i++)
            {
                float x = won ? 390 + i * 180 : 410 + i * 164, y = won ? 282 : 270;
                Bind("stamp" + i, Picture(screen.transform, "Stamp" + i, new Rect(x, y, won ? 80 : 48, won ? 80 : 48), art.stamp, Color.white));
                Bind("node" + i, Label(screen.transform, "Node" + i, new[] { "土豆", "洋葱", "胡萝卜" }[i], new Rect(x - 35, y + (won ? 88 : 50), 150, 32), 20, won ? Paper : Ink));
            }
            Bind("summary", Label(screen.transform, "Summary", "", new Rect(290, won ? 436 : 548, 700, 40), 24, Paper));
            var a = Button("retry", "RetryButton", "重新挑战", new Rect(360, won ? 520 : 430, 264, 64));
            var b = Button("title", "TitleButton", "返回标题", new Rect(656, won ? 520 : 430, 264, 64));
            if (!won)
            {
                Bind("progress", Label(screen.transform, "Progress", "", new Rect(390, 365, 500, 32), 20, Ink));
                var rail = Picture(screen.transform, "ProgressRail", new Rect(410, 404, 460, 4), null, new Color(0.7f, 0.7f, 0.65f));
                var fill = Picture(rail.transform, "ProgressFill", new Rect(0, 0, 460, 4), null, Green);
                fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one; fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero; Bind("progressFill", fill);
                var d = Button("detailsButton", "DetailsButton", "详情", new Rect(560, 500, 160, 40));
                Bind("details", Label(screen.transform, "Details", "", new Rect(370, 596, 540, 108), 20, Paper)); Nav(a, b, d);
            }
            else Nav(a, b);
            if (won) art.victory = Save("Victory"); else art.defeat = Save("Defeat");
        }
        public static void Validate()
        {
            var value = GardenUiCatalog.Load();
            foreach (var prefab in new[] { value.hud, value.energy, value.title, value.pause, value.settings, value.defeat, value.victory })
            {
                if (prefab == null || prefab.group == null) throw new System.InvalidOperationException("UI Prefab 或 CanvasGroup 缺失。");
                var ids = new HashSet<string>();
                foreach (var entry in prefab.bindings) if (entry.value == null || !ids.Add(entry.id)) throw new System.InvalidOperationException($"UI {prefab.name} 绑定缺失或重复：{entry.id}");
                foreach (var text in prefab.GetComponentsInChildren<Text>(true)) if (text.raycastTarget) throw new System.InvalidOperationException($"UI 装饰文字拦截输入：{text.name}");
            }
            Debug.Log("DEV-009 C/D：UI 序列化绑定和装饰 raycast 检查通过。");
        }
        private static void RenderPreviews()
        {
            string folder = "Plan/Validation/DEV-009-CD/PrefabPreviews"; Directory.CreateDirectory(folder);
            foreach (var resolution in new[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(1024, 768) })
            foreach (var prefab in new[] { art.hud, art.title, art.pause, art.settings, art.defeat, art.victory })
            {
                var cameraObject = new GameObject("UI资源预览相机"); var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = 3.8f; camera.nearClipPlane = 0.01f; camera.farClipPlane = 5f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.16f, 0.25f, 0.2f); camera.cullingMask = 1 << 30;
                var target = new RenderTexture(resolution.x, resolution.y, 24); camera.targetTexture = target;
                camera.aspect = 16f / 9f;
                if ((float)resolution.x / resolution.y < 16f / 9f)
                { float h = resolution.x / (16f / 9f) / resolution.y; camera.rect = new Rect(0, (1f - h) * 0.5f, 1, h); }
                var root = new GameObject("UI资源预览", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 0.5f;
                var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
                var view = Object.Instantiate(prefab, root.transform, false);
                if (prefab == art.hud) Object.Instantiate(art.energy, view.transform, false);
                foreach (var transform in root.GetComponentsInChildren<Transform>(true)) transform.gameObject.layer = 30;
                Canvas.ForceUpdateCanvases(); camera.Render();
                var previous = RenderTexture.active; RenderTexture.active = target;
                var image = new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0); image.Apply();
                File.WriteAllBytes($"{folder}/{prefab.name}_{resolution.x}x{resolution.y}.png", image.EncodeToPNG());
                RenderTexture.active = previous; camera.targetTexture = null;
                Object.DestroyImmediate(image); Object.DestroyImmediate(root); Object.DestroyImmediate(cameraObject); Object.DestroyImmediate(target);
            }
            Debug.Log("DEV-009 C/D：三尺寸 UI Prefab 静态预览已输出（不是 PlayMode 截图）。");
        }
    }
}
#endif

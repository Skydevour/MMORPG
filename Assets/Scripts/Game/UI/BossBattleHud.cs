using System;
using System.Collections;
using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace MMORPG.Game.UI
{
    public sealed class BossBattleHud : MonoBehaviour
    {
        private static readonly Color BossHealthColor = new Color(0.95f, 0.24f, 0.18f, 1f);
        private static readonly Color PlayerHealthColor = new Color(0.3f, 0.9f, 0.42f, 1f);

        private Canvas canvas;
        private HealthBarView bossHealthBar;
        private HealthBarView playerHealthBar;
        private GameObject resultPanel;
        private GameObject pausePanel;
        private Text resultText;
        private Text playerHealthText;
        private Button retryButton;
        private Action retryAction;
        private PlayerController2D player;
        private PotatoBossController boss;
        private bool resultShown;
        private bool defeatDelayRunning;

        public bool IsVictoryShown { get; private set; }

        public bool IsDefeatShown { get; private set; }

        public bool IsPaused { get; private set; }

        public float BossHealthNormalized => bossHealthBar == null ? 0f : bossHealthBar.NormalizedAmount;

        public float PlayerHealthNormalized => playerHealthBar == null ? 0f : playerHealthBar.NormalizedAmount;

        public static BossBattleHud Create(PlayerController2D player, PotatoBossController boss, Action onRetry)
        {
            GameObject canvasObject = new GameObject("BossBattleHudCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            Camera targetCamera = Camera.main;
            canvas.renderMode = targetCamera == null ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = targetCamera;
            canvas.planeDistance = 1f;
            canvas.sortingOrder = 110;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            BossBattleHud hud = canvasObject.AddComponent<BossBattleHud>();
            hud.canvas = canvas;
            hud.retryAction = onRetry;
            hud.Build(player, boss);
            return hud;
        }

        public void ShowVictory()
        {
            if (resultShown)
            {
                return;
            }

            resultShown = true;
            IsVictoryShown = true;
            IsDefeatShown = false;
            resultText.text = "YOU WIN";
            resultText.color = new Color(1f, 0.86f, 0.3f, 1f);
            resultPanel.SetActive(true);
            Debug.Log("Boss\u5173\u5361\u80dc\u5229\u754c\u9762\u5df2\u663e\u793a\u3002");
        }

        public void ShowDefeat()
        {
            if (resultShown)
            {
                return;
            }

            resultShown = true;
            IsVictoryShown = false;
            IsDefeatShown = true;
            resultText.text = "YOU LOSE";
            resultText.color = new Color(1f, 0.36f, 0.34f, 1f);
            resultPanel.SetActive(true);
            Debug.Log("Boss\u5173\u5361\u5931\u8d25\u754c\u9762\u5df2\u663e\u793a\u3002");
        }

        public void ShowDefeatAfterDelay(float delay)
        {
            if (resultShown || defeatDelayRunning)
            {
                return;
            }

            StartCoroutine(ShowDefeatAfterDelayRoutine(delay));
        }

        public void SetPaused(bool paused)
        {
            if (resultShown || IsPaused == paused)
            {
                return;
            }

            IsPaused = paused;
            pausePanel.SetActive(paused);
        }

        private void Build(PlayerController2D player, PotatoBossController boss)
        {
            this.player = player;
            this.boss = boss;

            GameObject root = new GameObject("BattleHudRoot");
            root.transform.SetParent(canvas.transform, false);

            CreateText(root.transform, "BossTitle", "POTATO BOSS", new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(640f, 30f), 22, Color.white);
            bossHealthBar = HealthBarView.Create(root.transform, "BossHealthBar", new Vector2(0f, -48f), new Vector2(640f, 28f), BossHealthColor, boss);

            playerHealthText = CreateText(
                root.transform,
                "PlayerHealthText",
                "HP. " + (player == null ? 0 : player.CurrentHealth),
                new Vector2(0f, 0f),
                new Vector2(24f, 48f),
                new Vector2(128f, 34f),
                24,
                Color.white);
            playerHealthText.alignment = TextAnchor.MiddleLeft;

            playerHealthBar = HealthBarView.Create(
                root.transform,
                "PlayerHealthBar",
                new Vector2(0f, 0f),
                new Vector2(24f, 16f),
                new Vector2(118f, 8f),
                PlayerHealthColor,
                player);

            Text phaseText = CreateText(root.transform, "BossPhase", "PHASE 1", new Vector2(1f, 1f), new Vector2(-32f, -22f), new Vector2(150f, 30f), 20, new Color(1f, 0.86f, 0.48f, 1f));
            phaseText.alignment = TextAnchor.MiddleRight;

            CreateResultPanel(root.transform);
            CreatePausePanel(root.transform);

            if (this.boss != null)
            {
                this.boss.Died += ShowVictory;
            }

            if (this.player != null)
            {
                this.player.HealthChanged += RefreshPlayerHealthText;
                RefreshPlayerHealthText(this.player.CurrentHealth, this.player.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.HealthChanged -= RefreshPlayerHealthText;
            }

            if (boss != null)
            {
                boss.Died -= ShowVictory;
            }
        }

        private void RefreshPlayerHealthText(int current, int maximum)
        {
            if (playerHealthText != null)
            {
                playerHealthText.text = $"HP. {current}";
            }
        }

        private IEnumerator ShowDefeatAfterDelayRoutine(float delay)
        {
            defeatDelayRunning = true;
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay));
            defeatDelayRunning = false;
            ShowDefeat();
        }

        private void CreateResultPanel(Transform parent)
        {
            resultPanel = CreatePanel(parent, "ResultPanel", new Vector2(0f, 0f), new Vector2(460f, 250f), new Color(0.035f, 0.025f, 0.045f, 0.94f));
            resultText = CreateText(resultPanel.transform, "ResultText", "BATTLE END", new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(400f, 76f), 48, Color.white);

            retryButton = CreateButton(resultPanel.transform, "RetryButton", "RETRY", new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(180f, 52f));
            retryButton.onClick.AddListener(() => retryAction?.Invoke());
            resultPanel.SetActive(false);
        }

        private void CreatePausePanel(Transform parent)
        {
            pausePanel = CreatePanel(parent, "PausePanel", new Vector2(0f, 0f), new Vector2(380f, 190f), new Color(0.025f, 0.035f, 0.055f, 0.94f));
            CreateText(pausePanel.transform, "PauseText", "PAUSED", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(340f, 70f), 42, Color.white);
            Button button = CreateButton(pausePanel.transform, "ResumeButton", "RESUME", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(160f, 48f));
            button.onClick.AddListener(() => SetPaused(false));
            pausePanel.SetActive(false);
        }

        private static GameObject CreatePanel(Transform parent, string objectName, Vector2 position, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(objectName);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string objectName, string content, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string objectName, string label, Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.42f, 0.54f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.26f, 0.58f, 0.7f, 1f);
            colors.pressedColor = new Color(0.12f, 0.3f, 0.4f, 1f);
            button.colors = colors;

            CreateText(buttonObject.transform, "Label", label, new Vector2(0.5f, 0.5f), Vector2.zero, size, 22, Color.white);
            return button;
        }
    }
}
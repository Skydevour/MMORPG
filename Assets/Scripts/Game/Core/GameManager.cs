using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Config;
using MMORPG.Game.Level;
using MMORPG.Game.Player;
using MMORPG.Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MMORPG.Game.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private static float TargetAspect => GameConfigService.Current.level.width / GameConfigService.Current.level.height;

        [SerializeField] private LevelMapLoader levelMapLoader;
        [SerializeField] private PlayerSpawner playerSpawner;
        [SerializeField] private PotatoBossSpawner bossSpawner;

        private Transform runtimeRoot;
        private PlayerController2D player;
        private PotatoBossController boss;
        private PlayerEnergyMeter energyMeter;
        private BossBattleHud battleHud;
        private int lastScreenWidth;
        private int lastScreenHeight;
        private bool battleEnded;

        public PlayerController2D Player => player;

        public PotatoBossController Boss => boss;

        public BossBattleHud BattleHud => battleHud;

        private void Awake()
        {
            Time.timeScale = 1f;
            GameConfigService.Load();
            runtimeRoot = EnsureChild("Runtime");
            levelMapLoader = levelMapLoader != null ? levelMapLoader : gameObject.AddComponent<LevelMapLoader>();
            playerSpawner = playerSpawner != null ? playerSpawner : gameObject.AddComponent<PlayerSpawner>();
            bossSpawner = bossSpawner != null ? bossSpawner : gameObject.AddComponent<PotatoBossSpawner>();
        }

        private void Start()
        {
            LoadMap();
            InitializePlayer();
            InitializeBoss();
            InitializeBattleHud();
            ConfigureCamera();
            Debug.Log("Boss 关卡初始化完成，战斗开始。");
        }

        private void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                ConfigureCamera();
            }

            if (battleEnded || !PausePressedThisFrame() || battleHud == null)
            {
                return;
            }

            bool paused = !battleHud.IsPaused;
            battleHud.SetPaused(paused);
            Time.timeScale = paused ? 0f : 1f;
            Debug.Log(paused ? "Boss 关卡已暂停。" : "Boss 关卡已继续。");
        }

        public void LoadMap()
        {
            levelMapLoader.LoadLevel(runtimeRoot);
        }

        public void InitializePlayer()
        {
            player = playerSpawner.SpawnPlayer(runtimeRoot, levelMapLoader.PlayerSpawnPosition);
            energyMeter = PlayerEnergyMeter.Create(player.Energy);
            player.Died += HandlePlayerDefeated;
        }

        public void InitializeBoss()
        {
            boss = bossSpawner.SpawnBoss(runtimeRoot, levelMapLoader.BossSpawnPosition, player);
            if (player != null)
            {
                player.SetSuperTarget(boss);
            }

            boss.Died += HandleBossDefeated;
        }

        private void InitializeBattleHud()
        {
            battleHud = BossBattleHud.Create(player, boss, RestartBattle);
        }

        private void HandleBossDefeated()
        {
            if (battleEnded)
            {
                return;
            }

            battleEnded = true;
            Time.timeScale = 1f;
            if (player != null)
            {
                player.SetBattleLocked(true);
            }

            Debug.Log("土豆 Boss 已死亡，玩家战斗输入已锁定。");
        }

        private void HandlePlayerDefeated()
        {
            if (battleEnded)
            {
                return;
            }

            battleEnded = true;
            Time.timeScale = 1f;
            if (boss != null)
            {
                boss.SetBattleLocked(true);
            }

            if (battleHud != null)
            {
                battleHud.ShowDefeatAfterDelay(GameConfigService.Current.player.deathResultDelay);
            }

            Debug.Log($"玩家已死亡，Boss 攻击流程已停止，{GameConfigService.Current.player.deathResultDelay:0.0} 秒后显示失败结算。");
        }

        private void RestartBattle()
        {
            Time.timeScale = 1f;
            Debug.Log("正在重新加载 Boss 关卡。");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private static bool PausePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private Transform EnsureChild(string childName)
        {
            Transform existing = transform.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            return child.transform;
        }

        private void ConfigureCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = LevelMapLoader.LevelHeight * 0.5f;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            ApplyLetterbox(mainCamera);
        }

        private void ApplyLetterbox(Camera targetCamera)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            if (Screen.height <= 0)
            {
                return;
            }

            float windowAspect = (float)Screen.width / Screen.height;
            Rect viewport = new Rect(0f, 0f, 1f, 1f);

            if (windowAspect > TargetAspect)
            {
                float width = TargetAspect / windowAspect;
                viewport.x = (1f - width) * 0.5f;
                viewport.width = width;
            }
            else if (windowAspect < TargetAspect)
            {
                float height = windowAspect / TargetAspect;
                viewport.y = (1f - height) * 0.5f;
                viewport.height = height;
            }

            targetCamera.rect = viewport;
        }
    }
}
using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Level;
using MMORPG.Game.Player;
using MMORPG.Game.Projectiles;
using MMORPG.Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using MMORPG.Framework.Timing;
using MMORPG.Game.Audio;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MMORPG.Game.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private const float TargetAspect = 16f / 9f;

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
        private bool changingScene;
        private static bool retryImmediately;
#if UNITY_INCLUDE_TESTS
        public static bool LegacyTestMode;
#endif
        public EncounterDirector Encounter { get; private set; }

        public PlayerController2D Player => player;

        public PotatoBossController Boss => boss;

        public BossBattleHud BattleHud => battleHud;

        private void Awake()
        {
            BattleClock.ResetSession();
            GameConfigService.Load();
#if UNITY_INCLUDE_TESTS
            if (LegacyTestMode) GameConfigService.Current.encounter.enabled = false;
#endif
            BattleAudio.Initialize();
            BattleStats.Begin();
            BossProjectile.ResetAll();
            BossRootHazard.ResetAll();
            BossInsect.ResetAll();
            BossLobbedSeed.ResetAll();
            EncounterDirector.ClearAttacks();
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
            if (GameConfigService.Current.encounter.enabled)
            {
                Encounter = gameObject.AddComponent<EncounterDirector>();
                Encounter.BossChanged += battleHud.BindBoss;
                Encounter.Finished += OnEncounterFinished;
                Encounter.Started += () => { battleHud.gameObject.SetActive(true); energyMeter.gameObject.SetActive(true); };
                Encounter.Initialize(player, boss, runtimeRoot);
                battleHud.BindEncounter(Encounter);
                battleHud.TitleRequested += ReturnToTitle;
                if (retryImmediately) { retryImmediately = false; Encounter.Begin(); }
                else
                {
                    battleHud.gameObject.SetActive(false); energyMeter.gameObject.SetActive(false);
                    BattleTitleView.Create(this);
                }
            }
            Debug.Log("Boss 关卡初始化完成，战斗开始。");
        }

        private void Update()
        {
            if (Encounter == null || Encounter.State == EncounterState.Fighting) BattleStats.Active?.Tick(Time.deltaTime);

            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                ConfigureCamera();
            }

            if (battleEnded || BattleTitleView.IsOpen || (Encounter != null && Encounter.State == EncounterState.Title) || !PausePressedThisFrame() || battleHud == null)
            {
                return;
            }

            SetPaused(!battleHud.IsPaused);
        }

        public void SetPaused(bool paused)
        {
            if (battleEnded || battleHud == null) return;
            battleHud.SetPaused(paused);
            player.SuppressActionInput();
            BattleClock.SetPaused(paused);
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
            boss.Died += HandleBossDefeated;
        }

        private void InitializeBattleHud()
        {
            battleHud = BossBattleHud.Create(player, boss, RestartBattle);
            battleHud.ResumeRequested += ResumeBattle;
        }

        private void ResumeBattle() => SetPaused(false);

        private void OnDestroy()
        {
            if (player != null) player.Died -= HandlePlayerDefeated;
            if (boss != null) boss.Died -= HandleBossDefeated;
            if (battleHud != null) battleHud.ResumeRequested -= ResumeBattle;
            Time.timeScale = 1f;
        }

        private void HandleBossDefeated()
        {
            if (Encounter != null) return;
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

            BattleStats.Active?.Finish(true);
            Debug.Log("土豆 Boss 已死亡，玩家战斗输入已锁定。");
        }

        private void HandlePlayerDefeated()
        {
            if (Encounter != null) return;
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

            BossProjectile.DespawnAllActive();
            BossRootHazard.DespawnAllActive();
            BossInsect.DespawnAllActive();
            BossLobbedSeed.DespawnAllActive();

            if (battleHud != null)
            {
                battleHud.ShowDefeatAfterDelay(GameConfigService.Current.player.deathResultDelay);
            }

            BattleStats.Active?.Finish(false);
            Debug.Log($"玩家已死亡，Boss 攻击流程已停止，{GameConfigService.Current.player.deathResultDelay:0.0} 秒后显示失败结算。");
        }

        public void RestartBattle()
        {
            if (changingScene) return;
            changingScene = true;
            retryImmediately = GameConfigService.Current.encounter.enabled;
            BattleAudio.StopBattle();
            BattleClock.ResetSession();
            Time.timeScale = 1f;
            Debug.Log("正在重新加载 Boss 关卡。");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToTitle()
        {
            if (changingScene) return;
            changingScene = true;
            retryImmediately = false; BattleAudio.StopBattle(); BattleClock.ResetSession();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnEncounterFinished(bool won)
        {
            battleEnded = true; BattleClock.SetPaused(false);
            battleHud.SetEncounterResult(Encounter.FormIndex, Encounter.Progress);
            if (won) battleHud.ShowVictory();
            else battleHud.ShowDefeatAfterDelay(GameConfigService.Current.player.deathResultDelay);
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
            var cameraConfig = GameConfigService.Current.camera;
            mainCamera.orthographicSize = cameraConfig.viewHeight * 0.5f;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            var shake = mainCamera.GetComponent<ScreenShakeEffect>();
            if (shake != null) shake.ClearShake();
            mainCamera.transform.position = new Vector3(cameraConfig.centerX, cameraConfig.centerY, -10f);
            ApplyLetterbox(mainCamera);
            mainCamera.aspect = TargetAspect;
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

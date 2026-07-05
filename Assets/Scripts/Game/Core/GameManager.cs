using MMORPG.Game.Level;
using MMORPG.Game.Player;
using UnityEngine;

namespace MMORPG.Game.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private const float TargetAspect = LevelMapLoader.LevelWidth / LevelMapLoader.LevelHeight;

        [SerializeField] private LevelMapLoader levelMapLoader;
        [SerializeField] private PlayerSpawner playerSpawner;

        private Transform runtimeRoot;
        private PlayerController2D player;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            runtimeRoot = EnsureChild("Runtime");
            levelMapLoader = levelMapLoader != null ? levelMapLoader : gameObject.AddComponent<LevelMapLoader>();
            playerSpawner = playerSpawner != null ? playerSpawner : gameObject.AddComponent<PlayerSpawner>();
        }

        private void Start()
        {
            LoadMap();
            InitializePlayer();
            ConfigureCamera();
        }

        private void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                ConfigureCamera();
            }
        }

        public void LoadMap()
        {
            levelMapLoader.LoadLevel(runtimeRoot);
        }

        public void InitializePlayer()
        {
            player = playerSpawner.SpawnPlayer(runtimeRoot, levelMapLoader.PlayerSpawnPosition);
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

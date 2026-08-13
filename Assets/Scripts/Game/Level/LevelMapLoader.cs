using MMORPG.Game.Config;
using MMORPG.Game.Core;
using UnityEngine;

namespace MMORPG.Game.Level
{
    public sealed class LevelMapLoader : MonoBehaviour
    {
        public static float LevelWidth => GameConfigService.Current.level.width;
        public static float LevelHeight => GameConfigService.Current.level.height;
        public static float StageFloorY => GameConfigService.Current.level.stageFloorY;

        private const string BackgroundPath = "Assets/Res/Level01_Garden/Background/garden_background_wide.png";

        private Transform levelRoot;

        public Vector3 PlayerSpawnPosition { get; private set; } = new Vector3(GameConfigService.Current.level.playerSpawnX, StageFloorY, 0f);

        public Vector3 BossSpawnPosition { get; private set; } = new Vector3(GameConfigService.Current.boss.spawnX, StageFloorY, 0f);

        public void LoadLevel(Transform parent)
        {
            GameConfig config = GameConfigService.Current;
            PlayerSpawnPosition = new Vector3(config.level.playerSpawnX, config.level.stageFloorY, 0f);
            BossSpawnPosition = new Vector3(config.boss.spawnX, config.level.stageFloorY - config.boss.groundEmbedDepth, 0f);

            if (levelRoot != null)
            {
                Destroy(levelRoot.gameObject);
            }

            GameObject rootObject = new GameObject("Level01_Garden");
            rootObject.transform.SetParent(parent, false);
            levelRoot = rootObject.transform;

            CreateBackground(levelRoot);
            CreateStageCollision(levelRoot);
            CreateSpawnMarkers(levelRoot);
        }

        private void CreateBackground(Transform parent)
        {
            Sprite background = PrototypeAssetLoader.LoadSprite(BackgroundPath);
            if (background == null)
            {
                Debug.LogWarning($"缺少关卡背景资源：{BackgroundPath}");
                return;
            }

            GameObject backgroundObject = new GameObject("GardenBackground");
            backgroundObject.transform.SetParent(parent, false);
            backgroundObject.transform.position = new Vector3(0f, 0f, 4f);

            SpriteRenderer renderer = backgroundObject.AddComponent<SpriteRenderer>();
            renderer.sprite = background;
            renderer.sortingOrder = -100;

            Vector2 spriteSize = renderer.sprite.bounds.size;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                backgroundObject.transform.localScale = new Vector3(LevelWidth / spriteSize.x, LevelHeight / spriteSize.y, 1f);
            }
            CreateForegroundCover(parent, background, backgroundObject.transform.localScale);
        }

        private static void CreateForegroundCover(Transform parent, Sprite background, Vector3 backgroundScale)
        {
            if (background == null || background.texture == null)
            {
                return;
            }

            GameConfig config = GameConfigService.Current;
            Rect sourceRect = background.textureRect;
            float cropHeight = Mathf.Clamp(sourceRect.height * config.level.foregroundCropHeight, 1f, sourceRect.height);
            Rect foregroundRect = new Rect(sourceRect.x, sourceRect.y, sourceRect.width, cropHeight);
            Sprite foregroundSprite = Sprite.Create(background.texture, foregroundRect, new Vector2(0.5f, 0f), background.pixelsPerUnit);

            GameObject foregroundObject = new GameObject("GardenForegroundSoil");
            foregroundObject.transform.SetParent(parent, false);
            foregroundObject.transform.position = new Vector3(0f, -LevelHeight * 0.5f, 2f);
            foregroundObject.transform.localScale = backgroundScale;

            SpriteRenderer renderer = foregroundObject.AddComponent<SpriteRenderer>();
            renderer.sprite = foregroundSprite;
            renderer.sortingOrder = config.level.foregroundSortingOrder;
        }

        private void CreateSpawnMarkers(Transform parent)
        {
            GameObject gameplay = new GameObject("Gameplay");
            gameplay.transform.SetParent(parent, false);

            GameObject playerSpawn = new GameObject("PlayerSpawnPoint");
            playerSpawn.transform.SetParent(gameplay.transform, false);
            playerSpawn.transform.position = PlayerSpawnPosition;

            GameObject bossSpawn = new GameObject("BossSpawnPoint");
            bossSpawn.transform.SetParent(gameplay.transform, false);
            bossSpawn.transform.position = BossSpawnPosition;
        }

        private static void CreateStageCollision(Transform parent)
        {
            GameObject colliderObject = new GameObject("StageCollision");
            colliderObject.transform.SetParent(parent, false);

            Rigidbody2D body = colliderObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;

            float thickness = GameConfigService.Current.level.stageColliderThickness;
            BoxCollider2D groundCollider = colliderObject.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(LevelWidth, thickness);
            groundCollider.offset = new Vector2(0f, StageFloorY - thickness * 0.5f);
        }
    }
}

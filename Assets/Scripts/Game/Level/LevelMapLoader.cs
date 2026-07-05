using System.Collections.Generic;
using MMORPG.Game.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MMORPG.Game.Level
{
    public sealed class LevelMapLoader : MonoBehaviour
    {
        public const float LevelWidth = 16f;
        public const float LevelHeight = 9f;
        public const float TileWorldSize = 1f;

        private const string BackgroundPath = "Assets/Res/Level01_Garden/Background/garden_background_wide.png";
        private const string TileFolder = "Assets/Res/Level01_Garden/Tiles";
        private const float StageFloorY = -2.5f;
        private const float StageColliderThickness = 0.8f;

        private Transform levelRoot;

        public Vector3 PlayerSpawnPosition { get; private set; } = new Vector3(-5.75f, -1.9f, 0f);

        public void LoadLevel(Transform parent)
        {
            if (levelRoot != null)
            {
                Destroy(levelRoot.gameObject);
            }

            GameObject rootObject = new GameObject("Level01_Garden");
            rootObject.transform.SetParent(parent, false);
            levelRoot = rootObject.transform;

            CreateBackground(levelRoot);
            CreateTilemap(levelRoot);
            CreateStageCollision(levelRoot);
            CreateSpawnMarkers(levelRoot);
        }

        private void CreateBackground(Transform parent)
        {
            Sprite background = PrototypeAssetLoader.LoadSprite(BackgroundPath);
            if (background == null)
            {
                Debug.LogWarning($"Missing background sprite: {BackgroundPath}");
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
        }

        private void CreateTilemap(Transform parent)
        {
            Sprite[] tileSprites = PrototypeAssetLoader.LoadSpritesInFolder(TileFolder);
            if (tileSprites.Length == 0)
            {
                Debug.LogWarning($"No tile sprites found in {TileFolder}");
                return;
            }

            Dictionary<string, Tile> tiles = BuildTiles(tileSprites);
            GameObject gridObject = new GameObject("Grid");
            gridObject.transform.SetParent(parent, false);

            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(TileWorldSize, TileWorldSize, 0f);

            Tilemap ground = CreateTilemapObject("ForegroundTilemap", gridObject.transform, false);
            Tilemap decoration = CreateTilemapObject("DecorationTilemap", gridObject.transform, false);

            PaintForeground(ground, tiles);
            PaintDecorations(decoration, tiles);
        }

        private static Tilemap CreateTilemapObject(string name, Transform parent, bool collidable)
        {
            GameObject tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(parent, false);

            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = collidable ? 0 : 10;

            return tilemap;
        }

        private static Dictionary<string, Tile> BuildTiles(IEnumerable<Sprite> sprites)
        {
            Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
            foreach (Sprite sprite in sprites)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;
                tiles[sprite.name] = tile;
            }

            return tiles;
        }

        private static void PaintForeground(Tilemap ground, IReadOnlyDictionary<string, Tile> tiles)
        {
            Tile grass = FindTile(tiles, "grass_top");
            Tile mound = FindTile(tiles, "mound");
            Tile crackedSoil = FindTile(tiles, "cracked_soil");

            SetTileIfNotNull(ground, grass, -7, -3);
            SetTileIfNotNull(ground, grass, -6, -3);
            SetTileIfNotNull(ground, mound, -5, -3);
            SetTileIfNotNull(ground, crackedSoil, -1, -3);
            SetTileIfNotNull(ground, mound, 4, -3);
            SetTileIfNotNull(ground, grass, 5, -3);
        }

        private static void PaintDecorations(Tilemap decoration, IReadOnlyDictionary<string, Tile> tiles)
        {
            SetIfFound(decoration, tiles, "sprout", -4, -1);
            SetIfFound(decoration, tiles, "weeds", -2, -1);
            SetIfFound(decoration, tiles, "stones", 0, -1);
            SetIfFound(decoration, tiles, "wood_fence", 4, -1);
            SetIfFound(decoration, tiles, "flower_patch", 5, -1);
        }

        private static void SetTileIfNotNull(Tilemap tilemap, Tile tile, int x, int y)
        {
            if (tile != null)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        private static Tile FindTile(IReadOnlyDictionary<string, Tile> tiles, string namePart)
        {
            foreach (KeyValuePair<string, Tile> pair in tiles)
            {
                if (pair.Key.Contains(namePart))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        private static void SetIfFound(Tilemap tilemap, IReadOnlyDictionary<string, Tile> tiles, string namePart, int x, int y)
        {
            Tile tile = FindTile(tiles, namePart);
            if (tile != null)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
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
            bossSpawn.transform.position = new Vector3(5.75f, -1.9f, 0f);
        }

        private static void CreateStageCollision(Transform parent)
        {
            GameObject colliderObject = new GameObject("StageCollision");
            colliderObject.transform.SetParent(parent, false);

            Rigidbody2D body = colliderObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;

            BoxCollider2D groundCollider = colliderObject.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(LevelWidth, StageColliderThickness);
            groundCollider.offset = new Vector2(0f, StageFloorY - StageColliderThickness * 0.5f);
        }
    }
}

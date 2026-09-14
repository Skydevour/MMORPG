using MMORPG.Framework.Animation;
using MMORPG.Game.Config;
using MMORPG.Game.Core;
using MMORPG.Game.Level;
using MMORPG.Game.Player;
using UnityEngine;

namespace MMORPG.Game.Bosses.Potato
{
    public sealed class PotatoBossSpawner : MonoBehaviour
    {
        private const string FrameRoot = "Assets/Res/Bosses/Potato/Frames";

        public PotatoBossController SpawnBoss(Transform parent, Vector3 position, PlayerController2D target)
        {
            GameObject bossObject = new GameObject("PotatoBoss");
            bossObject.transform.SetParent(parent, false);
            bossObject.transform.position = position;

            GameConfig config = GameConfigService.Current;
            float bossScale = config.boss.scale;
            bossObject.transform.localScale = new Vector3(bossScale, bossScale, 1f);

            SpriteRenderer renderer = bossObject.AddComponent<SpriteRenderer>();
            Sprite[] idleFrames = PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/idle");
            if (idleFrames != null && idleFrames.Length > 0)
            {
                renderer.sprite = idleFrames[0];
            }
            if (config.encounter.enabled && renderer.sprite != null)
            {
                var form = config.encounter.forms[0];
                position = new Vector3(form.spawnX, config.level.stageFloorY - form.embedDepth, position.z);
            }
            renderer.sortingOrder = 45;

            BoxCollider2D hitCollider = bossObject.AddComponent<BoxCollider2D>();
            hitCollider.isTrigger = true;
            hitCollider.size = new Vector2(0.9f, 0.65f);
            hitCollider.offset = new Vector2(0f, 0.42f);
            if (config.encounter.enabled)
            {
                bossObject.transform.position = position;
                Resources.Load<GardenBossAssets>("Config/GardenBossAssets").potatoGeometry.Apply(bossObject.transform, hitCollider, config.encounter.forms[0]);
                position = bossObject.transform.position;
            }

            FrameAnimator animator = bossObject.AddComponent<FrameAnimator>();
            animator.SetClip("idle", idleFrames, 6f);
            animator.SetClip("attack_spit", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/attack_spit"), config.boss.attackAnimationFramesPerSecond, false);
            animator.SetClip("dead", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/hurt"), 6f, false);
            animator.Play("idle", true);

            float halfWidth = renderer.bounds.extents.x;
            float safeMinX = config.level.minStageX + halfWidth;
            float safeMaxX = config.level.maxStageX - halfWidth;
            float stageCenterX = (config.level.minStageX + config.level.maxStageX) * 0.5f;
            float safeSpawnX;
            if (safeMinX > safeMaxX)
            {
                safeSpawnX = stageCenterX;
            }
            else
            {
                safeSpawnX = Mathf.Clamp(Mathf.Max(position.x, stageCenterX), safeMinX, safeMaxX);
            }
            if (config.encounter.enabled) safeSpawnX = position.x;
            bossObject.transform.position = new Vector3(safeSpawnX, position.y, position.z);
            Debug.Log($"横向对峙布局初始化：玩家位于左侧，土豆 Boss 位于右侧，Boss 实际位置 X={safeSpawnX:0.00}，与玩家保持水平出生线。");

            PotatoBossController controller = bossObject.AddComponent<PotatoBossController>();
            controller.Initialize(target);

            PotatoBossStateDriver stateDriver = bossObject.AddComponent<PotatoBossStateDriver>();
            stateDriver.Initialize(controller, animator);
            return controller;
        }

        public PotatoBossController SpawnBoss(Transform parent, Vector3 position)
        {
            return SpawnBoss(parent, position, null);
        }
    }
}

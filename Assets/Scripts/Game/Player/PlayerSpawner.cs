using MMORPG.Game.Config;
using MMORPG.Game.Core;
using UnityEngine;

namespace MMORPG.Game.Player
{
    public sealed class PlayerSpawner : MonoBehaviour
    {
        private const string FrameRoot = "Assets/Res/Hero/Frames";
        private static readonly Vector3 VisualCenterFallback = new Vector3(0f, 0.68f, 0f);

        public PlayerController2D SpawnPlayer(Transform parent, Vector3 position)
        {
            PlayerConfig config = GameConfigService.Current.player;
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetParent(parent, false);
            playerObject.transform.position = position;

            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.gravityScale = 3.2f;

            CapsuleCollider2D collider = playerObject.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(config.colliderSizeX, config.colliderSizeY);
            collider.offset = new Vector2(config.colliderOffsetX, config.colliderOffsetY);

            playerObject.AddComponent<PlayerEnergyController>();
            Vector3 visualCenter = new Vector3(0f, config.visualCenterY, 0f);
            if (visualCenter == Vector3.zero)
            {
                visualCenter = VisualCenterFallback;
            }

            GameObject visualPivotObject = new GameObject("VisualPivot");
            visualPivotObject.transform.SetParent(playerObject.transform, false);
            visualPivotObject.transform.localPosition = visualCenter;

            GameObject visualObject = new GameObject("Sprite");
            visualObject.transform.SetParent(visualPivotObject.transform, false);
            visualObject.transform.localPosition = -visualCenter;

            SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 50;

            Sprite[] idleFrames = PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/idle");
            Sprite[] jumpFrames = PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/jump");
            if (idleFrames != null && idleFrames.Length > 0)
            {
                renderer.sprite = idleFrames[0];
                visualObject.transform.localPosition = -renderer.sprite.bounds.center;
            }
            

            PlayerSpriteAnimator animator = visualObject.AddComponent<PlayerSpriteAnimator>();
            animator.SetClip("idle", idleFrames, 7f);
            animator.SetClip("run", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/run"), 10f);
            animator.SetClip("jump", jumpFrames, 8f);
            animator.SetClip("dead", jumpFrames, 9f, false);
            animator.SetClip("dash", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/dash"), 12f);
            animator.SetClip("shoot", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/shoot"), 10f);

            PlayerController2D controller = playerObject.AddComponent<PlayerController2D>();
            controller.SetVisualRoot(visualPivotObject.transform);

            PlayerStateDriver stateDriver = playerObject.AddComponent<PlayerStateDriver>();
            stateDriver.Initialize(controller, animator);
            return controller;
        }
    }
}
using MMORPG.Game.Core;
using UnityEngine;

namespace MMORPG.Game.Player
{
    public sealed class PlayerSpawner : MonoBehaviour
    {
        private const string FrameRoot = "Assets/Res/Hero/Frames";

        public PlayerController2D SpawnPlayer(Transform parent, Vector3 position)
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetParent(parent, false);
            playerObject.transform.position = position;

            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = 3.4f;

            CapsuleCollider2D collider = playerObject.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.75f, 1.35f);
            collider.offset = new Vector2(0f, 0.08f);

            GameObject visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(playerObject.transform, false);
            visualObject.transform.localPosition = new Vector3(0f, collider.offset.y - collider.size.y * 0.5f, 0f);

            SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 50;

            PlayerSpriteAnimator animator = visualObject.AddComponent<PlayerSpriteAnimator>();
            animator.SetClip("idle", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/idle"), 10f);
            animator.SetClip("run", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/run"), 14f);
            animator.SetClip("jump", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/jump"), 12f);
            animator.SetClip("dash", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/dash"), 18f);
            animator.SetClip("shoot", PrototypeAssetLoader.LoadSpritesInFolder($"{FrameRoot}/shoot"), 14f);

            PlayerController2D controller = playerObject.AddComponent<PlayerController2D>();
            controller.SetVisualRoot(visualObject.transform);

            PlayerStateDriver stateDriver = playerObject.AddComponent<PlayerStateDriver>();
            stateDriver.Initialize(controller, animator);

            return controller;
        }
    }
}

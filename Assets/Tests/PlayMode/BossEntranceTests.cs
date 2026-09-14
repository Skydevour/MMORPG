#if UNITY_INCLUDE_TESTS
using MMORPG.Game.Bosses;
using NUnit.Framework;
using UnityEngine;

namespace MMORPG.Tests.PlayMode
{
    public sealed class BossEntranceTests
    {
        [Test]
        public void EntrancePreservesRootAndColliderAndRestoresHiddenBody()
        {
            var root = new GameObject("入场测试");
            var texture = new Texture2D(4, 4);
            var frame = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f));
            try
            {
                root.transform.localScale = new Vector3(3f, 3f, 1f);
                root.transform.position = new Vector3(4f, -2f, 0f);
                var body = root.AddComponent<SpriteRenderer>();
                body.sprite = frame; body.enabled = false;
                var collider = root.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 2f);
                var player = root.AddComponent<BossEntrancePlayer>();
                player.Begin(body, new BossEntranceClip { frames = new[] { frame, frame }, framesPerSecond = 10f }, 1f);
                player.Tick(0.1f);
                Assert.IsFalse(player.IsComplete);
                Assert.IsFalse(body.enabled);
                Assert.AreEqual(new Vector3(3f, 3f, 1f), root.transform.localScale);
                Assert.AreEqual(new Vector3(4f, -2f, 0f), root.transform.position);
                Assert.AreEqual(new Vector2(1f, 2f), collider.size);
                player.Tick(0.11f);
                Assert.IsTrue(player.IsComplete);
                Assert.IsTrue(body.enabled);
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(frame); Object.DestroyImmediate(texture); }
        }

        [Test]
        public void EntrancePauseAndCancelDoNotLeaveOverlay()
        {
            var root = new GameObject("入场取消测试");
            var texture = new Texture2D(4, 4);
            var frame = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero);
            try
            {
                var body = root.AddComponent<SpriteRenderer>();
                var player = root.AddComponent<BossEntrancePlayer>();
                player.Begin(body, new BossEntranceClip { frames = new[] { frame }, framesPerSecond = 10f }, 1f);
                player.Tick(0f);
                Assert.IsFalse(player.IsComplete);
                player.Cancel(); player.Tick(1f);
                Assert.IsFalse(player.IsComplete, "取消不能当作正常完成。");
                Assert.IsTrue(player.IsCancelled);
                Assert.IsTrue(body.enabled);
                Assert.IsFalse(root.transform.Find("EntranceVisual").GetComponent<SpriteRenderer>().enabled);
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(frame); Object.DestroyImmediate(texture); }
        }
    }
}
#endif

#if UNITY_INCLUDE_TESTS
using MMORPG.Framework.Animation;
using MMORPG.Game.Bosses.Potato;
using NUnit.Framework;

namespace MMORPG.Tests.PlayMode
{
    public sealed class PotatoAttackTimelineTests
    {
        [Test]
        public void PauseAndBoundaryDoNotRepeatMarker()
        {
            var clock = new OneShotTimeline(2f, 0f, 1f);
            Assert.AreEqual(-1, clock.Advance(0f));
            Assert.AreEqual(0, clock.Advance(0.1f));
            Assert.AreEqual(-1, clock.Advance(0f));
            Assert.AreEqual(1, clock.Advance(1f));
            Assert.AreEqual(-1, clock.Advance(0.1f));
        }

        [Test]
        public void CancelBeforeReleaseAndRestartLeaveNoOldEvents()
        {
            var clock = new OneShotTimeline(2f, 1f);
            clock.Advance(0.999f);
            clock.Cancel();
            Assert.AreEqual(-1, clock.Advance(10f));
            Assert.IsFalse(clock.IsComplete);
            Assert.IsFalse(clock.IsActive);
            var restart = new OneShotTimeline(2f, 1f);
            Assert.AreEqual(0, restart.Advance(1f));
        }

        [Test]
        public void HugeFramesKeepThreeShotsAndFullFinalRecovery()
        {
            var sequence = new PotatoAttackSequence(0.5f, 0.18f, 3, 0.4f);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, sequence.Clock.Advance(10f));
                int frame = (int)(sequence.VisualProgress * 6f);
                Assert.That(frame, Is.InRange(2, 3));
                Assert.IsTrue(sequence.Clock.IsActive);
                Assert.Greater(sequence.Clock.DiscardedTime, 0f);
                Assert.AreEqual(-1, sequence.Clock.Advance(0f));
            }
            Assert.AreEqual(-1, sequence.Clock.Advance(0.2f));
            Assert.IsTrue(sequence.Clock.IsActive);
            Assert.AreEqual(-1, sequence.Clock.Advance(0.21f));
            Assert.IsTrue(sequence.Clock.IsComplete);
            Assert.AreEqual(1f, sequence.VisualProgress, 0.0001f);
            Assert.AreEqual(-1, sequence.Clock.Advance(10f));
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void NormalFramesRemainMonotonicAndReleaseExactlyThreeTimes(int fps)
        {
            var sequence = new PotatoAttackSequence(0.5f, 0.18f, 3, 0.4f);
            int shots = 0;
            float previous = 0f;
            for (int i = 0; i < fps * 3 && sequence.Clock.IsActive; i++)
            {
                int marker = sequence.Clock.Advance(1f / fps);
                Assert.GreaterOrEqual(sequence.VisualProgress, previous);
                previous = sequence.VisualProgress;
                if (marker < 0) continue;
                Assert.AreEqual(shots++, marker);
                Assert.That((int)(previous * 6f), Is.InRange(2, 3));
            }
            Assert.AreEqual(3, shots);
            Assert.IsTrue(sequence.Clock.IsComplete);
        }

        [Test]
        public void InvalidMarkersAreRejected()
        {
            Assert.Throws<System.ArgumentException>(() => new OneShotTimeline(1f, 0.5f, 0.5f));
            Assert.Throws<System.ArgumentException>(() => new OneShotTimeline(1f, 2f));
            var clock = new OneShotTimeline(1f);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => clock.Advance(float.NaN));
        }

        [TestCase(0f, 0.4f, 3)]
        [TestCase(0.5f, 0f, 3)]
        [TestCase(0f, 0f, 3)]
        [TestCase(0f, 0f, 1)]
        public void ZeroTellAndRecoveryPreserveReleases(float tell, float recovery, int count)
        {
            var sequence = new PotatoAttackSequence(tell, 0.18f, count, recovery);
            Assert.AreEqual(-1, sequence.Clock.Advance(0f));
            Assert.IsTrue(sequence.Clock.IsActive);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(i, sequence.Clock.Advance(10f));
                Assert.That((int)(sequence.VisualProgress * 6f), Is.InRange(2, 3));
                Assert.IsFalse(float.IsNaN(sequence.VisualProgress));
                Assert.IsFalse(float.IsInfinity(sequence.VisualProgress));
            }
            Assert.AreEqual(recovery == 0f, sequence.Clock.IsComplete);
            Assert.AreEqual(-1, sequence.Clock.Advance(10f));
            Assert.IsTrue(sequence.Clock.IsComplete);
        }
    }
}
#endif

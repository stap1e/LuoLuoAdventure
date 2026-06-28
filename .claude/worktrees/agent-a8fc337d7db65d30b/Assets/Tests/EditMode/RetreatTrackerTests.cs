using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class RetreatTrackerTests
    {
        [Test]
        public void Tick_Outside_IncrementsTimer()
        {
            var tracker = new RetreatTracker();
            tracker.Configure(10f);
            tracker.Tick(3f, playerInside: false);
            Assert.That(tracker.CurrentTimer, Is.EqualTo(3f));
        }

        [Test]
        public void Tick_Inside_ResetsTimer()
        {
            var tracker = new RetreatTracker();
            tracker.Configure(10f);
            tracker.Tick(5f, playerInside: false);
            tracker.Tick(1f, playerInside: true);
            Assert.That(tracker.CurrentTimer, Is.EqualTo(0f));
        }

        [Test]
        public void IsRetreating_True_WhenTimerExceedsThreshold()
        {
            var tracker = new RetreatTracker();
            tracker.Configure(5f);
            tracker.Tick(6f, playerInside: false);
            Assert.That(tracker.IsRetreating, Is.True);
        }

        [Test]
        public void Reset_ClearsTimer()
        {
            var tracker = new RetreatTracker();
            tracker.Configure(5f);
            tracker.Tick(4f, playerInside: false);
            tracker.Reset();
            Assert.That(tracker.CurrentTimer, Is.EqualTo(0f));
        }

        [Test]
        public void Progress_ClampedToOne()
        {
            var tracker = new RetreatTracker();
            tracker.Configure(5f);
            tracker.Tick(10f, playerInside: false);
            Assert.That(tracker.Progress, Is.EqualTo(1f));
        }
    }
}

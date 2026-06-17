using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionTriggerIdempotencyTests
    {
        [Test]
        public void ForceStart_AfterMarkCompleted_DoesNotRetrigger()
        {
            var go = new GameObject("TriggerZone");
            try
            {
                var zone = go.AddComponent<MissionTriggerZone>();
                zone.ForceStart();
                zone.MarkCompleted();
                zone.ForceStart();
                Assert.That(zone.MissionCompleted, Is.True);
                Assert.That(zone.MissionCompleted, Is.True, "Completed flag must remain set");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ForceStart_OnFreshZone_StartsMission()
        {
            var go = new GameObject("TriggerZone");
            try
            {
                var zone = go.AddComponent<MissionTriggerZone>();
                zone.ForceStart();
                Assert.That(zone.MissionStarted, Is.True);
                Assert.That(zone.MissionCompleted, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Reset_AllowsForceStartAgain()
        {
            var go = new GameObject("TriggerZone");
            try
            {
                var zone = go.AddComponent<MissionTriggerZone>();
                zone.ForceStart();
                zone.MarkCompleted();
                zone.Reset();
                zone.ForceStart();
                Assert.That(zone.MissionStarted, Is.True);
                Assert.That(zone.MissionCompleted, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

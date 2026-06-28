using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionTriggerZoneRegressionTests
    {
        [Test]
        public void MarkCompleted_PreventsRetrigger()
        {
            var go = new GameObject("TriggerZone");
            try
            {
                var zone = go.AddComponent<MissionTriggerZone>();
                zone.MarkCompleted();
                Assert.That(zone.MissionCompleted, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Reset_ClearsStartedAndCompleted()
        {
            var go = new GameObject("TriggerZone");
            try
            {
                var zone = go.AddComponent<MissionTriggerZone>();
                zone.ForceStart();
                zone.MarkCompleted();
                zone.Reset();
                Assert.That(zone.MissionStarted, Is.False);
                Assert.That(zone.MissionCompleted, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ForceStart_SetsMissionStarted()
        {
            var go = new GameObject("TriggerZone");
            try
            {
                var zone = go.AddComponent<MissionTriggerZone>();
                zone.ForceStart();
                Assert.That(zone.MissionStarted, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

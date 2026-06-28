using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionAreaRuntimeTests
    {
        [Test]
        public void Activate_SetsIsActive()
        {
            var go = new GameObject("Area");
            try
            {
                var area = go.AddComponent<MissionAreaRuntime>();
                area.Activate("test_mission");
                Assert.That(area.IsActive, Is.True);
                Assert.That(area.MissionId, Is.EqualTo("test_mission"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MarkComplete_SetsIsComplete()
        {
            var go = new GameObject("Area");
            try
            {
                var area = go.AddComponent<MissionAreaRuntime>();
                area.Activate("test");
                area.MarkComplete();
                Assert.That(area.IsComplete, Is.True);
                Assert.That(area.IsActive, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

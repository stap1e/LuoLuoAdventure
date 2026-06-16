using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class RetreatBoundarySmokeTests
    {
        [UnityTest]
        public IEnumerator RetreatTracker_TicksAndResets()
        {
            var tracker = new RetreatTracker();
            tracker.Configure(5f);
            tracker.Tick(3f, playerInside: false);
            yield return null;
            Assert.That(tracker.CurrentTimer, Is.EqualTo(3f));
            Assert.That(tracker.IsRetreating, Is.False);

            tracker.Tick(3f, playerInside: false);
            Assert.That(tracker.IsRetreating, Is.True);

            tracker.Tick(1f, playerInside: true);
            Assert.That(tracker.CurrentTimer, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator MissionBoundary_DetectsInside()
        {
            var go = new GameObject("Boundary");
            try
            {
                var boundary = go.AddComponent<MissionBoundary>();
                boundary.Center = Vector3.zero;
                boundary.Radius = 10f;
                yield return null;
                Assert.That(boundary.IsInside(new Vector3(5f, 0f, 0f)), Is.True);
                Assert.That(boundary.IsInside(new Vector3(15f, 0f, 0f)), Is.False);
            }
            finally
            {
                Object.Destroy(go);
            }
        }
    }
}

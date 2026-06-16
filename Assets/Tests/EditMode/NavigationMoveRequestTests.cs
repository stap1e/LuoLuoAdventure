using LuoLuoTrip.AI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class NavigationMoveRequestTests
    {
        [Test]
        public void To_CreatesRequest_WithCorrectValues()
        {
            var req = NavigationMoveRequest.To(new Vector3(5f, 0f, 3f), 4f, 1.5f);
            Assert.That(req.Destination, Is.EqualTo(new Vector3(5f, 0f, 3f)));
            Assert.That(req.Speed, Is.EqualTo(4f));
            Assert.That(req.StopDistance, Is.EqualTo(1.5f));
            Assert.That(req.StopOnArrive, Is.True);
        }

        [Test]
        public void Follow_CreatesRequest_WithStopOnArriveFalse()
        {
            var go = new GameObject("Target");
            try
            {
                var req = NavigationMoveRequest.Follow(go.transform, 3f, 2f);
                Assert.That(req.StopOnArrive, Is.False);
                Assert.That(req.Speed, Is.EqualTo(3f));
                Assert.That(req.StopDistance, Is.EqualTo(2f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasReached_ReturnsTrue_WhenWithinStopDistance()
        {
            var req = NavigationMoveRequest.To(new Vector3(10f, 0f, 0f), 4f, 2f);
            Assert.That(req.HasReached(new Vector3(9f, 0f, 0f)), Is.True);
        }

        [Test]
        public void HasReached_ReturnsFalse_WhenOutsideStopDistance()
        {
            var req = NavigationMoveRequest.To(new Vector3(10f, 0f, 0f), 4f, 2f);
            Assert.That(req.HasReached(new Vector3(5f, 0f, 0f)), Is.False);
        }
    }
}

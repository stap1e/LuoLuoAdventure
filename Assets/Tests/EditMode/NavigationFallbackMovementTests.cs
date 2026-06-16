using LuoLuoTrip;
using LuoLuoTrip.AI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class NavigationFallbackMovementTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        private NavigationAgentBridge CreateAgent(Vector3 startPos)
        {
            _go = new GameObject("AIRoot");
            _go.transform.position = startPos;
            _go.AddComponent<CharacterMovementMotor>();
            return _go.AddComponent<NavigationAgentBridge>();
        }

        [Test]
        public void Fallback_SetDestination_MovesRoot_KeepsY()
        {
            var bridge = CreateAgent(new Vector3(0f, 0.5f, 0f));
            bridge.SetDestination(NavigationMoveRequest.To(new Vector3(5f, 0.5f, 0f), 6f, 0.5f));
            for (int i = 0; i < 30; i++)
                bridge.TickFallback(0.1f);
            Assert.Greater(_go.transform.position.x, 1f);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-3, "Fallback must not move Y");
        }

        [Test]
        public void Fallback_HasReachedDestination_When_Close()
        {
            var bridge = CreateAgent(new Vector3(0f, 0.5f, 0f));
            bridge.SetDestination(NavigationMoveRequest.To(new Vector3(0.2f, 0.5f, 0f), 6f, 1f));
            Assert.IsTrue(bridge.HasReachedDestination());
        }

        [Test]
        public void Fallback_DoesNotChangeY_With_VerticalDestination()
        {
            var bridge = CreateAgent(new Vector3(0f, 0.5f, 0f));
            bridge.SetDestination(NavigationMoveRequest.To(new Vector3(3f, 99f, 0f), 6f, 0.5f));
            for (int i = 0; i < 30; i++)
                bridge.TickFallback(0.1f);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-3);
        }

        [Test]
        public void Fallback_StopsAtDestination()
        {
            var bridge = CreateAgent(new Vector3(0f, 0.5f, 0f));
            bridge.SetDestination(NavigationMoveRequest.To(new Vector3(2f, 0.5f, 0f), 4f, 0.5f));
            for (int i = 0; i < 50; i++)
                bridge.TickFallback(0.1f);
            Assert.LessOrEqual(Mathf.Abs(_go.transform.position.x - 2f), 0.6f);
        }
    }
}

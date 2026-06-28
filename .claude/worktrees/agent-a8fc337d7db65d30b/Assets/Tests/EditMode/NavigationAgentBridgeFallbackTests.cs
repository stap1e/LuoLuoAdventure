using LuoLuoTrip.AI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class NavigationAgentBridgeFallbackTests
    {
        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestBridge");
            _go.AddComponent<NavigationAgentBridge>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void Bridge_StartsIdle()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            Assert.That(bridge.State, Is.EqualTo(NavigationState.Idle));
        }

        [Test]
        public void UseNavMesh_IsFalse_WithoutNavMeshAgent()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            Assert.That(bridge.UseNavMesh, Is.False);
        }

        [Test]
        public void SetDestination_ChangesStateToMoving()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            bridge.SetDestination(new Vector3(10f, 0f, 0f), 4f);
            Assert.That(bridge.State, Is.EqualTo(NavigationState.Moving));
        }

        [Test]
        public void Stop_ChangesStateToStopped()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            bridge.SetDestination(new Vector3(10f, 0f, 0f), 4f);
            bridge.Stop();
            Assert.That(bridge.State, Is.EqualTo(NavigationState.Stopped));
        }

        [Test]
        public void ClearRequest_ChangesStateToIdle()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            bridge.SetDestination(new Vector3(10f, 0f, 0f), 4f);
            bridge.ClearRequest();
            Assert.That(bridge.State, Is.EqualTo(NavigationState.Idle));
        }

        [Test]
        public void HasReachedDestination_TrueWhenNoRequest()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            Assert.That(bridge.HasReachedDestination(), Is.True);
        }
    }
}

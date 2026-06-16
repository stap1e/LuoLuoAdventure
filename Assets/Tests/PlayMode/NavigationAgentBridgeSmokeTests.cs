using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class NavigationAgentBridgeSmokeTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _go = new GameObject("TestBridge");
            _go.AddComponent<NavigationAgentBridge>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Bridge_FallbackMovement_MovesTowardsDestination()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            bridge.SetDestination(new Vector3(5f, 0f, 0f), 10f, 0.5f);
            yield return null;

            for (int i = 0; i < 60; i++)
            {
                bridge.TickFallback(Time.deltaTime);
                yield return null;
                if (bridge.HasReachedDestination()) break;
            }

            Assert.That(bridge.HasReachedDestination(), Is.True);
        }

        [UnityTest]
        public IEnumerator Bridge_StopPreventsMovement()
        {
            var bridge = _go.GetComponent<NavigationAgentBridge>();
            bridge.SetDestination(new Vector3(20f, 0f, 0f), 5f);
            yield return null;

            bridge.Stop();
            var posBefore = _go.transform.position;
            yield return null;
            bridge.TickFallback(Time.deltaTime);
            yield return null;

            Assert.That(_go.transform.position, Is.EqualTo(posBefore));
        }
    }
}

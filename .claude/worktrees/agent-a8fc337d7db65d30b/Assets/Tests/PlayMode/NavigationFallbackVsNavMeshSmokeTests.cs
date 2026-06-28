using System.Collections;
using LuoLuoTrip.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class NavigationFallbackVsNavMeshSmokeTests
    {
        [UnityTest]
        public IEnumerator NavigationAgentBridge_Fallback_WithoutNavMeshAgent()
        {
            var go = new GameObject("Unit");
            go.transform.position = Vector3.zero;
            var motor = go.AddComponent<CharacterMovementMotor>();
            var bridge = go.AddComponent<NavigationAgentBridge>();

            yield return null;

            Assert.AreEqual(NavMeshMode.Fallback, bridge.Mode);
            Assert.IsFalse(bridge.HasNavMeshAgent);

            bridge.SetDestination(new Vector3(10f, 0f, 0f), 5f, 0.5f);

            for (int i = 0; i < 10; i++)
            {
                bridge.TickFallback(0.1f);
                yield return null;
            }

            Assert.Greater(go.transform.position.x, 0.5f, "Fallback should move X");
            Assert.AreEqual(0f, go.transform.position.y, 0.01f, "Y should be locked");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator NavigationAgentBridge_NavMeshMode_Report()
        {
            var go1 = new GameObject("Unit1");
            go1.transform.position = Vector3.zero;
            go1.AddComponent<CharacterMovementMotor>();
            var b1 = go1.AddComponent<NavigationAgentBridge>();

            var go2 = new GameObject("Unit2");
            go2.transform.position = new Vector3(5f, 0f, 0f);
            go2.AddComponent<CharacterMovementMotor>();
            var b2 = go2.AddComponent<NavigationAgentBridge>();

            yield return null;

            var sceneMode = NavigationAgentBridge.GetSceneMode();
            // Without baked NavMesh, both should be fallback
            Assert.AreEqual(NavMeshMode.Fallback, sceneMode);

            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }

        [UnityTest]
        public IEnumerator NavigationAgentBridge_Fallback_LocksY()
        {
            var go = new GameObject("Unit");
            go.transform.position = new Vector3(0f, 1.5f, 0f);
            var motor = go.AddComponent<CharacterMovementMotor>();
            var bridge = go.AddComponent<NavigationAgentBridge>();

            yield return null;

            bridge.SetDestination(new Vector3(5f, 0f, 5f), 4f, 0.5f);

            for (int i = 0; i < 10; i++)
            {
                bridge.TickFallback(0.1f);
                yield return null;
            }

            Assert.AreEqual(1.5f, go.transform.position.y, 0.1f, "Y should remain at initial value");

            Object.DestroyImmediate(go);
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CameraFollowControlTests
    {
        [UnityTest]
        public IEnumerator CameraFollowController_SetTarget_FollowsTarget()
        {
            var cameraGo = new GameObject("TestCamera");
            var follow = cameraGo.AddComponent<CameraFollowController>();

            var targetGo = new GameObject("Target");
            targetGo.transform.position = new Vector3(10f, 0f, 0f);

            follow.SetTarget(targetGo.transform);

            yield return null;
            yield return null;

            Assert.That(follow.Target, Is.EqualTo(targetGo.transform));

            Object.DestroyImmediate(cameraGo);
            Object.DestroyImmediate(targetGo);
        }

        [UnityTest]
        public IEnumerator CameraFollowController_NullTarget_NoFollow()
        {
            var cameraGo = new GameObject("TestCamera2");
            var follow = cameraGo.AddComponent<CameraFollowController>();
            var initialPos = cameraGo.transform.position;

            follow.SetTarget(null);

            yield return null;
            yield return null;

            Assert.That(cameraGo.transform.position, Is.EqualTo(initialPos));

            Object.DestroyImmediate(cameraGo);
        }
    }
}

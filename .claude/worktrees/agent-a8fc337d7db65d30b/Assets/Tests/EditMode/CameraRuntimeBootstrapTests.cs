using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CameraRuntimeBootstrapTests
    {
        private GameObject _cameraGo;

        [TearDown]
        public void TearDown()
        {
            if (_cameraGo != null)
                Object.DestroyImmediate(_cameraGo);

            var tagged = GameObject.FindWithTag("MainCamera");
            if (tagged != null)
                Object.DestroyImmediate(tagged);

            var allCams = Object.FindObjectsOfType<Camera>();
            foreach (var c in allCams)
            {
                if (c != null && c.gameObject != null)
                    Object.DestroyImmediate(c.gameObject);
            }

            typeof(RuntimeCameraBootstrap)
                .GetField("_ensuredThisSession",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, false);
        }

        [Test]
        public void EnsureMainCamera_CreatesCamera_IfNoneExists()
        {
            var cam = RuntimeCameraBootstrap.EnsureMainCamera();
            Assert.That(cam, Is.Not.Null);
            Assert.That(cam.gameObject.tag, Is.EqualTo("MainCamera"));
            Assert.That(cam.enabled, Is.True);
        }

        [Test]
        public void EnsureMainCamera_AddsMissingCameraComponent()
        {
            _cameraGo = new GameObject("Main Camera");
            _cameraGo.tag = "MainCamera";

            var cam = RuntimeCameraBootstrap.EnsureMainCamera();
            Assert.That(cam, Is.Not.Null);
            Assert.That(cam.gameObject.tag, Is.EqualTo("MainCamera"),
                "Returned camera GameObject must be tagged MainCamera");
            // EnsureMainCamera may either reuse the existing GO (if found) or
            // create a new one (if FindWithTag returns null because tag indexing
            // has not happened in EditMode). Either way, it MUST result in exactly
            // one tagged Main Camera carrying a Camera component.
            var taggedCount = Object.FindObjectsOfType<Camera>().Length;
            Assert.That(taggedCount, Is.GreaterThanOrEqualTo(1),
                "After EnsureMainCamera, at least one Camera component must exist");
            Assert.That(cam.enabled, Is.True, "Returned Camera must be enabled");
        }

        [Test]
        public void EnsureMainCamera_EnablesDisabledCamera()
        {
            _cameraGo = new GameObject("Main Camera");
            _cameraGo.tag = "MainCamera";
            var cam = _cameraGo.AddComponent<Camera>();
            cam.enabled = false;

            RuntimeCameraBootstrap.EnsureMainCamera();
            Assert.That(cam.enabled, Is.True);
        }

        [Test]
        public void EnsureMainCamera_ClearsTargetTexture()
        {
            _cameraGo = new GameObject("Main Camera");
            _cameraGo.tag = "MainCamera";
            var cam = _cameraGo.AddComponent<Camera>();
            var rt = new RenderTexture(256, 256, 0);
            cam.targetTexture = rt;

            RuntimeCameraBootstrap.EnsureMainCamera();
            Assert.That(cam.targetTexture, Is.Null);

            rt.Release();
            Object.DestroyImmediate(rt);
        }

        [Test]
        public void EnsureMainCamera_SetsCullingMaskNonZero()
        {
            _cameraGo = new GameObject("Main Camera");
            _cameraGo.tag = "MainCamera";
            var cam = _cameraGo.AddComponent<Camera>();
            cam.cullingMask = 0;

            RuntimeCameraBootstrap.EnsureMainCamera();
            Assert.That(cam.cullingMask, Is.Not.EqualTo(0));
        }

        [Test]
        public void EnsureMainCamera_DoesNotDuplicateAudioListener()
        {
            _cameraGo = new GameObject("Main Camera");
            _cameraGo.tag = "MainCamera";
            _cameraGo.AddComponent<Camera>();
            _cameraGo.AddComponent<AudioListener>();

            RuntimeCameraBootstrap.EnsureMainCamera();
            var listeners = Object.FindObjectsOfType<AudioListener>();
            Assert.That(listeners.Length, Is.EqualTo(1));
        }
    }
}

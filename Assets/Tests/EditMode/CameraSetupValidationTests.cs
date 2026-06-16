using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CameraSetupValidationTests
    {
        private GameObject _cameraGo;

        [TearDown]
        public void TearDown()
        {
            if (_cameraGo != null)
                Object.DestroyImmediate(_cameraGo);
            var existing = GameObject.Find("TestCamera");
            if (existing != null) Object.DestroyImmediate(existing);
        }

        [Test]
        public void Camera_TargetTexture_Null_ByDefault()
        {
            _cameraGo = new GameObject("TestCamera");
            var cam = _cameraGo.AddComponent<Camera>();
            Assert.That(cam.targetTexture, Is.Null);
        }

        [Test]
        public void Camera_CullingMask_NonZero_ByDefault()
        {
            _cameraGo = new GameObject("TestCamera");
            var cam = _cameraGo.AddComponent<Camera>();
            Assert.That(cam.cullingMask, Is.Not.EqualTo(0));
        }

        [Test]
        public void Camera_Enabled_ByDefault()
        {
            _cameraGo = new GameObject("TestCamera");
            var cam = _cameraGo.AddComponent<Camera>();
            Assert.That(cam.enabled, Is.True);
        }

        [Test]
        public void Camera_TargetDisplay_Zero_ByDefault()
        {
            _cameraGo = new GameObject("TestCamera");
            var cam = _cameraGo.AddComponent<Camera>();
            Assert.That(cam.targetDisplay, Is.EqualTo(0));
        }

        [Test]
        public void RuntimeCameraBootstrap_Type_Exists()
        {
            Assert.That(typeof(RuntimeCameraBootstrap), Is.Not.Null);
        }

        [Test]
        public void RuntimeCameraBootstrap_HasEnsureMainCamera_StaticMethod()
        {
            var method = typeof(RuntimeCameraBootstrap).GetMethod("EnsureMainCamera");
            Assert.That(method, Is.Not.Null);
            Assert.That(method.IsStatic, Is.True);
            Assert.That(method.ReturnType, Is.EqualTo(typeof(Camera)));
        }
    }
}

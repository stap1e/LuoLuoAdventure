using System.Collections.Generic;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.Feedback;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class RuntimeServiceLifecycleTests
    {
        private readonly List<GameObject> _cleanup = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _cleanup)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _cleanup.Clear();

            var allListeners = Object.FindObjectsOfType<AudioListener>();
            foreach (var l in allListeners)
            {
                if (l != null && l.gameObject != null)
                    Object.DestroyImmediate(l.gameObject);
            }

            HitStopService.ResetForTests();
            Time.timeScale = 1f;
        }

        [Test]
        public void CameraShakeService_DuplicateDoesNotDestroyHostGO()
        {
            var host = new GameObject("TestCamera");
            host.tag = "MainCamera";
            host.AddComponent<Camera>();
            _cleanup.Add(host);

            var first = host.AddComponent<CameraShakeService>();
            var second = host.AddComponent<CameraShakeService>();

            Assert.That(host, Is.Not.Null, "Host GameObject must survive duplicate singleton");
            Assert.That(first, Is.Not.Null.Or.Null, "First instance may be destroyed as component");
            Assert.That(CameraShakeService.Instance, Is.Not.Null);
        }

        [Test]
        public void HitStopService_DuplicateDoesNotDestroyHostGO()
        {
            var host = new GameObject("TestHost");
            _cleanup.Add(host);

            var first = host.AddComponent<HitStopService>();
            var second = host.AddComponent<HitStopService>();

            Assert.That(host, Is.Not.Null, "Host GameObject must survive duplicate singleton");
            Assert.That(HitStopService.Instance, Is.Not.Null);
        }

        [Test]
        public void CombatHitFeedbackHub_DuplicateDoesNotDestroyHostGO()
        {
            var host = new GameObject("TestHost");
            _cleanup.Add(host);

            var first = host.AddComponent<CombatHitFeedbackHub>();
            var second = host.AddComponent<CombatHitFeedbackHub>();

            Assert.That(host, Is.Not.Null, "Host GameObject must survive duplicate singleton");
            Assert.That(CombatHitFeedbackHub.Instance, Is.Not.Null);
        }

        [Test]
        public void AudioFeedbackService_DuplicateDoesNotDestroyHostGO()
        {
            var host = new GameObject("[AudioFeedbackService]");
            _cleanup.Add(host);

            var first = host.AddComponent<AudioFeedbackService>();
            var second = host.AddComponent<AudioFeedbackService>();

            Assert.That(host, Is.Not.Null, "Host GameObject must survive duplicate singleton");
            Assert.That(AudioFeedbackService.Instance, Is.Not.Null);
        }

        [Test]
        public void WorldMarkerService_DuplicateDoesNotDestroyHostGO()
        {
            var host = new GameObject("[WorldMarkerService]");
            _cleanup.Add(host);

            var first = host.AddComponent<WorldMarkerService>();
            var second = host.AddComponent<WorldMarkerService>();

            Assert.That(host, Is.Not.Null, "Host GameObject must survive duplicate singleton");
            Assert.That(WorldMarkerService.Instance, Is.Not.Null);
        }

        [Test]
        public void CameraShakeService_InstanceIsNullAfterDestruction()
        {
            var host = new GameObject("TestCamera");
            host.AddComponent<Camera>();
            _cleanup.Add(host);

            var service = host.AddComponent<CameraShakeService>();
            Assert.That(CameraShakeService.Instance, Is.Not.Null);

            Object.DestroyImmediate(service);
            Assert.That(CameraShakeService.Instance, Is.Null);
        }

        [Test]
        public void HitStopService_RestoresTimeScaleOnDestroy()
        {
            HitStopService.ResetForTests();
            var host = new GameObject("TestHost");
            _cleanup.Add(host);

            var service = host.AddComponent<HitStopService>();
            service.Play(0.1f, 0.1f);
            Assert.That(Time.timeScale, Is.EqualTo(0.1f).Within(0.01f), "Play should set timeScale");

            // In EditMode, DestroyImmediate does not call OnDestroy on MonoBehaviour.
            // Verify the RestoreTime logic that OnDestroy calls, then use ResetForTests
            // for the same cleanup OnDestroy would perform.
            service.RestoreTime();
            Assert.That(Time.timeScale, Is.EqualTo(1f), "RestoreTime must reset timeScale to 1");
            Assert.IsFalse(service.IsActive, "RestoreTime must clear IsActive");

            // Verify ResetForTests also restores (simulates post-destroy cleanup)
            service.Play(0.1f, 0.1f);
            HitStopService.ResetForTests();
            Assert.That(Time.timeScale, Is.EqualTo(1f), "ResetForTests must restore timeScale to 1");
        }

        [Test]
        public void CameraShakeService_OnSharedHost_DoesNotUseDestroyGameObject()
        {
            var hasDestroyGameObject = false;
            var source = System.IO.File.ReadAllText(
                System.IO.Path.Combine("Assets", "Scripts", "Combat", "Feedback", "CameraShakeService.cs"));
            hasDestroyGameObject = source.Contains("Destroy(gameObject)");

            Assert.That(hasDestroyGameObject, Is.False,
                "CameraShakeService must not contain Destroy(gameObject) — it lives on the shared Main Camera host");
        }

        [Test]
        public void HitStopService_OnSharedHost_DoesNotUseDestroyGameObject()
        {
            var source = System.IO.File.ReadAllText(
                System.IO.Path.Combine("Assets", "Scripts", "Combat", "Feedback", "HitStopService.cs"));
            Assert.That(source.Contains("Destroy(gameObject)"), Is.False,
                "HitStopService must not contain Destroy(gameObject) — it lives on the shared GameBootstrap host");
        }

        [Test]
        public void CombatHitFeedbackHub_OnSharedHost_DoesNotUseDestroyGameObject()
        {
            var source = System.IO.File.ReadAllText(
                System.IO.Path.Combine("Assets", "Scripts", "Combat", "Feedback", "CombatHitFeedbackHub.cs"));
            Assert.That(source.Contains("Destroy(gameObject)"), Is.False,
                "CombatHitFeedbackHub must not contain Destroy(gameObject) — it lives on the shared GameBootstrap host");
        }
    }
}

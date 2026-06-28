using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateRuntimeInitializationTests
    {
        [Test]
        public void Initialize_WithContext_AssignsRuntimeServices()
        {
            var go = new GameObject("CityGateDisputeRuntimeTest");
            try
            {
                var runtime = go.AddComponent<CityGateDisputeRuntime>();
                var context = new LuoLuoTripGameContext();

                runtime.Initialize(context);

                Assert.That(runtime.IsInitialized, Is.True);
                Assert.That(runtime.Encounter, Is.Not.Null);
                Assert.That(runtime.AreaRuntime, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Initialize_IsIdempotent()
        {
            var go = new GameObject("CityGateDisputeRuntimeIdempotentTest");
            try
            {
                var runtime = go.AddComponent<CityGateDisputeRuntime>();
                var context = new LuoLuoTripGameContext();

                runtime.Initialize(context);
                var encounter = runtime.Encounter;
                var area = runtime.AreaRuntime;
                runtime.Initialize(context);

                Assert.That(runtime.IsInitialized, Is.True);
                Assert.That(runtime.Encounter, Is.SameAs(encounter));
                Assert.That(runtime.AreaRuntime, Is.SameAs(area));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Initialize_MissingContext_WarnsOnce_AndDoesNotThrow()
        {
            var go = new GameObject("CityGateDisputeRuntimeMissingContextTest");
            try
            {
                var runtime = go.AddComponent<CityGateDisputeRuntime>();

                LogAssert.Expect(LogType.Warning, "[CityGateDispute] Initialize skipped: missing LuoLuoTripGameContext");
                Assert.DoesNotThrow(() => runtime.Initialize(null));
                Assert.DoesNotThrow(() => runtime.Initialize(null));

                Assert.That(runtime.IsInitialized, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

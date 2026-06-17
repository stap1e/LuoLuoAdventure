using LuoLuoTrip;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CharacterRuntimeComponentGuardTests
    {
        [TearDown]
        public void TearDown()
        {
            CharacterRuntimeComponentGuard.ResetWarnings();
        }

        [Test]
        public void EnsureForAI_AddsHealthBar_WhenMissing()
        {
            var go = new GameObject("AI");
            try
            {
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsTrue(result.HealthBarAdded);
                Assert.IsNotNull(go.GetComponent<CombatantHealthBarPresenter>());
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EnsureForAI_AddsHitFlash_WhenMissing()
        {
            var go = new GameObject("AI");
            try
            {
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsTrue(result.HitFlashAdded);
                Assert.IsNotNull(go.GetComponent<HitFlashFeedback>());
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EnsureForAI_DoesNotDuplicate_WhenPresent()
        {
            var go = new GameObject("AI");
            try
            {
                go.AddComponent<CombatantHealthBarPresenter>();
                go.AddComponent<HitFlashFeedback>();
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsFalse(result.HealthBarAdded);
                Assert.IsFalse(result.HitFlashAdded);
                Assert.AreEqual(1, go.GetComponents<CombatantHealthBarPresenter>().Length);
                Assert.AreEqual(1, go.GetComponents<HitFlashFeedback>().Length);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EnsureForAI_AddsMotor_WhenMissing()
        {
            var go = new GameObject("AI");
            try
            {
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsTrue(result.MotorAdded);
                Assert.IsNotNull(go.GetComponent<CharacterMovementMotor>());
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EnsureForAI_AddsNavBridge_WhenMissing()
        {
            var go = new GameObject("AI");
            try
            {
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsTrue(result.NavBridgeAdded);
                Assert.IsNotNull(go.GetComponent<LuoLuoTrip.AI.NavigationAgentBridge>());
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ResetWarnings_ClearsWarnedKeys()
        {
            var go = new GameObject("AI");
            try
            {
                CharacterRuntimeComponentGuard.EnsureForAI(go);
                CharacterRuntimeComponentGuard.ResetWarnings();
                // Should not throw or log duplicate
                CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.Pass();
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

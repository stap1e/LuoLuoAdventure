using LuoLuoTrip.AI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CharacterRuntimeComponentGuardTests
    {
        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            CharacterRuntimeComponentGuard.ResetWarnings();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void Ensure_AddsMissingMotor()
        {
            _go = new GameObject("Char");
            var result = CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.IsTrue(result.MotorAdded);
            Assert.IsNotNull(_go.GetComponent<CharacterMovementMotor>());
        }

        [Test]
        public void Ensure_AddsMissingRigidbody_AsKinematicNoGravity()
        {
            _go = new GameObject("Char");
            var result = CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.IsTrue(result.RigidbodyAdded);
            var rb = _go.GetComponent<Rigidbody>();
            Assert.IsNotNull(rb);
            Assert.IsTrue(rb.isKinematic);
            Assert.IsFalse(rb.useGravity);
            Assert.AreEqual(RigidbodyConstraints.FreezeRotation, rb.constraints);
        }

        [Test]
        public void Ensure_AddsFallbackCollider_WhenMissing()
        {
            _go = new GameObject("Char");
            var result = CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.IsTrue(result.ColliderAdded);
            Assert.IsNotNull(_go.GetComponentInChildren<Collider>());
        }

        [Test]
        public void Ensure_DoesNotDuplicate_OnSecondCall()
        {
            _go = new GameObject("Char");
            CharacterRuntimeComponentGuard.Ensure(_go);
            CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.AreEqual(1, _go.GetComponents<CharacterMovementMotor>().Length);
            Assert.AreEqual(1, _go.GetComponents<Rigidbody>().Length);
        }

        [Test]
        public void Ensure_DisablesAnimatorRootMotion()
        {
            _go = new GameObject("Char");
            var anim = _go.AddComponent<Animator>();
            anim.applyRootMotion = true;
            var result = CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.IsTrue(result.AnimatorRootMotionDisabled);
            Assert.IsFalse(anim.applyRootMotion);
        }

        [Test]
        public void EnsureForAI_AddsNavigationAgentBridge()
        {
            _go = new GameObject("AIChar");
            CharacterRuntimeComponentGuard.EnsureForAI(_go);
            Assert.IsNotNull(_go.GetComponent<NavigationAgentBridge>());
        }

        [Test]
        public void Ensure_PreservesExistingRigidbodySettings_WhenNotConflicting()
        {
            _go = new GameObject("Char");
            var rb = _go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            var result = CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.IsFalse(result.RigidbodyAdded, "Must not replace existing valid Rigidbody");
            Assert.IsTrue(rb.isKinematic);
        }

        [Test]
        public void VisualMissing_FlaggedInResult()
        {
            _go = new GameObject("Char");
            var result = CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.IsTrue(result.VisualMissing, "No 'Visual' child should be flagged");
        }

        [Test]
        public void VisualPresent_NotFlagged()
        {
            _go = new GameObject("Char");
            var v = new GameObject("Visual");
            v.transform.SetParent(_go.transform, false);
            var result = CharacterRuntimeComponentGuard.Ensure(_go);
            Assert.IsFalse(result.VisualMissing);
        }
    }
}

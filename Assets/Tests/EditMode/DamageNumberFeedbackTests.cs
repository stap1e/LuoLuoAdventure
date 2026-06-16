using LuoLuoTrip.Feedback;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DamageNumberFeedbackTests
    {
        [TearDown]
        public void TearDown()
        {
            if (DamageNumberFeedback.Instance != null)
                Object.DestroyImmediate(DamageNumberFeedback.Instance.gameObject);
        }

        [Test]
        public void Push_AddsEntry()
        {
            var f = DamageNumberFeedback.EnsureInstance();
            Assert.AreEqual(0, f.ActiveCount);
            DamageNumberFeedback.Push(Vector3.zero, 25f);
            Assert.AreEqual(1, f.ActiveCount);
        }

        [Test]
        public void Push_VariousKinds_AllAccepted()
        {
            var f = DamageNumberFeedback.EnsureInstance();
            DamageNumberFeedback.Push(Vector3.zero, 10f, DamageNumberFeedback.DamageKind.Damage);
            DamageNumberFeedback.Push(Vector3.zero, 0f, DamageNumberFeedback.DamageKind.Stagger);
            DamageNumberFeedback.Push(Vector3.zero, 0f, DamageNumberFeedback.DamageKind.Dead);
            DamageNumberFeedback.Push(Vector3.zero, 0f, DamageNumberFeedback.DamageKind.Miss);
            Assert.AreEqual(4, f.ActiveCount);
        }

        [Test]
        public void Clear_RemovesAll()
        {
            var f = DamageNumberFeedback.EnsureInstance();
            DamageNumberFeedback.Push(Vector3.zero, 10f);
            DamageNumberFeedback.Push(Vector3.zero, 20f);
            f.Clear();
            Assert.AreEqual(0, f.ActiveCount);
        }
    }
}

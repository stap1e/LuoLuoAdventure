using System.Collections;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Animation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AttackAnimationFeedbackTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [UnityTest]
        public IEnumerator AttackStateTriggersProceduralLunge()
        {
            var anim = CreateAnimator(out var visual);
            var rootBefore = _go.transform.position;
            anim.SetCombatState(CombatState.AttackWindup);
            yield return null;
            Assert.That(anim.VisualLocalOffset.magnitude, Is.GreaterThan(0f));
            Assert.That(_go.transform.position, Is.EqualTo(rootBefore));

            anim.SetCombatState(CombatState.Attacking);
            for (int i = 0; i < 6; i++) yield return null;
            Assert.That(visual.localPosition.z, Is.GreaterThan(0f));
            Assert.That(_go.transform.position, Is.EqualTo(rootBefore));
        }

        [UnityTest]
        public IEnumerator VisualLocalPositionRestoredAfterRecovery()
        {
            var anim = CreateAnimator(out var visual);
            var basePos = visual.localPosition;
            anim.SetCombatState(CombatState.Attacking);
            for (int i = 0; i < 20; i++) yield return null;
            anim.SetCombatState(CombatState.AttackRecovery);
            yield return null;
            Assert.That(Vector3.Distance(visual.localPosition, basePos), Is.LessThan(0.001f));
        }

        [Test]
        public void RootPositionNotChangedByProceduralAttackSetup()
        {
            var anim = CreateAnimator(out _);
            var rootBefore = _go.transform.position;
            anim.PlayLightAttack();
            Assert.That(_go.transform.position, Is.EqualTo(rootBefore));
            Assert.That(anim.IsOperatingOnVisualOnly, Is.True);
        }

        private ProceduralCombatAnimator CreateAnimator(out Transform visual)
        {
            _go = new GameObject("AnimRoot");
            visual = new GameObject("Visual").transform;
            visual.SetParent(_go.transform, false);
            var rendererGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rendererGo.transform.SetParent(visual, false);
            Object.DestroyImmediate(rendererGo.GetComponent<Collider>());
            return _go.AddComponent<ProceduralCombatAnimator>();
        }
    }
}

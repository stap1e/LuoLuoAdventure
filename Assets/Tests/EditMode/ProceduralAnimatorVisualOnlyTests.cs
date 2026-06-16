using LuoLuoTrip.Combat.Animation;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class ProceduralAnimatorVisualOnlyTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void Animator_WithVisualChild_OperatesOnVisual_NotRoot()
        {
            _go = new GameObject("CharRoot");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(_go.transform, false);
            var anim = _go.AddComponent<ProceduralCombatAnimator>();
            // Trigger Awake by enabling
            anim.enabled = true;

            Assert.IsNotNull(anim.VisualRoot, "Animator must have resolved a Visual transform");
            Assert.AreNotSame(_go.transform, anim.VisualRoot, "Animator must not target root transform");
            Assert.AreSame(visual.transform, anim.VisualRoot, "Animator must target the 'Visual' child");
            Assert.IsTrue(anim.IsOperatingOnVisualOnly);
            Assert.IsFalse(anim.IsDisabled);
        }

        [Test]
        public void Animator_WithoutVisualChild_DisablesItself_StrictMode()
        {
            _go = new GameObject("CharRootNoVisual");
            var anim = _go.AddComponent<ProceduralCombatAnimator>();

            Assert.IsTrue(anim.IsDisabled, "Strict mode must disable animator when no 'Visual' child exists");
            Assert.IsFalse(anim.enabled, "Disabled animator must have enabled=false");
        }

        [Test]
        public void Animator_PlayMethods_DoNotMoveRoot_WhenDisabled()
        {
            _go = new GameObject("CharRootNoVisual2");
            _go.transform.position = new Vector3(0f, 0.5f, 0f);
            var anim = _go.AddComponent<ProceduralCombatAnimator>();

            // Even calling Play methods should be no-op when disabled.
            anim.PlayLightAttack();
            anim.PlayDodge();
            anim.PlayHitReact(false);
            anim.PlayStagger();
            anim.PlayDeath();
            anim.PlayMove(0.5f);

            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4,
                "Disabled animator must NOT touch root transform");
            Assert.AreEqual(0f, _go.transform.position.x, 1e-4);
            Assert.AreEqual(0f, _go.transform.position.z, 1e-4);
        }

        [Test]
        public void Animator_VisualPlayMove_DoesNotAlterRootY()
        {
            _go = new GameObject("CharRoot2");
            _go.transform.position = new Vector3(0f, 0.5f, 0f);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(_go.transform, false);
            var anim = _go.AddComponent<ProceduralCombatAnimator>();
            anim.SetCombatState(LuoLuoTrip.Combat.CombatState.Idle);

            anim.PlayMove(0.8f);

            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4,
                "PlayMove must only adjust Visual.localPosition, never root world Y");
        }
    }
}

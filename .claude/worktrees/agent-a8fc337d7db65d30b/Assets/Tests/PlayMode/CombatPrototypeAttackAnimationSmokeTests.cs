using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Animation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CombatPrototypeAttackAnimationSmokeTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [UnityTest]
        public IEnumerator AttackAnimationVisibleViaVisualOffset()
        {
            var ctrl = CreateAnimatedPlayer(out var visual);
            yield return null;
            var basePos = visual.localPosition;
            ctrl.AttemptAttack();
            for (int i = 0; i < 12; i++) yield return null;
            Assert.That(Vector3.Distance(visual.localPosition, basePos), Is.GreaterThan(0.02f));
            Assert.That(_go.transform.position, Is.EqualTo(Vector3.zero));
        }

        [UnityTest]
        public IEnumerator AttackActiveStateAppearsAfterWindup()
        {
            var ctrl = CreateAnimatedPlayer(out _);
            var combatant = ctrl.GetComponent<Combatant>();
            combatant.AutoTickEnabled = false;
            ctrl.AttemptAttack();
            Assert.That(combatant.State, Is.EqualTo(CombatState.AttackWindup));
            combatant.Tick(combatant.AttackWindup + 0.001f);
            yield return null;
            Assert.That(combatant.State, Is.EqualTo(CombatState.Attacking));
            Assert.That(combatant.IsAttackActiveWindow, Is.True);
        }

        private CombatController CreateAnimatedPlayer(out Transform visual)
        {
            _go = new GameObject("Player");
            visual = new GameObject("Visual").transform;
            visual.SetParent(_go.transform, false);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(visual, false);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            var entity = _go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common));
            if (_go.GetComponent<ProceduralCombatAnimator>() == null)
                _go.AddComponent<ProceduralCombatAnimator>();
            var ctrl = _go.GetComponent<CombatController>();
            if (ctrl == null) ctrl = _go.AddComponent<CombatController>();
            return ctrl;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class InputOwnershipRegressionTests
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
        }

        private GameObject CreateUnitGo(string name, bool isPlayer)
        {
            var go = new GameObject(name);
            _cleanup.Add(go);
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create($"{name}_id", name,
                isPlayer ? SubFactionId.MotorIronRiders : SubFactionId.BeastIronClaw,
                isPlayer ? CharacterRole.Common : CharacterRole.Minion));
            go.AddComponent<Combatant>();

            if (isPlayer)
            {
                go.AddComponent<CombatController>();
            }
            else
            {
                go.AddComponent<SimpleCombatAI>();
                var ctrl = go.AddComponent<CombatController>();
                ctrl.SetInputEnabled(false);
            }

            return go;
        }

        [UnityTest]
        public IEnumerator CombatPrototype_PlayerDefaultInputEnabled()
        {
            var playerGo = CreateUnitGo("Player", true);
            var ctrl = playerGo.GetComponent<CombatController>();
            yield return null;
            Assert.That(ctrl.IsInputEnabled, Is.True);
        }

        [UnityTest]
        public IEnumerator CommanderPrototype_DirectControlSwitchesInput()
        {
            var playerGo = CreateUnitGo("Commander", true);
            var targetGo = CreateUnitGo("Target", false);
            var playerCtrl = playerGo.GetComponent<CombatController>();
            var targetCtrl = targetGo.GetComponent<CombatController>();
            var targetAI = targetGo.GetComponent<SimpleCombatAI>();

            yield return null;

            playerCtrl.SetInputEnabled(false);
            targetCtrl.SetInputEnabled(true);
            targetAI.enabled = false;

            Assert.That(playerCtrl.IsInputEnabled, Is.False);
            Assert.That(targetCtrl.IsInputEnabled, Is.True);
        }

        [UnityTest]
        public IEnumerator CommanderPrototype_ReleaseRestoresInput()
        {
            var playerGo = CreateUnitGo("Commander", true);
            var targetGo = CreateUnitGo("Target", false);
            var playerCtrl = playerGo.GetComponent<CombatController>();
            var targetCtrl = targetGo.GetComponent<CombatController>();

            yield return null;

            playerCtrl.SetInputEnabled(false);
            targetCtrl.SetInputEnabled(true);

            targetCtrl.SetInputEnabled(false);
            playerCtrl.SetInputEnabled(true);

            Assert.That(playerCtrl.IsInputEnabled, Is.True);
            Assert.That(targetCtrl.IsInputEnabled, Is.False);
        }

        [UnityTest]
        public IEnumerator InputDisabled_CombatantStillTicks()
        {
            var playerGo = CreateUnitGo("Player", true);
            var ctrl = playerGo.GetComponent<CombatController>();
            var combatant = playerGo.GetComponent<Combatant>();

            yield return null;

            ctrl.SetInputEnabled(false);
            Assert.That(combatant.IsAlive, Is.True);
            Assert.That(combatant.State, Is.EqualTo(CombatState.Idle));
        }

        [UnityTest]
        public IEnumerator DirectControl_PositionChangesOnEnabledInput()
        {
            var targetGo = CreateUnitGo("Target", false);
            var targetCtrl = targetGo.GetComponent<CombatController>();
            targetCtrl.SetInputEnabled(true);

            yield return null;

            var posBefore = targetGo.transform.position;
            targetCtrl.ApplyMoveInput(new Vector2(0f, 1f));
            var posAfter = targetGo.transform.position;

            Assert.That(Vector3.Distance(posBefore, posAfter), Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator DirectControl_PositionNoChangeOnDisabledInput()
        {
            var playerGo = CreateUnitGo("OldUnit", true);
            var playerCtrl = playerGo.GetComponent<CombatController>();
            playerCtrl.SetInputEnabled(false);

            yield return null;

            var posBefore = playerGo.transform.position;
            playerCtrl.ApplyMoveInput(new Vector2(1f, 1f));
            var posAfter = playerGo.transform.position;

            Assert.That(posAfter, Is.EqualTo(posBefore));
        }
    }
}

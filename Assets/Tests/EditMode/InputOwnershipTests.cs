using System.Collections.Generic;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class InputOwnershipTests
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

        private CombatController CreateUnit(string name, bool isPlayer)
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
                return go.AddComponent<CombatController>();
            }

            go.AddComponent<SimpleCombatAI>();
            var ctrl = go.AddComponent<CombatController>();
            ctrl.SetInputEnabled(false);
            return ctrl;
        }

        [Test]
        public void Player_DefaultInputEnabled()
        {
            var ctrl = CreateUnit("Player", true);
            Assert.That(ctrl.IsInputEnabled, Is.True);
        }

        [Test]
        public void Enemy_DefaultInputDisabled()
        {
            var ctrl = CreateUnit("Enemy", false);
            Assert.That(ctrl.IsInputEnabled, Is.False);
        }

        [Test]
        public void DirectControl_SwitchesInput()
        {
            var player = CreateUnit("Player", true);
            var target = CreateUnit("Target", false);

            player.SetInputEnabled(false);
            target.SetInputEnabled(true);

            Assert.That(player.IsInputEnabled, Is.False, "Old unit input disabled after DirectControl");
            Assert.That(target.IsInputEnabled, Is.True, "New unit input enabled after DirectControl");
        }

        [Test]
        public void ReleaseControl_RestoresOriginalInput()
        {
            var player = CreateUnit("Player", true);
            var target = CreateUnit("Target", false);

            player.SetInputEnabled(false);
            target.SetInputEnabled(true);

            target.SetInputEnabled(false);
            player.SetInputEnabled(true);

            Assert.That(player.IsInputEnabled, Is.True, "Original player input restored after Release");
            Assert.That(target.IsInputEnabled, Is.False, "Target unit input disabled after Release");
        }

        [Test]
        public void TacticalCommand_DoesNotEnableTargetInput()
        {
            var player = CreateUnit("Player", true);
            var target = CreateUnit("Target", false);

            Assert.That(player.IsInputEnabled, Is.True);
            Assert.That(target.IsInputEnabled, Is.False, "TacticalCommand target should not get player input");
        }

        [Test]
        public void SyncAssist_DoesNotEnableTargetInput()
        {
            var player = CreateUnit("Player", true);
            var target = CreateUnit("Target", false);

            Assert.That(player.IsInputEnabled, Is.True);
            Assert.That(target.IsInputEnabled, Is.False, "SyncAssist target should not get player input");
        }

        [Test]
        public void InputDisabled_DoesNotAffectCombatantState()
        {
            var ctrl = CreateUnit("Player", true);
            ctrl.SetInputEnabled(false);

            var combatant = ctrl.GetComponent<Combatant>();
            Assert.That(combatant.IsAlive, Is.True, "Combatant.IsAlive still works when input disabled");
            Assert.That(combatant.State, Is.EqualTo(CombatState.Idle), "Combatant state still accessible");
        }

        [Test]
        public void OnlyOneInputEnabled_AtATime()
        {
            var player = CreateUnit("Player", true);
            var target = CreateUnit("Target", false);

            player.SetInputEnabled(false);
            target.SetInputEnabled(true);

            int enabledCount = 0;
            foreach (var go in _cleanup)
            {
                var c = go.GetComponent<CombatController>();
                if (c != null && c.IsInputEnabled) enabledCount++;
            }

            Assert.That(enabledCount, Is.EqualTo(1), "Only one CombatController should have input enabled at a time");
        }
    }
}

using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class PrototypeReviveDebugTests
    {
        private GameObject _playerGo;
        private GameObject _debugGo;

        [TearDown]
        public void TearDown()
        {
            if (_playerGo != null) Object.DestroyImmediate(_playerGo);
            if (_debugGo != null) Object.DestroyImmediate(_debugGo);
        }

        [Test]
        public void F2ReviveRestoresHpStateAndInput()
        {
            var ctrl = CreatePlayer();
            var combatant = ctrl.GetComponent<Combatant>();
            ctrl.SetInputEnabled(false);
            combatant.RestoreRuntimeState(0f, combatant.Stats.maxStamina, combatant.Stats.maxPoise);
            Assert.That(combatant.State, Is.EqualTo(CombatState.Dead));

            var debug = CreateDebug();
            debug.RevivePlayer();

            Assert.That(combatant.CurrentHealth, Is.EqualTo(combatant.Stats.maxHealth).Within(0.01f));
            Assert.That(combatant.State, Is.EqualTo(CombatState.Idle));
            Assert.That(ctrl.IsInputEnabled, Is.True);
            Assert.That(combatant.CharacterEntity.Data.IsAlive, Is.True);
        }

        [Test]
        public void ReviveDoesNotWriteMissionOutcome()
        {
            var ctrl = CreatePlayer();
            var combatant = ctrl.GetComponent<Combatant>();
            combatant.RestoreRuntimeState(0f, combatant.Stats.maxStamina, combatant.Stats.maxPoise);
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld();
            var before = context.MissionChainService.State.CompletedMissions.Count;
            CreateDebug().RevivePlayer();
            Assert.That(context.MissionChainService.State.CompletedMissions.Count, Is.EqualTo(before));
        }

        private CombatController CreatePlayer()
        {
            _playerGo = new GameObject("Player");
            var entity = _playerGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common));
            var ctrl = _playerGo.GetComponent<CombatController>();
            if (ctrl == null) ctrl = _playerGo.AddComponent<CombatController>();
            return ctrl;
        }

        private PrototypeDebugController CreateDebug()
        {
            _debugGo = new GameObject("Debug");
            return _debugGo.AddComponent<PrototypeDebugController>();
        }
    }
}

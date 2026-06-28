using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeAttackUsabilitySmokeTests
    {
        private GameObject _playerGo;
        private GameObject _enemyGo;
        private GameObject _debugGo;

        [SetUp]
        public void SetUp()
        {
            CharacterEntity.HostilityResolver = (a, b) => a != b;
        }

        [TearDown]
        public void TearDown()
        {
            CharacterEntity.HostilityResolver = null;
            if (_playerGo != null) Object.DestroyImmediate(_playerGo);
            if (_enemyGo != null) Object.DestroyImmediate(_enemyGo);
            if (_debugGo != null) Object.DestroyImmediate(_debugGo);
        }

        [UnityTest]
        public IEnumerator FreshPlayerAliveAndLeftClickPathStartsAttack()
        {
            var ctrl = CreatePlayer();
            CreateEnemy(new Vector3(0f, 0f, 1.5f));
            yield return null;
            Assert.That(ctrl.GetComponent<Combatant>().IsAlive, Is.True);
            Assert.That(ctrl.AttemptAttack(), Is.True);
            Assert.That(ctrl.GetComponent<Combatant>().State, Is.EqualTo(CombatState.AttackWindup));
            Assert.That(ctrl.LastAttackResult, Is.EqualTo("STARTED"));
        }

        [UnityTest]
        public IEnumerator DeadPlayerShowsBlockedReasonAndF2ReviveRestoresAttack()
        {
            var ctrl = CreatePlayer();
            var enemy = CreateEnemy(new Vector3(0f, 0f, 1.5f));
            var player = ctrl.GetComponent<Combatant>();
            _debugGo = new GameObject("Debug");
            var debug = _debugGo.AddComponent<PrototypeDebugController>();
            yield return null;

            player.RestoreRuntimeState(0f, player.Stats.maxStamina, player.Stats.maxPoise);
            Assert.That(ctrl.AttemptAttack(enemy), Is.False);
            Assert.That(ctrl.LastAttackRejectReason, Is.EqualTo("PlayerDead"));

            debug.RevivePlayer();
            yield return null;

            Assert.That(player.IsAlive, Is.True);
            Assert.That(ctrl.AttemptAttack(enemy), Is.True);
            Assert.That(player.State, Is.EqualTo(CombatState.AttackWindup));
        }

        [UnityTest]
        public IEnumerator DirectControlUnitCanAttackHostile()
        {
            var ctrl = CreatePlayer();
            var enemy = CreateEnemy(new Vector3(0f, 0f, 1.5f));
            ctrl.IsExternallyControlled = true;
            yield return null;
            Assert.That(ctrl.AttemptAttack(enemy), Is.True);
            Assert.That(ctrl.GetComponent<Combatant>().State, Is.EqualTo(CombatState.AttackWindup));
        }

        private CombatController CreatePlayer()
        {
            _playerGo = new GameObject("Player_Commander");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(_playerGo.transform, false);
            var entity = _playerGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common));
            var ctrl = _playerGo.GetComponent<CombatController>();
            if (ctrl == null) ctrl = _playerGo.AddComponent<CombatController>();
            return ctrl;
        }

        private Combatant CreateEnemy(Vector3 position)
        {
            _enemyGo = new GameObject("Beast_Minion");
            _enemyGo.transform.position = position;
            var entity = _enemyGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("enemy", "Enemy", SubFactionId.BeastIronClaw, CharacterRole.Minion));
            return entity.Combatant;
        }
    }
}

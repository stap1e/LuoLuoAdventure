using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AttackInputDiagnosticsTests
    {
        private GameObject _playerGo;
        private GameObject _targetGo;

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
            if (_targetGo != null) Object.DestroyImmediate(_targetGo);
        }

        [Test]
        public void DeadPlayerAttackRejectedWithPlayerDead()
        {
            var ctrl = CreateController();
            var combatant = ctrl.GetComponent<Combatant>();
            combatant.RestoreRuntimeState(0f, combatant.Stats.maxStamina, combatant.Stats.maxPoise);
            var started = ctrl.AttemptAttack();
            Assert.That(started, Is.False);
            Assert.That(ctrl.LastAttackResult, Is.EqualTo("BLOCKED"));
            Assert.That(ctrl.LastAttackRejectReason, Is.EqualTo("PlayerDead"));
        }

        [Test]
        public void InputDisabledAttackRejectedWithInputDisabled()
        {
            var ctrl = CreateController();
            ctrl.SetInputEnabled(false);
            var started = ctrl.AttemptAttack();
            Assert.That(started, Is.False);
            Assert.That(ctrl.LastAttackRejectReason, Is.EqualTo("InputDisabled"));
        }

        [Test]
        public void NoTargetStartsWhiffWithNoTargetInRange()
        {
            var ctrl = CreateController();
            var started = ctrl.AttemptAttack();
            Assert.That(started, Is.True);
            Assert.That(ctrl.LastAttackResult, Is.EqualTo("MISS"));
            Assert.That(ctrl.LastAttackRejectReason, Is.EqualTo("NoTargetInRange"));
            Assert.That(ctrl.GetComponent<Combatant>().State, Is.EqualTo(CombatState.AttackWindup));
        }

        [Test]
        public void ValidTargetStartsAttackWindup()
        {
            var ctrl = CreateController();
            var target = CreateTarget(new Vector3(1.5f, 0f, 0f));
            var started = ctrl.AttemptAttack(target);
            Assert.That(started, Is.True);
            Assert.That(ctrl.LastAttackResult, Is.EqualTo("STARTED"));
            Assert.That(ctrl.LastAttackRejectReason, Is.EqualTo("None"));
            Assert.That(ctrl.LastAttackTargetName, Is.EqualTo(target.name));
            Assert.That(ctrl.GetComponent<Combatant>().State, Is.EqualTo(CombatState.AttackWindup));
        }

        [Test]
        public void OutOfRangeTargetStartsWhiffAndReportsOutOfRange()
        {
            var ctrl = CreateController();
            var target = CreateTarget(new Vector3(8f, 0f, 0f));
            var started = ctrl.AttemptAttack(target);
            Assert.That(started, Is.True);
            Assert.That(ctrl.LastAttackResult, Is.EqualTo("MISS"));
            Assert.That(ctrl.LastAttackRejectReason, Is.EqualTo("OutOfRange"));
        }

        [Test]
        public void AttackCooldownBlocksSecondAttack()
        {
            var ctrl = CreateController();
            var target = CreateTarget(new Vector3(1.5f, 0f, 0f));
            Assert.That(ctrl.AttemptAttack(target), Is.True);
            Assert.That(ctrl.AttemptAttack(target), Is.False);
            Assert.That(ctrl.LastAttackRejectReason, Does.Contain("StateBlocked"));
        }

        private CombatController CreateController()
        {
            _playerGo = new GameObject("Player");
            var entity = _playerGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common));
            var ctrl = _playerGo.GetComponent<CombatController>();
            if (ctrl == null) ctrl = _playerGo.AddComponent<CombatController>();
            return ctrl;
        }

        private Combatant CreateTarget(Vector3 position)
        {
            _targetGo = new GameObject("Target");
            _targetGo.transform.position = position;
            var entity = _targetGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("target", "Target", SubFactionId.BeastIronClaw, CharacterRole.Minion));
            return entity.Combatant;
        }
    }
}

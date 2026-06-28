using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AttackTargetAcquisitionTests
    {
        private GameObject _playerGo;
        private GameObject _a;
        private GameObject _b;

        [SetUp]
        public void SetUp()
        {
            CharacterEntity.HostilityResolver = (x, y) => x != y;
        }

        [TearDown]
        public void TearDown()
        {
            CharacterEntity.HostilityResolver = null;
            if (_playerGo != null) Object.DestroyImmediate(_playerGo);
            if (_a != null) Object.DestroyImmediate(_a);
            if (_b != null) Object.DestroyImmediate(_b);
        }

        [Test]
        public void LockedTargetPreferredOverNearerTarget()
        {
            var ctrl = CreateController();
            var near = CreateTarget(ref _a, "Near", SubFactionId.BeastIronClaw, new Vector3(0f, 0f, 1.2f));
            var locked = CreateTarget(ref _b, "Locked", SubFactionId.BeastIronClaw, new Vector3(0f, 0f, 2f));
            ctrl.SetLockTargetForTests(locked);
            ctrl.AttemptAttack();
            Assert.That(ctrl.LastAttackTargetName, Is.EqualTo(locked.name));
            Assert.That(ctrl.LastAttackTargetName, Is.Not.EqualTo(near.name));
        }

        [Test]
        public void NoTargetAutoAcquiresNearestHostileInFront()
        {
            var ctrl = CreateController();
            var far = CreateTarget(ref _a, "Far", SubFactionId.BeastIronClaw, new Vector3(0f, 0f, 3f));
            var near = CreateTarget(ref _b, "Near", SubFactionId.BeastIronClaw, new Vector3(0f, 0f, 1.5f));
            ctrl.AttemptAttack();
            Assert.That(ctrl.LastAttackTargetName, Is.EqualTo(near.name));
            Assert.That(ctrl.LastAttackTargetName, Is.Not.EqualTo(far.name));
        }

        [Test]
        public void DeadTargetIgnoredDuringAutoAcquire()
        {
            var ctrl = CreateController();
            var dead = CreateTarget(ref _a, "Dead", SubFactionId.BeastIronClaw, new Vector3(0f, 0f, 1f));
            dead.RestoreRuntimeState(0f, dead.Stats.maxStamina, dead.Stats.maxPoise);
            var live = CreateTarget(ref _b, "Live", SubFactionId.BeastIronClaw, new Vector3(0f, 0f, 1.8f));
            ctrl.AttemptAttack();
            Assert.That(ctrl.LastAttackTargetName, Is.EqualTo(live.name));
        }

        [Test]
        public void FriendlyTargetIgnoredDuringAutoAcquire()
        {
            var ctrl = CreateController();
            CreateTarget(ref _a, "Friendly", SubFactionId.MotorIronRiders, new Vector3(0f, 0f, 1f));
            var hostile = CreateTarget(ref _b, "Hostile", SubFactionId.BeastIronClaw, new Vector3(0f, 0f, 1.8f));
            ctrl.AttemptAttack();
            Assert.That(ctrl.LastAttackTargetName, Is.EqualTo(hostile.name));
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

        private static Combatant CreateTarget(ref GameObject go, string name, SubFactionId faction, Vector3 position)
        {
            go = new GameObject(name);
            go.transform.position = position;
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create(name, name, faction, CharacterRole.Minion));
            return entity.Combatant;
        }
    }
}

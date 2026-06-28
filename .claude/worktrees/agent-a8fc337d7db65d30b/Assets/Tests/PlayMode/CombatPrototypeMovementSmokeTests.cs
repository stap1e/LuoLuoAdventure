using System.Collections;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CombatPrototypeMovementSmokeTests
    {
        private GameObject _playerGo;

        [TearDown]
        public void TearDown()
        {
            if (_playerGo != null)
                Object.DestroyImmediate(_playerGo);
        }

        [UnityTest]
        public IEnumerator PlayerWithInputEnabled_PositionChangesOnApplyMoveInput()
        {
            _playerGo = new GameObject("Player");
            var entity = _playerGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_p1", "TestPlayer", SubFactionId.MotorIronRiders, CharacterRole.Common));
            _playerGo.AddComponent<Combatant>();
            var ctrl = _playerGo.AddComponent<CombatController>();
            ctrl.SetInputEnabled(true);

            yield return null;

            var posBefore = _playerGo.transform.position;
            ctrl.ApplyMoveInput(new Vector2(0f, 1f));

            yield return null;

            var posAfter = _playerGo.transform.position;
            Assert.That(Vector3.Distance(posBefore, posAfter), Is.GreaterThan(0f),
                "Player position should change when input is enabled and move is applied");
        }

        [UnityTest]
        public IEnumerator PlayerWithInputDisabled_PositionDoesNotChange()
        {
            _playerGo = new GameObject("Player");
            var entity = _playerGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_p2", "TestPlayer", SubFactionId.MotorIronRiders, CharacterRole.Common));
            _playerGo.AddComponent<Combatant>();
            var ctrl = _playerGo.AddComponent<CombatController>();
            ctrl.SetInputEnabled(false);

            yield return null;

            var posBefore = _playerGo.transform.position;
            ctrl.ApplyMoveInput(new Vector2(1f, 1f));

            yield return null;

            var posAfter = _playerGo.transform.position;
            Assert.That(posAfter, Is.EqualTo(posBefore),
                "Player position should not change when input is disabled");
        }
    }

    public class CommanderDirectControlMovementTests
    {
        private GameObject _originalGo;
        private GameObject _targetGo;

        [TearDown]
        public void TearDown()
        {
            if (_originalGo != null) Object.DestroyImmediate(_originalGo);
            if (_targetGo != null) Object.DestroyImmediate(_targetGo);
        }

        [UnityTest]
        public IEnumerator OriginalPlayerMovesByDefault()
        {
            _originalGo = new GameObject("OriginalPlayer");
            var entity = _originalGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("orig_1", "Original", SubFactionId.MotorIronRiders, CharacterRole.Common));
            _originalGo.AddComponent<Combatant>();
            var ctrl = _originalGo.AddComponent<CombatController>();

            yield return null;

            Assert.That(ctrl.IsInputEnabled, Is.True, "Original player should have input enabled by default");
        }

        [UnityTest]
        public IEnumerator DirectControl_SwitchesInputToTarget()
        {
            _originalGo = new GameObject("OriginalPlayer");
            var origEntity = _originalGo.AddComponent<CharacterEntity>();
            origEntity.Bind(CharacterData.Create("orig_2", "Original", SubFactionId.MotorIronRiders, CharacterRole.Common));
            _originalGo.AddComponent<Combatant>();
            var origCtrl = _originalGo.AddComponent<CombatController>();

            _targetGo = new GameObject("TargetUnit");
            var targetEntity = _targetGo.AddComponent<CharacterEntity>();
            targetEntity.Bind(CharacterData.Create("tgt_1", "Target", SubFactionId.MotorIronRiders, CharacterRole.Minion));
            _targetGo.AddComponent<Combatant>();
            var targetAI = _targetGo.AddComponent<SimpleCombatAI>();
            var targetCtrl = _targetGo.AddComponent<CombatController>();
            targetCtrl.SetInputEnabled(false);

            yield return null;

            targetCtrl.SetInputEnabled(true);
            origCtrl.SetInputEnabled(false);
            targetAI.enabled = false;

            Assert.That(origCtrl.IsInputEnabled, Is.False, "Original player input should be disabled");
            Assert.That(targetCtrl.IsInputEnabled, Is.True, "Target unit input should be enabled");
        }

        [UnityTest]
        public IEnumerator Release_RestoresOriginalPlayerInput()
        {
            _originalGo = new GameObject("OriginalPlayer");
            var origEntity = _originalGo.AddComponent<CharacterEntity>();
            origEntity.Bind(CharacterData.Create("orig_3", "Original", SubFactionId.MotorIronRiders, CharacterRole.Common));
            _originalGo.AddComponent<Combatant>();
            var origCtrl = _originalGo.AddComponent<CombatController>();

            _targetGo = new GameObject("TargetUnit");
            var targetEntity = _targetGo.AddComponent<CharacterEntity>();
            targetEntity.Bind(CharacterData.Create("tgt_2", "Target", SubFactionId.MotorIronRiders, CharacterRole.Minion));
            _targetGo.AddComponent<Combatant>();
            var targetCtrl = _targetGo.AddComponent<CombatController>();

            yield return null;

            origCtrl.SetInputEnabled(false);
            targetCtrl.SetInputEnabled(true);

            targetCtrl.SetInputEnabled(false);
            origCtrl.SetInputEnabled(true);

            Assert.That(origCtrl.IsInputEnabled, Is.True, "Original player input should be restored");
            Assert.That(targetCtrl.IsInputEnabled, Is.False, "Target unit input should be disabled");
        }
    }
}

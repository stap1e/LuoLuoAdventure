using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeRootMovementSmokeTests
    {
        private GameObject _playerGo;
        private GameObject _targetGo;

        [TearDown]
        public void TearDown()
        {
            if (_playerGo != null) Object.DestroyImmediate(_playerGo);
            if (_targetGo != null) Object.DestroyImmediate(_targetGo);
        }

        private GameObject CreatePlayer(Vector3 pos)
        {
            var go = new GameObject("PlayerCmd");
            go.transform.position = pos;
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_player_cmd_rm", "TestCmd", SubFactionId.MotorIronRiders, CharacterRole.Common));
            go.AddComponent<CharacterMovementMotor>();
            if (go.GetComponent<Combatant>() == null) go.AddComponent<Combatant>();
            var ctrl = go.AddComponent<CombatController>();
            ctrl.SetInputEnabled(true);
            return go;
        }

        private GameObject CreateAITarget(Vector3 pos)
        {
            var go = new GameObject("TargetAI");
            go.transform.position = pos;
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_target_rm", "TestAI", SubFactionId.MotorIronRiders, CharacterRole.Minion));
            go.AddComponent<CharacterMovementMotor>();
            if (go.GetComponent<Combatant>() == null) go.AddComponent<Combatant>();
            go.AddComponent<SimpleCombatAI>();
            return go;
        }

        [UnityTest]
        public IEnumerator DirectControl_TargetCanReceiveMovement()
        {
            _targetGo = CreateAITarget(new Vector3(2f, 0.5f, 0f));
            yield return null;
            var targetCtrl = _targetGo.AddComponent<CombatController>();
            targetCtrl.SetInputEnabled(true);
            var startY = _targetGo.transform.position.y;
            targetCtrl.ApplyMoveInput(new Vector2(0f, 1f));
            yield return null;
            Assert.AreEqual(startY, _targetGo.transform.position.y, 1e-3,
                "Direct-controlled target Y must not change");
        }

        [UnityTest]
        public IEnumerator Release_RestoresOriginalPlayerMovement()
        {
            _playerGo = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            yield return null;
            var ctrl = _playerGo.GetComponent<CombatController>();
            ctrl.SetInputEnabled(false);
            ctrl.SetInputEnabled(true);
            var startY = _playerGo.transform.position.y;
            ctrl.ApplyMoveInput(new Vector2(1f, 0f));
            yield return null;
            Assert.AreEqual(startY, _playerGo.transform.position.y, 1e-3,
                "Restored player movement must keep Y stable");
        }

        [UnityTest]
        public IEnumerator TacticalCommandFollow_AIRootMovesViaMotor()
        {
            _playerGo = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            _targetGo = CreateAITarget(new Vector3(8f, 0.5f, 0f));
            yield return null;
            var ai = _targetGo.GetComponent<SimpleCombatAI>();
            ai.FollowTarget = _playerGo.transform;
            var startY = _targetGo.transform.position.y;
            for (int i = 0; i < 30; i++)
                yield return null;
            Assert.AreEqual(startY, _targetGo.transform.position.y, 1e-3,
                "Tactical Follow must not change AI Y");
        }
    }
}

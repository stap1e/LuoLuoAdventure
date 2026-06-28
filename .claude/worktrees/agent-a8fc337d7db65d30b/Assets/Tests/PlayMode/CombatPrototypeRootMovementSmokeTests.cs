using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CombatPrototypeRootMovementSmokeTests
    {
        private GameObject _playerGo;
        private GameObject _enemyGo;

        [TearDown]
        public void TearDown()
        {
            if (_playerGo != null) Object.DestroyImmediate(_playerGo);
            if (_enemyGo != null) Object.DestroyImmediate(_enemyGo);
        }

        private GameObject CreatePlayer(Vector3 pos)
        {
            var go = new GameObject("Player");
            go.transform.position = pos;
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_player_rm", "TestPlayer", SubFactionId.MotorIronRiders, CharacterRole.Common));
            go.AddComponent<CharacterMovementMotor>();
            if (go.GetComponent<Combatant>() == null) go.AddComponent<Combatant>();
            var ctrl = go.AddComponent<CombatController>();
            ctrl.SetInputEnabled(true);
            return go;
        }

        private GameObject CreateEnemy(Vector3 pos)
        {
            var go = new GameObject("Enemy");
            go.transform.position = pos;
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("test_enemy_rm", "TestEnemy", SubFactionId.BeastIronClaw, CharacterRole.Minion));
            go.AddComponent<CharacterMovementMotor>();
            if (go.GetComponent<Combatant>() == null) go.AddComponent<Combatant>();
            return go;
        }

        [UnityTest]
        public IEnumerator ApplyMoveInput_MovesPlayerRoot_KeepsY()
        {
            _playerGo = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            yield return null;
            var ctrl = _playerGo.GetComponent<CombatController>();
            var startY = _playerGo.transform.position.y;
            ctrl.ApplyMoveInput(new Vector2(0f, 1f));
            yield return null;
            Assert.AreEqual(startY, _playerGo.transform.position.y, 1e-3, "WASD must not change Y");
        }

        [UnityTest]
        public IEnumerator Dodge_ChangesPlayerRoot_KeepsY()
        {
            _playerGo = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            yield return null;
            var combatant = _playerGo.GetComponent<Combatant>();
            var startPos = _playerGo.transform.position;
            combatant.TryDodge(Vector3.forward);
            for (int i = 0; i < 20; i++)
                yield return null;
            Assert.AreNotEqual(startPos.x + startPos.z, _playerGo.transform.position.x + _playerGo.transform.position.z,
                "Dodge must change root X/Z");
            Assert.AreEqual(startPos.y, _playerGo.transform.position.y, 1e-3,
                "Dodge must not change root Y");
        }

        [UnityTest]
        public IEnumerator Attack_DoesNotLowerPlayerRootY()
        {
            _playerGo = CreatePlayer(new Vector3(0f, 0.5f, 0f));
            _enemyGo = CreateEnemy(new Vector3(0.6f, 0.5f, 0f));
            yield return null;
            var combatant = _playerGo.GetComponent<Combatant>();
            var enemyCombatant = _enemyGo.GetComponent<Combatant>();
            var startY = _playerGo.transform.position.y;
            combatant.TryLightAttack(enemyCombatant);
            for (int i = 0; i < 60; i++)
                yield return null;
            Assert.AreEqual(startY, _playerGo.transform.position.y, 1e-3,
                "Attack must not lower root Y");
        }

        [UnityTest]
        public IEnumerator AIFallback_MovesEnemyRoot_KeepsY()
        {
            _enemyGo = CreateEnemy(new Vector3(0f, 0.5f, 0f));
            var bridge = _enemyGo.AddComponent<NavigationAgentBridge>();
            yield return null;
            bridge.SetDestination(NavigationMoveRequest.To(new Vector3(5f, 0.5f, 0f), 6f, 0.5f));
            for (int i = 0; i < 30; i++)
            {
                bridge.TickFallback(0.1f);
                yield return null;
            }
            Assert.Greater(_enemyGo.transform.position.x, 1f, "AI fallback must move root X");
            Assert.AreEqual(0.5f, _enemyGo.transform.position.y, 1e-3, "AI fallback must not change Y");
        }
    }
}

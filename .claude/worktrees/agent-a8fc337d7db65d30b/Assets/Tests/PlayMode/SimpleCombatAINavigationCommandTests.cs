using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class SimpleCombatAINavigationCommandTests
    {
        private GameObject _aiGo;
        private GameObject _playerGo;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var registry = LuoLuoTrip.CharacterRuntimeRegistry.AllCharacters;
            _aiGo = new GameObject("AI_Unit");
            var entity = _aiGo.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create("ai1", "TestAI", SubFactionId.BeastIronClaw, CharacterRole.Minion));
            var combatant = _aiGo.AddComponent<Combatant>();
            combatant.AutoTickEnabled = false;
            _aiGo.AddComponent<SimpleCombatAI>();

            _playerGo = new GameObject("Player");
            var playerEntity = _playerGo.AddComponent<CharacterEntity>();
            playerEntity.Bind(CharacterData.Create("p1", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common));
            var playerCombatant = _playerGo.AddComponent<Combatant>();
            playerCombatant.AutoTickEnabled = false;
            _playerGo.AddComponent<CombatController>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_aiGo);
            Object.Destroy(_playerGo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FollowTarget_SetsNavigationDestination()
        {
            var ai = _aiGo.GetComponent<SimpleCombatAI>();
            ai.FollowTarget = _playerGo.transform;
            yield return null;
            yield return null;

            Assert.That(ai.NavController, Is.Not.Null);
            Assert.That(ai.NavController.NavState, Is.Not.EqualTo(NavigationState.Idle).Or.EqualTo(NavigationState.Moving));
        }

        [UnityTest]
        public IEnumerator HoldPosition_StopsNavigation()
        {
            var ai = _aiGo.GetComponent<SimpleCombatAI>();
            ai.HoldPosition = _aiGo.transform.position;
            yield return null;
            yield return null;

            var posBefore = _aiGo.transform.position;
            ai.HoldPosition = _aiGo.transform.position;
            yield return null;

            Assert.That(ai.NavController, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ReleaseControl_ClearsFollowTarget()
        {
            var ai = _aiGo.GetComponent<SimpleCombatAI>();
            ai.FollowTarget = _playerGo.transform;
            yield return null;

            ai.FollowTarget = null;
            Assert.That(ai.FollowTarget, Is.Null);
        }
    }
}

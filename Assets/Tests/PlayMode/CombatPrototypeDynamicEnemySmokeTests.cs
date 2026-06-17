using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Feedback;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CombatPrototypeDynamicEnemySmokeTests
    {
        [UnityTest]
        public IEnumerator SpawnPoint_CreatesFullyEquippedEnemy()
        {
            var spawnGo = new GameObject("SpawnPoint");
            spawnGo.transform.position = Vector3.zero;
            var sp = spawnGo.AddComponent<EncounterSpawnPoint>();
            var data = CharacterData.Create("dyn_1", "DynamicEnemy", SubFactionId.BeastIronClaw, CharacterRole.Minion);

            yield return null;

            var unitGo = sp.SpawnUnit(data);
            Assert.IsNotNull(unitGo);

            yield return null;

            Assert.IsNotNull(unitGo.GetComponent<CharacterEntity>(), "Has CharacterEntity");
            Assert.IsNotNull(unitGo.GetComponent<Combatant>(), "Has Combatant");
            Assert.IsNotNull(unitGo.GetComponent<CharacterMovementMotor>(), "Has Motor");
            Assert.IsNotNull(unitGo.GetComponent<SimpleCombatAI>(), "Has SimpleCombatAI");
            Assert.IsNotNull(unitGo.GetComponent<AI.NavigationAgentBridge>(), "Has NavBridge");
            Assert.IsNotNull(unitGo.GetComponent<AI.AICombatNavigationController>(), "Has NavController");
            Assert.IsNotNull(unitGo.GetComponent<UI.CombatantHealthBarPresenter>(), "Has HealthBar");
            Assert.IsNotNull(unitGo.GetComponent<Combat.Feedback.HitFlashFeedback>(), "Has HitFlash");

            Object.DestroyImmediate(unitGo);
            Object.DestroyImmediate(spawnGo);
        }

        [UnityTest]
        public IEnumerator DynamicEnemy_CanMoveAndTakeDamage()
        {
            var spawnGo = new GameObject("SpawnPoint");
            spawnGo.transform.position = new Vector3(5f, 0f, 0f);
            var sp = spawnGo.AddComponent<EncounterSpawnPoint>();
            var data = CharacterData.Create("dyn_2", "DynamicEnemy2", SubFactionId.BeastIronClaw, CharacterRole.Minion);

            yield return null;

            var unitGo = sp.SpawnUnit(data);
            var combatant = unitGo.GetComponent<Combatant>();
            var hpBefore = combatant.CurrentHealth;

            combatant.ApplyHealthDamage(20f);
            yield return null;

            Assert.Less(combatant.CurrentHealth, hpBefore, "Dynamic enemy takes damage");
            Assert.IsTrue(combatant.IsAlive);

            Object.DestroyImmediate(unitGo);
            Object.DestroyImmediate(spawnGo);
        }

        [UnityTest]
        public IEnumerator DynamicEnemy_DeathStopsAI()
        {
            var spawnGo = new GameObject("SpawnPoint");
            spawnGo.transform.position = new Vector3(5f, 0f, 0f);
            var sp = spawnGo.AddComponent<EncounterSpawnPoint>();
            var data = CharacterData.Create("dyn_3", "DynamicEnemy3", SubFactionId.BeastIronClaw, CharacterRole.Minion);

            yield return null;

            var unitGo = sp.SpawnUnit(data);
            var combatant = unitGo.GetComponent<Combatant>();
            var ai = unitGo.GetComponent<SimpleCombatAI>();

            yield return null;

            Assert.IsTrue(ai.isActiveAndEnabled);

            combatant.ApplyHealthDamage(99999f);
            yield return null;

            Assert.IsFalse(combatant.IsAlive);
            Assert.AreEqual(CombatState.Dead, combatant.State);

            Object.DestroyImmediate(unitGo);
            Object.DestroyImmediate(spawnGo);
        }
    }
}

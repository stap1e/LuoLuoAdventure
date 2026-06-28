using System.Collections;
using LuoLuoTrip.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class NavMeshDynamicEncounterTests
    {
        private GameObject _encounterGo;
        private GameObject _spawnGo;

        [TearDown]
        public void TearDown()
        {
            if (_encounterGo != null) Object.DestroyImmediate(_encounterGo);
            if (_spawnGo != null) Object.DestroyImmediate(_spawnGo);
            var all = Object.FindObjectsOfType<CharacterEntity>();
            foreach (var e in all)
                if (e != null && e.gameObject != null)
                    Object.DestroyImmediate(e.gameObject);
        }

        [UnityTest]
        public IEnumerator DynamicWave_SpawnsAndRegistersUnits()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test" });

            _spawnGo = new GameObject("Spawn");
            _spawnGo.transform.position = Vector3.zero;
            encounter.AddSpawnPoint(_spawnGo.AddComponent<EncounterSpawnPoint>());

            encounter.SetWaves(new System.Collections.Generic.List<EncounterWave>
            {
                new EncounterWave { waveId = "w1", faction = SubFactionId.BeastIronClaw, unitCount = 2, delaySeconds = 0.5f },
            });

            yield return null;
            encounter.TickWaves(0.3f);
            Assert.That(encounter.SpawnedUnits.Count, Is.EqualTo(0));

            yield return null;
            encounter.TickWaves(0.3f);
            Assert.That(encounter.SpawnedUnits.Count, Is.GreaterThanOrEqualTo(2));
        }

        [UnityTest]
        public IEnumerator SpawnedUnit_HasNavigationBridge()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test" });

            _spawnGo = new GameObject("Spawn");
            _spawnGo.transform.position = Vector3.zero;
            encounter.AddSpawnPoint(_spawnGo.AddComponent<EncounterSpawnPoint>());

            encounter.SetWaves(new System.Collections.Generic.List<EncounterWave>
            {
                new EncounterWave { waveId = "w1", faction = SubFactionId.BeastIronClaw, unitCount = 1, delaySeconds = 0f },
            });

            yield return null;
            encounter.TickWaves(0.1f);

            Assert.That(encounter.SpawnedUnits.Count, Is.GreaterThan(0));
            var entity = encounter.SpawnedUnits[0].Entity;
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.GetComponent<NavigationAgentBridge>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator NavigationBridge_FallbackMovement_WhenNoNavMesh()
        {
            var unitGo = new GameObject("Unit");
            unitGo.transform.position = Vector3.zero;
            try
            {
                var bridge = unitGo.AddComponent<NavigationAgentBridge>();
                var entity = unitGo.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("nav_test", "NavTest", SubFactionId.BeastIronClaw, CharacterRole.Minion));
                unitGo.AddComponent<Combat.Combatant>();

                bridge.SetDestination(new Vector3(5f, 0f, 0f), 4f, 0.5f);

                var fixedDelta = 0.02f;
                for (int i = 0; i < 60; i++)
                {
                    bridge.TickFallback(fixedDelta);
                }

                Assert.That(unitGo.transform.position.x, Is.GreaterThan(1f));

                yield return null;

                yield return null;

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(unitGo);
            }
        }
    }
}

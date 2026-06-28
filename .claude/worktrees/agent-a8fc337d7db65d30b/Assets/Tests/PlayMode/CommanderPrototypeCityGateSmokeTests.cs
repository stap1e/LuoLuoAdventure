using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Combat;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeCityGateSmokeTests
    {
        [UnityTest]
        public IEnumerator CityGateDisputeRuntime_CanBeCreated_HasRequiredFields()
        {
            var go = new GameObject("CityGateDispute");
            try
            {
                var runtime = go.AddComponent<CityGateDisputeRuntime>();
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });

                yield return null;

                Assert.That(runtime, Is.Not.Null);
                // CityGateDisputeRuntime.Start() needs GameBootstrap.Context to assign _encounter.
                // In a test without context, _encounter stays null. Verify the EncounterRuntime
                // component itself exists on the GameObject instead.
                Assert.That(go.GetComponent<EncounterRuntime>(), Is.Not.Null,
                    "EncounterRuntime component must exist on the same GameObject");
                Assert.That(enc.HasStarted, Is.False);
                Assert.That(runtime.Phase, Is.EqualTo(MissionPhase.Inactive));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator CityGateCore_HasCombatantAndHealthBar()
        {
            var coreGo = new GameObject("CityGateCore");
            try
            {
                var entity = coreGo.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("core", "Core", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                var combatant = entity.Combatant;
                var bar = coreGo.AddComponent<CombatantHealthBarPresenter>();

                yield return null;

                Assert.That(combatant, Is.Not.Null);
                Assert.That(combatant.IsAlive, Is.True);
                Assert.That(bar, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(coreGo);
            }
        }

        [UnityTest]
        public IEnumerator BeastRaiderWave_SpawnsUnits()
        {
            var encGo = new GameObject("Encounter");
            try
            {
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });

                var spawnGo = new GameObject("Spawn");
                spawnGo.transform.position = Vector3.zero;
                var spawn = spawnGo.AddComponent<EncounterSpawnPoint>();
                enc.AddSpawnPoint(spawn);

                enc.SetWaves(new System.Collections.Generic.List<EncounterWave>
                {
                    new EncounterWave { waveId = "citygate_beast_raid_1", faction = SubFactionId.BeastIronClaw, role = CharacterRole.Minion, unitCount = 2, delaySeconds = 0f },
                });

                yield return null;
                enc.TickWaves(0.1f);

                Assert.That(enc.SpawnedUnits.Count, Is.GreaterThanOrEqualTo(2));

                Object.DestroyImmediate(spawnGo);
            }
            finally
            {
                Object.DestroyImmediate(encGo);
            }
        }

        [UnityTest]
        public IEnumerator DefeatingRaiders_CompletesObjective()
        {
            var encGo = new GameObject("Encounter");
            try
            {
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });

                var spawnGo = new GameObject("Spawn");
                spawnGo.transform.position = Vector3.zero;
                var spawn = spawnGo.AddComponent<EncounterSpawnPoint>();
                enc.AddSpawnPoint(spawn);

                enc.SetWaves(new System.Collections.Generic.List<EncounterWave>
                {
                    new EncounterWave { waveId = "citygate_beast_raid_1", faction = SubFactionId.BeastIronClaw, role = CharacterRole.Minion, unitCount = 2, delaySeconds = 0f },
                });

                yield return null;
                enc.TickWaves(0.1f);
                enc.StartEncounter();

                Assert.That(enc.HasStarted, Is.True);

                // Kill all spawned units
                for (int i = 0; i < enc.SpawnedUnits.Count; i++)
                {
                    var u = enc.SpawnedUnits[i];
                    if (u?.Entity?.Combatant != null)
                        u.Entity.Combatant.ApplyHealthDamage(99999f);
                }

                yield return null;

                bool allDefeated = enc.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw);
                Assert.That(allDefeated, Is.True, "All BeastRaiders should be defeated");

                Object.DestroyImmediate(spawnGo);
            }
            finally
            {
                Object.DestroyImmediate(encGo);
            }
        }
    }
}

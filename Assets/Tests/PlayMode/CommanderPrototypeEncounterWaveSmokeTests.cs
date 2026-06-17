using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeEncounterWaveSmokeTests
    {
        [UnityTest]
        public IEnumerator EncounterRuntime_SpawnsWave_WithValidUnits()
        {
            var encGo = new GameObject("Encounter");
            var enc = encGo.AddComponent<EncounterRuntime>();
            enc.Initialize(new EncounterDefinition());

            var spawnGo = new GameObject("SpawnPoint");
            spawnGo.transform.position = new Vector3(10f, 0f, 0f);
            var sp = spawnGo.AddComponent<EncounterSpawnPoint>();
            enc.AddSpawnPoint(sp);

            var wave = new EncounterWave
            {
                waveId = "test_wave",
                faction = SubFactionId.BeastIronClaw,
                role = CharacterRole.Minion,
                unitCount = 2,
                delaySeconds = 0f
            };

            yield return null;

            int spawned = enc.SpawnWave(wave);
            Assert.AreEqual(2, spawned, "Should spawn 2 units");

            yield return null;

            Assert.AreEqual(2, enc.SpawnedUnits.Count, "Spawned units should be tracked");

            foreach (var unit in enc.SpawnedUnits)
            {
                Assert.IsNotNull(unit.Entity, "Unit entity should exist");
                Assert.IsTrue(unit.IsAlive, "Unit should be alive");
            }

            enc.Clear();
            Object.DestroyImmediate(encGo);
            Object.DestroyImmediate(spawnGo);
        }

        [UnityTest]
        public IEnumerator EncounterRuntime_CasualtyCount_AfterKill()
        {
            var encGo = new GameObject("Encounter");
            var enc = encGo.AddComponent<EncounterRuntime>();
            enc.Initialize(new EncounterDefinition());

            var spawnGo = new GameObject("SpawnPoint");
            spawnGo.transform.position = new Vector3(10f, 0f, 0f);
            var sp = spawnGo.AddComponent<EncounterSpawnPoint>();
            enc.AddSpawnPoint(sp);

            var wave = new EncounterWave
            {
                waveId = "casualty_wave",
                faction = SubFactionId.BeastIronClaw,
                role = CharacterRole.Minion,
                unitCount = 1,
                delaySeconds = 0f
            };

            yield return null;

            enc.SpawnWave(wave);
            yield return null;

            Assert.AreEqual(0, enc.CountCasualties(SubFactionId.BeastIronClaw));

            var unit = enc.SpawnedUnits[0];
            unit.Entity.Data.IsAlive = false;
            if (unit.Entity.Combatant != null)
                unit.Entity.Combatant.ApplyHealthDamage(99999f);

            yield return null;

            Assert.AreEqual(1, enc.CountCasualties(SubFactionId.BeastIronClaw),
                "Killed spawned unit should count as casualty");

            enc.Clear();
            Object.DestroyImmediate(encGo);
            Object.DestroyImmediate(spawnGo);
        }
    }
}

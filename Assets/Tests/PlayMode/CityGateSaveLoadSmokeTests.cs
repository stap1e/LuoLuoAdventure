using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CityGateSaveLoadSmokeTests
    {
        [UnityTest]
        public IEnumerator CityGateEncounter_SnapshotRoundtrip_PreservesCompleted()
        {
            var go = new GameObject("Encounter");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });
                enc.StartEncounter();
                enc.CompleteEncounter("BalancedMediation");

                var snap = enc.GetSnapshot();
                Assert.That(snap.hasCompleted, Is.True);
                Assert.That(snap.lastOutcome, Is.EqualTo("BalancedMediation"));

                yield return null;

                var save = new GameSaveData();
                save.encounterSnapshots.Add(snap);
                var json = JsonUtility.ToJson(save);
                var loaded = JsonUtility.FromJson<GameSaveData>(json);

                Assert.That(loaded.encounterSnapshots.Count, Is.EqualTo(1));

                enc.ResetEncounter();
                Assert.That(enc.HasCompleted, Is.False);

                enc.RestoreSnapshot(loaded.encounterSnapshots[0]);
                Assert.That(enc.HasCompleted, Is.True);
                Assert.That(enc.LastOutcome, Is.EqualTo("BalancedMediation"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator CityGateWave_DuplicateGuard_PreventsDoubleSpawn()
        {
            var encGo = new GameObject("Enc");
            try
            {
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });
                var wave = new EncounterWave
                {
                    waveId = "citygate_beast_raid_1",
                    faction = SubFactionId.BeastIronClaw,
                    role = CharacterRole.Minion,
                    unitCount = 1
                };
                enc.SpawnWave(wave);
                int firstCount = enc.TotalSpawnedCount;

                yield return null;

                int second = enc.SpawnWave(wave);
                Assert.That(second, Is.EqualTo(0), "Duplicate wave ID must not spawn again");
                Assert.That(enc.TotalSpawnedCount, Is.EqualTo(firstCount));
            }
            finally
            {
                Object.DestroyImmediate(encGo);
            }
        }

        [UnityTest]
        public IEnumerator CityGateCompleted_DoesNotRespawn()
        {
            var go = new GameObject("Encounter");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });
                enc.CompleteEncounter("BalancedMediation");

                yield return null;

                enc.StartEncounter();
                Assert.That(enc.HasStarted, Is.False, "Completed encounter must not restart");
                Assert.That(enc.HasCompleted, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator CityGateInProgress_SetsNeedsRestartAfterLoad()
        {
            var go = new GameObject("Encounter");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });
                enc.StartEncounter();

                var snap = enc.GetSnapshot();
                Assert.That(snap.needsRestartAfterLoad, Is.True,
                    "In-progress encounter must flag needsRestartAfterLoad");

                yield return null;

                enc.ResetEncounter();
                enc.RestoreSnapshot(snap);
                Assert.That(enc.NeedsRestartAfterLoad, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

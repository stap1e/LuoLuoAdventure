using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeEncounterPersistenceSmokeTests
    {
        [UnityTest]
        public IEnumerator EncounterCompletedFlag_SurvivesSnapshotRoundtrip()
        {
            var go = new GameObject("Encounter");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "convoy_energy_conflict" });
                enc.StartEncounter();
                enc.CompleteEncounter("MechaVictory");

                var snap = enc.GetSnapshot();
                Assert.That(snap.hasCompleted, Is.True);
                Assert.That(snap.lastOutcome, Is.EqualTo("MechaVictory"));

                yield return null;

                // Simulate save+load: serialize via GameSaveData then restore
                var save = new GameSaveData();
                save.encounterSnapshots.Add(snap);
                var json = JsonUtility.ToJson(save);
                var loaded = JsonUtility.FromJson<GameSaveData>(json);

                Assert.That(loaded.encounterSnapshots.Count, Is.EqualTo(1));

                // Reset encounter then restore from loaded snapshot
                enc.ResetEncounter();
                Assert.That(enc.HasCompleted, Is.False);

                enc.RestoreSnapshot(loaded.encounterSnapshots[0]);
                Assert.That(enc.HasCompleted, Is.True, "Completed flag must persist across save+load");
                Assert.That(enc.LastOutcome, Is.EqualTo("MechaVictory"));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [UnityTest]
        public IEnumerator CompletedEncounter_DoesNotRestart()
        {
            var go = new GameObject("Encounter");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "completed_test" });
                enc.CompleteEncounter("Failed");

                yield return null;

                enc.StartEncounter();
                Assert.That(enc.HasStarted, Is.False, "StartEncounter must be a no-op once completed");
                Assert.That(enc.HasCompleted, Is.True);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [UnityTest]
        public IEnumerator ClearSpawnedUnits_DestroysOnlyDynamicUnits()
        {
            var encGo = new GameObject("Encounter");
            var manualGo = new GameObject("ManualUnit");
            try
            {
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "clear_test" });

                // Manually-placed unit registered via RegisterUnit (NOT a spawned dynamic unit)
                var entity = manualGo.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("manual", "ManualMecha", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                enc.RegisterUnit(entity);

                yield return null;

                int destroyed = enc.ClearSpawnedUnits();
                Assert.That(destroyed, Is.EqualTo(0), "Manually placed units must NOT be destroyed");
                Assert.That(manualGo, Is.Not.Null);
                Assert.That(enc.Units.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(manualGo);
                Object.DestroyImmediate(encGo);
            }
        }
    }
}

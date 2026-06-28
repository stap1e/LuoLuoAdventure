using LuoLuoTrip.Save;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class SaveLoadEncounterStateTests
    {
        [Test]
        public void NewSave_HasEmptyEncounterSnapshots()
        {
            var save = new GameSaveData();
            Assert.That(save.encounterSnapshots, Is.Not.Null);
            Assert.That(save.encounterSnapshots.Count, Is.EqualTo(0));
        }

        [Test]
        public void SaveRoundtrip_PreservesMultipleEncounters()
        {
            var save = new GameSaveData();
            save.encounterSnapshots.Add(new EncounterSnapshot
            {
                encounterId = "convoy_energy_conflict",
                hasCompleted = true,
                lastOutcome = "MechaVictory",
                totalSpawnedCount = 4,
                defeatedUnitCount = 4
            });
            save.encounterSnapshots.Add(new EncounterSnapshot
            {
                encounterId = "border_retaliation",
                hasStarted = true,
                hasCompleted = false,
                totalSpawnedCount = 2,
                defeatedUnitCount = 0
            });
            save.encounterSnapshots[1].spawnedWaveIds.Add("beast_wave1");

            var json = JsonUtility.ToJson(save, true);
            var loaded = JsonUtility.FromJson<GameSaveData>(json);

            Assert.That(loaded.encounterSnapshots.Count, Is.EqualTo(2));
            Assert.That(loaded.encounterSnapshots[0].encounterId, Is.EqualTo("convoy_energy_conflict"));
            Assert.That(loaded.encounterSnapshots[0].hasCompleted, Is.True);
            Assert.That(loaded.encounterSnapshots[1].encounterId, Is.EqualTo("border_retaliation"));
            Assert.That(loaded.encounterSnapshots[1].hasStarted, Is.True);
            Assert.That(loaded.encounterSnapshots[1].spawnedWaveIds.Count, Is.EqualTo(1));
            Assert.That(loaded.encounterSnapshots[1].spawnedWaveIds[0], Is.EqualTo("beast_wave1"));
        }

        [Test]
        public void V1Save_DefaultsEmptyEncounterSnapshots()
        {
            var save = new GameSaveData { version = 1 };
            Assert.That(save.encounterSnapshots, Is.Not.Null);
            Assert.That(save.encounterSnapshots.Count, Is.EqualTo(0));
        }

        [Test]
        public void RestoreSnapshot_OnFreshEncounter_AppliesCompletedFlag()
        {
            var go = new GameObject("Enc");
            try
            {
                var encounter = go.AddComponent<EncounterRuntime>();
                encounter.Initialize(new EncounterDefinition { encounterId = "convoy_energy_conflict" });
                var snap = new EncounterSnapshot
                {
                    encounterId = "convoy_energy_conflict",
                    hasCompleted = true,
                    lastOutcome = "MechaVictory",
                    totalSpawnedCount = 3
                };
                snap.spawnedWaveIds.Add("beast_wave1");
                encounter.RestoreSnapshot(snap);
                Assert.That(encounter.HasCompleted, Is.True);
                Assert.That(encounter.LastOutcome, Is.EqualTo("MechaVictory"));
                Assert.That(encounter.TotalSpawnedCount, Is.EqualTo(3));
                Assert.That(encounter.SpawnedWaveIds.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

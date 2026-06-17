using LuoLuoTrip.Save;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterSnapshotTests
    {
        private GameObject _go;
        private EncounterRuntime _encounter;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Encounter");
            _encounter = _go.AddComponent<EncounterRuntime>();
            _encounter.Initialize(new EncounterDefinition { encounterId = "snap_test" });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void GetSnapshot_CapturesEncounterId()
        {
            var snap = _encounter.GetSnapshot();
            Assert.That(snap, Is.Not.Null);
            Assert.That(snap.encounterId, Is.EqualTo("snap_test"));
            Assert.That(snap.hasStarted, Is.False);
            Assert.That(snap.hasCompleted, Is.False);
        }

        [Test]
        public void GetSnapshot_AfterStartAndComplete_ReflectsState()
        {
            _encounter.StartEncounter();
            _encounter.CompleteEncounter("BalancedResolution");
            var snap = _encounter.GetSnapshot();
            Assert.That(snap.hasCompleted, Is.True);
            Assert.That(snap.lastOutcome, Is.EqualTo("BalancedResolution"));
        }

        [Test]
        public void RestoreSnapshot_AppliesCompletedFlag()
        {
            var snap = new EncounterSnapshot
            {
                encounterId = "snap_test",
                hasStarted = false,
                hasCompleted = true,
                lastOutcome = "MechaVictory",
                totalSpawnedCount = 3
            };
            _encounter.RestoreSnapshot(snap);
            Assert.That(_encounter.HasCompleted, Is.True);
            Assert.That(_encounter.LastOutcome, Is.EqualTo("MechaVictory"));
            Assert.That(_encounter.TotalSpawnedCount, Is.EqualTo(3));
        }

        [Test]
        public void RestoreSnapshot_RebuildsSpawnedWaveIds()
        {
            var snap = new EncounterSnapshot { encounterId = "snap_test" };
            snap.spawnedWaveIds.Add("waveA");
            snap.spawnedWaveIds.Add("waveB");
            _encounter.RestoreSnapshot(snap);
            Assert.That(_encounter.SpawnedWaveIds.Count, Is.EqualTo(2));
        }

        [Test]
        public void RestoreSnapshot_NullDoesNotThrow()
        {
            Assert.DoesNotThrow(() => _encounter.RestoreSnapshot(null));
        }

        [Test]
        public void GetSnapshot_InProgress_FlagsNeedsRestart()
        {
            _encounter.StartEncounter();
            var snap = _encounter.GetSnapshot();
            Assert.That(snap.hasStarted, Is.True);
            Assert.That(snap.hasCompleted, Is.False);
            Assert.That(snap.needsRestartAfterLoad, Is.True,
                "In-progress encounters must flag needsRestartAfterLoad");
        }

        [Test]
        public void GetSnapshot_Completed_DoesNotFlagNeedsRestart()
        {
            _encounter.StartEncounter();
            _encounter.CompleteEncounter("MechaVictory");
            var snap = _encounter.GetSnapshot();
            Assert.That(snap.needsRestartAfterLoad, Is.False);
        }

        [Test]
        public void RestoreSnapshot_InProgress_SetsRuntimeNeedsRestartFlag()
        {
            var snap = new EncounterSnapshot
            {
                encounterId = "snap_test",
                hasStarted = true,
                hasCompleted = false,
            };
            _encounter.RestoreSnapshot(snap);
            Assert.That(_encounter.NeedsRestartAfterLoad, Is.True);
        }

        [Test]
        public void RestoreSnapshot_Completed_DoesNotSetNeedsRestartFlag()
        {
            var snap = new EncounterSnapshot
            {
                encounterId = "snap_test",
                hasStarted = false,
                hasCompleted = true,
                lastOutcome = "MechaVictory"
            };
            _encounter.RestoreSnapshot(snap);
            Assert.That(_encounter.NeedsRestartAfterLoad, Is.False);
            Assert.That(_encounter.HasCompleted, Is.True);
        }

        [Test]
        public void GameSaveData_HasEncounterSnapshotsField()
        {
            var save = new GameSaveData();
            Assert.That(save.encounterSnapshots, Is.Not.Null);
            Assert.That(save.encounterSnapshots.Count, Is.EqualTo(0));
        }

        [Test]
        public void GameSaveData_SerializesEncounterSnapshots()
        {
            var save = new GameSaveData();
            save.encounterSnapshots.Add(new EncounterSnapshot
            {
                encounterId = "convoy_energy_conflict",
                hasCompleted = true,
                lastOutcome = "MechaVictory",
                totalSpawnedCount = 4
            });
            var json = JsonUtility.ToJson(save);
            var roundtrip = JsonUtility.FromJson<GameSaveData>(json);
            Assert.That(roundtrip.encounterSnapshots.Count, Is.EqualTo(1));
            Assert.That(roundtrip.encounterSnapshots[0].encounterId, Is.EqualTo("convoy_energy_conflict"));
            Assert.That(roundtrip.encounterSnapshots[0].hasCompleted, Is.True);
            Assert.That(roundtrip.encounterSnapshots[0].lastOutcome, Is.EqualTo("MechaVictory"));
            Assert.That(roundtrip.encounterSnapshots[0].totalSpawnedCount, Is.EqualTo(4));
        }
    }
}

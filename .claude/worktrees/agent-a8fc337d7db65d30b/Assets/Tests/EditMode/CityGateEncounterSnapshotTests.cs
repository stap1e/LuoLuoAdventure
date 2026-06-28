using LuoLuoTrip.Save;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateEncounterSnapshotTests
    {
        private GameObject _go;
        private EncounterRuntime _encounter;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("CityGateEncounter");
            _encounter = _go.AddComponent<EncounterRuntime>();
            _encounter.Initialize(new EncounterDefinition { encounterId = "city_gate_dispute" });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void WaveDuplicateGuard_PreventsDoubleSpawn()
        {
            var wave = new EncounterWave
            {
                waveId = "citygate_beast_raid_1",
                faction = SubFactionId.BeastIronClaw,
                unitCount = 1
            };
            _encounter.SpawnWave(wave);
            int second = _encounter.SpawnWave(wave);
            Assert.That(second, Is.EqualTo(0), "Duplicate wave must not spawn again");
            Assert.That(_encounter.SpawnedWaveIds, Contains.Item("citygate_beast_raid_1"));
        }

        [Test]
        public void Snapshot_Completed_PreventsRespawn()
        {
            _encounter.StartEncounter();
            _encounter.CompleteEncounter("BalancedMediation");

            var snap = _encounter.GetSnapshot();
            Assert.That(snap.hasCompleted, Is.True);
            Assert.That(snap.lastOutcome, Is.EqualTo("BalancedMediation"));

            _encounter.ResetEncounter();
            Assert.That(_encounter.HasCompleted, Is.False);

            _encounter.RestoreSnapshot(snap);
            Assert.That(_encounter.HasCompleted, Is.True);
            _encounter.StartEncounter();
            Assert.That(_encounter.HasStarted, Is.False, "Completed encounter must not restart");
        }

        [Test]
        public void Snapshot_InProgress_SetsNeedsRestartAfterLoad()
        {
            _encounter.StartEncounter();
            var snap = _encounter.GetSnapshot();
            Assert.That(snap.hasStarted, Is.True);
            Assert.That(snap.hasCompleted, Is.False);
            Assert.That(snap.needsRestartAfterLoad, Is.True);

            _encounter.RestoreSnapshot(snap);
            Assert.That(_encounter.NeedsRestartAfterLoad, Is.True);
        }

        [Test]
        public void Snapshot_NotStarted_DoesNotFlagRestart()
        {
            var snap = _encounter.GetSnapshot();
            Assert.That(snap.hasStarted, Is.False);
            Assert.That(snap.needsRestartAfterLoad, Is.False);
        }

        [Test]
        public void CityGateSaveData_SerializesSnapshot()
        {
            var save = new GameSaveData();
            save.encounterSnapshots.Add(new EncounterSnapshot
            {
                encounterId = "city_gate_dispute",
                hasCompleted = true,
                lastOutcome = "BalancedMediation",
                totalSpawnedCount = 5
            });
            var json = JsonUtility.ToJson(save);
            var loaded = JsonUtility.FromJson<GameSaveData>(json);
            Assert.That(loaded.encounterSnapshots.Count, Is.EqualTo(1));
            Assert.That(loaded.encounterSnapshots[0].encounterId, Is.EqualTo("city_gate_dispute"));
            Assert.That(loaded.encounterSnapshots[0].hasCompleted, Is.True);
        }
    }
}

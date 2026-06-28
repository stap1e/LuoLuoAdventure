using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterLifecycleTests
    {
        private GameObject _go;
        private EncounterRuntime _encounter;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Encounter");
            _encounter = _go.AddComponent<EncounterRuntime>();
            _encounter.Initialize(new EncounterDefinition { encounterId = "test_encounter" });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void StartEncounter_SetsHasStarted()
        {
            Assert.That(_encounter.HasStarted, Is.False);
            _encounter.StartEncounter();
            Assert.That(_encounter.HasStarted, Is.True);
            Assert.That(_encounter.HasCompleted, Is.False);
        }

        [Test]
        public void CompleteEncounter_SetsHasCompletedAndOutcome()
        {
            _encounter.StartEncounter();
            _encounter.CompleteEncounter("MechaVictory");
            Assert.That(_encounter.HasCompleted, Is.True);
            Assert.That(_encounter.HasStarted, Is.False);
            Assert.That(_encounter.LastOutcome, Is.EqualTo("MechaVictory"));
        }

        [Test]
        public void StartEncounter_IgnoredAfterComplete()
        {
            _encounter.CompleteEncounter("BeastVictory");
            _encounter.StartEncounter();
            Assert.That(_encounter.HasStarted, Is.False);
            Assert.That(_encounter.HasCompleted, Is.True);
        }

        [Test]
        public void ResetEncounter_ClearsAllState()
        {
            _encounter.StartEncounter();
            _encounter.CompleteEncounter("Failed");
            _encounter.ResetEncounter();
            Assert.That(_encounter.HasStarted, Is.False);
            Assert.That(_encounter.HasCompleted, Is.False);
            Assert.That(_encounter.LastOutcome, Is.Null);
            Assert.That(_encounter.TotalSpawnedCount, Is.EqualTo(0));
            Assert.That(_encounter.SpawnedWaveIds.Count, Is.EqualTo(0));
        }

        [Test]
        public void Clear_ResetsAllCounters()
        {
            _encounter.StartEncounter();
            _encounter.CompleteEncounter("X");
            _encounter.Clear();
            Assert.That(_encounter.HasStarted, Is.False);
            Assert.That(_encounter.HasCompleted, Is.False);
            Assert.That(_encounter.TotalSpawnedCount, Is.EqualTo(0));
        }
    }
}

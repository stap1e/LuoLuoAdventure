using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterWaveIdempotencyTests
    {
        private GameObject _go;
        private EncounterRuntime _encounter;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Encounter");
            _encounter = _go.AddComponent<EncounterRuntime>();
            _encounter.Initialize(new EncounterDefinition { encounterId = "wave_test" });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void SpawnWave_NoSpawnPoint_RegistersWaveIdAnyway()
        {
            var wave = new EncounterWave
            {
                waveId = "beast_wave1",
                faction = SubFactionId.BeastIronClaw,
                role = CharacterRole.Minion,
                unitCount = 2
            };
            int spawned = _encounter.SpawnWave(wave);
            Assert.That(spawned, Is.EqualTo(0));
            Assert.That(_encounter.SpawnedWaveIds, Contains.Item("beast_wave1"));
            Assert.That(wave.spawned, Is.True, "wave.spawned must be set so future ticks skip it");
        }

        [Test]
        public void SpawnWave_DuplicateWaveId_SkipsAndReturnsZero()
        {
            var wave = new EncounterWave
            {
                waveId = "beast_wave1",
                faction = SubFactionId.BeastIronClaw,
                role = CharacterRole.Minion,
                unitCount = 1
            };
            _encounter.SpawnWave(wave);
            int second = _encounter.SpawnWave(wave);
            Assert.That(second, Is.EqualTo(0));
        }

        [Test]
        public void ResetEncounter_AllowsRespawnOfWaveId()
        {
            var wave = new EncounterWave
            {
                waveId = "beast_wave1",
                faction = SubFactionId.BeastIronClaw,
                role = CharacterRole.Minion,
                unitCount = 1
            };
            _encounter.SpawnWave(wave);
            Assert.That(_encounter.SpawnedWaveIds.Count, Is.EqualTo(1));

            _encounter.ResetEncounter();
            Assert.That(_encounter.SpawnedWaveIds.Count, Is.EqualTo(0));
        }

        [Test]
        public void RestoreSnapshot_MarksAlreadySpawnedWavesAsSpawned()
        {
            var waves = new System.Collections.Generic.List<EncounterWave>
            {
                new EncounterWave { waveId = "w1", faction = SubFactionId.BeastIronClaw, unitCount = 1 },
                new EncounterWave { waveId = "w2", faction = SubFactionId.BeastIronClaw, unitCount = 1 }
            };
            _encounter.SetWaves(waves);
            Assert.That(_encounter.PendingWaveCount, Is.EqualTo(2));

            var snap = new LuoLuoTrip.Save.EncounterSnapshot { encounterId = "wave_test" };
            snap.spawnedWaveIds.Add("w1");
            _encounter.RestoreSnapshot(snap);

            Assert.That(_encounter.PendingWaveCount, Is.EqualTo(1));
            Assert.That(waves[0].spawned, Is.True);
            Assert.That(waves[1].spawned, Is.False);
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterWaveDynamicTests
    {
        private GameObject _encounterGo;

        [TearDown]
        public void TearDown()
        {
            if (_encounterGo != null)
                Object.DestroyImmediate(_encounterGo);
        }

        [Test]
        public void EncounterWave_DefaultNotSpawned()
        {
            var wave = new EncounterWave { waveId = "test", faction = SubFactionId.BeastIronClaw, unitCount = 2, delaySeconds = 5f };
            Assert.That(wave.spawned, Is.False);
            Assert.That(wave.IsReady, Is.True);
        }

        [Test]
        public void EncounterWave_MarkSpawned_NoLongerReady()
        {
            var wave = new EncounterWave { waveId = "test", faction = SubFactionId.BeastIronClaw, unitCount = 2, delaySeconds = 5f };
            wave.spawned = true;
            Assert.That(wave.IsReady, Is.False);
        }

        [Test]
        public void EncounterRuntime_SetWaves_SetsPendingCount()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();

            var waves = new System.Collections.Generic.List<EncounterWave>
            {
                new EncounterWave { waveId = "w1", faction = SubFactionId.BeastIronClaw, unitCount = 2, delaySeconds = 5f },
                new EncounterWave { waveId = "w2", faction = SubFactionId.BeastIronClaw, unitCount = 3, delaySeconds = 15f },
            };
            encounter.SetWaves(waves);

            Assert.That(encounter.Waves.Count, Is.EqualTo(2));
            Assert.That(encounter.PendingWaveCount, Is.EqualTo(2));
        }

        [Test]
        public void EncounterRuntime_TickWaves_SpawnsAfterDelay()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test", BeastHostilityMultiplier = 1f });

            var spawnGo = new GameObject("SpawnPoint");
            try
            {
                var spawnPoint = spawnGo.AddComponent<EncounterSpawnPoint>();
                encounter.AddSpawnPoint(spawnPoint);

                var waves = new System.Collections.Generic.List<EncounterWave>
                {
                    new EncounterWave { waveId = "w1", faction = SubFactionId.BeastIronClaw, unitCount = 1, delaySeconds = 5f },
                };
                encounter.SetWaves(waves);

                encounter.TickWaves(4f);
                Assert.That(encounter.SpawnedUnits.Count, Is.EqualTo(0));

                encounter.TickWaves(2f);
                Assert.That(encounter.SpawnedUnits.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(encounter.PendingWaveCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(spawnGo);
                var spawned = Object.FindObjectsOfType<CharacterEntity>();
                foreach (var e in spawned)
                    if (e != null && e.gameObject != null)
                        Object.DestroyImmediate(e.gameObject);
            }
        }

        [Test]
        public void EncounterRuntime_TickWaves_DoesNotRespawnSameWave()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test", BeastHostilityMultiplier = 1f });

            var spawnGo = new GameObject("SpawnPoint");
            try
            {
                var spawnPoint = spawnGo.AddComponent<EncounterSpawnPoint>();
                encounter.AddSpawnPoint(spawnPoint);

                var waves = new System.Collections.Generic.List<EncounterWave>
                {
                    new EncounterWave { waveId = "w1", faction = SubFactionId.BeastIronClaw, unitCount = 1, delaySeconds = 1f },
                };
                encounter.SetWaves(waves);

                encounter.TickWaves(5f);
                var firstCount = encounter.SpawnedUnits.Count;

                encounter.TickWaves(5f);
                Assert.That(encounter.SpawnedUnits.Count, Is.EqualTo(firstCount));
            }
            finally
            {
                Object.DestroyImmediate(spawnGo);
                var spawned = Object.FindObjectsOfType<CharacterEntity>();
                foreach (var e in spawned)
                    if (e != null && e.gameObject != null)
                        Object.DestroyImmediate(e.gameObject);
            }
        }

        [Test]
        public void EncounterRuntime_GetFactionMultiplier_ReturnsCorrectMultiplier()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test", BeastHostilityMultiplier = 2f, MechaSupportMultiplier = 0.5f });

            Assert.That(encounter.GetFactionMultiplier(SubFactionId.BeastIronClaw), Is.EqualTo(2f));
            Assert.That(encounter.GetFactionMultiplier(SubFactionId.MotorIronRiders), Is.EqualTo(0.5f));
        }

        [Test]
        public void EncounterRuntime_AreAllRaidUnitsDefeated_IncludesSpawnedUnits()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test", BeastHostilityMultiplier = 1f });

            var spawnGo = new GameObject("SpawnPoint");
            try
            {
                var spawnPoint = spawnGo.AddComponent<EncounterSpawnPoint>();
                encounter.AddSpawnPoint(spawnPoint);

                var waves = new System.Collections.Generic.List<EncounterWave>
                {
                    new EncounterWave { waveId = "w1", faction = SubFactionId.BeastIronClaw, unitCount = 1, delaySeconds = 0f },
                };
                encounter.SetWaves(waves);
                encounter.TickWaves(1f);

                Assert.That(encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw), Is.False);

                foreach (var u in encounter.SpawnedUnits)
                    if (u.Entity != null && u.Entity.Data != null) u.Entity.Data.IsAlive = false;

                Assert.That(encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(spawnGo);
                var spawned = Object.FindObjectsOfType<CharacterEntity>();
                foreach (var e in spawned)
                    if (e != null && e.gameObject != null)
                        Object.DestroyImmediate(e.gameObject);
            }
        }
    }
}

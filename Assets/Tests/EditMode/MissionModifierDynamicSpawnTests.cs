using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionModifierDynamicSpawnTests
    {
        private GameObject _encounterGo;
        private GameObject _spawnGo;

        [TearDown]
        public void TearDown()
        {
            if (_encounterGo != null) Object.DestroyImmediate(_encounterGo);
            if (_spawnGo != null) Object.DestroyImmediate(_spawnGo);
            var all = Object.FindObjectsOfType<CharacterEntity>();
            foreach (var e in all)
                if (e != null && e.gameObject != null)
                    Object.DestroyImmediate(e.gameObject);
        }

        [Test]
        public void BeastHostilityMultiplier_IncreasesSpawnCount()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test", BeastHostilityMultiplier = 2f });

            _spawnGo = new GameObject("Spawn");
            var sp = _spawnGo.AddComponent<EncounterSpawnPoint>();
            encounter.AddSpawnPoint(sp);

            var wave = new EncounterWave { waveId = "beast_wave", faction = SubFactionId.BeastIronClaw, unitCount = 2, delaySeconds = 0f };

            encounter.SpawnWave(wave);

            Assert.That(encounter.SpawnedUnits.Count, Is.EqualTo(4));
        }

        [Test]
        public void MechaSupportMultiplier_IncreasesMechaSpawnCount()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test", MechaSupportMultiplier = 3f });

            _spawnGo = new GameObject("Spawn");
            var sp = _spawnGo.AddComponent<EncounterSpawnPoint>();
            encounter.AddSpawnPoint(sp);

            var wave = new EncounterWave { waveId = "mecha_wave", faction = SubFactionId.MotorIronRiders, unitCount = 1, delaySeconds = 0f };

            encounter.SpawnWave(wave);

            Assert.That(encounter.SpawnedUnits.Count, Is.EqualTo(3));
        }

        [Test]
        public void DefaultMultiplier_OneToOne()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test" });

            _spawnGo = new GameObject("Spawn");
            var sp = _spawnGo.AddComponent<EncounterSpawnPoint>();
            encounter.AddSpawnPoint(sp);

            var wave = new EncounterWave { waveId = "wave1", faction = SubFactionId.BeastIronClaw, unitCount = 3, delaySeconds = 0f };

            encounter.SpawnWave(wave);

            Assert.That(encounter.SpawnedUnits.Count, Is.EqualTo(3));
        }

        [Test]
        public void ApplyMissionModifier_UpdatesMultipliersForDynamicSpawn()
        {
            _encounterGo = new GameObject("Encounter");
            var encounter = _encounterGo.AddComponent<EncounterRuntime>();
            encounter.Initialize(new EncounterDefinition { encounterId = "test" });
            encounter.ApplyMissionModifier(new MissionModifier { BeastHostilityMultiplier = 1.5f, MechaSupportMultiplier = 2f });

            Assert.That(encounter.GetFactionMultiplier(SubFactionId.BeastIronClaw), Is.EqualTo(1.5f));
            Assert.That(encounter.GetFactionMultiplier(SubFactionId.MotorIronRiders), Is.EqualTo(2f));
        }
    }
}

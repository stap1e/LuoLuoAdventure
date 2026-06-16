using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip
{
    public class EncounterRuntime : MonoBehaviour
    {
        [SerializeField] private EncounterDefinition _definition;
        [SerializeField] private List<EncounterWave> _waves = new List<EncounterWave>();
        [SerializeField] private List<EncounterSpawnPoint> _spawnPoints = new List<EncounterSpawnPoint>();

        private readonly List<EncounterUnitHandle> _units = new List<EncounterUnitHandle>();
        private readonly List<EncounterUnitHandle> _spawnedUnits = new List<EncounterUnitHandle>();
        private float _waveTimer;

        public EncounterDefinition Definition => _definition;
        public IReadOnlyList<EncounterUnitHandle> Units => _units;
        public IReadOnlyList<EncounterUnitHandle> SpawnedUnits => _spawnedUnits;
        public IReadOnlyList<EncounterWave> Waves => _waves;
        public IReadOnlyList<EncounterSpawnPoint> SpawnPoints => _spawnPoints;
        public int PendingWaveCount { get; private set; }

        public void Initialize(EncounterDefinition definition)
        {
            _definition = definition;
        }

        public void SetWaves(List<EncounterWave> waves)
        {
            _waves = waves ?? new List<EncounterWave>();
            PendingWaveCount = 0;
            for (int i = 0; i < _waves.Count; i++)
                if (_waves[i].IsReady) PendingWaveCount++;
        }

        public void AddSpawnPoint(EncounterSpawnPoint spawnPoint)
        {
            if (spawnPoint != null && !_spawnPoints.Contains(spawnPoint))
                _spawnPoints.Add(spawnPoint);
        }

        public void RegisterUnit(CharacterEntity unit)
        {
            if (unit == null) return;
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i].Entity == unit) return;
            }
            var handle = EncounterUnitHandle.FromEntity(unit);
            if (handle != null)
                _units.Add(handle);
        }

        public void UnregisterUnit(CharacterEntity unit)
        {
            if (unit == null) return;
            for (int i = _units.Count - 1; i >= 0; i--)
            {
                if (_units[i].Entity == unit)
                {
                    _units.RemoveAt(i);
                    return;
                }
            }
        }

        public void RegisterUnitsBySubFaction(SubFactionId faction)
        {
            var entities = CharacterRuntimeRegistry.Count > 0
                ? CharacterRuntimeRegistry.QueryBySubFaction(faction)
                : new List<CharacterEntity>();

            if (entities.Count == 0)
            {
                foreach (var entity in FindObjectsOfType<CharacterEntity>())
                {
                    if (entity.Data != null && entity.Data.IsAlive && entity.Data.Faction == faction)
                        entities.Add(entity);
                }
            }

            foreach (var entity in entities)
                RegisterUnit(entity);
        }

        public List<EncounterUnitHandle> GetAliveUnits(MainRace race)
        {
            var result = new List<EncounterUnitHandle>();
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (u.Race == race && u.IsAlive)
                    result.Add(u);
            }
            return result;
        }

        public List<EncounterUnitHandle> GetAliveUnits(SubFactionId faction)
        {
            var result = new List<EncounterUnitHandle>();
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (u.Entity != null && u.Entity.Data != null && u.Entity.Data.Faction == faction && u.IsAlive)
                    result.Add(u);
            }
            return result;
        }

        public bool AreAllRaidUnitsDefeated(SubFactionId raidFaction)
        {
            bool any = false;
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (u.Entity == null || u.Entity.Data == null) continue;
                if (u.Entity.Data.Faction != raidFaction) continue;
                any = true;
                if (u.IsAlive) return false;
            }
            for (int i = 0; i < _spawnedUnits.Count; i++)
            {
                var u = _spawnedUnits[i];
                if (u.Entity == null || u.Entity.Data == null) continue;
                if (u.Entity.Data.Faction != raidFaction) continue;
                any = true;
                if (u.IsAlive) return false;
            }
            return any;
        }

        public int CountCasualties(MainRace race)
        {
            int count = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (u.Race == race && u.WasAliveAtStart && !u.IsAlive)
                    count++;
            }
            for (int i = 0; i < _spawnedUnits.Count; i++)
            {
                var u = _spawnedUnits[i];
                if (u.Race == race && u.WasAliveAtStart && !u.IsAlive)
                    count++;
            }
            return count;
        }

        public int CountCasualties(SubFactionId faction)
        {
            int count = 0;
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (u.Entity != null && u.Entity.Data != null && u.Entity.Data.Faction == faction && u.WasAliveAtStart && !u.IsAlive)
                    count++;
            }
            for (int i = 0; i < _spawnedUnits.Count; i++)
            {
                var u = _spawnedUnits[i];
                if (u.Entity != null && u.Entity.Data != null && u.Entity.Data.Faction == faction && u.WasAliveAtStart && !u.IsAlive)
                    count++;
            }
            return count;
        }

        public int CountSpawnedCasualties(SubFactionId faction)
        {
            int count = 0;
            for (int i = 0; i < _spawnedUnits.Count; i++)
            {
                var u = _spawnedUnits[i];
                if (u.Entity != null && u.Entity.Data != null && u.Entity.Data.Faction == faction && u.WasAliveAtStart && !u.IsAlive)
                    count++;
            }
            return count;
        }

        public void ApplyMissionModifier(MissionModifier modifier)
        {
            if (_definition == null) return;
            _definition.BeastHostilityMultiplier = modifier.BeastHostilityMultiplier;
            _definition.MechaSupportMultiplier = modifier.MechaSupportMultiplier;
            _definition.InitialHostilityOffset = modifier.InitialHostilityOffset;
        }

        public void TickWaves(float deltaTime)
        {
            _waveTimer += deltaTime;

            for (int i = 0; i < _waves.Count; i++)
            {
                var wave = _waves[i];
                if (!wave.IsReady) continue;
                if (_waveTimer < wave.delaySeconds) continue;

                SpawnWave(wave);
                wave.spawned = true;
                PendingWaveCount--;
            }
        }

        public int SpawnWave(EncounterWave wave)
        {
            int spawned = 0;
            var spawnPoint = FindSpawnPoint(wave.faction);
            var count = Mathf.RoundToInt(wave.unitCount * GetFactionMultiplier(wave.faction));

            for (int i = 0; i < count; i++)
            {
                var data = CharacterData.Create(
                    $"wave_{wave.waveId}_{i}",
                    $"{wave.faction}_{wave.role}",
                    wave.faction,
                    wave.role);

                GameObject unitGo = null;
                if (spawnPoint != null)
                    unitGo = spawnPoint.SpawnUnit(data);

                if (unitGo != null)
                {
                    var entity = unitGo.GetComponent<CharacterEntity>();
                    if (entity != null)
                    {
                        var handle = EncounterUnitHandle.FromEntity(entity);
                        if (handle != null)
                        {
                            _spawnedUnits.Add(handle);
                            spawned++;
                        }
                    }
                }
            }

            if (spawned > 0)
                Debug.Log($"[EncounterRuntime] Spawned wave '{wave.waveId}': {spawned} {wave.faction} units");

            return spawned;
        }

        public float GetFactionMultiplier(SubFactionId faction)
        {
            if (_definition == null) return 1f;
            var race = GameConstants.GetMainRace(faction);
            if (race == MainRace.BeastTribe)
                return _definition.BeastHostilityMultiplier;
            if (race == MainRace.MotorTribe)
                return _definition.MechaSupportMultiplier;
            return 1f;
        }

        private EncounterSpawnPoint FindSpawnPoint(SubFactionId faction)
        {
            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                if (_spawnPoints[i].Faction == faction)
                    return _spawnPoints[i];
            }
            return _spawnPoints.Count > 0 ? _spawnPoints[0] : null;
        }

        public void Clear()
        {
            _units.Clear();
            _spawnedUnits.Clear();
            _waveTimer = 0f;
            PendingWaveCount = 0;
        }
    }
}

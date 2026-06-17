using System.Collections.Generic;
using LuoLuoTrip.Save;
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
        private readonly HashSet<string> _spawnedWaveIds = new HashSet<string>();
        private float _waveTimer;
        private bool _hasStarted;
        private bool _hasCompleted;
        private string _lastOutcome;
        private int _totalSpawnedCount;

        public EncounterDefinition Definition => _definition;
        public IReadOnlyList<EncounterUnitHandle> Units => _units;
        public IReadOnlyList<EncounterUnitHandle> SpawnedUnits => _spawnedUnits;
        public IReadOnlyList<EncounterWave> Waves => _waves;
        public IReadOnlyList<EncounterSpawnPoint> SpawnPoints => _spawnPoints;
        public int PendingWaveCount { get; private set; }
        public bool HasStarted => _hasStarted;
        public bool HasCompleted => _hasCompleted;
        public string LastOutcome => _lastOutcome;
        public int TotalSpawnedCount => _totalSpawnedCount;
        public IReadOnlyCollection<string> SpawnedWaveIds => _spawnedWaveIds;

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
                if (u == null || u.Entity == null || u.Entity.Data == null) continue;
                if (u.Race == race && u.WasAliveAtStart && !u.IsAlive)
                    count++;
            }
            for (int i = 0; i < _spawnedUnits.Count; i++)
            {
                var u = _spawnedUnits[i];
                if (u == null || u.Entity == null || u.Entity.Data == null) continue;
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
            if (wave == null) return 0;
            if (!string.IsNullOrEmpty(wave.waveId) && _spawnedWaveIds.Contains(wave.waveId))
            {
                Debug.Log($"[EncounterRuntime] Skip duplicate wave '{wave.waveId}' (already spawned)");
                wave.spawned = true;
                return 0;
            }

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
                    unitGo = spawnPoint.SpawnUnit(data, null, wave.spawnRadius, wave.initialBehavior);

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

            if (!string.IsNullOrEmpty(wave.waveId))
                _spawnedWaveIds.Add(wave.waveId);
            wave.spawned = true;
            _totalSpawnedCount += spawned;

            if (spawned > 0)
                Debug.Log($"[EncounterRuntime] Spawned wave '{wave.waveId}': {spawned} {wave.faction} units (behavior={wave.initialBehavior})");

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
            _spawnedWaveIds.Clear();
            _waveTimer = 0f;
            PendingWaveCount = 0;
            _totalSpawnedCount = 0;
            _hasStarted = false;
            _hasCompleted = false;
            _lastOutcome = null;
            NeedsRestartAfterLoad = false;
        }

        public void StartEncounter()
        {
            if (_hasCompleted)
            {
                Debug.Log($"[EncounterRuntime] StartEncounter ignored: '{_definition?.encounterId}' already completed");
                return;
            }
            if (_hasStarted)
            {
                Debug.Log($"[EncounterRuntime] StartEncounter ignored: '{_definition?.encounterId}' already started");
                return;
            }
            _hasStarted = true;
            Debug.Log($"[EncounterRuntime] StartEncounter '{_definition?.encounterId ?? gameObject.name}'");
        }

        public void CompleteEncounter(string outcome = null)
        {
            if (_hasCompleted)
            {
                Debug.Log($"[EncounterRuntime] CompleteEncounter ignored: '{_definition?.encounterId}' already completed");
                return;
            }
            _hasCompleted = true;
            _hasStarted = false;
            _lastOutcome = outcome;
            Debug.Log($"[EncounterRuntime] CompleteEncounter '{_definition?.encounterId ?? gameObject.name}' outcome={outcome ?? "(none)"}");
        }

        public void ResetEncounter()
        {
            ClearSpawnedUnits();
            CharacterRuntimeComponentGuard.ResetWarnings();
            _spawnedWaveIds.Clear();
            _waveTimer = 0f;
            for (int i = 0; i < _waves.Count; i++)
                _waves[i].spawned = false;
            PendingWaveCount = 0;
            for (int i = 0; i < _waves.Count; i++)
                if (_waves[i].IsReady) PendingWaveCount++;
            _totalSpawnedCount = 0;
            _hasStarted = false;
            _hasCompleted = false;
            _lastOutcome = null;
            NeedsRestartAfterLoad = false;
            Debug.Log($"[EncounterRuntime] ResetEncounter '{_definition?.encounterId ?? gameObject.name}'");
        }

        public int ClearSpawnedUnits()
        {
            int destroyed = 0;
            for (int i = 0; i < _spawnedUnits.Count; i++)
            {
                var u = _spawnedUnits[i];
                if (u?.Entity != null && u.Entity.gameObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(u.Entity.gameObject);
                    else
                        DestroyImmediate(u.Entity.gameObject);
                    destroyed++;
                }
            }
            _spawnedUnits.Clear();
            if (destroyed > 0)
                Debug.Log($"[EncounterRuntime] ClearSpawnedUnits '{_definition?.encounterId ?? gameObject.name}' destroyed={destroyed} (manual scene units preserved)");
            return destroyed;
        }

        public int DespawnDeadUnits()
        {
            int destroyed = 0;
            for (int i = _spawnedUnits.Count - 1; i >= 0; i--)
            {
                var u = _spawnedUnits[i];
                if (u?.Entity == null || u.Entity.gameObject == null)
                {
                    _spawnedUnits.RemoveAt(i);
                    continue;
                }
                if (u.Entity.Data == null || !u.Entity.Data.IsAlive)
                {
                    if (Application.isPlaying)
                        Destroy(u.Entity.gameObject);
                    else
                        DestroyImmediate(u.Entity.gameObject);
                    _spawnedUnits.RemoveAt(i);
                    destroyed++;
                }
            }
            return destroyed;
        }

        public bool NeedsRestartAfterLoad { get; private set; }

        public EncounterSnapshot GetSnapshot()
        {
            var snapshot = new EncounterSnapshot
            {
                encounterId = _definition?.encounterId ?? gameObject.name,
                hasStarted = _hasStarted,
                hasCompleted = _hasCompleted,
                lastOutcome = _lastOutcome,
                totalSpawnedCount = _totalSpawnedCount,
                defeatedUnitCount = CountAllSpawnedDefeated(),
                // In-progress encounters have not committed an outcome and dynamic
                // unit HP/position is not serialized — mark for caller awareness.
                needsRestartAfterLoad = _hasStarted && !_hasCompleted,
            };
            snapshot.spawnedWaveIds.Clear();
            foreach (var id in _spawnedWaveIds)
                snapshot.spawnedWaveIds.Add(id);
            return snapshot;
        }

        public void RestoreSnapshot(EncounterSnapshot snapshot)
        {
            if (snapshot == null) return;
            ClearSpawnedUnits();
            CharacterRuntimeComponentGuard.ResetWarnings();
            _spawnedWaveIds.Clear();
            if (snapshot.spawnedWaveIds != null)
                for (int i = 0; i < snapshot.spawnedWaveIds.Count; i++)
                    if (!string.IsNullOrEmpty(snapshot.spawnedWaveIds[i]))
                        _spawnedWaveIds.Add(snapshot.spawnedWaveIds[i]);

            _hasStarted = snapshot.hasStarted;
            _hasCompleted = snapshot.hasCompleted;
            _lastOutcome = snapshot.lastOutcome;
            _totalSpawnedCount = snapshot.totalSpawnedCount;
            NeedsRestartAfterLoad = !_hasCompleted && _hasStarted;

            for (int i = 0; i < _waves.Count; i++)
            {
                var w = _waves[i];
                w.spawned = !string.IsNullOrEmpty(w.waveId) && _spawnedWaveIds.Contains(w.waveId);
            }
            PendingWaveCount = 0;
            for (int i = 0; i < _waves.Count; i++)
                if (_waves[i].IsReady) PendingWaveCount++;
            _waveTimer = 0f;

            string lifecycleTag = _hasCompleted ? "completed"
                : (_hasStarted ? "in-progress (NeedsRestartAfterLoad=true)" : "not-started");
            Debug.Log($"[EncounterRuntime] RestoreSnapshot '{_definition?.encounterId ?? gameObject.name}' state={lifecycleTag} waveIds={_spawnedWaveIds.Count} totalSpawned={_totalSpawnedCount}");
        }

        private int CountAllSpawnedDefeated()
        {
            int count = 0;
            for (int i = 0; i < _spawnedUnits.Count; i++)
            {
                var u = _spawnedUnits[i];
                if (u == null) continue;
                if (u.Entity == null) { count++; continue; }
                if (u.Entity.Data == null || !u.Entity.Data.IsAlive) count++;
            }
            return count;
        }
    }
}

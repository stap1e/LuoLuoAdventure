using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip
{
    public class EncounterRuntime : MonoBehaviour
    {
        [SerializeField] private EncounterDefinition _definition;

        private readonly List<EncounterUnitHandle> _units = new List<EncounterUnitHandle>();

        public EncounterDefinition Definition => _definition;
        public IReadOnlyList<EncounterUnitHandle> Units => _units;

        public void Initialize(EncounterDefinition definition)
        {
            _definition = definition;
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
            return count;
        }

        public void ApplyMissionModifier(MissionModifier modifier)
        {
            if (_definition == null) return;
            _definition.BeastHostilityMultiplier = modifier.BeastHostilityMultiplier;
            _definition.MechaSupportMultiplier = modifier.MechaSupportMultiplier;
            _definition.InitialHostilityOffset = modifier.InitialHostilityOffset;
        }

        public void Clear()
        {
            _units.Clear();
        }
    }
}

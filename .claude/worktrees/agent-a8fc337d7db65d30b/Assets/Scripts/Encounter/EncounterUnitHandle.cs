using UnityEngine;

namespace LuoLuoTrip
{
    public class EncounterUnitHandle
    {
        public CharacterEntity Entity;
        public MainRace Race;
        public bool WasAliveAtStart;

        public bool IsAlive => Entity != null && Entity.Data != null && Entity.Data.IsAlive;

        public static EncounterUnitHandle FromEntity(CharacterEntity entity)
        {
            if (entity == null || entity.Data == null) return null;
            return new EncounterUnitHandle
            {
                Entity = entity,
                Race = GameConstants.GetMainRace(entity.Data.Faction),
                WasAliveAtStart = entity.Data.IsAlive
            };
        }
    }
}

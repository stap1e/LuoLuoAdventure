using System;

namespace LuoLuoTrip
{
    [Serializable]
    public struct CharacterControlInfo
    {
        public string CharacterId;
        public SubFactionId Faction;
        public MainRace Race;
        public CharacterRole Role;
        public int CommandRank;
        public int RequiredCommanderLevel;
        public int TrustToPlayer;
        public bool IsHeroOrLeader;
        public bool AllowDirectControl;
        public bool AllowTacticalCommand;

        public static CharacterControlInfo FromCharacterData(CharacterData data)
        {
            return new CharacterControlInfo
            {
                CharacterId = data.Id,
                Faction = data.Faction,
                Race = data.Race,
                Role = data.Role,
                CommandRank = data.CommandRank,
                RequiredCommanderLevel = data.RequiredCommanderLevel,
                TrustToPlayer = data.TrustToPlayer,
                IsHeroOrLeader = data.Role == CharacterRole.CityLord || data.Role == CharacterRole.WarKing,
                AllowDirectControl = data.AllowDirectControl,
                AllowTacticalCommand = data.AllowTacticalCommand
            };
        }
    }

    [Serializable]
    public class ControlPermissionRequest
    {
        public CommanderProfile Commander;
        public CharacterControlInfo Target;
        public bool IsCrossRaceControl;
        public int CurrentControlledUnitCount;
        public int FactionTrust;
        public int FactionHostility;
    }
}

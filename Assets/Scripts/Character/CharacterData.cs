using System;

namespace LuoLuoTrip
{
    /// <summary>角色运行时数据（纯数据，不依赖 Unity 组件）</summary>
    [Serializable]
    public class CharacterData
    {
        public string Id;
        public string DisplayName;
        public SubFactionId Faction;
        public CharacterRole Role;
        public int Level;
        public bool IsAlive = true;

        public int CommandRank = 1;
        public int RequiredCommanderLevel = 1;
        public int TrustToPlayer;
        public bool IsHeroOrLeader;
        public bool AllowDirectControl = true;
        public bool AllowTacticalCommand = true;

        public MainRace Race => GameConstants.GetMainRace(Faction);

        public CharacterData() { }

        public CharacterData(string id, string displayName, SubFactionId faction, CharacterRole role, int level)
        {
            Id = id;
            DisplayName = displayName;
            Faction = faction;
            Role = role;
            Level = level;
            ApplyRoleDefaults();
        }

        public static CharacterData Create(string id, string displayName, SubFactionId faction, CharacterRole role)
        {
            return new CharacterData(
                id,
                displayName,
                faction,
                role,
                GameConstants.GetInitialLevel(role));
        }

        private void ApplyRoleDefaults()
        {
            switch (Role)
            {
                case CharacterRole.Common:
                    CommandRank = 1;
                    RequiredCommanderLevel = 1;
                    AllowDirectControl = true;
                    AllowTacticalCommand = true;
                    IsHeroOrLeader = false;
                    break;
                case CharacterRole.Minion:
                    CommandRank = 1;
                    RequiredCommanderLevel = 1;
                    AllowDirectControl = true;
                    AllowTacticalCommand = true;
                    IsHeroOrLeader = false;
                    break;
                case CharacterRole.CityLord:
                    CommandRank = 4;
                    RequiredCommanderLevel = 35;
                    AllowDirectControl = false;
                    AllowTacticalCommand = true;
                    IsHeroOrLeader = true;
                    break;
                case CharacterRole.WarKing:
                    CommandRank = 5;
                    RequiredCommanderLevel = 45;
                    AllowDirectControl = false;
                    AllowTacticalCommand = false;
                    IsHeroOrLeader = true;
                    break;
            }
        }
    }
}

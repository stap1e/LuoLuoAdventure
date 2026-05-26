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

        public MainRace Race => GameConstants.GetMainRace(Faction);

        public CharacterData() { }

        public CharacterData(string id, string displayName, SubFactionId faction, CharacterRole role, int level)
        {
            Id = id;
            DisplayName = displayName;
            Faction = faction;
            Role = role;
            Level = level;
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
    }
}

using System;

namespace LuoLuoTrip
{
    /// <summary>子阵营静态定义</summary>
    [Serializable]
    public struct SubFactionDefinition
    {
        public SubFactionId Id;
        public MainRace Race;
        public string DisplayName;
        public string Description;

        public SubFactionDefinition(SubFactionId id, string displayName, string description = "")
        {
            Id = id;
            Race = GameConstants.GetMainRace(id);
            DisplayName = displayName;
            Description = description;
        }
    }
}

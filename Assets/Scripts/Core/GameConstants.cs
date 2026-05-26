namespace LuoLuoTrip
{
    /// <summary>全局常量与关系数值工具</summary>
    public static class GameConstants
    {
        public const int MinRelationshipValue = -100;
        public const int MaxRelationshipValue = 100;

        /// <summary>同族初始友好度</summary>
        public const int DefaultSameRaceRelationship = 60;

        /// <summary>异族初始敌对度</summary>
        public const int DefaultCrossRaceRelationship = -70;

        /// <summary>同阵营（自身）关系</summary>
        public const int SelfRelationship = 100;

        public const int WarKingInitialLevel = 100;
        public const int CityLordInitialLevel = 20;
        public const int CommonInitialLevel = 1;

        public const int MotorSubFactionCount = 6;
        public const int BeastSubFactionCount = 3;
        public const int TotalSubFactionCount = MotorSubFactionCount + BeastSubFactionCount;

        public static bool IsMotorSubFaction(SubFactionId id) =>
            (int)id >= 0 && (int)id < MotorSubFactionCount;

        public static bool IsBeastSubFaction(SubFactionId id) =>
            (int)id >= MotorSubFactionCount && (int)id < TotalSubFactionCount;

        public static MainRace GetMainRace(SubFactionId id) =>
            IsMotorSubFaction(id) ? MainRace.MotorTribe : MainRace.BeastTribe;

        public static int GetInitialLevel(CharacterRole role) => role switch
        {
            CharacterRole.WarKing => WarKingInitialLevel,
            CharacterRole.CityLord => CityLordInitialLevel,
            _ => CommonInitialLevel
        };

        public static RelationshipStance ValueToStance(int value)
        {
            if (value <= -60) return RelationshipStance.Hostile;
            if (value <= -20) return RelationshipStance.Unfriendly;
            if (value <= 20) return RelationshipStance.Neutral;
            if (value <= 60) return RelationshipStance.Friendly;
            return RelationshipStance.Allied;
        }

        public static int CreateDefaultRelationship(SubFactionId source, SubFactionId target)
        {
            if (source == target) return SelfRelationship;
            return GetMainRace(source) == GetMainRace(target)
                ? DefaultSameRaceRelationship
                : DefaultCrossRaceRelationship;
        }
    }
}

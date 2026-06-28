namespace LuoLuoTrip
{
    /// <summary>角色身份/职位，决定初始等级与阵营规则</summary>
    public enum CharacterRole
    {
        /// <summary>普通角色 / 玩家，初始 1 级</summary>
        Common = 0,

        /// <summary>小兵，初始 1 级</summary>
        Minion = 1,

        /// <summary>城主（机车族阵营领袖），初始 20 级</summary>
        CityLord = 2,

        /// <summary>战王（猛兽族阵营领袖），初始 100 级</summary>
        WarKing = 3
    }
}

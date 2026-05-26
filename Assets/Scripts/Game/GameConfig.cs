using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>可在 Inspector 中调节的阵营/等级初始配置</summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "LuoLuoTrip/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Relationship Defaults")]
        [Range(-100, 100)] public int sameRaceRelationship = GameConstants.DefaultSameRaceRelationship;
        [Range(-100, 100)] public int crossRaceRelationship = GameConstants.DefaultCrossRaceRelationship;

        [Header("Level Defaults")]
        public int warKingLevel = GameConstants.WarKingInitialLevel;
        public int cityLordLevel = GameConstants.CityLordInitialLevel;
        public int commonLevel = GameConstants.CommonInitialLevel;

        [Header("World Spawn")]
        public bool spawnMinionSquads = true;
        [Min(0)] public int minionsPerFaction = 5;
    }
}

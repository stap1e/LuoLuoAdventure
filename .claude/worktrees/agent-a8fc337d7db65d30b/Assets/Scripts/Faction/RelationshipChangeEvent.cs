using System;

namespace LuoLuoTrip
{
    /// <summary>阵营关系变更事件，供运营/剧情系统订阅</summary>
    [Serializable]
    public struct RelationshipChangeEvent
    {
        public SubFactionId Source;
        public SubFactionId Target;
        public int OldValue;
        public int NewValue;
        public string Reason;

        public RelationshipStance OldStance => GameConstants.ValueToStance(OldValue);
        public RelationshipStance NewStance => GameConstants.ValueToStance(NewValue);
        public bool StanceChanged => OldStance != NewStance;
    }
}

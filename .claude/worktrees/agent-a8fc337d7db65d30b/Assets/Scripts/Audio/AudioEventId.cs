namespace LuoLuoTrip.Audio
{
    public enum AudioEventId
    {
        None = 0,
        AttackStart,
        Hit,
        Dodge,
        Stagger,
        DeniedControl,
        DirectControlSuccess,
        TacticalCommandIssued,
        SyncAssistActive,
        MissionComplete,
        MissionFailed,
        LevelUp,
        FactionDelta,
        AIWindupWarning
    }
}

using System;

namespace LuoLuoTrip
{
    [Serializable]
    public struct ControlPermissionResult
    {
        public ControlMode Mode;
        public float SyncRate;
        public string Reason;
        public bool IsAllowed;

        public static ControlPermissionResult DeniedResult(string reason) =>
            new ControlPermissionResult
            {
                Mode = ControlMode.Denied,
                SyncRate = 0f,
                Reason = reason,
                IsAllowed = false
            };

        public static ControlPermissionResult SyncAssistResult(float syncRate, string reason) =>
            new ControlPermissionResult
            {
                Mode = ControlMode.SyncAssist,
                SyncRate = syncRate,
                Reason = reason,
                IsAllowed = true
            };

        public static ControlPermissionResult TacticalCommandResult(float syncRate, string reason) =>
            new ControlPermissionResult
            {
                Mode = ControlMode.TacticalCommand,
                SyncRate = syncRate,
                Reason = reason,
                IsAllowed = true
            };

        public static ControlPermissionResult DirectControlResult(float syncRate, string reason) =>
            new ControlPermissionResult
            {
                Mode = ControlMode.DirectControl,
                SyncRate = syncRate,
                Reason = reason,
                IsAllowed = true
            };
    }
}

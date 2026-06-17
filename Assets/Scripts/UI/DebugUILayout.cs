using UnityEngine;

namespace LuoLuoTrip.UI
{
    public static class DebugUILayout
    {
        // Layout zones (left side, top-down):
        //   MissionObjective : 10..210
        //   ControlHint      : 220..340
        //   CommanderHud     : 350..650
        // Right side:
        //   MissionResultDebug : 10..150
        //   FactionStanding    : 160..560 (anchored to right edge)
        //   FactionDeltaToast  : drifts on top of right column
        public static readonly Rect MissionObjective = new Rect(10, 10, 400, 200);
        public static readonly Rect ControlHint = new Rect(10, 220, 400, 120);
        public static readonly Rect CommanderHud = new Rect(10, 350, 320, 300);
        public static readonly Rect MissionResultSummary = new Rect(Screen.width / 2 - 200, 80, 400, 400);
        public static readonly Rect MissionChainSummary = new Rect(10, 660, 400, 250);
        public static readonly Rect FactionStanding = new Rect(Screen.width - 330, 160, 320, 400);
        public static readonly Rect FactionDeltaToast = new Rect(Screen.width - 320, 100, 300, 300);
        public static readonly Rect Tutorial = new Rect(Screen.width / 2 - 200, 30, 400, 60);
        public static readonly Rect MissionResultDebug = new Rect(Screen.width - 330, 10, 320, 140);

        public static Rect WithOffset(Rect baseRect, float x, float y)
        {
            return new Rect(baseRect.x + x, baseRect.y + y, baseRect.width, baseRect.height);
        }
    }
}

using UnityEngine;

namespace LuoLuoTrip.UI
{
    public static class DebugUILayout
    {
        public const int CompactWidthThreshold = 1024;

        private const float Margin = 10f;
        private const float Gap = 10f;
        private const float DefaultLeftWidth = 410f;
        private const float DefaultRightWidth = 390f;
        private const float CompactWidth = 300f;

        public static Rect DemoFlow => GetDemoFlowRect(Screen.width, Screen.height);
        public static Rect DemoShortcutHelp => GetDemoShortcutHelpRect(Screen.width, Screen.height);
        public static Rect MissionObjective => GetMissionObjectiveRect(Screen.width, Screen.height);
        public static Rect ControlHint => GetControlHintRect(Screen.width, Screen.height);
        public static Rect CommanderHud => GetCommanderHudRect(Screen.width, Screen.height);
        public static Rect MissionResultSummary => GetMissionResultSummaryRect(Screen.width, Screen.height);
        public static Rect MissionOutcomePreview => GetMissionOutcomePreviewRect(Screen.width, Screen.height);
        public static Rect MissionChainSummary => GetMissionChainSummaryRect(Screen.width, Screen.height);
        public static Rect FactionStanding => GetFactionStandingRect(Screen.width, Screen.height);
        public static Rect FactionDeltaToast => GetFactionDeltaToastRect(Screen.width, Screen.height);
        public static Rect Tutorial => GetTutorialRect(Screen.width, Screen.height);
        public static Rect MissionResultDebug => GetMissionResultDebugRect(Screen.width, Screen.height);

        public static bool IsCompact(float screenWidth) => screenWidth < CompactWidthThreshold;

        public static Rect GetDemoFlowRect(float screenWidth, float screenHeight)
        {
            var compact = IsCompact(screenWidth);
            return new Rect(Margin, Margin, compact ? CompactWidth : DefaultLeftWidth, compact ? 118f : 132f);
        }

        public static Rect GetDemoShortcutHelpRect(float screenWidth, float screenHeight)
        {
            var flow = GetDemoFlowRect(screenWidth, screenHeight);
            return new Rect(flow.x, flow.y + flow.height + Gap, flow.width, IsCompact(screenWidth) ? 92f : 132f);
        }

        public static Rect GetMissionObjectiveRect(float screenWidth, float screenHeight)
        {
            var compact = IsCompact(screenWidth);
            var shortcut = GetDemoShortcutHelpRect(screenWidth, screenHeight);
            return new Rect(Margin, shortcut.y + shortcut.height + Gap, compact ? CompactWidth : DefaultLeftWidth, compact ? 176f : 220f);
        }

        public static Rect GetControlHintRect(float screenWidth, float screenHeight)
        {
            if (IsCompact(screenWidth))
                return new Rect(Margin, 408f, CompactWidth, 126f);

            var x = Mathf.Max(Margin, screenWidth - DefaultRightWidth - Margin);
            return new Rect(x, Margin, DefaultRightWidth, 142f);
        }

        public static Rect GetCommanderHudRect(float screenWidth, float screenHeight)
        {
            if (IsCompact(screenWidth))
                return new Rect(Margin, 544f, CompactWidth, 220f);

            var hint = GetControlHintRect(screenWidth, screenHeight);
            return new Rect(hint.x, hint.y + hint.height + Gap, DefaultRightWidth, 300f);
        }

        public static Rect GetMissionResultSummaryRect(float screenWidth, float screenHeight)
        {
            var compact = IsCompact(screenWidth);
            var width = compact ? CompactWidth : DefaultRightWidth;
            var x = compact ? Mathf.Max(Margin, screenWidth - width - Margin) : Mathf.Max(Margin, screenWidth - width - Margin);
            if (compact)
                return new Rect(x, 408f, width, 260f);

            var commander = GetCommanderHudRect(screenWidth, screenHeight);
            var y = commander.y + commander.height + Gap;
            var height = Mathf.Max(220f, Mathf.Min(330f, screenHeight - y - Margin));
            return new Rect(x, y, width, height);
        }

        public static Rect GetMissionChainSummaryRect(float screenWidth, float screenHeight)
        {
            var preview = GetMissionOutcomePreviewRect(screenWidth, screenHeight);
            return new Rect(preview.x, preview.y + preview.height + Gap, preview.width, IsCompact(screenWidth) ? 120f : 180f);
        }

        public static Rect GetMissionOutcomePreviewRect(float screenWidth, float screenHeight)
        {
            var objective = GetMissionObjectiveRect(screenWidth, screenHeight);
            var compact = IsCompact(screenWidth);
            var height = compact ? 142f : 176f;
            return new Rect(objective.x, objective.y + objective.height + Gap, objective.width, height);
        }

        public static Rect GetFactionStandingRect(float screenWidth, float screenHeight)
        {
            var summary = GetMissionResultSummaryRect(screenWidth, screenHeight);
            return new Rect(summary.x, summary.y + summary.height + Gap, summary.width, 180f);
        }

        public static Rect GetFactionDeltaToastRect(float screenWidth, float screenHeight)
        {
            var commander = GetCommanderHudRect(screenWidth, screenHeight);
            return new Rect(commander.x, commander.y + 40f, commander.width, 180f);
        }

        public static Rect GetTutorialRect(float screenWidth, float screenHeight)
        {
            var width = IsCompact(screenWidth) ? CompactWidth : 400f;
            return new Rect(Mathf.Max(Margin, screenWidth / 2f - width / 2f), 30f, width, 60f);
        }

        public static Rect GetMissionResultDebugRect(float screenWidth, float screenHeight)
        {
            var summary = GetMissionResultSummaryRect(screenWidth, screenHeight);
            return new Rect(summary.x, summary.y - 150f, summary.width, 140f);
        }

        public static Rect WithOffset(Rect baseRect, float x, float y)
        {
            return new Rect(baseRect.x + x, baseRect.y + y, baseRect.width, baseRect.height);
        }

        public static bool OverlapsHeavily(Rect a, Rect b, float allowedOverlapRatio = 0.2f)
        {
            var xOverlap = Mathf.Max(0f, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin));
            var yOverlap = Mathf.Max(0f, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
            var overlapArea = xOverlap * yOverlap;
            if (overlapArea <= 0f) return false;
            var smallerArea = Mathf.Min(a.width * a.height, b.width * b.height);
            return smallerArea > 0f && overlapArea / smallerArea > allowedOverlapRatio;
        }
    }
}

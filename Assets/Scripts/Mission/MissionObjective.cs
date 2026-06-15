using System;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionObjective
    {
        public string ObjectiveId;
        public string Description;
        public bool IsCompleted;
        public bool IsFailed;
        public int Progress;
        public int RequiredProgress = 1;

        public float CompletionRatio => RequiredProgress > 0
            ? (float)Progress / RequiredProgress
            : 0f;
    }
}

using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip.AI
{
    public struct AITargetScore
    {
        public Combatant Target;
        public float Score;
        public string Reason;
        public bool IsValid;

        public static AITargetScore Invalid(string reason)
        {
            return new AITargetScore { Target = null, Score = float.MinValue, Reason = reason, IsValid = false };
        }

        public static AITargetScore Valid(Combatant target, float score, string reason)
        {
            return new AITargetScore { Target = target, Score = score, Reason = reason, IsValid = true };
        }
    }
}

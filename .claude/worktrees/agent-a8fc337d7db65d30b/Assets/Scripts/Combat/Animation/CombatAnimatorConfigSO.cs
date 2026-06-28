using UnityEngine;

namespace LuoLuoTrip.Combat.Animation
{
    /// <summary>Animator 参数名配置，便于对接不同 Animator Controller</summary>
    [CreateAssetMenu(fileName = "CombatAnimatorConfig", menuName = "LuoLuoTrip/Combat Animator Config")]
    public class CombatAnimatorConfigSO : ScriptableObject
    {
        [Header("Float Parameters")]
        public string moveSpeedParam = "MoveSpeed";

        [Header("Trigger Parameters")]
        public string attackTrigger = "Attack";
        public string dodgeTrigger = "Dodge";
        public string staggerTrigger = "Stagger";
        public string hitLightTrigger = "HitLight";
        public string hitHeavyTrigger = "HitHeavy";
        public string deathTrigger = "Death";

        [Header("Bool Parameters")]
        public string isDeadBool = "IsDead";

        [Header("Cross-Fade (Optional State Names)")]
        public string idleState = "Idle";
        public string moveState = "Move";
        public float crossFadeDuration = 0.1f;
    }
}

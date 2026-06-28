using UnityEngine;

namespace LuoLuoTrip.Combat.Feedback
{
    /// <summary>受击反馈参数配置</summary>
    [CreateAssetMenu(fileName = "HitFeedbackProfile", menuName = "LuoLuoTrip/Hit Feedback Profile")]
    public class HitFeedbackProfileSO : ScriptableObject
    {
        [Header("Hit Stop (卡肉)")]
        [Range(0f, 0.3f)] public float lightHitStopDuration = 0.04f;
        [Range(0f, 0.3f)] public float heavyHitStopDuration = 0.08f;
        [Range(0f, 0.5f)] public float fatalHitStopDuration = 0.12f;
        [Range(0f, 1f)] public float hitStopTimeScale = 0.05f;

        [Header("Camera Shake")]
        [Range(0f, 1f)] public float lightShakeAmplitude = 0.08f;
        [Range(0f, 0.5f)] public float lightShakeDuration = 0.1f;
        [Range(0f, 1.5f)] public float heavyShakeAmplitude = 0.2f;
        [Range(0f, 0.5f)] public float heavyShakeDuration = 0.18f;
        [Range(1f, 3f)] public float poiseBreakShakeMultiplier = 1.5f;
        [Range(1f, 3f)] public float fatalShakeMultiplier = 2f;

        [Header("Thresholds")]
        public float heavyDamagePercent = 0.15f;
    }
}

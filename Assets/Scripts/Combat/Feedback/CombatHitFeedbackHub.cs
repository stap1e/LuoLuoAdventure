using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Combat.Feedback
{
    /// <summary>
    /// 战斗受击反馈中枢：监听命中事件，触发卡肉 + 屏幕震动。
    /// 挂载到场景常驻对象（如 GameBootstrap）。
    /// </summary>
    public class CombatHitFeedbackHub : MonoBehaviour
    {
        public static CombatHitFeedbackHub Instance { get; private set; }

        [SerializeField] private HitFeedbackProfileSO _profile;
        [SerializeField] private bool _onlyShakeWhenPlayerInvolved = true;

        private readonly HashSet<Combatant> _registered = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _profile = _profile ?? ScriptableObject.CreateInstance<HitFeedbackProfileSO>();
            EnsureServices();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            foreach (var combatant in FindObjectsOfType<Combatant>())
                Register(combatant);
        }

        public void Register(Combatant combatant)
        {
            if (combatant == null || !_registered.Add(combatant)) return;
            combatant.OnHitLanded += HandleHitLanded;
        }

        public void Unregister(Combatant combatant)
        {
            if (combatant == null || !_registered.Remove(combatant)) return;
            combatant.OnHitLanded -= HandleHitLanded;
        }

        private void HandleHitLanded(CombatHitEvent hit)
        {
            if (hit.Result.finalDamage <= 0f) return;
            if (_onlyShakeWhenPlayerInvolved && !IsPlayerInvolved(hit)) return;

            var intensity = EvaluateIntensity(hit);
            ApplyHitStop(intensity);
            ApplyCameraShake(intensity, hit.Result.wasPoiseBroken);
        }

        private void ApplyHitStop(HitIntensity intensity)
        {
            var hitStop = HitStopService.Instance;
            if (hitStop == null) return;

            var duration = intensity switch
            {
                HitIntensity.Fatal => _profile.fatalHitStopDuration,
                HitIntensity.Heavy => _profile.heavyHitStopDuration,
                _ => _profile.lightHitStopDuration
            };

            hitStop.Play(duration, _profile.hitStopTimeScale);
        }

        private void ApplyCameraShake(HitIntensity intensity, bool poiseBroken)
        {
            var shake = CameraShakeService.Instance;
            if (shake == null) return;

            float duration;
            float amplitude;

            switch (intensity)
            {
                case HitIntensity.Fatal:
                    duration = _profile.heavyShakeDuration * 1.2f;
                    amplitude = _profile.heavyShakeAmplitude * _profile.fatalShakeMultiplier;
                    break;
                case HitIntensity.Heavy:
                    duration = _profile.heavyShakeDuration;
                    amplitude = _profile.heavyShakeAmplitude;
                    if (poiseBroken)
                        amplitude *= _profile.poiseBreakShakeMultiplier;
                    break;
                default:
                    duration = _profile.lightShakeDuration;
                    amplitude = _profile.lightShakeAmplitude;
                    break;
            }

            shake.Shake(duration, amplitude);
        }

        private enum HitIntensity { Light, Heavy, Fatal }

        private HitIntensity EvaluateIntensity(CombatHitEvent hit)
        {
            if (hit.Result.wasFatal) return HitIntensity.Fatal;
            if (hit.Result.wasPoiseBroken) return HitIntensity.Heavy;

            var maxHp = hit.Defender.Stats.maxHealth;
            if (maxHp > 0f && hit.Result.finalDamage / maxHp >= _profile.heavyDamagePercent)
                return HitIntensity.Heavy;

            return HitIntensity.Light;
        }

        private static bool IsPlayerInvolved(CombatHitEvent hit)
        {
            return HasPlayerController(hit.Attacker) || HasPlayerController(hit.Defender);
        }

        private static bool HasPlayerController(Combatant combatant) =>
            combatant != null && combatant.GetComponent<CombatController>() != null;

        private void EnsureServices()
        {
            if (HitStopService.Instance == null)
                gameObject.AddComponent<HitStopService>();

            if (CameraShakeService.Instance == null)
            {
                var cam = Camera.main;
                if (cam != null)
                    cam.gameObject.AddComponent<CameraShakeService>();
            }
        }
    }
}

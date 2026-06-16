using UnityEngine;

namespace LuoLuoTrip.Combat.Feedback
{
    /// <summary>全局 Hit Stop（卡肉）服务，使用 unscaledTime 计时</summary>
    public class HitStopService : MonoBehaviour
    {
        public static HitStopService Instance { get; private set; }

        private float _remaining;
        private float _activeTimeScale = 1f;
        private float _defaultFixedDelta = 0.02f;
        private bool _isActive;

        public bool IsActive => _isActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            _defaultFixedDelta = Time.fixedDeltaTime;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                RestoreTime();
                Instance = null;
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0f)
                RestoreTime();
        }

        /// <summary>触发卡肉。较长 duration 会覆盖较短 duration。</summary>
        public void Play(float duration, float timeScale)
        {
            if (duration <= 0f) return;
            if (_isActive && duration <= _remaining) return;

            _remaining = duration;
            _activeTimeScale = Mathf.Clamp(timeScale, 0f, 1f);
            _isActive = true;

            Time.timeScale = _activeTimeScale;
            Time.fixedDeltaTime = _defaultFixedDelta * _activeTimeScale;
        }

        public void RestoreTime()
        {
            _isActive = false;
            _remaining = 0f;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _defaultFixedDelta;
        }
    }
}

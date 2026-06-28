using UnityEngine;

namespace LuoLuoTrip.Combat.Feedback
{
    /// <summary>
    /// 主摄像机屏幕震动。宿主: Main Camera (共享对象)。
    /// 重复服务只销毁组件本身，不能销毁 Main Camera 宿主。
    /// 由 CombatHitFeedbackHub.EnsureServices() 在运行时 AddComponent，不要在编辑器序列化到场景。
    /// </summary>
    public class CameraShakeService : MonoBehaviour
    {
        private static CameraShakeService _instance;
        public static CameraShakeService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<CameraShakeService>();
                return _instance;
            }
            private set => _instance = value;
        }

        [SerializeField] private Camera _camera;
        [SerializeField] private bool _shakeRotation = true;

        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;
        private float _remaining;
        private float _amplitude;
        private float _frequency = 35f;
        private float _seed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            _camera = _camera ?? GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;

            if (_camera != null)
            {
                _originalLocalPosition = _camera.transform.localPosition;
                _originalLocalRotation = _camera.transform.localRotation;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void LateUpdate()
        {
            if (_camera == null || _remaining <= 0f) return;

            _remaining -= Time.unscaledDeltaTime;

            var decay = Mathf.Clamp01(_remaining / 0.2f);
            var shake = _amplitude * decay;
            var noise = (Mathf.PerlinNoise(_seed, Time.unscaledTime * _frequency) - 0.5f) * 2f;

            _camera.transform.localPosition = _originalLocalPosition + new Vector3(
                noise * shake,
                (Mathf.PerlinNoise(_seed + 1f, Time.unscaledTime * _frequency) - 0.5f) * 2f * shake,
                0f);

            if (_shakeRotation)
            {
                var rotZ = noise * shake * 4f;
                _camera.transform.localRotation = _originalLocalRotation * Quaternion.Euler(0f, 0f, rotZ);
            }

            if (_remaining <= 0f)
                ResetCamera();
        }

        public void Shake(float duration, float amplitude)
        {
            if (_camera == null || duration <= 0f || amplitude <= 0f) return;

            if (_remaining <= 0f)
            {
                _originalLocalPosition = _camera.transform.localPosition;
                _originalLocalRotation = _camera.transform.localRotation;
            }

            _remaining = Mathf.Max(_remaining, duration);
            _amplitude = Mathf.Max(_amplitude, amplitude);
            _seed = Random.value * 100f;
        }

        public void ResetCamera()
        {
            if (_camera == null) return;
            _camera.transform.localPosition = _originalLocalPosition;
            _camera.transform.localRotation = _originalLocalRotation;
            _remaining = 0f;
            _amplitude = 0f;
        }
    }
}

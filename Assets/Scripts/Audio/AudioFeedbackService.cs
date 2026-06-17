using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Audio
{
    /// <summary>
    /// 音频反馈服务。宿主: 专用 GameObject [AudioFeedbackService]。
    /// Destroy(this) 安全——只删除重复组件，专用 GO 上无其他关键组件。
    /// </summary>
    public class AudioFeedbackService : MonoBehaviour
    {
        [SerializeField] private AudioFeedbackProfileSO _profile;
        [SerializeField] private int _voicePoolSize = 12;
        [SerializeField] private float _defaultMinInterval = 0.05f;

        private readonly List<AudioSource> _pool = new List<AudioSource>();
        private readonly Dictionary<AudioEventId, float> _lastPlayedAt = new Dictionary<AudioEventId, float>();
        private readonly Dictionary<AudioEventId, float> _throttleByEvent = new Dictionary<AudioEventId, float>();
        private AudioSource _uiSource;

        private static AudioFeedbackService _instance;
        public static AudioFeedbackService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<AudioFeedbackService>();
                return _instance;
            }
        }

        public AudioFeedbackProfileSO Profile => _profile;

        public void SetProfile(AudioFeedbackProfileSO profile) => _profile = profile;

        public void SetThrottle(AudioEventId id, float minInterval)
        {
            _throttleByEvent[id] = minInterval;
        }

        public bool TryPlay(AudioEventId id, Vector3 position)
        {
            if (!ShouldPlay(id)) return false;
            var clip = _profile != null ? _profile.PickClip(id) : null;
            if (clip == null) return false;

            var src = GetFreeSource();
            src.transform.position = position;
            src.spatialBlend = _profile.IsSpatial(id) ? 1f : 0f;
            src.clip = clip;
            src.volume = _profile.GetVolume(id);
            src.pitch = _profile.PickPitch(id);
            src.Play();
            return true;
        }

        public bool TryPlayUI(AudioEventId id)
        {
            if (!ShouldPlay(id)) return false;
            var clip = _profile != null ? _profile.PickClip(id) : null;
            if (clip == null) return false;

            EnsureUiSource();
            _uiSource.PlayOneShot(clip, _profile.GetVolume(id));
            return true;
        }

        public static void Play(AudioEventId id, Vector3 position)
        {
            if (_instance == null) return;
            _instance.TryPlay(id, position);
        }

        public static void PlayUI(AudioEventId id)
        {
            if (_instance == null) return;
            _instance.TryPlayUI(id);
        }

        public static AudioFeedbackService EnsureInstance()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[AudioFeedbackService]");
            _instance = go.AddComponent<AudioFeedbackService>();
            _instance._profile = LoadDefaultProfile();
            return _instance;
        }

        public static AudioFeedbackProfileSO LoadDefaultProfile()
        {
            return Resources.Load<AudioFeedbackProfileSO>("AudioFeedbackProfile");
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;
            if (_profile == null)
                _profile = LoadDefaultProfile();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private bool ShouldPlay(AudioEventId id)
        {
            if (_profile == null)
            {
                LogMissingProfileOnce();
                return false;
            }
            if (!_profile.HasEntry(id))
                return false;

            var minInterval = _throttleByEvent.TryGetValue(id, out var custom)
                ? custom
                : _defaultMinInterval;

            if (_lastPlayedAt.TryGetValue(id, out var last))
            {
                if (Time.unscaledTime - last < minInterval) return false;
            }
            _lastPlayedAt[id] = Time.unscaledTime;
            return true;
        }

        private AudioSource GetFreeSource()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null && !_pool[i].isPlaying)
                    return _pool[i];
            }
            if (_pool.Count >= _voicePoolSize && _pool.Count > 0)
                return _pool[0];

            var go = new GameObject($"AudioVoice_{_pool.Count}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            _pool.Add(src);
            return src;
        }

        private void EnsureUiSource()
        {
            if (_uiSource != null) return;
            var go = new GameObject("AudioVoice_UI");
            go.transform.SetParent(transform, false);
            _uiSource = go.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.spatialBlend = 0f;
        }

        private bool _profileWarningLogged;
        private void LogMissingProfileOnce()
        {
            if (_profileWarningLogged) return;
            _profileWarningLogged = true;
            Debug.LogWarning("[AudioFeedbackService] No AudioFeedbackProfile assigned/loaded - audio events will be silent.");
        }
    }
}

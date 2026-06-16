using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Audio
{
    [CreateAssetMenu(fileName = "AudioFeedbackProfile", menuName = "LuoLuoTrip/Audio Feedback Profile")]
    public class AudioFeedbackProfileSO : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public AudioEventId eventId = AudioEventId.None;
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.1f, 3f)] public float pitchMin = 1f;
            [Range(0.1f, 3f)] public float pitchMax = 1f;
            public bool spatial = true;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        private Dictionary<AudioEventId, Entry> _lookup;

        public IReadOnlyList<Entry> Entries => _entries;

        public Entry GetEntry(AudioEventId id)
        {
            BuildLookupIfNeeded();
            return _lookup.TryGetValue(id, out var entry) ? entry : null;
        }

        public bool HasEntry(AudioEventId id)
        {
            BuildLookupIfNeeded();
            return _lookup.ContainsKey(id);
        }

        public AudioClip PickClip(AudioEventId id)
        {
            var entry = GetEntry(id);
            if (entry == null || entry.clips == null || entry.clips.Length == 0) return null;
            var idx = Random.Range(0, entry.clips.Length);
            return entry.clips[idx];
        }

        public float PickPitch(AudioEventId id)
        {
            var entry = GetEntry(id);
            if (entry == null) return 1f;
            return Random.Range(entry.pitchMin, entry.pitchMax);
        }

        public float GetVolume(AudioEventId id)
        {
            var entry = GetEntry(id);
            return entry == null ? 1f : entry.volume;
        }

        public bool IsSpatial(AudioEventId id)
        {
            var entry = GetEntry(id);
            return entry?.spatial ?? true;
        }

        public void EnsureAllEvents()
        {
            BuildLookupIfNeeded();
            foreach (AudioEventId id in System.Enum.GetValues(typeof(AudioEventId)))
            {
                if (id == AudioEventId.None) continue;
                if (_lookup.ContainsKey(id)) continue;
                var entry = new Entry { eventId = id };
                _entries.Add(entry);
                _lookup[id] = entry;
            }
        }

        public void RebuildLookup()
        {
            _lookup = null;
            BuildLookupIfNeeded();
        }

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<AudioEventId, Entry>();
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (e == null) continue;
                _lookup[e.eventId] = e;
            }
        }

        private void OnValidate()
        {
            _lookup = null;
        }
    }
}

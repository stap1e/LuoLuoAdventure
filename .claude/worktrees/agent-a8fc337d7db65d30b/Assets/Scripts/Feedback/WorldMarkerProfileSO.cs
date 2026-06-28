using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Feedback
{
    [CreateAssetMenu(fileName = "WorldMarkerProfile", menuName = "LuoLuoTrip/World Marker Profile")]
    public class WorldMarkerProfileSO : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public WorldMarkerType type;
            public string label = "";
            public Color color = Color.white;
            public float worldOffsetY = 2.2f;
            public bool showLabel = true;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        private Dictionary<WorldMarkerType, Entry> _lookup;

        public Entry GetEntry(WorldMarkerType type)
        {
            BuildLookupIfNeeded();
            if (_lookup.TryGetValue(type, out var e)) return e;
            return DefaultEntry(type);
        }

        public void EnsureAllTypes()
        {
            BuildLookupIfNeeded();
            foreach (WorldMarkerType t in System.Enum.GetValues(typeof(WorldMarkerType)))
            {
                if (t == WorldMarkerType.None) continue;
                if (_lookup.ContainsKey(t)) continue;
                var def = DefaultEntry(t);
                _entries.Add(def);
                _lookup[t] = def;
            }
        }

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<WorldMarkerType, Entry>();
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (e == null) continue;
                _lookup[e.type] = e;
            }
        }

        private void OnValidate()
        {
            _lookup = null;
        }

        public static Entry DefaultEntry(WorldMarkerType type)
        {
            switch (type)
            {
                case WorldMarkerType.SelectedCommanderTarget:
                    return new Entry { type = type, label = "[TARGET]", color = new Color(1f, 0.85f, 0.2f), worldOffsetY = 2.5f };
                case WorldMarkerType.LockOnTarget:
                    return new Entry { type = type, label = "[LOCK]", color = new Color(1f, 0.4f, 0.4f), worldOffsetY = 2.5f };
                case WorldMarkerType.MissionObjective:
                    return new Entry { type = type, label = "[OBJ]", color = new Color(0.3f, 1f, 0.5f), worldOffsetY = 2.8f };
                case WorldMarkerType.Interactable:
                    return new Entry { type = type, label = "[E]", color = new Color(0.5f, 1f, 0.9f), worldOffsetY = 2.4f };
                case WorldMarkerType.AIWindupWarning:
                    return new Entry { type = type, label = "[!]", color = new Color(1f, 0.2f, 0.2f), worldOffsetY = 2.6f };
                case WorldMarkerType.HostileUnit:
                    return new Entry { type = type, label = "[HOSTILE]", color = new Color(1f, 0.3f, 0.3f), worldOffsetY = 2.2f, showLabel = false };
                case WorldMarkerType.FriendlyUnit:
                    return new Entry { type = type, label = "[FRIEND]", color = new Color(0.4f, 0.8f, 1f), worldOffsetY = 2.2f, showLabel = false };
                case WorldMarkerType.ControlledUnit:
                    return new Entry { type = type, label = "[YOU]", color = new Color(0.2f, 0.7f, 1f), worldOffsetY = 2.5f };
                case WorldMarkerType.SyncAssistActive:
                    return new Entry { type = type, label = "[SYNC]", color = new Color(0.8f, 1f, 0.2f), worldOffsetY = 2.6f };
                default:
                    return new Entry { type = type, label = "", color = Color.white };
            }
        }
    }
}

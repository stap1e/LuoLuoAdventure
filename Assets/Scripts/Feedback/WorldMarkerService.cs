using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Feedback
{
    public class WorldMarkerService : MonoBehaviour
    {
        [SerializeField] private WorldMarkerProfileSO _profile;
        [SerializeField] private bool _markersEnabled = true;
        [SerializeField] private int _fontSize = 14;

        private static WorldMarkerService _instance;
        public static WorldMarkerService Instance => _instance;

        private readonly List<WorldMarker> _markers = new List<WorldMarker>();

        public WorldMarkerProfileSO Profile => _profile;
        public bool MarkersEnabled
        {
            get => _markersEnabled;
            set => _markersEnabled = value;
        }

        public IReadOnlyList<WorldMarker> Markers => _markers;

        public void SetProfile(WorldMarkerProfileSO profile) => _profile = profile;

        public static WorldMarkerService EnsureInstance()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[WorldMarkerService]");
            _instance = go.AddComponent<WorldMarkerService>();
            if (_instance._profile == null)
                _instance._profile = LoadDefaultProfile();
            return _instance;
        }

        public static WorldMarkerProfileSO LoadDefaultProfile()
        {
            return Resources.Load<WorldMarkerProfileSO>("WorldMarkerProfile");
        }

        public WorldMarker AttachMarker(GameObject host, WorldMarkerType type, string customLabel = null)
        {
            if (host == null) return null;
            var marker = host.GetComponent<WorldMarker>();
            if (marker == null)
                marker = host.AddComponent<WorldMarker>();
            marker.Configure(type, host.transform, customLabel);
            Register(marker);
            return marker;
        }

        public void DetachMarker(GameObject host)
        {
            if (host == null) return;
            var marker = host.GetComponent<WorldMarker>();
            if (marker == null) return;
            Unregister(marker);
            Destroy(marker);
        }

        public void Register(WorldMarker marker)
        {
            if (marker == null) return;
            if (!_markers.Contains(marker))
                _markers.Add(marker);
        }

        public void Unregister(WorldMarker marker)
        {
            if (marker == null) return;
            _markers.Remove(marker);
        }

        public int CountByType(WorldMarkerType type)
        {
            int n = 0;
            for (int i = 0; i < _markers.Count; i++)
            {
                if (_markers[i] != null && _markers[i].Type == type) n++;
            }
            return n;
        }

        public bool HasMarker(GameObject host, WorldMarkerType type)
        {
            if (host == null) return false;
            var marker = host.GetComponent<WorldMarker>();
            return marker != null && marker.Type == type;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            if (_profile == null)
                _profile = LoadDefaultProfile();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            _markers.Clear();
        }

        private void OnGUI()
        {
            if (!_markersEnabled) return;
            if (_markers.Count == 0) return;

            var cam = Camera.main;
            if (cam == null) return;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            for (int i = _markers.Count - 1; i >= 0; i--)
            {
                var marker = _markers[i];
                if (marker == null)
                {
                    _markers.RemoveAt(i);
                    continue;
                }
                if (!marker.IsVisible) continue;
                if (marker.Target == null) continue;

                var entry = _profile != null
                    ? _profile.GetEntry(marker.Type)
                    : WorldMarkerProfileSO.DefaultEntry(marker.Type);
                if (entry == null) continue;
                if (!entry.showLabel && string.IsNullOrEmpty(marker.CustomLabel)) continue;

                var worldPos = marker.GetWorldPosition() + Vector3.up * entry.worldOffsetY;
                var screenPos = cam.WorldToScreenPoint(worldPos);
                if (screenPos.z <= 0f) continue;

                var label = !string.IsNullOrEmpty(marker.CustomLabel) ? marker.CustomLabel : entry.label;
                style.normal.textColor = entry.color;

                var rect = new Rect(screenPos.x - 60f, Screen.height - screenPos.y - 12f, 120f, 24f);
                GUI.Label(rect, label, style);
            }
        }
    }
}

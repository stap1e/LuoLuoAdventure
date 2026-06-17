using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Feedback
{
    /// <summary>
    /// Static-queue, OnGUI-only floating damage numbers.
    /// No GameObject spawning per hit. Items decay by lifetime.
    /// Singleton: ensure once via EnsureInstance().
    /// </summary>
    public class DamageNumberFeedback : MonoBehaviour
    {
        public enum DamageKind
        {
            Damage,
            Stagger,
            Dead,
            Miss,
            Heal
        }

        private struct Entry
        {
            public string Text;
            public Vector3 World;
            public float Born;
            public float Lifetime;
            public Color Color;
        }

        private static DamageNumberFeedback _instance;
        public static DamageNumberFeedback Instance => _instance;

        [SerializeField] private float _lifetime = 1.0f;
        [SerializeField] private float _riseSpeed = 1.2f;
        [SerializeField] private int _maxEntries = 32;
        [SerializeField] private int _fontSize = 18;

        private readonly List<Entry> _entries = new List<Entry>();

        public IReadOnlyList<object> EntriesForTest
        {
            get
            {
                var list = new List<object>();
                foreach (var e in _entries) list.Add(e);
                return list;
            }
        }

        public int ActiveCount => _entries.Count;
        public float Lifetime => _lifetime;

        public void ApplyTuning(float duration)
        {
            if (duration > 0f) _lifetime = duration;
        }

        public static DamageNumberFeedback EnsureInstance()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[DamageNumberFeedback]");
            _instance = go.AddComponent<DamageNumberFeedback>();
            return _instance;
        }

        public static void Push(Vector3 world, float amount, DamageKind kind = DamageKind.Damage)
        {
            if (_instance == null) return;
            _instance.PushInternal(world, amount, kind);
        }

        public static void PushText(Vector3 world, string text, Color color)
        {
            if (_instance == null) return;
            _instance.PushTextInternal(world, text, color);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Clear() => _entries.Clear();

        private void PushInternal(Vector3 world, float amount, DamageKind kind)
        {
            string text;
            Color color;
            switch (kind)
            {
                case DamageKind.Stagger:
                    text = "STAGGER";
                    color = new Color(1f, 0.85f, 0.2f, 1f);
                    break;
                case DamageKind.Dead:
                    text = "DEAD";
                    color = new Color(1f, 0.2f, 0.2f, 1f);
                    break;
                case DamageKind.Miss:
                    text = "MISS";
                    color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    break;
                case DamageKind.Heal:
                    text = $"+{Mathf.RoundToInt(amount)}";
                    color = new Color(0.3f, 1f, 0.4f, 1f);
                    break;
                default:
                    text = $"-{Mathf.RoundToInt(amount)}";
                    color = new Color(1f, 0.4f, 0.4f, 1f);
                    break;
            }
            PushTextInternal(world, text, color);
        }

        private void PushTextInternal(Vector3 world, string text, Color color)
        {
            if (_entries.Count >= _maxEntries)
                _entries.RemoveAt(0);

            _entries.Add(new Entry
            {
                Text = text,
                World = world,
                Born = Time.unscaledTime,
                Lifetime = _lifetime,
                Color = color
            });
        }

        private void Update()
        {
            var now = Time.unscaledTime;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (now - _entries[i].Born > _entries[i].Lifetime)
                    _entries.RemoveAt(i);
            }
        }

        private void OnGUI()
        {
            if (_entries.Count == 0) return;
            var cam = Camera.main;
            if (cam == null) return;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            var now = Time.unscaledTime;
            var prev = GUI.color;
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                var t = (now - e.Born) / Mathf.Max(0.0001f, e.Lifetime);
                if (t > 1f) continue;

                var world = e.World + Vector3.up * (_riseSpeed * t);
                var screen = cam.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;

                var alpha = 1f - t;
                var c = e.Color;
                c.a *= alpha;
                GUI.color = c;
                var rect = new Rect(screen.x - 60f, Screen.height - screen.y - 20f, 120f, 24f);
                GUI.Label(rect, e.Text, style);
            }
            GUI.color = prev;
        }
    }
}

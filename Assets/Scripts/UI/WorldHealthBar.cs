using UnityEngine;

namespace LuoLuoTrip.UI
{
    /// <summary>
    /// World-space OnGUI health bar that follows a target transform's head.
    /// Draws into Game view; no UI Canvas required.
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] private Transform _follow;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2.4f, 0f);
        [SerializeField] private Vector2 _size = new Vector2(80f, 8f);
        [SerializeField] private Color _fillColor = new Color(0.85f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color _backColor = new Color(0f, 0f, 0f, 0.65f);
        [SerializeField] private Color _deadColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        [SerializeField] private bool _visible = true;
        [SerializeField] private bool _showText = true;

        private float _ratio = 1f;
        private float _current;
        private float _max = 1f;
        private bool _isDead;
        private string _label;

        public bool IsVisible
        {
            get => _visible;
            set => _visible = value;
        }

        public Transform Follow
        {
            get => _follow;
            set => _follow = value;
        }

        public Vector3 WorldOffset
        {
            get => _worldOffset;
            set => _worldOffset = value;
        }

        public float Ratio => _ratio;
        public bool IsDead => _isDead;

        public void SetValues(float current, float max, bool isDead)
        {
            _current = Mathf.Max(0f, current);
            _max = Mathf.Max(0.0001f, max);
            _ratio = Mathf.Clamp01(_current / _max);
            _isDead = isDead;
        }

        public void SetLabel(string label)
        {
            _label = label;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (_follow == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var world = _follow.position + _worldOffset;
            var screen = cam.WorldToScreenPoint(world);
            if (screen.z <= 0f) return;

            var x = screen.x - _size.x * 0.5f;
            var y = Screen.height - screen.y - _size.y;

            var prevColor = GUI.color;
            GUI.color = _backColor;
            GUI.DrawTexture(new Rect(x - 1, y - 1, _size.x + 2, _size.y + 2), Texture2D.whiteTexture);

            GUI.color = _isDead ? _deadColor : _fillColor;
            GUI.DrawTexture(new Rect(x, y, _size.x * _ratio, _size.y), Texture2D.whiteTexture);

            GUI.color = Color.white;
            if (_showText)
            {
                var text = _isDead
                    ? "DEAD"
                    : (string.IsNullOrEmpty(_label)
                        ? $"{(int)_current}/{(int)_max}"
                        : $"{_label} {(int)_current}/{(int)_max}");
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                GUI.Label(new Rect(x, y - 14f, _size.x, 14f), text, style);
            }
            GUI.color = prevColor;
        }
    }
}

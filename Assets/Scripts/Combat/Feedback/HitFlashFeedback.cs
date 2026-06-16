using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.Combat.Feedback
{
    /// <summary>
    /// On hit, tint Visual subtree renderers to flash color, then restore originals.
    /// Only operates on the "Visual" child subtree to avoid touching PrefabRoot/Collision/Marker.
    /// Does not allocate per hit beyond a single timer.
    /// </summary>
    public class HitFlashFeedback : MonoBehaviour
    {
        [SerializeField] private string _visualChildName = "Visual";
        [SerializeField] private Color _hitColor = new Color(1f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color _deathColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private float _duration = 0.12f;
        [SerializeField] private float _deathDuration = 0.6f;

        private readonly List<Renderer> _renderers = new List<Renderer>();
        private readonly List<Color> _originalColors = new List<Color>();
        private float _flashTimer;
        private bool _isFlashing;
        private bool _resolved;

        public bool IsFlashing => _isFlashing;

        private void Awake()
        {
            ResolveRenderers();
        }

        private void ResolveRenderers()
        {
            if (_resolved) return;
            _renderers.Clear();
            _originalColors.Clear();

            Transform visualRoot = null;
            var t = transform.Find(_visualChildName);
            if (t != null) visualRoot = t;

            if (visualRoot == null)
            {
                _resolved = true;
                return;
            }

            foreach (var r in visualRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                _renderers.Add(r);
                if (r.material != null && r.material.HasProperty("_Color"))
                    _originalColors.Add(r.material.color);
                else
                    _originalColors.Add(Color.white);
            }
            _resolved = true;
        }

        public void PlayHitFlash()
        {
            ResolveRenderers();
            if (_renderers.Count == 0) return;
            ApplyTint(_hitColor);
            _flashTimer = _duration;
            _isFlashing = true;
        }

        public void PlayDeathFlash()
        {
            ResolveRenderers();
            if (_renderers.Count == 0) return;
            ApplyTint(_deathColor);
            _flashTimer = _deathDuration;
            _isFlashing = true;
        }

        public void RestoreImmediate()
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (r == null || r.material == null) continue;
                if (r.material.HasProperty("_Color"))
                    r.material.color = _originalColors[i];
            }
            _isFlashing = false;
            _flashTimer = 0f;
        }

        private void ApplyTint(Color c)
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (r == null || r.material == null) continue;
                if (r.material.HasProperty("_Color"))
                    r.material.color = c;
            }
        }

        private void Update()
        {
            if (!_isFlashing) return;
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f) RestoreImmediate();
        }

        public int RendererCount => _renderers.Count;
    }
}

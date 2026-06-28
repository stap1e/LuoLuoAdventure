using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class FactionDeltaToastPanel : MonoBehaviour
    {
        private struct ToastEntry
        {
            public string Text;
            public Color Color;
            public float RemainingTime;
        }

        [SerializeField] private float _toastDuration = 3f;
        [SerializeField] private bool _visible = true;

        private readonly List<ToastEntry> _toasts = new List<ToastEntry>();

        public void ShowDelta(SubFactionId faction, float delta)
        {
            var color = delta >= 0 ? Color.green : Color.red;
            var sign = delta >= 0 ? "+" : "";
            _toasts.Add(new ToastEntry
            {
                Text = $"{faction}: {sign}{delta:F0} standing",
                Color = color,
                RemainingTime = _toastDuration
            });
        }

        public void ShowDeltas(Dictionary<SubFactionId, float> deltas)
        {
            if (deltas == null) return;
            foreach (var kv in deltas)
                ShowDelta(kv.Key, kv.Value);
        }

        public void ShowFactionDeltas(List<FactionStandingDelta> deltas)
        {
            if (deltas == null) return;
            foreach (var d in deltas)
            {
                var netDelta = d.TrustDelta + d.RespectDelta - d.HostilityDelta;
                if (netDelta != 0)
                    ShowDelta(d.FactionId, netDelta);

                if (d.HostilityDelta != 0)
                {
                    var hColor = d.HostilityDelta > 0 ? Color.red : Color.green;
                    var hSign = d.HostilityDelta > 0 ? "+" : "";
                    _toasts.Add(new ToastEntry
                    {
                        Text = $"{d.FactionId}: {hSign}{d.HostilityDelta} hostility",
                        Color = hColor,
                        RemainingTime = _toastDuration
                    });
                }
            }
        }

        private void Update()
        {
            for (var i = _toasts.Count - 1; i >= 0; i--)
            {
                var t = _toasts[i];
                t.RemainingTime -= Time.deltaTime;
                if (t.RemainingTime <= 0f)
                    _toasts.RemoveAt(i);
                else
                    _toasts[i] = t;
            }
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (_toasts.Count == 0) return;

            var layout = DebugUILayout.FactionDeltaToast;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            for (var i = 0; i < _toasts.Count; i++)
            {
                var toast = _toasts[i];
                var alpha = Mathf.Clamp01(toast.RemainingTime / 0.5f);
                var color = toast.Color;
                color.a = alpha;
                GUI.color = color;
                GUI.Label(new Rect(x, y + i * 22, width, 20), toast.Text);
            }
            GUI.color = Color.white;
        }
    }
}

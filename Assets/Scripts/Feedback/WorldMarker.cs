using UnityEngine;

namespace LuoLuoTrip.Feedback
{
    public class WorldMarker : MonoBehaviour
    {
        public WorldMarkerType Type { get; private set; }
        public Transform Target { get; private set; }
        public string CustomLabel { get; private set; }
        public bool IsVisible { get; private set; } = true;

        private bool _registered;

        public void Configure(WorldMarkerType type, Transform target, string customLabel = null)
        {
            Type = type;
            Target = target == null ? transform : target;
            CustomLabel = customLabel;
            TryRegister();
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
        }

        public Vector3 GetWorldPosition()
        {
            if (Target == null) return transform.position;
            return Target.position;
        }

        private void Start()
        {
            TryRegister();
        }

        private void OnDestroy()
        {
            if (_registered && WorldMarkerService.Instance != null)
                WorldMarkerService.Instance.Unregister(this);
            _registered = false;
        }

        private void TryRegister()
        {
            if (_registered) return;
            var svc = WorldMarkerService.Instance;
            if (svc == null) return;
            svc.Register(this);
            _registered = true;
        }
    }
}

using UnityEngine;

namespace LuoLuoTrip
{
    public class MissionBoundary : MonoBehaviour
    {
        [SerializeField] private Vector3 _center;
        [SerializeField] private float _radius = 12f;
        [SerializeField] private bool _useTriggerZonePosition = true;

        public Vector3 Center
        {
            get => _useTriggerZonePosition ? transform.position : _center;
            set => _center = value;
        }

        public float Radius
        {
            get => _radius;
            set => _radius = Mathf.Max(0.1f, value);
        }

        public bool IsInside(Vector3 position)
        {
            var diff = position - Center;
            diff.y = 0f;
            return diff.sqrMagnitude <= _radius * _radius;
        }

        public void ConfigureFromTriggerZone(MissionTriggerZone triggerZone)
        {
            if (triggerZone == null) return;
            _useTriggerZonePosition = true;
            _radius = triggerZone.ZoneRadius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Center, _radius);
        }
    }
}

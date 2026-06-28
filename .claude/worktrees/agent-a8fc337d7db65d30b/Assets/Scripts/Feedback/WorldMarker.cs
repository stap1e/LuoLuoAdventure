using UnityEngine;

namespace LuoLuoTrip.Feedback
{
    public class WorldMarker : MonoBehaviour
    {
        [SerializeField] private WorldMarkerType _type;
        [SerializeField] private Transform _target;
        [SerializeField] private string _customLabel;
        [SerializeField] private bool _isVisible = true;

        public WorldMarkerType Type => _type;
        public Transform Target => _target;
        public string CustomLabel => _customLabel;
        public bool IsVisible => _isVisible;

        private bool _registered;

        public void Configure(WorldMarkerType type, Transform target, string customLabel = null)
        {
            _type = type;
            _target = target == null ? transform : target;
            _customLabel = customLabel;
            TryRegister();
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        public Vector3 GetWorldPosition()
        {
            if (_target == null) return transform.position;
            return _target.position;
        }

        public static string BuildReadableLabel(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return string.Empty;
            if (objectName.Contains("Area_ConvoyMission")) return "Convoy Mission Area";
            if (objectName.Contains("Convoy")) return "Convoy";
            if (objectName.Contains("Energy_Node") || objectName.Contains("EnergyNode")) return "Energy Node";
            if (objectName.Contains("Area_BorderRetaliation")) return "Border Retaliation Area";
            if (objectName.Contains("BorderSpawnPoint_Beast")) return "Raider Spawn";
            if (objectName.Contains("Border_ObjectiveMarker")) return "Allied Defense Point";
            if (objectName.Contains("Area_CityGateDispute")) return "City Gate Mission Area";
            if (objectName.Contains("CityGateCore")) return "CityGateCore";
            if (objectName.Contains("BeastNegotiator")) return "BeastNegotiator";
            if (objectName.Contains("CityGateSpawnPoint_Beast")) return "BeastRaider Spawn";
            if (objectName.Contains("MechaGateGuard")) return "Low-Rank Ally: Can Receive Commands";
            if (objectName.Contains("MechaHardliner")) return "High-Rank Unit: Tactical Command Only";
            return string.Empty;
        }

        public static WorldMarkerType InferType(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return WorldMarkerType.MissionObjective;
            if (objectName.Contains("Energy")) return WorldMarkerType.Interactable;
            if (objectName.Contains("Spawn") || objectName.Contains("Raider")) return WorldMarkerType.HostileUnit;
            if (objectName.Contains("Guard") || objectName.Contains("Hardliner")) return WorldMarkerType.FriendlyUnit;
            return WorldMarkerType.MissionObjective;
        }

        private void Start()
        {
            EnsureConfiguredDefaults();
            TryRegister();
        }

        private void OnDestroy()
        {
            if (_registered && WorldMarkerService.Instance != null)
                WorldMarkerService.Instance.Unregister(this);
            _registered = false;
        }

        private void EnsureConfiguredDefaults()
        {
            if (_target == null)
                _target = transform;
            if (_type == WorldMarkerType.None)
                _type = InferType(gameObject.name);
            if (string.IsNullOrEmpty(_customLabel))
                _customLabel = BuildReadableLabel(gameObject.name);
        }

        private void TryRegister()
        {
            EnsureConfiguredDefaults();
            if (_registered) return;
            var svc = WorldMarkerService.Instance;
            if (svc == null) return;
            svc.Register(this);
            _registered = true;
        }
    }
}

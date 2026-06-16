using UnityEngine;

namespace LuoLuoTrip
{
    public class MissionAreaRuntime : MonoBehaviour
    {
        [SerializeField] private MissionBoundary _boundary;
        [SerializeField] private float _retreatTime = 10f;
        [SerializeField] private string _missionId;

        private RetreatTracker _retreatTracker;
        private bool _isActive;
        private bool _isComplete;

        public MissionBoundary Boundary => _boundary;
        public RetreatTracker Retreat => _retreatTracker;
        public bool IsPlayerInside { get; private set; }
        public bool IsActive => _isActive;
        public bool IsComplete => _isComplete;
        public string MissionId => _missionId;

        private void Awake()
        {
            _retreatTracker = new RetreatTracker();
            _retreatTracker.Configure(_retreatTime);
        }

        public void Activate(string missionId)
        {
            _missionId = missionId;
            _isActive = true;
            _isComplete = false;
            _retreatTracker.Reset();
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void MarkComplete()
        {
            _isComplete = true;
            _isActive = false;
            _retreatTracker.Reset();
        }

        public void SetBoundary(MissionBoundary boundary)
        {
            _boundary = boundary;
        }

        public void SetRetreatTime(float retreatTime)
        {
            _retreatTime = Mathf.Max(0f, retreatTime);
            if (_retreatTracker != null)
                _retreatTracker.Configure(_retreatTime);
        }

        public void ConfigureBoundary(MissionTriggerZone triggerZone)
        {
            if (_boundary == null)
            {
                var boundaryGo = new GameObject("MissionBoundary");
                boundaryGo.transform.SetParent(transform, false);
                _boundary = boundaryGo.AddComponent<MissionBoundary>();
            }
            _boundary.ConfigureFromTriggerZone(triggerZone);
        }

        public void Tick(float deltaTime)
        {
            if (!_isActive || _isComplete) return;

            var player = FindPlayerEntity();
            IsPlayerInside = player != null && _boundary != null && _boundary.IsInside(player.transform.position);

            _retreatTracker.Tick(deltaTime, IsPlayerInside);
        }

        public bool ShouldTriggerRetreat()
        {
            return _isActive && !_isComplete && _retreatTracker.IsRetreating;
        }

        private CharacterEntity FindPlayerEntity()
        {
            if (CharacterRuntimeRegistry.Count > 0)
            {
                var all = CharacterRuntimeRegistry.AllCharacters;
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] != null && all[i].GetComponent<Combat.CombatController>() != null)
                        return all[i];
                }
            }
            foreach (var ctrl in FindObjectsOfType<Combat.CombatController>())
                return ctrl.GetComponent<CharacterEntity>();
            return null;
        }
    }
}

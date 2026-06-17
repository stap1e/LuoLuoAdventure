using UnityEngine;

namespace LuoLuoTrip
{
    public class MissionTriggerZone : MonoBehaviour
    {
        [SerializeField] private string _missionId = "convoy_energy_conflict";
        [SerializeField] private float _zoneRadius = 12f;
        [SerializeField] private bool _autoDetectPlayer = true;

        private bool _missionStarted;
        private bool _missionCompleted;

        public string MissionId => _missionId;
        public bool MissionStarted => _missionStarted;
        public bool MissionCompleted => _missionCompleted;
        public float ZoneRadius => _zoneRadius;

        private void Update()
        {
            if (_missionStarted || _missionCompleted) return;
            if (!_autoDetectPlayer) return;

            var player = FindPlayerEntity();
            if (player == null) return;

            var dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist <= _zoneRadius)
            {
                _missionStarted = true;
            }
        }

        public void ForceStart()
        {
            if (_missionCompleted)
            {
                Debug.Log($"[MissionTriggerZone] ForceStart ignored: mission '{_missionId}' already completed");
                return;
            }
            _missionStarted = true;
        }

        public void MarkCompleted()
        {
            _missionCompleted = true;
        }

        public void Reset()
        {
            _missionStarted = false;
            _missionCompleted = false;
        }

        public bool IsPlayerInZone()
        {
            var player = FindPlayerEntity();
            if (player == null) return false;
            return Vector3.Distance(player.transform.position, transform.position) <= _zoneRadius;
        }

        private CharacterEntity FindPlayerEntity()
        {
            foreach (var ctrl in FindObjectsOfType<Combat.CombatController>())
            {
                return ctrl.GetComponent<CharacterEntity>();
            }
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _missionStarted ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _zoneRadius);
        }
    }
}

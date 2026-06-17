using LuoLuoTrip;
using UnityEngine;
#if UNITY_2022_3_OR_NEWER
using UnityEngine.AI;
#endif

namespace LuoLuoTrip.AI
{
    public enum NavigationState
    {
        Idle,
        Moving,
        Stopped
    }

    public enum NavMeshMode
    {
        NavMesh,
        Fallback,
        Mixed
    }

    public class NavigationAgentBridge : MonoBehaviour
    {
        [SerializeField] private float _fallbackSpeed = 4f;
        [SerializeField] private float _defaultStopDistance = 1f;

        private NavMeshAgent _navAgent;
        private NavigationMoveRequest _currentRequest;
        private NavigationState _state = NavigationState.Idle;
        private bool _stopped;
        private bool _fallbackWarned;
        private bool _useNavMesh;
        private bool _navMeshAvailable;
        private CharacterMovementMotor _motor;

        public NavigationState State => _state;
        public bool IsNavigating => _state == NavigationState.Moving;
        public Vector3 Destination => _currentRequest?.Destination ?? transform.position;
        public float DistanceToDestination => _currentRequest != null
            ? Vector3.Distance(transform.position, _currentRequest.Destination)
            : 0f;
        public bool UseNavMesh => _useNavMesh;
        public bool HasNavMeshAgent => _navAgent != null;
        public bool IsOnNavMesh => _navAgent != null && _navAgent.isOnNavMesh;
        public NavMeshMode Mode
        {
            get
            {
                if (_navAgent == null) return NavMeshMode.Fallback;
                if (_navAgent.isOnNavMesh) return NavMeshMode.NavMesh;
                return NavMeshMode.Fallback;
            }
        }

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _useNavMesh = _navAgent != null;
            if (_useNavMesh)
                _navMeshAvailable = true;
            _motor = GetComponent<CharacterMovementMotor>();
        }

        public void SetDestination(NavigationMoveRequest request)
        {
            _currentRequest = request;
            _stopped = false;
            _state = NavigationState.Moving;

            if (_useNavMesh && _navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = false;
                _navAgent.speed = request.Speed > 0f ? request.Speed : _fallbackSpeed;
                _navAgent.stoppingDistance = request.StopDistance;
                _navAgent.SetDestination(request.Destination);
            }
        }

        public void SetDestination(Vector3 position, float speed, float stopDistance = 1f)
        {
            SetDestination(NavigationMoveRequest.To(position, speed, stopDistance));
        }

        public void Stop()
        {
            _stopped = true;
            _state = NavigationState.Stopped;

            if (_useNavMesh && _navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
            }
        }

        public void Resume()
        {
            if (_currentRequest == null) return;
            _stopped = false;
            _state = NavigationState.Moving;

            if (_useNavMesh && _navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = false;
            }
        }

        public bool HasReachedDestination()
        {
            if (_currentRequest == null) return true;
            return _currentRequest.HasReached(transform.position);
        }

        public bool IsPathAvailable()
        {
            if (!_useNavMesh || _navAgent == null) return true;
            if (!_navAgent.isOnNavMesh) return false;
            return _navAgent.hasPath || _navAgent.pathStatus == NavMeshPathStatus.PathComplete;
        }

        public void ClearRequest()
        {
            _currentRequest = null;
            _state = NavigationState.Idle;
            _stopped = false;

            if (_useNavMesh && _navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
            }
        }

        public void TickFallback(float deltaTime)
        {
            // If NavMesh is available and agent is on NavMesh, let NavMeshAgent handle movement.
            if (_useNavMesh && _navAgent != null && _navAgent.isOnNavMesh) return;
            if (_stopped || _currentRequest == null) return;

            if (!_fallbackWarned)
            {
                Debug.LogWarning($"[NavigationAgentBridge] {name} using fallback movement (no NavMesh or not on NavMesh)");
                _fallbackWarned = true;
            }

            var diff = _currentRequest.Destination - transform.position;
            diff.y = 0f;
            var dist = diff.magnitude;

            if (dist <= _currentRequest.StopDistance)
            {
                if (_currentRequest.StopOnArrive)
                {
                    _state = NavigationState.Idle;
                    _currentRequest = null;
                }
                return;
            }

            var speed = _currentRequest.Speed > 0f ? _currentRequest.Speed : _fallbackSpeed;
            if (_motor == null)
                _motor = GetComponent<CharacterMovementMotor>();
            if (_motor != null)
                _motor.MoveDirection(diff.normalized, speed, deltaTime);
            else
            {
                var delta = diff.normalized * (speed * deltaTime);
                delta.y = 0f;
                transform.position += delta;
            }

            if (diff.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(diff.normalized);
        }

        public void UpdateNavMeshState()
        {
            if (!_useNavMesh || _navAgent == null || _currentRequest == null) return;
            if (_stopped) return;

            if (!_navAgent.isOnNavMesh)
            {
                if (_navMeshAvailable && !_fallbackWarned)
                {
                    Debug.LogWarning($"[NavigationAgentBridge] {name} NavMeshAgent lost NavMesh, switching to fallback");
                    _fallbackWarned = true;
                    _navMeshAvailable = false;
                }
                return;
            }

            if (HasReachedDestination())
            {
                if (_currentRequest.StopOnArrive)
                {
                    _state = NavigationState.Idle;
                    _currentRequest = null;
                }
            }
        }

        public static NavMeshMode GetSceneMode()
        {
            bool anyNavMesh = false;
            bool anyFallback = false;
            var bridges = FindObjectsOfType<NavigationAgentBridge>();
            for (int i = 0; i < bridges.Length; i++)
            {
                if (bridges[i].Mode == NavMeshMode.NavMesh)
                    anyNavMesh = true;
                else
                    anyFallback = true;
            }
            if (anyNavMesh && anyFallback) return NavMeshMode.Mixed;
            if (anyNavMesh) return NavMeshMode.NavMesh;
            return NavMeshMode.Fallback;
        }
    }
}

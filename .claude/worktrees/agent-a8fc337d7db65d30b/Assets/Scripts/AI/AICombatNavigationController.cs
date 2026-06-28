using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip.AI
{
    public class AICombatNavigationController : MonoBehaviour
    {
        [SerializeField] private float _chaseSpeed = 4f;
        [SerializeField] private float _followStopDistance = 2.5f;
        [SerializeField] private float _attackStopDistance = 1.2f;

        private NavigationAgentBridge _bridge;
        private Combatant _self;

        public NavigationAgentBridge Bridge => _bridge;
        public NavigationState NavState => _bridge != null ? _bridge.State : NavigationState.Idle;
        public float DistanceToDestination => _bridge != null ? _bridge.DistanceToDestination : 0f;

        private void Awake()
        {
            EnsureBridge();
            _self = GetComponent<Combatant>();
        }

        private void EnsureBridge()
        {
            if (_bridge == null)
                _bridge = GetComponent<NavigationAgentBridge>();
            if (_bridge == null)
                _bridge = gameObject.AddComponent<NavigationAgentBridge>();
        }

        public void ChaseTarget(Transform target)
        {
            ChaseTarget(target, _attackStopDistance);
        }

        public void ChaseTarget(Transform target, float stopDistance)
        {
            if (target == null) return;
            EnsureBridge();
            _bridge.SetDestination(NavigationMoveRequest.Follow(target, _chaseSpeed, stopDistance));
        }

        public void FollowTarget(Transform target)
        {
            if (target == null) return;
            EnsureBridge();
            _bridge.SetDestination(NavigationMoveRequest.Follow(target, _chaseSpeed * 0.8f, _followStopDistance));
        }

        public void MoveToPosition(Vector3 position)
        {
            EnsureBridge();
            _bridge.SetDestination(NavigationMoveRequest.To(position, _chaseSpeed * 0.5f, 1f));
        }

        public void StopNavigation()
        {
            EnsureBridge();
            _bridge.Stop();
        }

        public void ClearNavigation()
        {
            EnsureBridge();
            _bridge.ClearRequest();
        }

        public bool IsInAttackRange(Transform target)
        {
            if (target == null || _self == null) return false;
            var attackRange = _self.Stats.attackRange + 0.5f;
            var diff = target.position - transform.position;
            diff.y = 0f;
            return diff.magnitude <= attackRange;
        }

        public void Tick(float deltaTime)
        {
            EnsureBridge();
            _bridge.TickFallback(deltaTime);
            _bridge.UpdateNavMeshState();
        }
    }
}

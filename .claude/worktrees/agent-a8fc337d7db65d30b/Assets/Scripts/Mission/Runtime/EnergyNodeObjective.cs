using UnityEngine;

namespace LuoLuoTrip
{
    public class EnergyNodeObjective : MonoBehaviour
    {
        [SerializeField] private float _captureRadius = 3f;
        [SerializeField] private float _beastCaptureTime = 5f;
        [SerializeField] private float _playerInteractTime = 2f;

        private float _beastCaptureProgress;
        private float _playerInteractProgress;

        public float CaptureRadius => _captureRadius;
        public float BeastCaptureProgress => _beastCaptureProgress;
        public float BeastCaptureTime => _beastCaptureTime;
        public bool IsCapturedByBeast => _beastCaptureProgress >= _beastCaptureTime;
        public bool IsSharedByPlayer => _playerInteractProgress >= _playerInteractTime;

        public void TickBeastCapture(float deltaTime, int beastCount)
        {
            if (IsCapturedByBeast) return;
            _beastCaptureProgress += deltaTime * beastCount;
        }

        public void TickPlayerInteract(float deltaTime, bool playerInRange)
        {
            if (IsSharedByPlayer) return;
            if (playerInRange)
                _playerInteractProgress += deltaTime;
        }

        public void Reset()
        {
            _beastCaptureProgress = 0f;
            _playerInteractProgress = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _captureRadius);
        }
    }
}

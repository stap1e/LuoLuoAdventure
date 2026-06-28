using UnityEngine;

namespace LuoLuoTrip
{
    public class CameraFollowController : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 8f, -10f);
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _lookAtTarget = true;

        public Transform Target { get; private set; }

        public void SetTarget(Transform target)
        {
            Target = target;
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            var desiredPos = Target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, _smoothSpeed * Time.deltaTime);

            if (_lookAtTarget)
                transform.LookAt(Target.position + Vector3.up * 1.5f);
        }
    }
}

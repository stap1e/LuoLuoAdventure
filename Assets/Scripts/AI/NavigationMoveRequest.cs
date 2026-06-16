using UnityEngine;

namespace LuoLuoTrip.AI
{
    public class NavigationMoveRequest
    {
        public Vector3 Destination;
        public float StopDistance;
        public bool StopOnArrive;
        public float Speed;

        public static NavigationMoveRequest To(Vector3 destination, float speed, float stopDistance = 1f, bool stopOnArrive = true)
        {
            return new NavigationMoveRequest
            {
                Destination = destination,
                Speed = speed,
                StopDistance = stopDistance,
                StopOnArrive = stopOnArrive
            };
        }

        public static NavigationMoveRequest Follow(Transform target, float speed, float stopDistance = 2.5f)
        {
            return new NavigationMoveRequest
            {
                Destination = target.position,
                Speed = speed,
                StopDistance = stopDistance,
                StopOnArrive = false
            };
        }

        public bool HasReached(Vector3 position)
        {
            var diff = Destination - position;
            diff.y = 0f;
            return diff.magnitude <= StopDistance;
        }
    }
}

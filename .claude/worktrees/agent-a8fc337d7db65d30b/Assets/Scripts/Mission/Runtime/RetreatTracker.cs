using UnityEngine;

namespace LuoLuoTrip
{
    public class RetreatTracker
    {
        public float RetreatTime { get; set; } = 10f;
        public float CurrentTimer { get; private set; }
        public bool IsRetreating => CurrentTimer >= RetreatTime;
        public float Progress => RetreatTime > 0f ? Mathf.Clamp01(CurrentTimer / RetreatTime) : 0f;

        public void Tick(float deltaTime, bool playerInside)
        {
            if (playerInside)
            {
                CurrentTimer = 0f;
            }
            else
            {
                CurrentTimer += deltaTime;
            }
        }

        public void Reset()
        {
            CurrentTimer = 0f;
        }

        public void Configure(float retreatTime)
        {
            RetreatTime = Mathf.Max(0f, retreatTime);
            Reset();
        }
    }
}

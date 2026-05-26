using UnityEngine;

namespace LuoLuoTrip.Combat.Animation
{
    /// <summary>空实现，用于无动画资源时的占位</summary>
    public class NullCombatAnimator : MonoBehaviour, ICombatAnimator
    {
        public void PlayIdle() { }
        public void PlayMove(float normalizedSpeed) { }
        public void PlayLightAttack() { }
        public void PlayDodge() { }
        public void PlayStagger() { }
        public void PlayHitReact(bool isHeavy) { }
        public void PlayDeath() { }

        public void SetCombatState(CombatState state) { }
    }
}

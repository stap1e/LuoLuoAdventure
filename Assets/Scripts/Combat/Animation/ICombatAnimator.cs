namespace LuoLuoTrip.Combat.Animation
{
    /// <summary>
    /// 战斗动画驱动接口。可替换为 Animator、Playables、或第三方动画系统实现。
    /// </summary>
    public interface ICombatAnimator
    {
        void PlayIdle();
        void PlayMove(float normalizedSpeed);
        void PlayLightAttack();
        void PlayDodge();
        void PlayStagger();
        void PlayHitReact(bool isHeavy);
        void PlayDeath();
        void SetCombatState(CombatState state);
    }
}

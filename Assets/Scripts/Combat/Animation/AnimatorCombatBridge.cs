using UnityEngine;

namespace LuoLuoTrip.Combat.Animation
{
    /// <summary>
    /// Unity Animator 实现。将 CombatState / 战斗事件映射到 Animator 参数。
    /// 需在 Animator Controller 中配置同名 Trigger / Float / Bool。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorCombatBridge : MonoBehaviour, ICombatAnimator
    {
        [SerializeField] private CombatAnimatorConfigSO _config;
        [SerializeField] private Animator _animator;

        private int _hashMoveSpeed;
        private int _hashAttack;
        private int _hashDodge;
        private int _hashStagger;
        private int _hashHitLight;
        private int _hashHitHeavy;
        private int _hashDeath;
        private int _hashIsDead;

        private void Awake()
        {
            _animator ??= GetComponent<Animator>();
            _config ??= ScriptableObject.CreateInstance<CombatAnimatorConfigSO>();
            CacheHashes();
        }

        public void PlayIdle()
        {
            if (_animator == null) return;
            _animator.SetFloat(_hashMoveSpeed, 0f);
            if (!string.IsNullOrEmpty(_config.idleState))
                _animator.CrossFade(_config.idleState, _config.crossFadeDuration);
        }

        public void PlayMove(float normalizedSpeed)
        {
            if (_animator == null) return;
            _animator.SetFloat(_hashMoveSpeed, normalizedSpeed);
            if (normalizedSpeed > 0.1f && !string.IsNullOrEmpty(_config.moveState))
                _animator.CrossFade(_config.moveState, _config.crossFadeDuration);
        }

        public void PlayLightAttack()
        {
            if (_animator == null) return;
            _animator.ResetTrigger(_hashAttack);
            _animator.SetTrigger(_hashAttack);
        }

        public void PlayDodge()
        {
            if (_animator == null) return;
            _animator.ResetTrigger(_hashDodge);
            _animator.SetTrigger(_hashDodge);
        }

        public void PlayStagger()
        {
            if (_animator == null) return;
            _animator.ResetTrigger(_hashStagger);
            _animator.SetTrigger(_hashStagger);
        }

        public void PlayHitReact(bool isHeavy)
        {
            if (_animator == null) return;
            var hash = isHeavy ? _hashHitHeavy : _hashHitLight;
            _animator.ResetTrigger(hash);
            _animator.SetTrigger(hash);
        }

        public void PlayDeath()
        {
            if (_animator == null) return;
            _animator.SetBool(_hashIsDead, true);
            _animator.ResetTrigger(_hashDeath);
            _animator.SetTrigger(_hashDeath);
        }

        public void SetCombatState(CombatState state)
        {
            switch (state)
            {
                case CombatState.Idle: PlayIdle(); break;
                case CombatState.Attacking: PlayLightAttack(); break;
                case CombatState.Dodging: PlayDodge(); break;
                case CombatState.Staggered: PlayStagger(); break;
                case CombatState.Dead: PlayDeath(); break;
            }
        }

        /// <summary>供 Animation Event 回调：攻击判定帧</summary>
        public void AnimEvent_AttackActive()
        {
            var combatant = GetComponent<Combatant>();
            combatant?.AnimEvent_OnAttackActive();
        }

        /// <summary>供 Animation Event 回调：攻击结束</summary>
        public void AnimEvent_AttackEnd()
        {
            var combatant = GetComponent<Combatant>();
            combatant?.AnimEvent_OnAttackEnd();
        }

        private void CacheHashes()
        {
            _hashMoveSpeed = Animator.StringToHash(_config.moveSpeedParam);
            _hashAttack = Animator.StringToHash(_config.attackTrigger);
            _hashDodge = Animator.StringToHash(_config.dodgeTrigger);
            _hashStagger = Animator.StringToHash(_config.staggerTrigger);
            _hashHitLight = Animator.StringToHash(_config.hitLightTrigger);
            _hashHitHeavy = Animator.StringToHash(_config.hitHeavyTrigger);
            _hashDeath = Animator.StringToHash(_config.deathTrigger);
            _hashIsDead = Animator.StringToHash(_config.isDeadBool);
        }
    }
}

using UnityEngine;

namespace LuoLuoTrip.Combat.Animation
{
    /// <summary>
    /// 监听 Combatant 状态/受击事件，驱动 ICombatAnimator 实现。
    /// 自动选择：AnimatorCombatBridge > ProceduralCombatAnimator > NullCombatAnimator
    /// </summary>
    [RequireComponent(typeof(Combatant))]
    public class CombatAnimationDriver : MonoBehaviour
    {
        [SerializeField] private bool _preferAnimatorBridge = true;
        [SerializeField] private bool _useProceduralFallback = true;
        [SerializeField] private float _moveSpeedForAnim = 6f;

        private Combatant _combatant;
        private ICombatAnimator _animator;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            _animator = ResolveAnimator();
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            _combatant.OnStateChanged += HandleStateChanged;
            _combatant.OnHitReceived += HandleHitReceived;
            _combatant.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _combatant.OnStateChanged -= HandleStateChanged;
            _combatant.OnHitReceived -= HandleHitReceived;
            _combatant.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (_combatant.State != CombatState.Idle || _animator == null) return;

            var delta = transform.position - _lastPosition;
            delta.y = 0f;
            var speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            var normalized = Mathf.Clamp01(speed / _moveSpeedForAnim);
            _animator.PlayMove(normalized);
            _lastPosition = transform.position;
        }

        private ICombatAnimator ResolveAnimator()
        {
            if (_preferAnimatorBridge)
            {
                var bridge = GetComponent<AnimatorCombatBridge>();
                if (bridge != null) return bridge;
            }

            foreach (var mb in GetComponents<MonoBehaviour>())
            {
                if (mb is ICombatAnimator anim && mb is not CombatAnimationDriver)
                    return anim;
            }

            if (_useProceduralFallback)
                return gameObject.AddComponent<ProceduralCombatAnimator>();

            return gameObject.AddComponent<NullCombatAnimator>();
        }

        private void HandleStateChanged(CombatState state) => _animator?.SetCombatState(state);

        private void HandleHitReceived(CombatHitEvent hit)
        {
            var isHeavy = hit.Result.wasPoiseBroken || hit.Result.finalDamage >= _combatant.Stats.maxHealth * 0.15f;
            _animator?.PlayHitReact(isHeavy);
        }

        private void HandleDeath(Combatant _) => _animator?.PlayDeath();
    }
}

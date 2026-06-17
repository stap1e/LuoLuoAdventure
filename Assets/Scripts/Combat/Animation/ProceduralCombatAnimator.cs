using System.Collections;
using UnityEngine;

namespace LuoLuoTrip.Combat.Animation
{
    /// <summary>
    /// 无 Animator Controller 时的程序化动画反馈（原型用）。
    /// 通过位移/缩放/颜色脉冲模拟攻击、受击、硬直。
    /// </summary>
    public class ProceduralCombatAnimator : MonoBehaviour, ICombatAnimator
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private float _attackLunge = 0.9f;
        [SerializeField] private float _attackWindupLift = 0.28f;
        [SerializeField] private float _attackScalePulse = 1.18f;
        [SerializeField] private float _hitKickback = 0.2f;
        [SerializeField] private float _dodgeTilt = 15f;
        [SerializeField] private bool _strictVisualOnly = true;

        private Vector3 _baseLocalPos;
        private Vector3 _baseScale;
        private Quaternion _baseLocalRot;
        private Coroutine _activeRoutine;
        private Renderer _renderer;
        private Color _baseColor;
        private CombatState _currentState = CombatState.Idle;
        private bool _disabled;
        private bool _initialized;

        public Transform VisualRoot => _visualRoot;
        public bool IsOperatingOnVisualOnly => _visualRoot != null && _visualRoot != transform;
        public bool IsDisabled => _disabled;
        public Vector3 VisualLocalOffset => _visualRoot != null ? _visualRoot.localPosition - _baseLocalPos : Vector3.zero;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            if (_visualRoot == null)
            {
                var visualChild = transform.Find("Visual");
                _visualRoot = visualChild != null ? visualChild : null;
            }

            if (_visualRoot == null || _visualRoot == transform)
            {
                if (_strictVisualOnly)
                {
                    Debug.LogWarning($"[ProceduralCombatAnimator] '{name}' has no 'Visual' child — disabling procedural animator to protect root transform from being driven.");
                    _disabled = true;
                    enabled = false;
                    return;
                }

                Debug.LogWarning($"[ProceduralCombatAnimator] '{name}' has no 'Visual' child; falling back to root transform. Attack/dodge visuals may collide with gameplay movement.");
                _visualRoot = transform;
            }

            _baseLocalPos = _visualRoot.localPosition;
            _baseScale = _visualRoot.localScale;
            _baseLocalRot = _visualRoot.localRotation;
            _renderer = _visualRoot.GetComponentInChildren<Renderer>();
            if (_renderer != null && _renderer.sharedMaterial != null)
                _baseColor = _renderer.sharedMaterial.color;
        }

        public void PlayIdle()
        {
            EnsureInitialized();
            if (_disabled) return;
            ResetVisual();
        }

        public void PlayMove(float normalizedSpeed)
        {
            EnsureInitialized();
            if (_disabled) return;
            if (_currentState != CombatState.Idle) return;
            var bob = Mathf.Sin(Time.time * 8f * Mathf.Clamp01(normalizedSpeed)) * 0.03f * normalizedSpeed;
            _visualRoot.localPosition = _baseLocalPos + Vector3.up * bob;
        }

        public void PlayLightAttack()
        {
            EnsureInitialized();
            if (_disabled) return;
            RunRoutine(AttackRoutine(_attackLunge, 0.12f));
        }

        public void PlayDodge()
        {
            EnsureInitialized();
            if (_disabled) return;
            RunRoutine(DodgeRoutine());
        }

        public void PlayStagger()
        {
            EnsureInitialized();
            if (_disabled) return;
            RunRoutine(StaggerRoutine());
        }

        public void PlayHitReact(bool isHeavy)
        {
            EnsureInitialized();
            if (_disabled) return;
            RunRoutine(HitRoutine(isHeavy ? _hitKickback * 1.6f : _hitKickback));
        }

        public void PlayDeath()
        {
            EnsureInitialized();
            if (_disabled) return;
            RunRoutine(DeathRoutine());
        }

        public void SetCombatState(CombatState state)
        {
            EnsureInitialized();
            _currentState = state;
            if (_disabled) return;
            switch (state)
            {
                case CombatState.Idle:
                    PlayIdle();
                    break;
                case CombatState.AttackWindup:
                    ApplyAttackWindupPose();
                    RunRoutine(AttackWindupRoutine());
                    break;
                case CombatState.Attacking:
                    PlayLightAttack();
                    break;
                case CombatState.AttackRecovery:
                    StopActiveRoutine();
                    ResetVisual();
                    break;
                case CombatState.Dodging:
                    PlayDodge();
                    break;
                case CombatState.Staggered:
                    PlayStagger();
                    break;
                case CombatState.Dead:
                    PlayDeath();
                    break;
            }
        }

        private void ApplyAttackWindupPose()
        {
            _visualRoot.localPosition = _baseLocalPos + Vector3.up * _attackWindupLift - Vector3.forward * (_attackLunge * 0.2f);
            _visualRoot.localScale = _baseScale * _attackScalePulse;
        }

        private IEnumerator AttackWindupRoutine()
        {
            var targetPos = _baseLocalPos + Vector3.up * _attackWindupLift - Vector3.forward * (_attackLunge * 0.2f);
            var targetScale = _baseScale * _attackScalePulse;
            var t = 0f;
            var duration = 0.12f;
            var startPos = _visualRoot.localPosition;
            var startScale = _visualRoot.localScale;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var ratio = Mathf.Clamp01(t / duration);
                _visualRoot.localPosition = Vector3.Lerp(startPos, targetPos, ratio);
                _visualRoot.localScale = Vector3.Lerp(startScale, targetScale, ratio);
                yield return null;
            }
        }

        private IEnumerator AttackRoutine(float distance, float duration)
        {
            var forward = Vector3.forward * distance;
            FlashColor(Color.yellow, 0.08f);
            _visualRoot.localScale = _baseScale * _attackScalePulse;
            yield return LerpLocalPosition(_baseLocalPos + forward, duration * 0.45f);
            _visualRoot.localScale = _baseScale;
            yield return LerpLocalPosition(_baseLocalPos, duration * 0.55f);
            ResetVisual();
        }

        private IEnumerator DodgeRoutine()
        {
            var tilt = Quaternion.Euler(0f, 0f, _dodgeTilt) * _baseLocalRot;
            yield return LerpLocalRotation(tilt, 0.08f);
            yield return LerpLocalRotation(_baseLocalRot, 0.12f);
        }

        private IEnumerator StaggerRoutine()
        {
            var tilt = Quaternion.Euler(-12f, 0f, 0f) * _baseLocalRot;
            yield return LerpLocalRotation(tilt, 0.1f);
            yield return new WaitForSeconds(0.5f);
            yield return LerpLocalRotation(_baseLocalRot, 0.15f);
        }

        private IEnumerator HitRoutine(float kickback)
        {
            FlashColor(Color.white, 0.06f);
            var back = _baseLocalPos - transform.forward * kickback;
            yield return LerpLocalPosition(back, 0.05f);
            yield return LerpLocalPosition(_baseLocalPos, 0.08f);
        }

        private IEnumerator DeathRoutine()
        {
            var targetRot = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
            yield return LerpWorldRotation(targetRot, 0.35f);
            if (_renderer != null && _renderer.sharedMaterial != null)
                _renderer.sharedMaterial.color = new Color(_baseColor.r * 0.5f, _baseColor.g * 0.5f, _baseColor.b * 0.5f);
        }

        private IEnumerator LerpLocalPosition(Vector3 target, float duration)
        {
            var start = _visualRoot.localPosition;
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _visualRoot.localPosition = Vector3.Lerp(start, target, t / duration);
                yield return null;
            }
            _visualRoot.localPosition = target;
        }

        private IEnumerator LerpLocalRotation(Quaternion target, float duration)
        {
            var start = _visualRoot.localRotation;
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _visualRoot.localRotation = Quaternion.Slerp(start, target, t / duration);
                yield return null;
            }
            _visualRoot.localRotation = target;
        }

        private IEnumerator LerpWorldRotation(Quaternion target, float duration)
        {
            var start = _visualRoot.rotation;
            var t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _visualRoot.rotation = Quaternion.Slerp(start, target, t / duration);
                yield return null;
            }
            _visualRoot.rotation = target;
        }

        private void FlashColor(Color flash, float duration)
        {
            if (_renderer == null || _renderer.sharedMaterial == null) return;
            _renderer.sharedMaterial.color = flash;
            StartCoroutine(RestoreColor(duration));
        }

        private IEnumerator RestoreColor(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_renderer != null && _renderer.sharedMaterial != null)
                _renderer.sharedMaterial.color = _baseColor;
        }

        private void ResetVisual()
        {
            if (_visualRoot == null) return;
            _visualRoot.localPosition = _baseLocalPos;
            _visualRoot.localScale = _baseScale;
            _visualRoot.localRotation = _baseLocalRot;
        }

        private void OnDisable()
        {
            StopActiveRoutine();
            ResetVisual();
        }

        private void RunRoutine(IEnumerator routine)
        {
            StopActiveRoutine();
            _activeRoutine = StartCoroutine(routine);
        }

        private void StopActiveRoutine()
        {
            if (_activeRoutine == null) return;
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }
    }
}

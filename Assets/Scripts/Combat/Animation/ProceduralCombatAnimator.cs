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
        [SerializeField] private float _attackLunge = 0.35f;
        [SerializeField] private float _hitKickback = 0.2f;
        [SerializeField] private float _dodgeTilt = 15f;

        private Vector3 _baseLocalPos;
        private Vector3 _baseScale;
        private Quaternion _baseLocalRot;
        private Coroutine _activeRoutine;
        private Renderer _renderer;
        private Color _baseColor;
        private CombatState _currentState = CombatState.Idle;

        private void Awake()
        {
            _visualRoot = _visualRoot ?? transform;
            _baseLocalPos = _visualRoot.localPosition;
            _baseScale = _visualRoot.localScale;
            _baseLocalRot = _visualRoot.localRotation;
            _renderer = _visualRoot.GetComponentInChildren<Renderer>();
            if (_renderer != null)
                _baseColor = _renderer.material.color;
        }

        public void PlayIdle() => ResetVisual();

        public void PlayMove(float normalizedSpeed)
        {
            if (_currentState != CombatState.Idle) return;
            var bob = Mathf.Sin(Time.time * 8f * Mathf.Clamp01(normalizedSpeed)) * 0.03f * normalizedSpeed;
            _visualRoot.localPosition = _baseLocalPos + Vector3.up * bob;
        }

        public void PlayLightAttack()
        {
            RunRoutine(AttackRoutine(_attackLunge, 0.12f));
        }

        public void PlayDodge()
        {
            RunRoutine(DodgeRoutine());
        }

        public void PlayStagger()
        {
            RunRoutine(StaggerRoutine());
        }

        public void PlayHitReact(bool isHeavy)
        {
            RunRoutine(HitRoutine(isHeavy ? _hitKickback * 1.6f : _hitKickback));
        }

        public void PlayDeath()
        {
            RunRoutine(DeathRoutine());
        }

        public void SetCombatState(CombatState state)
        {
            _currentState = state;
            switch (state)
            {
                case CombatState.Idle:
                    PlayIdle();
                    break;
                case CombatState.Attacking:
                    PlayLightAttack();
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

        private IEnumerator AttackRoutine(float distance, float duration)
        {
            var forward = transform.forward * distance;
            yield return LerpLocalPosition(_baseLocalPos + forward, duration * 0.4f);
            yield return LerpLocalPosition(_baseLocalPos, duration * 0.6f);
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
            if (_renderer != null)
                _renderer.material.color = new Color(_baseColor.r * 0.5f, _baseColor.g * 0.5f, _baseColor.b * 0.5f);
        }

        private IEnumerator LerpLocalPosition(Vector3 target, float duration)
        {
            var start = _visualRoot.localPosition;
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
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
                t += Time.deltaTime;
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
                t += Time.deltaTime;
                _visualRoot.rotation = Quaternion.Slerp(start, target, t / duration);
                yield return null;
            }
            _visualRoot.rotation = target;
        }

        private void FlashColor(Color flash, float duration)
        {
            if (_renderer == null) return;
            _renderer.material.color = flash;
            StartCoroutine(RestoreColor(duration));
        }

        private IEnumerator RestoreColor(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_renderer != null)
                _renderer.material.color = _baseColor;
        }

        private void ResetVisual()
        {
            _visualRoot.localPosition = _baseLocalPos;
            _visualRoot.localScale = _baseScale;
            _visualRoot.localRotation = _baseLocalRot;
        }

        private void RunRoutine(IEnumerator routine)
        {
            if (_activeRoutine != null)
                StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(routine);
        }
    }
}

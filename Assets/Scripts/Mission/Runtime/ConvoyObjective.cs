using UnityEngine;

namespace LuoLuoTrip
{
    public class ConvoyObjective : MonoBehaviour
    {
        [SerializeField] private float _health = 100f;
        [SerializeField] private float _maxHealth = 100f;

        public float Health => _health;
        public float MaxHealth => _maxHealth;
        public bool IsDestroyed => _health <= 0f;
        public float HealthRatio => _maxHealth > 0 ? _health / _maxHealth : 0f;

        public void TakeDamage(float amount)
        {
            if (IsDestroyed) return;
            _health = Mathf.Max(0f, _health - amount);
        }

        public void Repair(float amount)
        {
            _health = Mathf.Min(_maxHealth, _health + amount);
        }

        public void Reset()
        {
            _health = _maxHealth;
        }
    }
}

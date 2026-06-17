using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    /// <summary>
    /// Bridges a Combatant to a WorldHealthBar, kept in sync each frame.
    /// Auto-creates a WorldHealthBar if one is not set. Warns once on missing Combatant.
    /// </summary>
    public class CombatantHealthBarPresenter : MonoBehaviour
    {
        [SerializeField] private Combatant _combatant;
        [SerializeField] private WorldHealthBar _bar;
        [SerializeField] private bool _hideOnDeath;
        [SerializeField] private string _label;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2.4f, 0f);

        private bool _missingWarned;

        public WorldHealthBar Bar
        {
            get
            {
                EnsureReferences();
                return _bar;
            }
        }
        public Combatant Combatant => _combatant;

        public void SetCombatant(Combatant combatant)
        {
            _combatant = combatant;
            if (_bar != null) _bar.Follow = combatant != null ? combatant.transform : null;
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
        {
            if (_combatant == null) _combatant = GetComponent<Combatant>();
            if (_bar == null) _bar = GetComponent<WorldHealthBar>();
            if (_bar == null) _bar = gameObject.AddComponent<WorldHealthBar>();
            _bar.Follow = _combatant != null ? _combatant.transform : transform;
            _bar.WorldOffset = _worldOffset;
            if (!string.IsNullOrEmpty(_label)) _bar.SetLabel(_label);
        }

        private void LateUpdate()
        {
            if (_combatant == null) _combatant = GetComponent<Combatant>();
            if (_combatant != null && _bar == null) EnsureReferences();
            if (_combatant == null)
            {
                if (!_missingWarned)
                {
                    Debug.LogWarning($"[CombatantHealthBarPresenter] {name} has no Combatant; bar will idle.");
                    _missingWarned = true;
                }
                return;
            }

            var stats = _combatant.Stats;
            var max = Mathf.Max(1f, stats.maxHealth);
            var hp = _combatant.CurrentHealth;
            var dead = !_combatant.IsAlive;

            _bar.SetValues(hp, max, dead);

            if (_hideOnDeath && dead)
                _bar.IsVisible = false;
            else
                _bar.IsVisible = true;
        }
    }
}

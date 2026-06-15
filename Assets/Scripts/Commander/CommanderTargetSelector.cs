using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip
{
    public class CommanderTargetSelector : MonoBehaviour
    {
        [SerializeField] private float _scanRadius = 15f;
        [SerializeField] private KeyCode _selectNextKey = KeyCode.Tab;

        private CharacterEntity _ownerEntity;
        private readonly List<CharacterEntity> _candidates = new List<CharacterEntity>();
        private int _currentIndex = -1;
        private float _refreshInterval = 0.5f;
        private float _refreshTimer;

        public CharacterEntity CurrentTarget { get; private set; }

        public CharacterControlInfo CurrentTargetInfo
        {
            get
            {
                if (CurrentTarget == null || CurrentTarget.Data == null)
                    return default;
                return CharacterControlInfo.FromCharacterData(CurrentTarget.Data);
            }
        }

        private void Awake()
        {
            _ownerEntity = GetComponent<CharacterEntity>();
        }

        private void Update()
        {
            _refreshTimer -= Time.deltaTime;

            if (Input.GetKeyDown(_selectNextKey))
                SelectNext();

            if (CurrentTarget != null && (CurrentTarget.Data == null || !CurrentTarget.Data.IsAlive))
                ClearSelection();
        }

        public void SelectNext()
        {
            RefreshCandidates();

            if (_candidates.Count == 0)
            {
                CurrentTarget = null;
                _currentIndex = -1;
                return;
            }

            _currentIndex = (_currentIndex + 1) % _candidates.Count;
            CurrentTarget = _candidates[_currentIndex];
        }

        public void ClearSelection()
        {
            CurrentTarget = null;
            _currentIndex = -1;
            _candidates.Clear();
        }

        private void RefreshCandidates()
        {
            _candidates.Clear();
            _currentIndex = -1;

            if (CharacterRuntimeRegistry.Count > 0)
            {
                var all = CharacterRuntimeRegistry.QueryCharactersInRadius(transform.position, _scanRadius);
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] != _ownerEntity)
                        _candidates.Add(all[i]);
                }
            }
            else
            {
                foreach (var entity in FindObjectsOfType<CharacterEntity>())
                {
                    if (entity == _ownerEntity) continue;
                    if (entity.Data == null || !entity.Data.IsAlive) continue;

                    var dist = Vector3.Distance(transform.position, entity.transform.position);
                    if (dist > _scanRadius) continue;

                    _candidates.Add(entity);
                }

                if (_refreshTimer <= 0f)
                {
                    Debug.LogWarning("[CommanderTargetSelector] CharacterRuntimeRegistry empty, using FindObjectsOfType fallback");
                    _refreshTimer = _refreshInterval;
                }
            }

            if (CurrentTarget != null && !_candidates.Contains(CurrentTarget))
                CurrentTarget = null;
        }

        public IReadOnlyList<CharacterEntity> GetCandidates()
        {
            RefreshCandidates();
            return _candidates;
        }
    }
}

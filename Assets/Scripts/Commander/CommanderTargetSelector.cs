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
            if (Input.GetKeyDown(_selectNextKey))
                SelectNext();
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

            foreach (var entity in FindObjectsOfType<CharacterEntity>())
            {
                if (entity == _ownerEntity) continue;
                if (entity.Data == null || !entity.Data.IsAlive) continue;
                if (entity == null) continue;

                var dist = Vector3.Distance(transform.position, entity.transform.position);
                if (dist > _scanRadius) continue;

                _candidates.Add(entity);
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

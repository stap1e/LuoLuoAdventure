using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip
{
    public static class CharacterRuntimeRegistry
    {
        private static readonly List<CharacterEntity> _characters = new List<CharacterEntity>();
        private static readonly Dictionary<CharacterEntity, int> _indexMap = new Dictionary<CharacterEntity, int>();

        public static IReadOnlyList<CharacterEntity> AllCharacters => _characters;
        public static int Count => _characters.Count;

        public static void Register(CharacterEntity entity)
        {
            if (entity == null || _indexMap.ContainsKey(entity)) return;
            _indexMap[entity] = _characters.Count;
            _characters.Add(entity);
        }

        public static void Unregister(CharacterEntity entity)
        {
            if (entity == null || !_indexMap.TryGetValue(entity, out var index)) return;
            _indexMap.Remove(entity);
            var last = _characters.Count - 1;
            if (index != last)
            {
                var swapped = _characters[last];
                _characters[index] = swapped;
                _indexMap[swapped] = index;
            }
            _characters.RemoveAt(last);
        }

        public static void Clear()
        {
            _characters.Clear();
            _indexMap.Clear();
        }

        public static List<CharacterEntity> QueryAliveCharacters()
        {
            var result = new List<CharacterEntity>(_characters.Count);
            for (int i = 0; i < _characters.Count; i++)
            {
                var c = _characters[i];
                if (c != null && c.Data != null && c.Data.IsAlive)
                    result.Add(c);
            }
            return result;
        }

        public static List<CharacterEntity> QueryCharactersInRadius(Vector3 center, float radius)
        {
            var result = new List<CharacterEntity>();
            var rSq = radius * radius;
            for (int i = 0; i < _characters.Count; i++)
            {
                var c = _characters[i];
                if (c == null || c.Data == null || !c.Data.IsAlive) continue;
                var diff = c.transform.position - center;
                diff.y = 0f;
                if (diff.sqrMagnitude <= rSq)
                    result.Add(c);
            }
            return result;
        }

        public static List<CharacterEntity> QueryByRace(MainRace race)
        {
            var result = new List<CharacterEntity>();
            for (int i = 0; i < _characters.Count; i++)
            {
                var c = _characters[i];
                if (c != null && c.Data != null && c.Data.IsAlive && c.Data.Race == race)
                    result.Add(c);
            }
            return result;
        }

        public static List<CharacterEntity> QueryBySubFaction(SubFactionId faction)
        {
            var result = new List<CharacterEntity>();
            for (int i = 0; i < _characters.Count; i++)
            {
                var c = _characters[i];
                if (c != null && c.Data != null && c.Data.IsAlive && c.Data.Faction == faction)
                    result.Add(c);
            }
            return result;
        }
    }
}

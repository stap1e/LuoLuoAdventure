using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    /// <summary>
    /// 子阵营间关系矩阵。对称存储，支持运行时变更与快照。
    /// </summary>
    [Serializable]
    public class FactionRelationshipMatrix
    {
        private readonly Dictionary<(SubFactionId, SubFactionId), int> _values = new();

        public static FactionRelationshipMatrix CreateDefault()
        {
            var matrix = new FactionRelationshipMatrix();
            matrix.InitializeDefaults();
            return matrix;
        }

        public void InitializeDefaults()
        {
            _values.Clear();
            for (int i = 0; i < GameConstants.TotalSubFactionCount; i++)
            {
                for (int j = i; j < GameConstants.TotalSubFactionCount; j++)
                {
                    var a = (SubFactionId)i;
                    var b = (SubFactionId)j;
                    SetInternal(a, b, GameConstants.CreateDefaultRelationship(a, b));
                }
            }
        }

        public int Get(SubFactionId source, SubFactionId target)
        {
            var key = NormalizeKey(source, target);
            return _values.TryGetValue(key, out var value)
                ? value
                : GameConstants.CreateDefaultRelationship(source, target);
        }

        public RelationshipStance GetStance(SubFactionId source, SubFactionId target) =>
            GameConstants.ValueToStance(Get(source, target));

        public bool IsHostile(SubFactionId source, SubFactionId target) =>
            GetStance(source, target) <= RelationshipStance.Unfriendly;

        public bool IsFriendly(SubFactionId source, SubFactionId target) =>
            GetStance(source, target) >= RelationshipStance.Friendly;

        public int Set(SubFactionId source, SubFactionId target, int value, string reason = null)
        {
            var oldValue = Get(source, target);
            var clamped = Clamp(value);
            SetInternal(source, target, clamped);
            return oldValue;
        }

        public int Modify(SubFactionId source, SubFactionId target, int delta)
        {
            var current = Get(source, target);
            SetInternal(source, target, Clamp(current + delta));
            return current;
        }

        public FactionRelationshipSnapshot CreateSnapshot() =>
            new(_values);

        public void RestoreFromSnapshot(FactionRelationshipSnapshot snapshot)
        {
            _values.Clear();
            foreach (var entry in snapshot.Entries)
                SetInternal(entry.Source, entry.Target, entry.Value);
        }

        private void SetInternal(SubFactionId a, SubFactionId b, int value)
        {
            _values[NormalizeKey(a, b)] = Clamp(value);
        }

        private static (SubFactionId, SubFactionId) NormalizeKey(SubFactionId a, SubFactionId b)
        {
            return (int)a <= (int)b ? (a, b) : (b, a);
        }

        private static int Clamp(int value) =>
            Math.Clamp(value, GameConstants.MinRelationshipValue, GameConstants.MaxRelationshipValue);
    }

    [Serializable]
    public struct FactionRelationshipEntry
    {
        public SubFactionId Source;
        public SubFactionId Target;
        public int Value;
    }

    /// <summary>关系矩阵快照，可用于存档或运营回滚</summary>
    [Serializable]
    public class FactionRelationshipSnapshot
    {
        public List<FactionRelationshipEntry> Entries = new();

        public FactionRelationshipSnapshot() { }

        internal FactionRelationshipSnapshot(Dictionary<(SubFactionId, SubFactionId), int> source)
        {
            foreach (var pair in source)
            {
                Entries.Add(new FactionRelationshipEntry
                {
                    Source = pair.Key.Item1,
                    Target = pair.Key.Item2,
                    Value = pair.Value
                });
            }
        }
    }
}

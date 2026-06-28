using System;

namespace LuoLuoTrip
{
    /// <summary>
    /// 阵营关系服务：封装矩阵操作，发布变更事件供运营/剧情系统订阅。
    /// </summary>
    public class FactionRelationshipService
    {
        public event Action<RelationshipChangeEvent> OnRelationshipChanged;

        public FactionRelationshipMatrix Matrix { get; private set; }

        public FactionRelationshipService()
        {
            Matrix = FactionRelationshipMatrix.CreateDefault();
        }

        public FactionRelationshipService(FactionRelationshipMatrix matrix)
        {
            Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
        }

        public int GetRelationship(SubFactionId source, SubFactionId target) =>
            Matrix.Get(source, target);

        public RelationshipStance GetStance(SubFactionId source, SubFactionId target) =>
            Matrix.GetStance(source, target);

        public void SetRelationship(SubFactionId source, SubFactionId target, int value, string reason = null)
        {
            var oldValue = Matrix.Set(source, target, value);
            PublishChange(source, target, oldValue, Matrix.Get(source, target), reason);
        }

        public void ModifyRelationship(SubFactionId source, SubFactionId target, int delta, string reason = null)
        {
            var oldValue = Matrix.Modify(source, target, delta);
            PublishChange(source, target, oldValue, Matrix.Get(source, target), reason);
        }

        /// <summary>运营事件：两族全面和解</summary>
        public void ApplyCrossRacePeace(string reason = "运营活动：两族和解")
        {
            for (int i = 0; i < GameConstants.MotorSubFactionCount; i++)
            {
                for (int j = GameConstants.MotorSubFactionCount; j < GameConstants.TotalSubFactionCount; j++)
                {
                    SetRelationship((SubFactionId)i, (SubFactionId)j, 0, reason);
                }
            }
        }

        /// <summary>运营事件：同族内讧</summary>
        public void ApplySameRaceConflict(MainRace race, int hostilityDelta, string reason = "运营活动：同族冲突")
        {
            var ids = new System.Collections.Generic.List<SubFactionId>();
            foreach (var def in SubFactionRegistry.GetByRace(race))
                ids.Add(def.Id);

            for (int i = 0; i < ids.Count; i++)
            {
                for (int j = i + 1; j < ids.Count; j++)
                    ModifyRelationship(ids[i], ids[j], hostilityDelta, reason);
            }
        }

        public FactionRelationshipSnapshot SaveSnapshot() => Matrix.CreateSnapshot();

        public void LoadSnapshot(FactionRelationshipSnapshot snapshot)
        {
            Matrix.RestoreFromSnapshot(snapshot);
        }

        private void PublishChange(SubFactionId source, SubFactionId target, int oldValue, int newValue, string reason)
        {
            if (oldValue == newValue) return;

            OnRelationshipChanged?.Invoke(new RelationshipChangeEvent
            {
                Source = source,
                Target = target,
                OldValue = oldValue,
                NewValue = newValue,
                Reason = reason ?? string.Empty
            });
        }
    }
}

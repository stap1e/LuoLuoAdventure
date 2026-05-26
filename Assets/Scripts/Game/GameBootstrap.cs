using LuoLuoTrip.Save;
using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>
    /// 游戏入口 MonoBehaviour：挂载到场景中即可初始化世界数据。
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SubFactionDatabaseSO _factionDatabase;

        [Header("World Init")]
        [SerializeField] private bool _spawnMinionSquads = true;
        [SerializeField] private int _minionsPerFaction = 5;
        [SerializeField] private bool _tryLoadSaveOnStart = false;
        [SerializeField] private bool _logInitialization = true;

        public static LuoLuoTripGameContext Context { get; private set; }
        public static SubFactionDatabaseSO FactionDatabase { get; private set; }

        private void Awake()
        {
            if (Context != null)
            {
                Debug.LogWarning("[LuoLuoTrip] GameContext already exists, skipping duplicate bootstrap.");
                return;
            }

            InitializeFactionDatabase();
            Context = new LuoLuoTripGameContext();
            Context.RelationshipService.OnRelationshipChanged += HandleRelationshipChanged;

            if (_tryLoadSaveOnStart && SaveService.SaveExists())
            {
                if (SaveService.TryRead(out var save))
                    Context.ApplySave(save);
                else
                    Context.InitializeWorld(_spawnMinionSquads, _minionsPerFaction);
            }
            else
            {
                Context.InitializeWorld(_spawnMinionSquads, _minionsPerFaction);
            }

            if (_logInitialization)
                LogWorldSummary();
        }

        private void OnDestroy()
        {
            if (Context?.RelationshipService != null)
                Context.RelationshipService.OnRelationshipChanged -= HandleRelationshipChanged;
        }

        private void InitializeFactionDatabase()
        {
            if (_factionDatabase == null)
                _factionDatabase = Resources.Load<SubFactionDatabaseSO>("SubFactionDatabase");

            FactionDatabase = _factionDatabase;
            if (_factionDatabase != null)
                SubFactionRegistry.Initialize(_factionDatabase);
            else
                Debug.LogWarning("[LuoLuoTrip] 未找到 SubFactionDatabase，使用内置默认阵营配置");
        }

        private void HandleRelationshipChanged(RelationshipChangeEvent evt)
        {
            if (!_logInitialization) return;

            Debug.Log($"[关系变更] {evt.Source} ↔ {evt.Target}: " +
                      $"{evt.OldValue} → {evt.NewValue} ({evt.OldStance} → {evt.NewStance}) " +
                      $"原因: {evt.Reason}");
        }

        private void LogWorldSummary()
        {
            Debug.Log("========== LuoLuoTrip 世界初始化 ==========");

            foreach (var pair in Context.FactionStates)
            {
                var state = pair.Value;
                var leader = state.Leader;
                Debug.Log($"[{state.Definition.DisplayName}] " +
                          $"领袖: {leader.DisplayName} Lv.{leader.Level} ({leader.Role}) | " +
                          $"成员: {state.Members.Count}");
            }

            Debug.Log("--- 跨族关系示例 ---");
            var motor = SubFactionId.MotorIronRiders;
            var beast = SubFactionId.BeastIronClaw;
            var rel = Context.RelationshipService.GetRelationship(motor, beast);
            Debug.Log($"{motor} vs {beast}: {rel} ({Context.RelationshipService.GetStance(motor, beast)})");

            Debug.Log("==========================================");
        }
    }
}

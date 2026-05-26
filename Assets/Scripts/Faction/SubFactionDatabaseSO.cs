using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip
{
    /// <summary>全部 9 个子阵营的配置表容器</summary>
    [CreateAssetMenu(fileName = "SubFactionDatabase", menuName = "LuoLuoTrip/Sub Faction Database")]
    public class SubFactionDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<SubFactionConfigSO> _configs = new();

        public IReadOnlyList<SubFactionConfigSO> Configs => _configs;

        public SubFactionConfigSO Get(SubFactionId id)
        {
            foreach (var config in _configs)
            {
                if (config != null && config.factionId == id)
                    return config;
            }
            return null;
        }

        public bool TryGet(SubFactionId id, out SubFactionConfigSO config)
        {
            config = Get(id);
            return config != null;
        }

        public void SetConfigs(IEnumerable<SubFactionConfigSO> configs)
        {
            _configs.Clear();
            _configs.AddRange(configs);
        }
    }
}

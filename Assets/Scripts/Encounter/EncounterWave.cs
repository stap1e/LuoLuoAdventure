using System;
using LuoLuoTrip.AI;
using UnityEngine;

namespace LuoLuoTrip
{
    public enum SpawnBehavior
    {
        Hold,
        Chase,
        Patrol,
        Defend
    }

    [Serializable]
    public class EncounterWave
    {
        public string waveId;
        public SubFactionId faction;
        public CharacterRole role = CharacterRole.Minion;
        public int unitCount;
        public float delaySeconds;
        public bool spawned;
        [Tooltip("Radius around spawn point to scatter units. 0 = use spawn point default.")]
        public float spawnRadius;
        [Tooltip("Initial AI behavior after spawn.")]
        public SpawnBehavior initialBehavior = SpawnBehavior.Chase;
        [Tooltip("If true, spawned units are hostile to player faction.")]
        public bool isHostile = true;
        [Tooltip("Optional behavior profile applied to spawned SimpleCombatAI units.")]
        public AIBehaviorProfileSO behaviorProfile;

        public bool IsReady => !spawned;
    }
}

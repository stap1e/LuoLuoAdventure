using System;
using UnityEngine;

namespace LuoLuoTrip
{
    [Serializable]
    public class EncounterWave
    {
        public string waveId;
        public SubFactionId faction;
        public CharacterRole role = CharacterRole.Minion;
        public int unitCount;
        public float delaySeconds;
        public bool spawned;

        public bool IsReady => !spawned;
    }
}

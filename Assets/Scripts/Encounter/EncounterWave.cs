using System;
using UnityEngine;

namespace LuoLuoTrip
{
    [Serializable]
    public class EncounterWave
    {
        public string waveId;
        public SubFactionId faction;
        public int unitCount;
        public float delaySeconds;
    }
}

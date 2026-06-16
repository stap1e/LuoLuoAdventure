using UnityEngine;

namespace LuoLuoTrip
{
    public class EncounterSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnPointId;
        [SerializeField] private SubFactionId _faction;

        public string SpawnPointId => _spawnPointId;
        public SubFactionId Faction => _faction;

        public Vector3 GetSpawnPosition()
        {
            return transform.position;
        }
    }
}

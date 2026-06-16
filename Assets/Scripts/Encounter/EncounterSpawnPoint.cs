using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using UnityEngine;
#if UNITY_2022_3_OR_NEWER
using UnityEngine.AI;
#endif

namespace LuoLuoTrip
{
    public class EncounterSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnPointId;
        [SerializeField] private SubFactionId _faction;
        [SerializeField] private CharacterRole _defaultRole = CharacterRole.Minion;
        [SerializeField] private float _spawnRadius = 2f;

        public string SpawnPointId => _spawnPointId;
        public SubFactionId Faction => _faction;
        public float SpawnRadius => _spawnRadius;

        public Vector3 GetSpawnPosition()
        {
            return transform.position;
        }

        public Vector3 GetRandomSpawnPosition()
        {
            var offset = Random.insideUnitSphere * _spawnRadius;
            offset.y = 0f;
            return transform.position + offset;
        }

        public GameObject SpawnUnit(CharacterData data, GameObject prefab = null)
        {
            var position = GetRandomSpawnPosition();
            GameObject unitGo;

            if (prefab != null)
            {
                unitGo = Instantiate(prefab, position, Quaternion.identity);
                unitGo.name = $"{data.DisplayName}_{data.Id}";
            }
            else
            {
                unitGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                unitGo.name = $"{data.DisplayName}_{data.Id}";
                unitGo.transform.position = position;
                var existingCollider = unitGo.GetComponent<Collider>();
                if (existingCollider != null)
                    DestroyImmediate(existingCollider);
                var capsule = unitGo.AddComponent<CapsuleCollider>();
                capsule.height = 2f;
                capsule.radius = 0.5f;
                capsule.center = new Vector3(0f, 1f, 0f);
            }

            var entity = unitGo.GetComponent<CharacterEntity>();
            if (entity == null)
                entity = unitGo.AddComponent<CharacterEntity>();

            // Guard runs BEFORE Bind() so Combatant.Awake (triggered via EnsureCombatant)
            // can grab the motor/rigidbody.
            CharacterRuntimeComponentGuard.EnsureForAI(unitGo);

            entity.Bind(data);

            if (unitGo.GetComponent<Combatant>() == null)
                unitGo.AddComponent<Combatant>();

            if (unitGo.GetComponent<SimpleCombatAI>() == null)
                unitGo.AddComponent<SimpleCombatAI>();

            if (unitGo.GetComponent<NavigationAgentBridge>() == null)
                unitGo.AddComponent<NavigationAgentBridge>();

            var navAgent = unitGo.GetComponent<NavMeshAgent>();
            if (navAgent == null)
                unitGo.AddComponent<NavMeshAgent>();

            return unitGo;
        }
    }
}

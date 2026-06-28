using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.UI;
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

        public Vector3 GetRandomSpawnPosition(float radius = -1f)
        {
            var r = radius > 0f ? radius : _spawnRadius;
            var offset = Random.insideUnitSphere * r;
            offset.y = 0f;
            return transform.position + offset;
        }

        public GameObject SpawnUnit(CharacterData data, GameObject prefab = null, float radius = -1f, SpawnBehavior behavior = SpawnBehavior.Chase)
        {
            return SpawnUnit(data, prefab, radius, behavior, null);
        }

        public GameObject SpawnUnit(CharacterData data, GameObject prefab, float radius, SpawnBehavior behavior, AIBehaviorProfileSO behaviorProfile)
        {
            var position = GetRandomSpawnPosition(radius);
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

            var ai = unitGo.GetComponent<SimpleCombatAI>();
            if (ai == null)
                ai = unitGo.AddComponent<SimpleCombatAI>();
            ai.BehaviorProfile = behaviorProfile;

            if (unitGo.GetComponent<NavigationAgentBridge>() == null)
                unitGo.AddComponent<NavigationAgentBridge>();

            if (unitGo.GetComponent<AICombatNavigationController>() == null)
                unitGo.AddComponent<AICombatNavigationController>();

            var navAgent = unitGo.GetComponent<NavMeshAgent>();
            if (navAgent == null)
                unitGo.AddComponent<NavMeshAgent>();

            // Combat readability: health bar + hit flash for all dynamic AI units.
            if (unitGo.GetComponent<CombatantHealthBarPresenter>() == null)
                unitGo.AddComponent<CombatantHealthBarPresenter>();
            if (unitGo.GetComponent<HitFlashFeedback>() == null)
                unitGo.AddComponent<HitFlashFeedback>();

            ApplyInitialBehavior(unitGo, behavior);

            return unitGo;
        }

        private static void ApplyInitialBehavior(GameObject unitGo, SpawnBehavior behavior)
        {
            var ai = unitGo.GetComponent<SimpleCombatAI>();
            if (ai == null) return;

            switch (behavior)
            {
                case SpawnBehavior.Hold:
                    ai.HoldPosition = unitGo.transform.position;
                    break;
                case SpawnBehavior.Defend:
                    ai.HoldPosition = unitGo.transform.position;
                    break;
                case SpawnBehavior.Patrol:
                    // Default AI behavior (chase + return to spawn on disengage) already covers patrol.
                    break;
                case SpawnBehavior.Chase:
                default:
                    // Default behavior — find and chase hostile targets.
                    break;
            }
        }
    }
}

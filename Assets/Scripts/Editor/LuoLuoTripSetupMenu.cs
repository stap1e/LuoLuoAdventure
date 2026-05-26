#if UNITY_EDITOR
using System.Collections.Generic;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Animation;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.Save;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    public static class LuoLuoTripSetupMenu
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string CombatScenePath = "Assets/Scenes/CombatPrototype.unity";
        private const string FactionDataFolder = "Assets/Data/Factions";
        private const string DatabasePath = "Assets/Data/Factions/SubFactionDatabase.asset";
        private const string ResourcesDatabasePath = "Assets/Resources/SubFactionDatabase.asset";
        private const string HitFeedbackProfilePath = "Assets/Data/HitFeedbackProfile.asset";

        [MenuItem("LuoLuoTrip/Setup/Generate All Sub Faction Configs")]
        public static void GenerateAllSubFactionConfigs()
        {
            EnsureFolder(FactionDataFolder);
            var configs = new List<SubFactionConfigSO>();

            foreach (SubFactionId id in System.Enum.GetValues(typeof(SubFactionId)))
            {
                var assetPath = $"{FactionDataFolder}/{id}.asset";
                var config = AssetDatabase.LoadAssetAtPath<SubFactionConfigSO>(assetPath);
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<SubFactionConfigSO>();
                    config.factionId = id;
                    AssetDatabase.CreateAsset(config, assetPath);
                }

                config.SetDefaultsFromId();
                EditorUtility.SetDirty(config);
                configs.Add(config);
            }

            var database = AssetDatabase.LoadAssetAtPath<SubFactionDatabaseSO>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<SubFactionDatabaseSO>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }
            database.SetConfigs(configs);
            EditorUtility.SetDirty(database);

            EnsureFolder("Assets/Resources");
            var resourcesDb = AssetDatabase.LoadAssetAtPath<SubFactionDatabaseSO>(ResourcesDatabasePath);
            if (resourcesDb == null)
            {
                AssetDatabase.CopyAsset(DatabasePath, ResourcesDatabasePath);
            }
            else
            {
                resourcesDb.SetConfigs(configs);
                EditorUtility.SetDirty(resourcesDb);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = database;

            Debug.Log($"[LuoLuoTrip] 已生成 9 个阵营配置 + Database: {DatabasePath}");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();
            bootstrapGo.AddComponent<SaveLoadManager>();

            new GameObject("--- LuoLuoTrip World ---");

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[LuoLuoTrip] Bootstrap 场景已创建: {BootstrapScenePath}");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Combat Prototype Scene")]
        public static void CreateCombatPrototypeScene()
        {
            GenerateAllSubFactionConfigs();
            InitializeRegistryFromDatabase();

            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();
            var saveManager = bootstrapGo.AddComponent<SaveLoadManager>();
            SetupHitFeedback(bootstrapGo);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3f, 1f, 3f);

            var player = CreateCombatCharacter(
                "Player",
                new Vector3(0f, 0.5f, 0f),
                CharacterData.Create("player_001", "流浪者", SubFactionId.MotorIronRiders, CharacterRole.Common),
                isPlayer: true);
            saveManager.SetPlayerCharacterId("player_001");

            CreateCombatCharacter(
                "Enemy_Beast",
                new Vector3(5f, 0.5f, 3f),
                CharacterData.Create("enemy_beast_001", "铁爪部·小兵", SubFactionId.BeastIronClaw, CharacterRole.Minion),
                isPlayer: false);

            ApplyAnimatorToGameObject(player);

            EditorSceneManager.SaveScene(scene, CombatScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[LuoLuoTrip] 战斗原型场景已创建: {CombatScenePath}");
            Debug.Log("操作说明: WASD移动 | 鼠标左键攻击 | Space闪避 | Q锁定 | Tab切换目标 | F5存档 | F9读档");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Hit Feedback Profile")]
        public static void CreateHitFeedbackProfile()
        {
            EnsureFolder("Assets/Data");
            var profile = AssetDatabase.LoadAssetAtPath<HitFeedbackProfileSO>(HitFeedbackProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<HitFeedbackProfileSO>();
                AssetDatabase.CreateAsset(profile, HitFeedbackProfilePath);
            }
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            Debug.Log($"[LuoLuoTrip] HitFeedbackProfile 已创建: {HitFeedbackProfilePath}");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Combat Animator Config")]
        public static void CreateCombatAnimatorConfig()
        {
            EnsureFolder("Assets/Data/Animation");
            const string path = "Assets/Data/Animation/CombatAnimatorConfig.asset";

            var config = AssetDatabase.LoadAssetAtPath<CombatAnimatorConfigSO>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CombatAnimatorConfigSO>();
                AssetDatabase.CreateAsset(config, path);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = config;
            Debug.Log($"[LuoLuoTrip] CombatAnimatorConfig 已创建: {path}");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Game Config Asset")]
        public static void CreateGameConfigAsset()
        {
            EnsureFolder("Assets/Data");
            const string path = "Assets/Data/GameConfig.asset";

            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = config;
            Debug.Log($"[LuoLuoTrip] GameConfig 已创建: {path}");
        }

        [MenuItem("LuoLuoTrip/Debug/Print World Summary")]
        public static void PrintWorldSummary()
        {
            var context = new LuoLuoTripGameContext();
            context.InitializeWorld(spawnMinionSquads: true, minionsPerFaction: 3);

            foreach (var pair in context.FactionStates)
            {
                var s = pair.Value;
                Debug.Log($"{s.Definition.DisplayName}: 领袖 {s.Leader.DisplayName} Lv.{s.Leader.Level}, 成员 {s.Members.Count}");
            }
        }

        private static void SetupHitFeedback(GameObject bootstrapGo)
        {
            CreateHitFeedbackProfile();
            var profile = AssetDatabase.LoadAssetAtPath<HitFeedbackProfileSO>(HitFeedbackProfilePath);

            var hub = bootstrapGo.AddComponent<CombatHitFeedbackHub>();
            if (profile != null)
            {
                var so = new SerializedObject(hub);
                so.FindProperty("_profile").objectReferenceValue = profile;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var cam = Camera.main;
            if (cam != null && cam.GetComponent<CameraShakeService>() == null)
                cam.gameObject.AddComponent<CameraShakeService>();
        }

        private static GameObject CreateCombatCharacter(string name, Vector3 position, CharacterData data, bool isPlayer)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = position;

            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(data);

            var combatant = go.GetComponent<Combatant>();
            if (isPlayer)
                go.AddComponent<CombatController>();
            else
                go.AddComponent<SimpleCombatAI>();

            if (isPlayer)
            {
                var hudGo = new GameObject("CombatHUD");
                hudGo.AddComponent<CombatDebugHUD>();
            }

            var config = SubFactionRegistry.GetConfig(data.Faction);
            if (config != null)
            {
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = config.themeColor;
            }

            return go;
        }

        private static void ApplyAnimatorToGameObject(GameObject go)
        {
            var result = CombatAnimatorControllerGenerator.Generate();

            var animator = go.GetComponent<Animator>();
            if (animator == null)
                animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = result.Controller;

            var bridge = go.GetComponent<AnimatorCombatBridge>();
            if (bridge == null)
                bridge = go.AddComponent<AnimatorCombatBridge>();

            var bridgeSo = new SerializedObject(bridge);
            bridgeSo.FindProperty("_config").objectReferenceValue = result.Config;
            bridgeSo.FindProperty("_animator").objectReferenceValue = animator;
            bridgeSo.ApplyModifiedPropertiesWithoutUndo();

            var procedural = go.GetComponent<ProceduralCombatAnimator>();
            if (procedural != null)
                Object.DestroyImmediate(procedural);

            var driver = go.GetComponent<CombatAnimationDriver>();
            if (driver != null)
            {
                var driverSo = new SerializedObject(driver);
                driverSo.FindProperty("_preferAnimatorBridge").boolValue = true;
                driverSo.FindProperty("_useProceduralFallback").boolValue = false;
                driverSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void InitializeRegistryFromDatabase()
        {
            var db = AssetDatabase.LoadAssetAtPath<SubFactionDatabaseSO>(DatabasePath);
            if (db != null)
                SubFactionRegistry.Initialize(db);
        }

        private static void EnsureFolder(string path)
        {
            EnsureFolderPublic(path);
        }

        public static void EnsureFolderPublic(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif

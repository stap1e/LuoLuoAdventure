#if UNITY_EDITOR
using System.Collections.Generic;
using LuoLuoTrip.AI;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Animation;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.Feedback;
using LuoLuoTrip.Save;
using LuoLuoTrip.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    public static class LuoLuoTripSetupMenu
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string CombatScenePath = "Assets/Scenes/CombatPrototype.unity";
        private const string CommanderScenePath = "Assets/Scenes/CommanderPrototype.unity";
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
            PlaceholderAssetGenerator.GenerateAll();
            CreateCombatTuningConfig();

            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();
            var saveManager = bootstrapGo.AddComponent<SaveLoadManager>();
            SetupHitFeedback(bootstrapGo);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            MarkNavMeshStatic(ground);

            var player = CreateCombatCharacter(
                "Player",
                new Vector3(0f, 0.5f, 0f),
                CharacterData.Create("player_001", "流浪者", SubFactionId.MotorIronRiders, CharacterRole.Common),
                PlaceholderAssetGenerator.PlayerCommanderPrefab,
                isPlayer: true);
            saveManager.SetPlayerCharacterId("player_001");

            CreateCombatCharacter(
                "Enemy_Beast",
                new Vector3(5f, 0.5f, 3f),
                CharacterData.Create("enemy_beast_001", "铁爪部·小兵", SubFactionId.BeastIronClaw, CharacterRole.Minion),
                PlaceholderAssetGenerator.BeastMinionPrefab,
                isPlayer: false);

            ApplyAnimatorToGameObject(player);

            var cameraGo = EnsureMainCamera();
            var cameraFollow = cameraGo.AddComponent<CameraFollowController>();
            cameraFollow.SetTarget(player.transform);

            bootstrapGo.AddComponent<RuntimeCameraBootstrap>();

            var debugGo = new GameObject("CombatPrototypeDebug");
            debugGo.AddComponent<CombatPrototypeDebugController>();

            EditorSceneManager.SaveScene(scene, CombatScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[LuoLuoTrip] 战斗原型场景已创建: {CombatScenePath}");
            Debug.Log("操作说明: WASD移动 | 鼠标左键攻击 | Space闪避 | Q锁定 | Tab切换目标 | F5存档 | F9读档");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Commander Prototype Data")]
        public static void CreateCommanderPrototypeData()
        {
            EnsureFolder("Assets/Data/Missions");
            EnsureFolder("Assets/Data/Commander");

            var mission = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/ConvoyEscort.asset");
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinitionSO>();
                mission.MissionId = "convoy_escort_01";
                mission.DisplayName = "护送机车族运输队";
                mission.Description = "保护机车族运输队安全通过猛兽族领地，击退袭击者。";
                mission.RecommendedCommanderLevel = 1;
                mission.DefaultObjectives = new List<MissionObjective>
                {
                    new MissionObjective { ObjectiveId = "protect_convoy", Description = "保护运输队", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "defeat_ambush", Description = "击退伏击", RequiredProgress = 3 }
                };
                AssetDatabase.CreateAsset(mission, "Assets/Data/Missions/ConvoyEscort.asset");
                Debug.Log("[MissionDefinition] Created ConvoyEscort.asset");
            }
            else
            {
                PopulateMissionDefinitionIfMissing(mission, "convoy_escort_01", "护送机车族运输队",
                    "保护机车族运输队安全通过猛兽族领地，击退袭击者。", 1,
                    new List<MissionObjective>
                    {
                        new MissionObjective { ObjectiveId = "protect_convoy", Description = "保护运输队", RequiredProgress = 1 },
                        new MissionObjective { ObjectiveId = "defeat_ambush", Description = "击退伏击", RequiredProgress = 3 }
                    },
                    new List<MissionOutcomeConsequenceMapping>());
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = mission;
            Debug.Log("[MissionDefinition] Commander Prototype Data ensured");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Mission Prototype Data")]
        public static void CreateMissionPrototypeData()
        {
            CreateCommanderPrototypeData();

            var raid = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/EnergyRaid.asset");
            if (raid == null)
            {
                raid = ScriptableObject.CreateInstance<MissionDefinitionSO>();
                raid.MissionId = "energy_raid_01";
                raid.DisplayName = "猛兽族能源抢夺";
                raid.Description = "帮助猛兽族从机车族控制区夺取能源。";
                raid.RecommendedCommanderLevel = 3;
                raid.DefaultObjectives = new List<MissionObjective>
                {
                    new MissionObjective { ObjectiveId = "reach_source", Description = "抵达能源点", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "extract_energy", Description = "夺取能源", RequiredProgress = 2 }
                };
                AssetDatabase.CreateAsset(raid, "Assets/Data/Missions/EnergyRaid.asset");
            }

            var balance = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/BalanceAllocation.asset");
            if (balance == null)
            {
                balance = ScriptableObject.CreateInstance<MissionDefinitionSO>();
                balance.MissionId = "balance_alloc_01";
                balance.DisplayName = "能源平衡分配";
                balance.Description = "尝试在机车族和猛兽族之间平衡分配能源，避免冲突升级。";
                balance.RecommendedCommanderLevel = 5;
                balance.DefaultObjectives = new List<MissionObjective>
                {
                    new MissionObjective { ObjectiveId = "negotiate", Description = "谈判分配方案", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "distribute", Description = "执行分配", RequiredProgress = 2 }
                };
                AssetDatabase.CreateAsset(balance, "Assets/Data/Missions/BalanceAllocation.asset");
            }

            EnsureMissionChainDefinition(
                "Assets/Data/Missions/ConvoyEnergyConflict.asset",
                DemoFlowManager.ConvoyMissionId,
                "Convoy Energy Conflict",
                "Protect the convoy, share energy with the contested node, and keep casualties low.",
                new List<MissionObjective>
                {
                    new MissionObjective { ObjectiveId = "protect_convoy", Description = "Protect convoy", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "share_energy", Description = "Share energy", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "avoid_excessive_casualties", Description = "Avoid excessive casualties", RequiredProgress = 1 }
                },
                BuildSharedMissionOutcomeMappings(
                    "Mecha convoy secured; Beast hostility rises around the energy route.",
                    "Beast forces seize leverage; Mecha trust drops.",
                    "Energy shared; mainstream hostility drops while extremists remain.",
                    "Convoy survives with unresolved tension and limited trust gain.",
                    "Convoy route destabilized; both sides lose confidence."));

            EnsureMissionChainDefinition(
                "Assets/Data/Missions/BorderRetaliation.asset",
                DemoFlowManager.BorderMissionId,
                "Border Retaliation",
                "Survive the border retaliation, defeat raiders, protect allied units, and keep casualties low.",
                new List<MissionObjective>
                {
                    new MissionObjective { ObjectiveId = "survive_retaliation", Description = "Survive retaliation", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "defeat_raiders", Description = "Defeat raiders", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "protect_allied_units", Description = "Protect allied units", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "keep_casualties_low", Description = "Keep casualties low", RequiredProgress = 1 }
                },
                BuildSharedMissionOutcomeMappings(
                    "Mecha border line holds; Beast retaliation pressure increases.",
                    "Beast retaliation succeeds; Mecha support weakens.",
                    "Border ceasefire stabilizes the route into City Gate.",
                    "Retaliation contained, but both sides remain wary.",
                    "Border defense fails; city stability is at risk."));

            var cityGate = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/CityGateDispute.asset");
            if (cityGate == null)
            {
                cityGate = ScriptableObject.CreateInstance<MissionDefinitionSO>();
                AssetDatabase.CreateAsset(cityGate, "Assets/Data/Missions/CityGateDispute.asset");
                Debug.Log("[MissionDefinition] Created CityGateDispute.asset");
            }
            PopulateMissionDefinitionIfMissing(
                cityGate,
                "city_gate_dispute",
                "City Gate Dispute",
                "Mediate a city-gate standoff while protecting the core, preserving the Beast negotiator, and containing extremist raiders.",
                1,
                new List<MissionObjective>
                {
                    new MissionObjective { ObjectiveId = "protect_core", Description = "Protect CityGateCore", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "protect_negotiator", Description = "Keep BeastNegotiator alive", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "defeat_raiders", Description = "Defeat BeastRaiders", RequiredProgress = 1 },
                    new MissionObjective { ObjectiveId = "keep_casualties_low", Description = "Keep casualties low", RequiredProgress = 1 }
                },
                new List<MissionOutcomeConsequenceMapping>
                {
                    new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.BalancedMediation, CommanderExperienceDelta = 350, SummaryText = "Mainstream hostility reduced below 40; extremists remain contained." },
                    new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.MechaSuppression, CommanderExperienceDelta = 250, SummaryText = "Mecha order is restored, but Beast trust suffers." },
                    new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.BeastNegotiation, CommanderExperienceDelta = 250, SummaryText = "Beast hostility drops after negotiation, while Mecha support softens." },
                    new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.FailedEscalation, CommanderExperienceDelta = 30, SummaryText = "The city gate collapses into broader faction hostility." },
                    new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.PartialContainment, CommanderExperienceDelta = 100, SummaryText = "The dispute is contained, but casualties leave both sides wary." }
                });

            EditorUtility.SetDirty(raid);
            EditorUtility.SetDirty(balance);
            EditorUtility.SetDirty(cityGate);
            AssetDatabase.SaveAssets();
            Debug.Log("[MissionDefinition] Mission Prototype Data ensured");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Commander Mission Prototype Scene")]
        public static void CreateCommanderMissionPrototypeScene()
        {
            GenerateAllSubFactionConfigs();
            InitializeRegistryFromDatabase();
            CreateCommanderPrototypeData();
            CreateMissionPrototypeData();
            CreateHitFeedbackProfile();
            CreateCombatTuningConfig();
            CreateAudioFeedbackProfile();
            CreateWorldMarkerProfile();
            CreateAIBehaviorProfiles();
            PlaceholderAssetGenerator.GenerateAll();

            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();
            var saveManager = bootstrapGo.AddComponent<SaveLoadManager>();
            SetupHitFeedback(bootstrapGo);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
            MarkNavMeshStatic(ground);

            CreateAIBehaviorProfiles();
            var aggressiveRaiderProfile = LoadAIProfile(AIBehaviorProfileType.AggressiveRaider);
            var defensiveGuardProfile = LoadAIProfile(AIBehaviorProfileType.DefensiveGuard);
            var negotiatorProfile = LoadAIProfile(AIBehaviorProfileType.Negotiator);
            var hardlinerProfile = LoadAIProfile(AIBehaviorProfileType.Hardliner);
            var commanderUnitProfile = LoadAIProfile(AIBehaviorProfileType.CommanderUnit);

            var playerData = CharacterData.Create("player_cmd_001", "见习机战王", SubFactionId.MotorIronRiders, CharacterRole.Common);
            playerData.CommandRank = 1;
            var player = CreateCombatCharacter("Player_Commander", new Vector3(0f, 0.5f, 0f), playerData,
                PlaceholderAssetGenerator.PlayerCommanderPrefab, isPlayer: true);
            saveManager.SetPlayerCharacterId("player_cmd_001");

            var mechaMinionData = CharacterData.Create("mecha_minion_001", "铁骑团·小兵", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            mechaMinionData.CommandRank = 1;
            CreateCombatCharacter("Mecha_Minion", new Vector3(-3f, 0.5f, 2f), mechaMinionData,
                PlaceholderAssetGenerator.MechaMinionPrefab, isPlayer: false);

            var beastMinionData = CharacterData.Create("beast_minion_001", "铁爪部·小兵", SubFactionId.BeastIronClaw, CharacterRole.Minion);
            beastMinionData.CommandRank = 1;
            CreateCombatCharacter("Beast_Minion", new Vector3(5f, 0.5f, 3f), beastMinionData,
                PlaceholderAssetGenerator.BeastMinionPrefab, isPlayer: false);

            var highRankData = CharacterData.Create("city_lord_001", "铁骑团·城主", SubFactionId.MotorIronRiders, CharacterRole.CityLord);
            CreateCombatCharacter("HighRank_CityLord", new Vector3(-6f, 0.5f, -2f), highRankData,
                PlaceholderAssetGenerator.CityLordPrefab, isPlayer: false);

            var warKingData = CharacterData.Create("war_king_001", "铁爪部·战王", SubFactionId.BeastIronClaw, CharacterRole.WarKing);
            CreateCombatCharacter("HighRank_WarKing", new Vector3(8f, 0.5f, -3f), warKingData,
                PlaceholderAssetGenerator.WarKingPrefab, isPlayer: false);

            var convoyObj = CreateObjective("Convoy_Objective", new Vector3(-2f, 0.3f, 5f),
                PlaceholderAssetGenerator.ConvoyPrefab);
            AttachSceneMarker(convoyObj, WorldMarkerType.MissionObjective, "Convoy");
            var convoyComponent = convoyObj.AddComponent<ConvoyObjective>();

            var energyObj = CreateObjective("Energy_Node", new Vector3(4f, 0.2f, -4f),
                PlaceholderAssetGenerator.EnergyNodePrefab);
            AttachSceneMarker(energyObj, WorldMarkerType.Interactable, "Energy Node");
            var energyComponent = energyObj.AddComponent<EnergyNodeObjective>();

            var triggerGo = new GameObject("MissionTriggerZone");
            triggerGo.transform.position = new Vector3(0f, 0f, 2f);
            var triggerZone = triggerGo.AddComponent<MissionTriggerZone>();

            ApplyAnimatorToGameObject(player);

            player.AddComponent<CommanderControlController>();

            var cameraGo = EnsureMainCamera();
            var cameraFollow = cameraGo.AddComponent<CameraFollowController>();
            cameraFollow.SetTarget(player.transform);

            bootstrapGo.AddComponent<RuntimeCameraBootstrap>();

            var commanderHud = new GameObject("CommanderDebugHud").AddComponent<CommanderDebugHud>();
            var factionPanel = new GameObject("FactionStandingDebugPanel").AddComponent<FactionStandingDebugPanel>();
            var missionPanel = new GameObject("MissionResultDebugPanel").AddComponent<MissionResultDebugPanel>();
            var objectiveHud = new GameObject("MissionObjectiveHud").AddComponent<MissionObjectiveHud>();
            var summaryPanel = new GameObject("MissionResultSummaryPanel").AddComponent<MissionResultSummaryPanel>();
            var hintPanel = new GameObject("CommanderControlHintPanel").AddComponent<CommanderControlHintPanel>();
            var toastPanel = new GameObject("FactionDeltaToastPanel").AddComponent<FactionDeltaToastPanel>();
            var chainSummaryPanel = new GameObject("MissionChainSummaryPanel").AddComponent<MissionChainSummaryPanel>();
            var demoFlowManager = new GameObject("DemoFlowManager").AddComponent<DemoFlowManager>();
            var demoFlowHud = new GameObject("DemoFlowHud").AddComponent<DemoFlowHud>();
            demoFlowHud.SetFlowManager(demoFlowManager);
            objectiveHud.SetDemoFlowManager(demoFlowManager);

            new GameObject("[AudioFeedbackService]").AddComponent<AudioFeedbackService>();
            new GameObject("[WorldMarkerService]").AddComponent<WorldMarkerService>();

            CreateAreaLabel("Area_Tutorial", new Vector3(0f, 0f, 0f), "Tutorial Area", new Color(0.6f, 0.85f, 1f));
            CreateAreaLabel("Area_ConvoyMission", new Vector3(0f, 0f, 5f), "Convoy Mission", new Color(0.3f, 1f, 0.5f));
            CreateAreaLabel("Area_BorderRetaliation", new Vector3(25f, 0f, 0f), "Border Retaliation", new Color(1f, 0.6f, 0.3f));
            CreateAreaLabel("Area_AdvancedShowcase", new Vector3(22f, 0f, -2f), "Advanced Units", new Color(1f, 0.85f, 0.2f));
            CreateAreaLabel("Area_CityGateDispute", new Vector3(50f, 0f, 0f), "City Gate Mission Area", new Color(0.8f, 0.5f, 1f));

            // === Mission 3: CityGateDispute ===
            var cityGateTriggerGo = new GameObject("CityGateDisputeTrigger");
            cityGateTriggerGo.transform.position = new Vector3(50f, 0f, 0f);
            var cityGateTrigger = cityGateTriggerGo.AddComponent<MissionTriggerZone>();
            var cityGateTriggerSo = new SerializedObject(cityGateTrigger);
            cityGateTriggerSo.FindProperty("_missionId").stringValue = "city_gate_dispute";
            cityGateTriggerSo.FindProperty("_zoneRadius").floatValue = 12f;
            cityGateTriggerSo.ApplyModifiedPropertiesWithoutUndo();

            var cityGateCoreObj = CreateObjective("CityGateCore_Objective", new Vector3(50f, 0.3f, 2f),
                PlaceholderAssetGenerator.EnergyNodePrefab);
            cityGateCoreObj.AddComponent<WorldMarker>().Configure(WorldMarkerType.MissionObjective, cityGateCoreObj.transform, "[CORE] CityGateCore");
            var cityGateCoreEntity = cityGateCoreObj.AddComponent<CharacterEntity>();
            cityGateCoreEntity.Bind(CharacterData.Create("citygate_core", "CityGateCore", SubFactionId.MotorIronRiders, CharacterRole.Minion));
            var cityGateCoreCombatant = cityGateCoreObj.GetComponent<Combatant>();
            if (cityGateCoreCombatant == null)
                cityGateCoreCombatant = cityGateCoreObj.AddComponent<Combatant>();
            var cityGateCoreBar = cityGateCoreObj.AddComponent<CombatantHealthBarPresenter>();
            var cityGateCoreBarSo = new SerializedObject(cityGateCoreBar);
            cityGateCoreBarSo.FindProperty("_combatant").objectReferenceValue = cityGateCoreCombatant;
            cityGateCoreBarSo.FindProperty("_hideOnDeath").boolValue = true;
            cityGateCoreBarSo.FindProperty("_label").stringValue = "CityGate Core";
            cityGateCoreBarSo.ApplyModifiedPropertiesWithoutUndo();

            var mechaGuardData = CharacterData.Create("mecha_guard_001", "MechaGateGuard", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            mechaGuardData.CommandRank = 1;
            mechaGuardData.TrustToPlayer = 20;
            var mechaGuardGo = CreateCombatCharacter("MechaGateGuard", new Vector3(47f, 0.5f, 1f), mechaGuardData,
                PlaceholderAssetGenerator.MechaMinionPrefab, isPlayer: false);
            AssignAIProfile(mechaGuardGo, defensiveGuardProfile, cityGateCoreObj.transform);
            AttachSceneMarker(mechaGuardGo, WorldMarkerType.FriendlyUnit, "Guard: Defensive | Can Receive Commands");

            var mechaHardlinerData = CharacterData.Create("mecha_hardliner_001", "MechaHardliner", SubFactionId.MotorStormGang, CharacterRole.Minion);
            mechaHardlinerData.CommandRank = 2;
            mechaHardlinerData.RequiredCommanderLevel = 5;
            mechaHardlinerData.TrustToPlayer = 10;
            mechaHardlinerData.AllowDirectControl = false;
            mechaHardlinerData.AllowTacticalCommand = true;
            var mechaHardlinerGo = CreateCombatCharacter("MechaHardliner", new Vector3(46f, 0.5f, 3f), mechaHardlinerData,
                PlaceholderAssetGenerator.MechaMinionPrefab, isPlayer: false);
            AssignAIProfile(mechaHardlinerGo, hardlinerProfile, null);
            AttachSceneMarker(mechaHardlinerGo, WorldMarkerType.FriendlyUnit, "Hardliner: Escalation risk | Tactical Command Only");

            var beastNegotiatorData = CharacterData.Create("beast_negotiator_001", "BeastNegotiator", SubFactionId.BeastShadowFang, CharacterRole.Minion);
            beastNegotiatorData.CommandRank = 1;
            beastNegotiatorData.TrustToPlayer = 5;
            beastNegotiatorData.AllowDirectControl = true;
            var beastNegotiatorGo = CreateCombatCharacter("BeastNegotiator", new Vector3(53f, 0.5f, 1f), beastNegotiatorData,
                PlaceholderAssetGenerator.BeastMinionPrefab, isPlayer: false);
            AssignAIProfile(beastNegotiatorGo, negotiatorProfile, cityGateCoreObj.transform);
            AssignProtectedTarget(mechaHardlinerGo, beastNegotiatorGo.transform);
            AttachSceneMarker(beastNegotiatorGo, WorldMarkerType.MissionObjective, "Negotiator: Non-combatant | Protect it");
            var beastNegotiatorEntity = beastNegotiatorGo.GetComponent<CharacterEntity>();

            var beastRaiderData = CharacterData.Create("beast_raider_01", "BeastRaider_01", SubFactionId.BeastIronClaw, CharacterRole.Minion);
            beastRaiderData.CommandRank = 1;
            beastRaiderData.RequiredCommanderLevel = 1;
            beastRaiderData.TrustToPlayer = 60;
            beastRaiderData.AllowDirectControl = true;
            beastRaiderData.AllowTacticalCommand = true;
            var beastRaiderGo = CreateCombatCharacter("BeastRaider_01", new Vector3(55f, 0.5f, 1f), beastRaiderData,
                PlaceholderAssetGenerator.BeastMinionPrefab, isPlayer: false);
            AssignAIProfile(beastRaiderGo, aggressiveRaiderProfile, cityGateCoreObj.transform);
            AttachSceneMarker(beastRaiderGo, WorldMarkerType.HostileUnit, "Raider: Aggressive | FocusFire target");

            var cityLordData = CharacterData.Create("city_lord_gate_001", "CityLord", SubFactionId.MotorIronRiders, CharacterRole.CityLord);
            var cityLordGo = CreateCombatCharacter("CityLord_HighRank", new Vector3(48f, 0.5f, -2f), cityLordData,
                PlaceholderAssetGenerator.CityLordPrefab, isPlayer: false);
            AssignAIProfile(cityLordGo, commanderUnitProfile, cityGateCoreObj.transform);
            AttachSceneMarker(cityLordGo, WorldMarkerType.FriendlyUnit, "CommanderUnit: Tactical only | CityLord");

            var warKingGateData = CharacterData.Create("war_king_gate_001", "WarKing", SubFactionId.BeastIronClaw, CharacterRole.WarKing);
            var warKingGo = CreateCombatCharacter("WarKing_HighRank", new Vector3(54f, 0.5f, -2f), warKingGateData,
                PlaceholderAssetGenerator.WarKingPrefab, isPlayer: false);
            AssignAIProfile(warKingGo, commanderUnitProfile, cityGateCoreObj.transform);
            AttachSceneMarker(warKingGo, WorldMarkerType.HostileUnit, "CommanderUnit: Tactical only | WarKing");

            var cityGateConflictGo = new GameObject("CityGateDispute");
            var cityGateConflict = cityGateConflictGo.AddComponent<CityGateDisputeRuntime>();
            cityGateConflictGo.AddComponent<EncounterRuntime>();
            cityGateConflictGo.AddComponent<MissionAreaRuntime>();
            var cityGateConflictSo = new SerializedObject(cityGateConflict);
            cityGateConflictSo.FindProperty("_triggerZone").objectReferenceValue = cityGateTrigger;
            cityGateConflictSo.FindProperty("_objectiveHud").objectReferenceValue = objectiveHud;
            cityGateConflictSo.FindProperty("_cityGateCore").objectReferenceValue = cityGateCoreCombatant;
            cityGateConflictSo.FindProperty("_beastNegotiator").objectReferenceValue = beastNegotiatorEntity;
            cityGateConflictSo.ApplyModifiedPropertiesWithoutUndo();

            var cityGateBeastSpawn = new GameObject("CityGateSpawnPoint_Beast");
            cityGateBeastSpawn.transform.position = new Vector3(55f, 0f, 3f);
            cityGateBeastSpawn.AddComponent<WorldMarker>().Configure(WorldMarkerType.HostileUnit, cityGateBeastSpawn.transform, "BeastRaider Spawn");
            var cityGateBeastSpawnComp = cityGateBeastSpawn.AddComponent<EncounterSpawnPoint>();
            var cityGateBeastSpawnSo = new SerializedObject(cityGateBeastSpawnComp);
            cityGateBeastSpawnSo.FindProperty("_spawnPointId").stringValue = "citygate_beast_spawn";
            cityGateBeastSpawnSo.FindProperty("_faction").enumValueIndex = (int)SubFactionId.BeastIronClaw;
            cityGateBeastSpawnSo.ApplyModifiedPropertiesWithoutUndo();

            var cityGateMechaSpawn = new GameObject("CityGateSpawnPoint_Mecha");
            cityGateMechaSpawn.transform.position = new Vector3(45f, 0f, -1f);
            var cityGateMechaSpawnComp = cityGateMechaSpawn.AddComponent<EncounterSpawnPoint>();
            var cityGateMechaSpawnSo = new SerializedObject(cityGateMechaSpawnComp);
            cityGateMechaSpawnSo.FindProperty("_spawnPointId").stringValue = "citygate_mecha_spawn";
            cityGateMechaSpawnSo.FindProperty("_faction").enumValueIndex = (int)SubFactionId.MotorIronRiders;
            cityGateMechaSpawnSo.ApplyModifiedPropertiesWithoutUndo();

            if (cityGateConflictGo.GetComponent<EncounterRuntime>() is EncounterRuntime cityGateEncounter)
            {
                cityGateEncounter.AddSpawnPoint(cityGateBeastSpawnComp);
                cityGateEncounter.AddSpawnPoint(cityGateMechaSpawnComp);
                cityGateEncounter.SetWaves(new List<EncounterWave>
                {
                    new EncounterWave { waveId = "citygate_beast_raid_1", faction = SubFactionId.BeastIronClaw, role = CharacterRole.Minion, unitCount = 2, delaySeconds = 12f, behaviorProfile = aggressiveRaiderProfile },
                    new EncounterWave { waveId = "citygate_beast_raid_2", faction = SubFactionId.BeastIronClaw, role = CharacterRole.Minion, unitCount = 3, delaySeconds = 30f, behaviorProfile = aggressiveRaiderProfile },
                });
            }

            var conflictGo = new GameObject("ConvoyEnergyConflict");
            var conflict = conflictGo.AddComponent<ConvoyEnergyConflictRuntime>();
            conflictGo.AddComponent<EncounterRuntime>();
            conflictGo.AddComponent<MissionAreaRuntime>();
            var conflictSo = new SerializedObject(conflict);
            conflictSo.FindProperty("_convoy").objectReferenceValue = convoyComponent;
            conflictSo.FindProperty("_energyNode").objectReferenceValue = energyComponent;
            conflictSo.FindProperty("_triggerZone").objectReferenceValue = triggerZone;
            conflictSo.FindProperty("_objectiveHud").objectReferenceValue = objectiveHud;
            conflictSo.ApplyModifiedPropertiesWithoutUndo();

            var convoySpawn = new GameObject("SpawnPoint_Beast");
            convoySpawn.transform.position = new Vector3(5f, 0f, 3f);
            var beastSpawnComp = convoySpawn.AddComponent<EncounterSpawnPoint>();
            var beastSpawnSo = new SerializedObject(beastSpawnComp);
            beastSpawnSo.FindProperty("_spawnPointId").stringValue = "beast_spawn_main";
            beastSpawnSo.FindProperty("_faction").enumValueIndex = (int)SubFactionId.BeastIronClaw;
            beastSpawnSo.ApplyModifiedPropertiesWithoutUndo();

            var mechaSpawn = new GameObject("SpawnPoint_Mecha");
            mechaSpawn.transform.position = new Vector3(-3f, 0f, 2f);
            var mechaSpawnComp = mechaSpawn.AddComponent<EncounterSpawnPoint>();
            var mechaSpawnSo = new SerializedObject(mechaSpawnComp);
            mechaSpawnSo.FindProperty("_spawnPointId").stringValue = "mecha_spawn_main";
            mechaSpawnSo.FindProperty("_faction").enumValueIndex = (int)SubFactionId.MotorIronRiders;
            mechaSpawnSo.ApplyModifiedPropertiesWithoutUndo();

            if (conflictGo.GetComponent<EncounterRuntime>() is EncounterRuntime convoyEncounter)
            {
                convoyEncounter.AddSpawnPoint(beastSpawnComp);
                convoyEncounter.AddSpawnPoint(mechaSpawnComp);
                convoyEncounter.SetWaves(new List<EncounterWave>
                {
                    new EncounterWave { waveId = "convoy_beast_1", faction = SubFactionId.BeastIronClaw, role = CharacterRole.Minion, unitCount = 2, delaySeconds = 15f },
                    new EncounterWave { waveId = "convoy_beast_2", faction = SubFactionId.BeastIronClaw, role = CharacterRole.Minion, unitCount = 3, delaySeconds = 35f },
                });
            }

            var debugGo = new GameObject("CommanderPrototypeDebug");
            debugGo.AddComponent<PrototypeDebugController>();

            var runtimeGo = new GameObject("CommanderPrototypeRuntime");
            var runtime = runtimeGo.AddComponent<CommanderPrototypeRuntime>();
            var runtimeSo = new SerializedObject(runtime);
            runtimeSo.FindProperty("_commanderHud").objectReferenceValue = commanderHud;
            runtimeSo.FindProperty("_factionPanel").objectReferenceValue = factionPanel;
            runtimeSo.FindProperty("_missionPanel").objectReferenceValue = missionPanel;
            runtimeSo.FindProperty("_summaryPanel").objectReferenceValue = summaryPanel;
            runtimeSo.FindProperty("_hintPanel").objectReferenceValue = hintPanel;
            runtimeSo.FindProperty("_toastPanel").objectReferenceValue = toastPanel;
            runtimeSo.FindProperty("_chainSummaryPanel").objectReferenceValue = chainSummaryPanel;
            runtimeSo.ApplyModifiedPropertiesWithoutUndo();

            var tutorialGo = new GameObject("TutorialFlow");
            var tutorial = tutorialGo.AddComponent<TutorialFlowRuntime>();
            var combatController = player.GetComponent<CombatController>();
            var commanderController = player.GetComponent<CommanderControlController>();
            tutorial.Initialize(combatController, commanderController);
            commanderController.SetHintPanel(hintPanel);

            var borderTriggerGo = new GameObject("BorderRetaliationTrigger");
            borderTriggerGo.transform.position = new Vector3(25f, 0f, 0f);
            var borderTrigger = borderTriggerGo.AddComponent<MissionTriggerZone>();
            var borderTriggerSo = new SerializedObject(borderTrigger);
            borderTriggerSo.FindProperty("_missionId").stringValue = "border_retaliation";
            borderTriggerSo.FindProperty("_zoneRadius").floatValue = 10f;
            borderTriggerSo.ApplyModifiedPropertiesWithoutUndo();

            var borderObjectiveMarker = CreateObjective("Border_ObjectiveMarker", new Vector3(25f, 0.2f, 0f),
                PlaceholderAssetGenerator.ObjectiveMarkerPrefab);
            AttachSceneMarker(borderObjectiveMarker, WorldMarkerType.MissionObjective, "Allied Defense Point");

            var borderConflictGo = new GameObject("BorderRetaliation");
            var borderConflict = borderConflictGo.AddComponent<BorderRetaliationRuntime>();
            borderConflictGo.AddComponent<EncounterRuntime>();
            borderConflictGo.AddComponent<MissionAreaRuntime>();
            var borderConflictSo = new SerializedObject(borderConflict);
            borderConflictSo.FindProperty("_triggerZone").objectReferenceValue = borderTrigger;
            borderConflictSo.FindProperty("_objectiveHud").objectReferenceValue = objectiveHud;
            borderConflictSo.ApplyModifiedPropertiesWithoutUndo();

            var borderBeastSpawn = new GameObject("BorderSpawnPoint_Beast");
            borderBeastSpawn.transform.position = new Vector3(28f, 0f, 3f);
            AttachSceneMarker(borderBeastSpawn, WorldMarkerType.HostileUnit, "Raider Spawn");
            var borderBeastSpawnComp = borderBeastSpawn.AddComponent<EncounterSpawnPoint>();
            var borderBeastSpawnSo = new SerializedObject(borderBeastSpawnComp);
            borderBeastSpawnSo.FindProperty("_spawnPointId").stringValue = "border_beast_spawn";
            borderBeastSpawnSo.FindProperty("_faction").enumValueIndex = (int)SubFactionId.BeastIronClaw;
            borderBeastSpawnSo.ApplyModifiedPropertiesWithoutUndo();

            var borderMechaSpawn = new GameObject("BorderSpawnPoint_Mecha");
            borderMechaSpawn.transform.position = new Vector3(22f, 0f, -3f);
            var borderMechaSpawnComp = borderMechaSpawn.AddComponent<EncounterSpawnPoint>();
            var borderMechaSpawnSo = new SerializedObject(borderMechaSpawnComp);
            borderMechaSpawnSo.FindProperty("_spawnPointId").stringValue = "border_mecha_spawn";
            borderMechaSpawnSo.FindProperty("_faction").enumValueIndex = (int)SubFactionId.MotorIronRiders;
            borderMechaSpawnSo.ApplyModifiedPropertiesWithoutUndo();

            if (borderConflictGo.GetComponent<EncounterRuntime>() is EncounterRuntime borderEncounter)
            {
                borderEncounter.AddSpawnPoint(borderBeastSpawnComp);
                borderEncounter.AddSpawnPoint(borderMechaSpawnComp);
            }

            var rank2MechaData = CharacterData.Create("captain_001", "MechaCaptain", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            rank2MechaData.CommandRank = 2;
            rank2MechaData.RequiredCommanderLevel = 5;
            rank2MechaData.TrustToPlayer = 30;
            rank2MechaData.AllowDirectControl = false;
            rank2MechaData.AllowTacticalCommand = true;
            var rank2MechaGo = CreateCombatCharacter("MechaCaptain_Rank2", new Vector3(20f, 0.5f, -3f), rank2MechaData,
                PlaceholderAssetGenerator.MechaMinionPrefab, isPlayer: false);
            AssignAIProfile(rank2MechaGo, commanderUnitProfile, null);

            var rank2BeastData = CharacterData.Create("elite_001", "BeastElite", SubFactionId.BeastIronClaw, CharacterRole.Minion);
            rank2BeastData.CommandRank = 2;
            rank2BeastData.RequiredCommanderLevel = 5;
            rank2BeastData.TrustToPlayer = -10;
            rank2BeastData.AllowDirectControl = false;
            rank2BeastData.AllowTacticalCommand = true;
            var rank2BeastGo = CreateCombatCharacter("BeastElite_Rank2", new Vector3(28f, 0.5f, 2f), rank2BeastData,
                PlaceholderAssetGenerator.BeastMinionPrefab, isPlayer: false);
            AssignAIProfile(rank2BeastGo, commanderUnitProfile, null);

            var rank3DeputyData = CharacterData.Create("deputy_001", "DeputyCommander", SubFactionId.MotorIronRiders, CharacterRole.CityLord);
            rank3DeputyData.CommandRank = 3;
            rank3DeputyData.RequiredCommanderLevel = 10;
            rank3DeputyData.TrustToPlayer = 20;
            rank3DeputyData.IsHeroOrLeader = true;
            rank3DeputyData.AllowDirectControl = false;
            rank3DeputyData.AllowTacticalCommand = false;
            CreateCombatCharacter("DeputyCommander_Rank3", new Vector3(22f, 0.5f, -1f), rank3DeputyData,
                PlaceholderAssetGenerator.CityLordPrefab, isPlayer: false);

            EditorSceneManager.SaveScene(scene, CommanderScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[LuoLuoTrip] Commander Mission Prototype Scene 已创建: {CommanderScenePath}");
            Debug.Log("Controls: WASD move | LClick attack | Space dodge | Q lock-on | Tab select target | E interact/control | R release control | 1/2/3 test missions | F7 CityGate test | F8 CityGate teleport | F5 save | F9 load | F10 clear save");
            Debug.Log("Areas: Tutorial (0,0,0) | Convoy Mission (0,0,5) | Border Retaliation (25,0,0) | Advanced Units (22,0,-2) | City Gate Dispute (50,0,0)");
            Debug.Log("Mission 1: ConvoyEnergyConflict at (0,0,2) | Mission 2: BorderRetaliation at (25,0,0) | Mission 3: CityGateDispute at (50,0,0)");
            Debug.Log("Services: AudioFeedbackService + WorldMarkerService spawned (profiles in Resources/)");
            Debug.Log("Encounter: EncounterRuntime + MissionAreaRuntime on each mission | NavMeshAgent on AI units (fallback if no NavMesh baked)");
            Debug.Log("Manual validation: Play scene → complete tutorial → trigger mission 1 → complete → walk to mission 2 → verify branch → F5/F9 cycle → F10 clear");
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

        [MenuItem("LuoLuoTrip/Setup/Create Combat Tuning Config")]
        public static void CreateCombatTuningConfig()
        {
            EnsureFolder("Assets/Data/Combat");
            EnsureFolder("Assets/Resources");
            const string path = "Assets/Data/Combat/CombatTuningConfig.asset";

            var config = AssetDatabase.LoadAssetAtPath<CombatTuningConfigSO>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
                AssetDatabase.CreateAsset(config, path);
                Debug.Log($"[CombatTuning] Created authoring asset: {path}");
            }
            else
            {
                Debug.Log($"[CombatTuning] Preserved existing authoring asset: {path}");
            }

            var resourcesPath = "Assets/Resources/CombatTuningConfig.asset";
            var resourcesConfig = AssetDatabase.LoadAssetAtPath<CombatTuningConfigSO>(resourcesPath);
            if (resourcesConfig == null)
            {
                resourcesConfig = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
                AssetDatabase.CreateAsset(resourcesConfig, resourcesPath);
                Debug.Log($"[CombatTuning] Created runtime Resources asset: {resourcesPath}");
            }
            else
            {
                Debug.Log($"[CombatTuning] Preserved existing runtime Resources asset: {resourcesPath}");
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = config;
            Debug.Log($"[Authoring] CombatTuningConfig assets ensured: {path}");
        }

        [MenuItem("LuoLuoTrip/Setup/Create Audio Feedback Profile")]
        public static void CreateAudioFeedbackProfile()
        {
            EnsureFolder("Assets/Data/Audio");
            EnsureFolder("Assets/Resources");
            const string path = "Assets/Data/Audio/AudioFeedbackProfile.asset";

            var profile = AssetDatabase.LoadAssetAtPath<AudioFeedbackProfileSO>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<AudioFeedbackProfileSO>();
                AssetDatabase.CreateAsset(profile, path);
            }
            profile.EnsureAllEvents();
            EditorUtility.SetDirty(profile);

            const string resourcesPath = "Assets/Resources/AudioFeedbackProfile.asset";
            var resourcesProfile = AssetDatabase.LoadAssetAtPath<AudioFeedbackProfileSO>(resourcesPath);
            if (resourcesProfile == null)
                AssetDatabase.CopyAsset(path, resourcesPath);
            else
            {
                EditorUtility.CopySerialized(profile, resourcesProfile);
                EditorUtility.SetDirty(resourcesProfile);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            Debug.Log($"[LuoLuoTrip] AudioFeedbackProfile 已创建: {path}");
        }

        [MenuItem("LuoLuoTrip/Setup/Create World Marker Profile")]
        public static void CreateWorldMarkerProfile()
        {
            EnsureFolder("Assets/Data/Feedback");
            EnsureFolder("Assets/Resources");
            const string path = "Assets/Data/Feedback/WorldMarkerProfile.asset";

            var profile = AssetDatabase.LoadAssetAtPath<WorldMarkerProfileSO>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<WorldMarkerProfileSO>();
                AssetDatabase.CreateAsset(profile, path);
            }
            profile.EnsureAllTypes();
            EditorUtility.SetDirty(profile);

            const string resourcesPath = "Assets/Resources/WorldMarkerProfile.asset";
            var resourcesProfile = AssetDatabase.LoadAssetAtPath<WorldMarkerProfileSO>(resourcesPath);
            if (resourcesProfile == null)
                AssetDatabase.CopyAsset(path, resourcesPath);
            else
            {
                EditorUtility.CopySerialized(profile, resourcesProfile);
                EditorUtility.SetDirty(resourcesProfile);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            Debug.Log($"[LuoLuoTrip] WorldMarkerProfile 已创建: {path}");
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

            Debug.Log($"Commander: Lv.{context.CommanderProfile.CommanderLevel}, Capacity: {context.CommanderProfile.CommandCapacity}");
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

            // Combat readability stack: damage numbers + central feedback broadcaster.
            var damageNum = bootstrapGo.GetComponent<DamageNumberFeedback>();
            if (damageNum == null)
                damageNum = bootstrapGo.AddComponent<DamageNumberFeedback>();
            var broadcaster = bootstrapGo.GetComponent<CombatFeedbackBroadcaster>();
            if (broadcaster == null)
                broadcaster = bootstrapGo.AddComponent<CombatFeedbackBroadcaster>();

            // Apply tuning durations from config.
            CreateCombatTuningConfig();
            var tuning = AssetDatabase.LoadAssetAtPath<CombatTuningConfigSO>(
                "Assets/Data/Combat/CombatTuningConfig.asset");
            if (tuning != null)
            {
                damageNum.ApplyTuning(tuning.damageNumberDuration);
                foreach (var flash in UnityEngine.Object.FindObjectsOfType<HitFlashFeedback>())
                    flash.ApplyTuning(tuning.hitFlashDuration, tuning.hitFlashDuration * 5f);
            }
        }

        private static GameObject CreateCombatCharacter(string name, Vector3 position, CharacterData data, string prefabPath, bool isPlayer)
        {
            var prefab = PlaceholderAssetGenerator.GetPrefab(prefabPath);
            GameObject go;

            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = name;
                go.transform.position = position;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = name;
                go.transform.position = position;

                var existingCollider = go.GetComponent<Collider>();
                if (existingCollider != null)
                    Object.DestroyImmediate(existingCollider);

                var capsule = go.AddComponent<CapsuleCollider>();
                capsule.height = 2f;
                capsule.radius = 0.5f;
                capsule.center = new Vector3(0f, 1f, 0f);

                var config = SubFactionRegistry.GetConfig(data.Faction);
                if (config != null)
                {
                    var renderer = go.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = config.themeColor;
                }
            }

            var entity = go.GetComponent<CharacterEntity>();
            if (entity == null)
                entity = go.AddComponent<CharacterEntity>();

            // Run runtime component guard BEFORE Bind() so Combatant.Awake can pick up
            // motor/rigidbody/collider in a deterministic order.
            if (isPlayer)
                CharacterRuntimeComponentGuard.EnsureForPlayer(go);
            else
                CharacterRuntimeComponentGuard.EnsureForAI(go);

            entity.Bind(data);

            if (go.GetComponent<Combatant>() == null)
                go.AddComponent<Combatant>();
            if (go.GetComponent<ProceduralCombatAnimator>() == null)
                go.AddComponent<ProceduralCombatAnimator>();
            if (go.GetComponent<CombatAnimationDriver>() == null)
                go.AddComponent<CombatAnimationDriver>();

            if (isPlayer)
            {
                if (go.GetComponent<CombatController>() == null)
                    go.AddComponent<CombatController>();
            }
            else
            {
                if (go.GetComponent<SimpleCombatAI>() == null)
                    go.AddComponent<SimpleCombatAI>();
                var navAgent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navAgent == null)
                    go.AddComponent<UnityEngine.AI.NavMeshAgent>();

                // Combat readability: enemies always get a health bar + hit flash.
                if (go.GetComponent<CombatantHealthBarPresenter>() == null)
                    go.AddComponent<CombatantHealthBarPresenter>();
                if (go.GetComponent<HitFlashFeedback>() == null)
                    go.AddComponent<HitFlashFeedback>();
            }

            // Player optionally gets HitFlashFeedback too (visible when hit).
            if (isPlayer && go.GetComponent<HitFlashFeedback>() == null)
                go.AddComponent<HitFlashFeedback>();

            if (isPlayer)
            {
                var hudGo = new GameObject("CombatHUD");
                hudGo.AddComponent<CombatDebugHUD>();
            }

            return go;
        }

        [MenuItem("LuoLuoTrip/Setup/Create AI Behavior Profiles")]
        public static void CreateAIBehaviorProfiles()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder("Assets/Data/AIProfiles");
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/AIProfiles");

            foreach (AIBehaviorProfileType type in System.Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                CreateOrUpdateAIProfile(type, $"Assets/Data/AIProfiles/{type}.asset");
                CreateOrUpdateAIProfile(type, $"Assets/Resources/AIProfiles/{type}.asset");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AIProfile] AI behavior profiles ensured without overwriting authored tuning.");
        }

        private static AIBehaviorProfileSO CreateOrUpdateAIProfile(AIBehaviorProfileType type, string path)
        {
            var profile = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                AIBehaviorProfileSO.ConfigureDefaults(profile, type);
                AssetDatabase.CreateAsset(profile, path);
                Debug.Log($"[AIProfile] Created {type} profile: {path}");
                return profile;
            }

            var changed = false;
            if (profile.profileType != type)
            {
                profile.profileType = type;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(profile.profileId))
            {
                profile.profileId = AIBehaviorProfileSO.ToProfileId(type);
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(profile.displayName))
            {
                profile.displayName = AIBehaviorProfileSO.ToDisplayName(type);
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(profile.debugDescription))
            {
                var defaults = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                AIBehaviorProfileSO.ConfigureDefaults(defaults, type);
                profile.debugDescription = defaults.debugDescription;
                Object.DestroyImmediate(defaults);
                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(profile);
            Debug.Log($"[AIProfile] Preserved existing {type} profile: {path}");
            return profile;
        }

        private static AIBehaviorProfileSO LoadAIProfile(AIBehaviorProfileType type)
        {
            var profile = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>($"Assets/Data/AIProfiles/{type}.asset");
            if (profile == null)
                profile = CreateOrUpdateAIProfile(type, $"Assets/Data/AIProfiles/{type}.asset");
            return profile;
        }

        private static void AssignAIProfile(GameObject go, AIBehaviorProfileSO profile, Transform protectedTarget)
        {
            if (go == null) return;
            var ai = go.GetComponent<SimpleCombatAI>();
            if (ai == null) return;
            ai.BehaviorProfile = profile;
            ai.ProtectedTarget = protectedTarget;
            EditorUtility.SetDirty(ai);
        }

        private static void AssignProtectedTarget(GameObject go, Transform protectedTarget)
        {
            if (go == null) return;
            var ai = go.GetComponent<SimpleCombatAI>();
            if (ai == null) return;
            ai.ProtectedTarget = protectedTarget;
            EditorUtility.SetDirty(ai);
        }

        private static void AttachSceneMarker(GameObject go, WorldMarkerType type, string label)
        {
            if (go == null) return;
            var marker = go.GetComponent<WorldMarker>();
            if (marker == null)
                marker = go.AddComponent<WorldMarker>();
            marker.Configure(type, go.transform, label);
        }

        private static GameObject CreateAreaLabel(string name, Vector3 position, string label, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = position + new Vector3(0f, 3f, 0f);

            var marker = go.AddComponent<WorldMarker>();
            marker.Configure(WorldMarkerType.MissionObjective, go.transform, label);

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = name + "_Ring";
            Object.DestroyImmediate(ring.GetComponent<Collider>());
            ring.transform.SetParent(go.transform, false);
            ring.transform.localPosition = new Vector3(0f, -2.95f, 0f);
            ring.transform.localScale = new Vector3(6f, 0.05f, 6f);
            var renderer = ring.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(color.r, color.g, color.b, 0.5f);
                renderer.sharedMaterial = mat;
            }
            return go;
        }

        private static GameObject CreateObjective(string name, Vector3 position, string prefabPath)
        {
            var prefab = PlaceholderAssetGenerator.GetPrefab(prefabPath);
            GameObject go;

            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = name;
                go.transform.position = position;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = name;
                go.transform.position = position;
                go.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f);
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
            // CRITICAL: never let Animator drive root transform. Gameplay movement
            // (CombatController/SimpleCombatAI/CharacterMovementMotor) owns the root.
            animator.applyRootMotion = false;

            var bridge = go.GetComponent<AnimatorCombatBridge>();
            if (bridge == null)
                bridge = go.AddComponent<AnimatorCombatBridge>();

            var bridgeSo = new SerializedObject(bridge);
            bridgeSo.FindProperty("_config").objectReferenceValue = result.Config;
            bridgeSo.FindProperty("_animator").objectReferenceValue = animator;
            bridgeSo.ApplyModifiedPropertiesWithoutUndo();

            if (go.GetComponent<ProceduralCombatAnimator>() == null)
                go.AddComponent<ProceduralCombatAnimator>();

            var driver = go.GetComponent<CombatAnimationDriver>();
            if (driver == null)
                driver = go.AddComponent<CombatAnimationDriver>();
            var driverSo = new SerializedObject(driver);
            driverSo.FindProperty("_preferAnimatorBridge").boolValue = false;
            driverSo.FindProperty("_useProceduralFallback").boolValue = true;
            driverSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void InitializeRegistryFromDatabase()
        {
            var db = AssetDatabase.LoadAssetAtPath<SubFactionDatabaseSO>(DatabasePath);
            if (db != null)
                SubFactionRegistry.Initialize(db);
        }

        public static GameObject EnsureMainCamera()
        {
            var cameraGo = GameObject.Find("Main Camera");

            if (cameraGo == null)
            {
                var tagged = GameObject.FindWithTag("MainCamera");
                if (tagged != null)
                    cameraGo = tagged;
            }

            if (cameraGo == null)
            {
                cameraGo = new GameObject("Main Camera");
                cameraGo.transform.position = new Vector3(0f, 8f, -10f);
                cameraGo.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            }

            cameraGo.tag = "MainCamera";
            cameraGo.SetActive(true);

            var cam = cameraGo.GetComponent<Camera>();
            if (cam == null)
                cam = cameraGo.AddComponent<Camera>();

            cam.enabled = true;
            cam.targetDisplay = 0;
            cam.targetTexture = null;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;

            if (cam.clearFlags == CameraClearFlags.SolidColor && cam.backgroundColor == Color.black)
                cam.clearFlags = CameraClearFlags.Skybox;

            if (cam.cullingMask == 0)
                cam.cullingMask = -1;

            var existingListener = cameraGo.GetComponent<AudioListener>();
            var allListeners = Object.FindObjectsOfType<AudioListener>();
            if (existingListener == null && allListeners.Length == 0)
                cameraGo.AddComponent<AudioListener>();

            return cameraGo;
        }

        private static MissionDefinitionSO EnsureMissionChainDefinition(string assetPath, string missionId, string displayName,
            string description, List<MissionObjective> objectives, List<MissionOutcomeConsequenceMapping> outcomes)
        {
            var mission = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(assetPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinitionSO>();
                AssetDatabase.CreateAsset(mission, assetPath);
                Debug.Log($"[MissionDefinition] Created mission asset: {assetPath}");
            }

            PopulateMissionDefinitionIfMissing(mission, missionId, displayName, description, 1, objectives, outcomes);
            return mission;
        }

        private static void PopulateMissionDefinitionIfMissing(MissionDefinitionSO mission, string missionId, string displayName,
            string description, int recommendedLevel, List<MissionObjective> objectives, List<MissionOutcomeConsequenceMapping> outcomes)
        {
            if (mission == null) return;
            var changed = false;

            if (string.IsNullOrWhiteSpace(mission.MissionId))
            {
                mission.MissionId = missionId;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(mission.DisplayName))
            {
                mission.DisplayName = displayName;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(mission.Description))
            {
                mission.Description = description;
                changed = true;
            }
            if (mission.RecommendedCommanderLevel <= 0)
            {
                mission.RecommendedCommanderLevel = recommendedLevel;
                changed = true;
            }
            if (mission.DefaultObjectives == null || mission.DefaultObjectives.Count == 0)
            {
                mission.DefaultObjectives = objectives ?? new List<MissionObjective>();
                changed = true;
            }
            if (mission.OutcomeConsequences == null || mission.OutcomeConsequences.Count == 0)
            {
                mission.OutcomeConsequences = outcomes ?? new List<MissionOutcomeConsequenceMapping>();
                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(mission);
            Debug.Log($"[MissionDefinition] Preserved existing mission asset: {missionId}");
        }

        private static List<MissionOutcomeConsequenceMapping> BuildSharedMissionOutcomeMappings(
            string mechaVictory, string beastVictory, string balanced, string partial, string failed)
        {
            return new List<MissionOutcomeConsequenceMapping>
            {
                new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.MechaVictory, CommanderExperienceDelta = 200, SummaryText = mechaVictory },
                new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.BeastVictory, CommanderExperienceDelta = 200, SummaryText = beastVictory },
                new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.BalancedResolution, CommanderExperienceDelta = 300, SummaryText = balanced },
                new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.PartialSuccess, CommanderExperienceDelta = 80, SummaryText = partial },
                new MissionOutcomeConsequenceMapping { Outcome = MissionOutcomeType.Failed, CommanderExperienceDelta = 30, SummaryText = failed }
            };
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

        private static void MarkNavMeshStatic(GameObject go)
        {
            if (go == null) return;
            const int navMeshStaticFlag = 1 << 0;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
            foreach (Transform child in go.transform)
            {
                var existing = GameObjectUtility.GetStaticEditorFlags(child.gameObject);
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, existing | StaticEditorFlags.NavigationStatic);
            }
        }
    }
}
#endif

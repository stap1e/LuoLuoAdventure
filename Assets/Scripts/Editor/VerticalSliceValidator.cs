#if UNITY_EDITOR
using LuoLuoTrip.Combat;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat.Animation;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.Feedback;
using LuoLuoTrip.Save;
using LuoLuoTrip.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    public static class VerticalSliceValidator
    {
        [MenuItem("LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation")]
        public static void RunValidation()
        {
            var report = new List<string>();
            report.Add("========================================");
            report.Add("LuoLuoTrip Vertical Slice Validation Report");
            report.Add($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.Add("========================================");

            int errors = 0;
            int warnings = 0;

            CheckUnityVersion(report, ref errors);
            CheckPlaceholderPrefabs(report, ref errors, ref warnings);
            CheckCommanderPrototypeScene(report, ref errors, ref warnings);
            CheckMissingScripts(report, ref errors);
            CheckRuntimeEditorReferences(report, ref errors);
            CheckRuntimeFindObjectsOfType(report, ref warnings);
            CheckAssemblyDefinitions(report, ref errors);
            CheckCombatPrototypeScene(report, ref errors, ref warnings);
            CheckMissionBranchDefinitionAssets(report, ref warnings);
            CheckCombatTuningConfig(report, ref warnings);
            CheckAuthoringAssets(report, ref errors, ref warnings);
            CheckSaveLoadManager(report, ref warnings);
            CheckDebugTriggerType(report, ref warnings);
            CheckAudioFeedbackProfile(report, ref warnings);
            CheckWorldMarkerProfile(report, ref warnings);
            CheckEnhancedPlaceholderHierarchy(report, ref warnings);
            CheckNavigationAgentBridge(report, ref warnings);
            CheckEncounterRuntime(report, ref warnings);
            CheckMissionAreaRuntime(report, ref warnings);
            CheckCameraSetup(report, ref errors, ref warnings);
            CheckRuntimeCameraBootstrap(report, ref warnings);
            CheckServiceLifecycle(report, ref warnings);
            CheckNavMeshSetup(report, ref warnings);
            CheckEncounterWaveConfig(report, ref warnings);
            CheckInputOwnership(report, ref warnings);
            CheckRootMovement(report, ref warnings);
            CheckAnimationClipBindings(report, ref warnings);
            CheckGameplayScriptAttachment(report, ref warnings);
            CheckCombatReadability(report, ref errors, ref warnings);
            CheckCombatBalance(report, ref errors, ref warnings);
            CheckEncounterReliability(report, ref errors, ref warnings);
            CheckEncounterPersistence(report, ref errors, ref warnings);
            CheckPlayerAttackUsability(report, ref errors, ref warnings);
            CheckCommanderControlUsability(report, ref errors, ref warnings);
            CheckDemoFlow(report, ref errors, ref warnings);
            CheckPlayableDemoReadability(report, ref errors, ref warnings);
            CheckHudLayout(report, ref errors, ref warnings);
            CheckMissionMarkerCoverage(report, ref errors, ref warnings);
            CheckMissionAuthoring(report, ref errors, ref warnings);
            CheckCommanderActionReadability(report, ref errors, ref warnings);
            CheckCommanderActionExpansion(report, ref errors, ref warnings);
            CheckAIBehaviorProfiles(report, ref errors, ref warnings);
            CheckCityGateDispute(report, ref errors, ref warnings);

            report.Add("");
            report.Add("========================================");
            report.Add($"Validation complete: {errors} errors, {warnings} warnings");
            report.Add("========================================");

            var text = string.Join("\n", report);
            Debug.Log(text);

            var path = Path.Combine(Application.dataPath, "Docs", "VERTICAL_SLICE_VALIDATION_REPORT.md");
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, text);
            Debug.Log($"Report saved to: {path}");
        }

        private static void CheckUnityVersion(List<string> report, ref int errors)
        {
            report.Add("");
            report.Add("--- Unity Version ---");
            var current = Application.unityVersion;
            var expected = "2022.3.62f3";
            if (current.StartsWith("2022.3"))
            {
                report.Add($"  OK: Unity {current}");
            }
            else
            {
                report.Add($"  ERROR: Unity {current} (expected {expected})");
                errors++;
            }
        }

        private static void CheckPlaceholderPrefabs(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Placeholder Prefabs ---");

            var expectedPrefabs = new[]
            {
                "PH_PlayerCommander_Cylinder",
                "PH_MechaMinion_Cylinder",
                "PH_BeastMinion_Cylinder",
                "PH_CityLord_Cylinder",
                "PH_WarKing_Cylinder",
                "PH_Convoy_Cylinder",
                "PH_EnergyNode_Cylinder",
                "PH_ObjectiveMarker_Cylinder"
            };

            var expectedChildren = new[] { "Visual", "Collision", "Marker" };
            var prefabDir = "Assets/Art/Placeholders/Prefabs";
            var anyMissing = false;

            foreach (var name in expectedPrefabs)
            {
                var path = $"{prefabDir}/{name}.prefab";
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    report.Add($"  MISSING: {name} (run Generate Placeholder Assets)");
                    anyMissing = true;
                    errors++;
                    continue;
                }

                var childNames = new HashSet<string>();
                for (int i = 0; i < asset.transform.childCount; i++)
                    childNames.Add(asset.transform.GetChild(i).name);

                var missing = expectedChildren.Where(e => !childNames.Contains(e)).ToList();
                if (missing.Count > 0)
                {
                    report.Add($"  ERROR: {name} missing children: {string.Join(", ", missing)}");
                    errors++;
                }
                else
                {
                    report.Add($"  OK: {name} (Visual/Collision/Marker)");
                }
            }

            if (anyMissing)
                warnings++;
        }

        private static void CheckCommanderPrototypeScene(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- CommanderPrototype Scene ---");

            var scenePath = "Assets/Scenes/CommanderPrototype.unity";
            if (!File.Exists(scenePath))
            {
                report.Add("  MISSING: CommanderPrototype.unity (run Create Commander Mission Prototype Scene)");
                errors++;
                return;
            }

            report.Add("  OK: CommanderPrototype.unity exists");

            var requiredComponents = new[]
            {
                typeof(GameBootstrap),
                typeof(SaveLoadManager),
                typeof(CommanderPrototypeRuntime),
                typeof(CommanderControlController),
                typeof(CommanderTargetSelector),
                typeof(CameraFollowController),
                typeof(ConvoyObjective),
                typeof(EnergyNodeObjective),
                typeof(MissionTriggerZone),
                typeof(ConvoyEnergyConflictRuntime),
                typeof(TutorialFlowRuntime)
            };

            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var loadedSceneObjects = allObjects.Where(go => go.scene.isLoaded && go.scene.name == "CommanderPrototype").ToList();

            foreach (var compType in requiredComponents)
            {
                var found = Resources.FindObjectsOfTypeAll(compType);
                if (found.Length > 0)
                {
                    report.Add($"  OK: {compType.Name} found");
                }
                else
                {
                    report.Add($"  WARNING: {compType.Name} not found in loaded scenes");
                    warnings++;
                }
            }

            var hudTypes = new[] { "CommanderDebugHud", "FactionStandingDebugPanel", "MissionResultDebugPanel", "MissionResultSummaryPanel", "MissionObjectiveHud", "CommanderControlHintPanel", "FactionDeltaToastPanel", "MissionChainSummaryPanel" };
            foreach (var hudName in hudTypes)
            {
                var hudType = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.Name == hudName);
                if (hudType != null)
                {
                    var found = Resources.FindObjectsOfTypeAll(hudType);
                    if (found.Length > 0)
                        report.Add($"  OK: {hudName} found");
                    else
                    {
                        report.Add($"  WARNING: {hudName} not found");
                        warnings++;
                    }
                }
            }
        }

        private static void CheckMissingScripts(List<string> report, ref int errors)
        {
            report.Add("");
            report.Add("--- Missing Scripts ---");
            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var missingCount = 0;

            foreach (var go in allGameObjects)
            {
                if (!go.scene.isLoaded) continue;
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null)
                    {
                        report.Add($"  Missing script on: {GetHierarchyPath(go)}");
                        missingCount++;
                    }
                }
            }

            if (missingCount == 0)
                report.Add("  OK: No missing scripts");
            else
            {
                report.Add($"  Total missing scripts: {missingCount}");
                errors += missingCount;
            }
        }

        private static void CheckRuntimeEditorReferences(List<string> report, ref int errors)
        {
            report.Add("");
            report.Add("--- Runtime/Editor API Separation ---");
            var editorDir = Path.Combine(Application.dataPath, "Scripts", "Editor");
            var scriptsDir = Path.Combine(Application.dataPath, "Scripts");

            if (!Directory.Exists(scriptsDir))
            {
                report.Add("  Scripts directory not found");
                return;
            }

            var runtimeScripts = Directory.GetFiles(scriptsDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.StartsWith(editorDir))
                .ToList();

            var editorKeywords = new[] { "using UnityEditor", "AssetDatabase", "MenuItem", "SerializedObject" };
            var issues = 0;

            foreach (var script in runtimeScripts)
            {
                var content = File.ReadAllText(script);
                foreach (var keyword in editorKeywords)
                {
                    if (content.Contains(keyword))
                    {
                        var relative = script.Replace(Application.dataPath, "Assets");
                        report.Add($"  ERROR: {relative} contains '{keyword}'");
                        issues++;
                    }
                }
            }

            if (issues == 0)
                report.Add("  OK: No UnityEditor references in runtime scripts");
            else
                errors += issues;
        }

        private static void CheckRuntimeFindObjectsOfType(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Runtime FindObjectsOfType Usage ---");

            var editorDir = Path.Combine(Application.dataPath, "Scripts", "Editor");
            var scriptsDir = Path.Combine(Application.dataPath, "Scripts");

            if (!Directory.Exists(scriptsDir)) return;

            var runtimeScripts = Directory.GetFiles(scriptsDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.StartsWith(editorDir))
                .ToList();

            var count = 0;
            foreach (var script in runtimeScripts)
            {
                var content = File.ReadAllText(script);
                if (content.Contains("FindObjectsOfType") || content.Contains("FindObjectOfType"))
                {
                    var relative = script.Replace(Application.dataPath, "Assets");
                    report.Add($"  WARNING: {relative} uses FindObjects(s)OfType");
                    count++;
                }
            }

            if (count == 0)
                report.Add("  OK: No runtime FindObjectsOfType usage");
            else
                warnings += count;
        }

        private static void CheckAssemblyDefinitions(List<string> report, ref int errors)
        {
            report.Add("");
            report.Add("--- Assembly Definitions ---");
            var asmdefFiles = Directory.GetFiles(Application.dataPath, "*.asmdef", SearchOption.AllDirectories);

            foreach (var asmdef in asmdefFiles)
            {
                var relative = asmdef.Replace(Application.dataPath, "Assets");
                var content = File.ReadAllText(asmdef);
                var name = ExtractJsonField(content, "name");
                var includePlatforms = ExtractJsonArray(content, "includePlatforms");
                var refs = ExtractJsonArray(content, "references");
                var isEditor = includePlatforms.Contains("Editor");
                var hasEditorRef = refs.Any(r => r.EndsWith(".Editor"));

                if (!isEditor && hasEditorRef)
                {
                    report.Add($"  ERROR: {relative} - runtime assembly references Editor assembly!");
                    errors++;
                }
                else
                {
                    report.Add($"  OK: {name} (platforms: {(includePlatforms.Count == 0 ? "All" : string.Join(",", includePlatforms))})");
                }
            }
        }

        private static void CheckCombatPrototypeScene(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- CombatPrototype Scene ---");
            var scenePath = "Assets/Scenes/CombatPrototype.unity";
            if (!File.Exists(scenePath))
            {
                report.Add("  ERROR: CombatPrototype.unity missing (run Create Combat Prototype Scene)");
                errors++;
            }
            else
            {
                report.Add("  OK: CombatPrototype.unity exists");
            }
        }

        private static void CheckMissionBranchDefinitionAssets(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- MissionBranchDefinition Assets ---");
            var guids = AssetDatabase.FindAssets("t:MissionBranchDefinition");
            if (guids.Length == 0)
            {
                report.Add("  WARNING: No MissionBranchDefinition assets found under Assets/Data/");
                warnings++;
            }
            else
            {
                report.Add($"  OK: {guids.Length} MissionBranchDefinition asset(s) found");
            }
        }

        private static void CheckCombatTuningConfig(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- CombatTuningConfig Asset ---");
            var path = "Assets/Data/Combat/CombatTuningConfig.asset";
            var resourcesPath = "Assets/Resources/CombatTuningConfig.asset";
            if (!File.Exists(path))
            {
                report.Add("  WARNING: CombatTuningConfig.asset missing (run LuoLuoTrip/Setup/Create Combat Tuning Config)");
                warnings++;
            }
            else
            {
                report.Add("  OK: CombatTuningConfig authoring asset exists");
            }

            if (!File.Exists(resourcesPath))
            {
                report.Add("  WARNING: Resources/CombatTuningConfig.asset missing; runtime will use defaults");
                warnings++;
            }
            else
            {
                report.Add("  OK: CombatTuningConfig Resources copy exists");
            }
        }

        private static void CheckAuthoringAssets(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Authoring Asset Persistence ---");
            var audit = AuthoringAssetAudit.AuditRequiredAssets(checkGit: true);
            if (audit.Issues.Count == 0)
            {
                report.Add("  OK: Required MissionDefinitionSO, AIBehaviorProfileSO, and CombatTuningConfigSO assets exist, validate, have .meta files, and are not git-ignored");
                return;
            }

            foreach (var issue in audit.Issues)
            {
                report.Add($"  {(issue.IsError ? "ERROR" : "WARNING")}: {issue.Path}: {issue.Message}");
                if (issue.IsError) errors++;
                else warnings++;
            }
        }

        private static void CheckSaveLoadManager(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- SaveLoadManager ---");
            var managers = Resources.FindObjectsOfTypeAll<SaveLoadManager>();
            if (managers.Length == 0)
            {
                report.Add("  WARNING: No SaveLoadManager found in any loaded scene");
                warnings++;
            }
            else
            {
                report.Add($"  OK: SaveLoadManager found ({managers.Length} instance(s))");
            }
        }

        private static void CheckDebugTriggerType(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Debug Trigger Type ---");
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CommanderPrototypeRuntime");
            if (type == null)
            {
                report.Add("  WARNING: CommanderPrototypeRuntime type not found");
                warnings++;
            }
            else
            {
                report.Add("  OK: CommanderPrototypeRuntime type exists");
            }
        }

        private static void CheckAudioFeedbackProfile(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- AudioFeedbackProfile ---");
            var path = "Assets/Data/Audio/AudioFeedbackProfile.asset";
            var resourcesPath = "Assets/Resources/AudioFeedbackProfile.asset";
            if (!File.Exists(path))
            {
                report.Add("  WARNING: AudioFeedbackProfile.asset missing (run Create Audio Feedback Profile)");
                warnings++;
            }
            else
            {
                report.Add("  OK: AudioFeedbackProfile.asset exists");
            }
            if (!File.Exists(resourcesPath))
            {
                report.Add("  WARNING: Resources/AudioFeedbackProfile.asset missing (re-run profile menu)");
                warnings++;
            }
            else
            {
                report.Add("  OK: Resources copy exists");
            }
        }

        private static void CheckWorldMarkerProfile(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- WorldMarkerProfile ---");
            var path = "Assets/Data/Feedback/WorldMarkerProfile.asset";
            var resourcesPath = "Assets/Resources/WorldMarkerProfile.asset";
            if (!File.Exists(path))
            {
                report.Add("  WARNING: WorldMarkerProfile.asset missing (run Create World Marker Profile)");
                warnings++;
            }
            else
            {
                report.Add("  OK: WorldMarkerProfile.asset exists");
            }
            if (!File.Exists(resourcesPath))
            {
                report.Add("  WARNING: Resources/WorldMarkerProfile.asset missing (re-run profile menu)");
                warnings++;
            }
            else
            {
                report.Add("  OK: Resources copy exists");
            }
        }

        private static void CheckEnhancedPlaceholderHierarchy(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Enhanced Placeholder Visual Hierarchy ---");

            var prefabDir = "Assets/Art/Placeholders/Prefabs";
            var prefabs = new[]
            {
                "PH_PlayerCommander_Cylinder",
                "PH_MechaMinion_Cylinder",
                "PH_BeastMinion_Cylinder",
                "PH_CityLord_Cylinder",
                "PH_WarKing_Cylinder",
                "PH_Convoy_Cylinder",
                "PH_EnergyNode_Cylinder",
                "PH_ObjectiveMarker_Cylinder"
            };

            foreach (var name in prefabs)
            {
                var path = $"{prefabDir}/{name}.prefab";
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null) continue;

                var visual = asset.transform.Find("Visual");
                if (visual == null)
                {
                    report.Add($"  WARNING: {name} missing Visual child");
                    warnings++;
                    continue;
                }

                if (visual.childCount < 2)
                {
                    report.Add($"  WARNING: {name} Visual has only {visual.childCount} primitive(s) (enhanced expected >=2). Run Regenerate Enhanced Placeholders (Force).");
                    warnings++;
                }
                else
                {
                    report.Add($"  OK: {name} Visual has {visual.childCount} primitives");
                }
            }
        }

        private static string GetHierarchyPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private static string ExtractJsonField(string json, string fieldName)
        {
            var search = $"\"{fieldName}\":";
            var idx = json.IndexOf(search, System.StringComparison.Ordinal);
            if (idx < 0) return "";
            var start = json.IndexOf('"', idx + search.Length) + 1;
            var end = json.IndexOf('"', start);
            return json.Substring(start, end - start);
        }

        private static List<string> ExtractJsonArray(string json, string fieldName)
        {
            var result = new List<string>();
            var search = $"\"{fieldName}\":";
            var idx = json.IndexOf(search, System.StringComparison.Ordinal);
            if (idx < 0) return result;
            var openBracket = json.IndexOf('[', idx);
            if (openBracket < 0) return result;
            var closeBracket = json.IndexOf(']', openBracket);
            if (closeBracket < 0) return result;
            var arrayContent = json.Substring(openBracket + 1, closeBracket - openBracket - 1);
            var entries = arrayContent.Split(',');

            foreach (var entry in entries)
            {
                var trimmed = entry.Trim().Trim('"').Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }

        private static void CheckNavigationAgentBridge(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- NavigationAgentBridge ---");
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "NavigationAgentBridge");
            if (type == null)
            {
                report.Add("  WARNING: NavigationAgentBridge type not found (check Assets/Scripts/AI/)");
                warnings++;
            }
            else
            {
                report.Add("  OK: NavigationAgentBridge type exists");
            }
        }

        private static void CheckEncounterRuntime(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- EncounterRuntime ---");
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "EncounterRuntime");
            if (type == null)
            {
                report.Add("  WARNING: EncounterRuntime type not found (check Assets/Scripts/Encounter/)");
                warnings++;
            }
            else
            {
                report.Add("  OK: EncounterRuntime type exists");
            }

            var convoyType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "ConvoyEnergyConflictRuntime");
            if (convoyType != null)
            {
                var hasEncounter = convoyType.GetProperty("Encounter") != null;
                if (hasEncounter)
                    report.Add("  OK: ConvoyEnergyConflictRuntime has Encounter property");
                else
                {
                    report.Add("  WARNING: ConvoyEnergyConflictRuntime missing Encounter property");
                    warnings++;
                }
            }

            var borderType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "BorderRetaliationRuntime");
            if (borderType != null)
            {
                var hasEncounter = borderType.GetProperty("Encounter") != null;
                if (hasEncounter)
                    report.Add("  OK: BorderRetaliationRuntime has Encounter property");
                else
                {
                    report.Add("  WARNING: BorderRetaliationRuntime missing Encounter property");
                    warnings++;
                }
            }
        }

        private static void CheckMissionAreaRuntime(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- MissionAreaRuntime ---");
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "MissionAreaRuntime");
            if (type == null)
            {
                report.Add("  WARNING: MissionAreaRuntime type not found (check Assets/Scripts/Mission/Runtime/)");
                warnings++;
            }
            else
            {
                report.Add("  OK: MissionAreaRuntime type exists");
            }

            var retreatType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "RetreatTracker");
            if (retreatType == null)
            {
                report.Add("  WARNING: RetreatTracker type not found");
                warnings++;
            }
            else
            {
                report.Add("  OK: RetreatTracker type exists");
            }
        }

        private static void CheckCameraSetup(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Camera Setup ---");

            var camGo = GameObject.FindWithTag("MainCamera");
            if (camGo == null)
            {
                report.Add("  ERROR: No MainCamera tagged object in current scene (run EnsureMainCamera or re-create scene)");
                errors++;
                return;
            }

            var cameraComp = camGo.GetComponent<Camera>();
            if (cameraComp == null)
            {
                report.Add("  ERROR: Main Camera has no Camera component");
                errors++;
                return;
            }

            if (!cameraComp.enabled)
            {
                report.Add("  ERROR: Main Camera Camera component is disabled");
                errors++;
            }
            else
            {
                report.Add("  OK: Main Camera is active and enabled");
            }

            if (cameraComp.targetTexture != null)
            {
                report.Add("  ERROR: Main Camera targetTexture is not null (will not render to display)");
                errors++;
            }
            else
            {
                report.Add("  OK: Main Camera targetTexture is null");
            }

            if (cameraComp.cullingMask == 0)
            {
                report.Add("  ERROR: Main Camera cullingMask is 0 (nothing rendered)");
                errors++;
            }
            else
            {
                report.Add("  OK: Main Camera cullingMask is non-zero");
            }

            if (cameraComp.targetDisplay != 0)
            {
                report.Add($"  WARNING: Main Camera targetDisplay={cameraComp.targetDisplay} (expected 0 for Display 1)");
                warnings++;
            }

            var follow = camGo.GetComponent<CameraFollowController>();
            if (follow != null && !cameraComp.enabled)
            {
                report.Add("  ERROR: CameraFollowController present but Camera is disabled — follow controller must not disable Camera");
                errors++;
            }

            if (follow != null)
            {
                report.Add("  OK: CameraFollowController present (CommanderPrototype)");
            }
        }

        private static void CheckRuntimeCameraBootstrap(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- RuntimeCameraBootstrap ---");

            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "RuntimeCameraBootstrap");
            if (type == null)
            {
                report.Add("  WARNING: RuntimeCameraBootstrap type not found (check Assets/Scripts/Camera/)");
                warnings++;
                return;
            }

            report.Add("  OK: RuntimeCameraBootstrap type exists");

            var instances = Object.FindObjectsOfType(type);
            if (instances.Length == 0)
            {
                report.Add("  WARNING: No RuntimeCameraBootstrap in current scene — Add RuntimeCameraBootstrap to GameBootstrap GO or re-create scene");
                warnings++;
            }
            else
            {
                report.Add($"  OK: RuntimeCameraBootstrap found in scene ({instances.Length} instance(s))");
            }
        }

        private static void CheckServiceLifecycle(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Service Lifecycle ---");

            var camGo = GameObject.FindWithTag("MainCamera");
            if (camGo != null)
            {
                var shakeType = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.Name == "CameraShakeService");
                if (shakeType != null && camGo.GetComponent(shakeType) != null)
                {
                    report.Add("  WARNING: CameraShakeService serialized on Main Camera — remove from scene; CombatHitFeedbackHub adds it at runtime");
                    warnings++;
                }
            }

            var sourceDir = Path.Combine(Application.dataPath, "Scripts");
            var dangerousSingletons = new[]
            {
                new { Name = "CameraShakeService", File = "Combat/Feedback/CameraShakeService.cs", Host = "Main Camera" },
                new { Name = "HitStopService", File = "Combat/Feedback/HitStopService.cs", Host = "GameBootstrap" },
                new { Name = "CombatHitFeedbackHub", File = "Combat/Feedback/CombatHitFeedbackHub.cs", Host = "GameBootstrap" },
            };

            foreach (var svc in dangerousSingletons)
            {
                var filePath = Path.Combine(sourceDir, svc.File.Replace('/', '\\'));
                if (!File.Exists(filePath)) continue;

                var source = File.ReadAllText(filePath);
                if (source.Contains("Destroy(gameObject)"))
                {
                    report.Add($"  WARNING: {svc.Name} uses Destroy(gameObject) — host is shared ({svc.Host}), use Destroy(this) instead");
                    warnings++;
                }
                else
                {
                    report.Add($"  OK: {svc.Name} uses Destroy(this) (shared host: {svc.Host})");
                }
            }
        }

        private static void CheckNavMeshSetup(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- NavMesh Setup ---");

            var navMeshAgentType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "NavMeshAgent");
            var bridgeType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "NavigationAgentBridge");

            if (navMeshAgentType == null || bridgeType == null)
            {
                report.Add("  WARNING: NavMeshAgent or NavigationAgentBridge type not found");
                warnings++;
                return;
            }

            var bridges = Object.FindObjectsOfType(bridgeType);
            int navMeshCount = 0;
            int fallbackCount = 0;
            foreach (var b in bridges)
            {
                var bridge = b as MonoBehaviour;
                if (bridge == null) continue;
                var useNavMeshProp = bridgeType.GetProperty("UseNavMesh");
                if (useNavMeshProp != null && (bool)useNavMeshProp.GetValue(bridge))
                    navMeshCount++;
                else
                    fallbackCount++;
            }

            if (navMeshCount > 0)
                report.Add($"  OK: {navMeshCount} NavigationAgentBridge(s) using NavMesh mode");
            if (fallbackCount > 0)
                report.Add($"  WARNING: {fallbackCount} NavigationAgentBridge(s) in fallback mode (no NavMesh baked or no NavMeshAgent)");
            if (navMeshCount == 0 && fallbackCount == 0)
                report.Add("  INFO: No NavigationAgentBridge instances in current scene");

            var spawnPointType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "EncounterSpawnPoint");
            if (spawnPointType != null)
            {
                var spawnPoints = Object.FindObjectsOfType(spawnPointType);
                report.Add($"  INFO: {spawnPoints.Length} EncounterSpawnPoint(s) in scene");
            }

            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                var flags = GameObjectUtility.GetStaticEditorFlags(ground);
                if ((flags & StaticEditorFlags.NavigationStatic) != 0)
                    report.Add("  OK: Ground is marked NavigationStatic");
                else
                {
                    report.Add("  WARNING: Ground is not marked NavigationStatic — NavMesh bake will ignore it");
                    warnings++;
                }
            }
        }

        private static void CheckEncounterWaveConfig(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Encounter Wave Configuration ---");

            var encounterType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "EncounterRuntime");
            if (encounterType == null)
            {
                report.Add("  WARNING: EncounterRuntime type not found");
                warnings++;
                return;
            }

            var encounters = Object.FindObjectsOfType(encounterType);
            if (encounters.Length == 0)
            {
                report.Add("  INFO: No EncounterRuntime instances in current scene");
                return;
            }

            var wavesProp = encounterType.GetProperty("Waves");
            var spawnPointsProp = encounterType.GetProperty("SpawnPoints");
            var pendingProp = encounterType.GetProperty("PendingWaveCount");

            foreach (var enc in encounters)
            {
                var mono = enc as MonoBehaviour;
                if (mono == null) continue;
                report.Add($"  Encounter: {mono.gameObject.name}");

                if (wavesProp != null)
                {
                    var waves = wavesProp.GetValue(enc) as System.Collections.ICollection;
                    if (waves != null && waves.Count > 0)
                        report.Add($"    OK: {waves.Count} wave(s) configured");
                    else
                    {
                        report.Add("    WARNING: No waves configured — dynamic spawning disabled");
                        warnings++;
                    }
                }

                if (spawnPointsProp != null)
                {
                    var sps = spawnPointsProp.GetValue(enc) as System.Collections.ICollection;
                    if (sps != null && sps.Count > 0)
                        report.Add($"    OK: {sps.Count} spawn point(s)");
                    else
                        report.Add("    INFO: No spawn points — wave spawning will have nowhere to place units");
                }
            }
        }

        private static void CheckInputOwnership(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Input Ownership ---");

            var combatCtrlType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CombatController");
            if (combatCtrlType == null)
            {
                report.Add("  WARNING: CombatController type not found");
                warnings++;
                return;
            }

            var inputEnabledProp = combatCtrlType.GetProperty("IsInputEnabled");
            var allCombatCtrls = Object.FindObjectsOfType(combatCtrlType);
            var aiType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "SimpleCombatAI");

            int inputEnabledCount = 0;
            int inputDisabledCount = 0;
            foreach (var ctrl in allCombatCtrls)
            {
                var mono = ctrl as MonoBehaviour;
                if (mono == null) continue;

                var hasAI = aiType != null ? mono.GetComponent(aiType) : null;
                bool? inputEnabled = inputEnabledProp?.GetValue(ctrl) as bool?;

                if (hasAI != null && ((MonoBehaviour)hasAI).enabled && inputEnabled == true)
                {
                    report.Add($"  WARNING: '{mono.gameObject.name}' has both CombatController input enabled AND SimpleCombatAI active — only one should control input");
                    warnings++;
                }
                else if (inputEnabled == true)
                {
                    inputEnabledCount++;
                }
                else
                {
                    inputDisabledCount++;
                }
            }

            if (inputEnabledCount > 1)
            {
                report.Add($"  WARNING: {inputEnabledCount} CombatController(s) with input enabled — typically only 1 should accept player input");
                warnings++;
            }
            else if (inputEnabledCount == 1)
            {
                report.Add($"  OK: 1 CombatController with input enabled, {inputDisabledCount} disabled");
            }
            else if (allCombatCtrls.Length > 0)
            {
                report.Add("  WARNING: No CombatController has input enabled — player cannot move");
                warnings++;
            }

            var commanderCtrlType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CommanderControlController");
            if (commanderCtrlType != null)
            {
                var commanders = Object.FindObjectsOfType(commanderCtrlType);
                if (commanders.Length > 0)
                {
                    var stateProp = commanderCtrlType.GetProperty("State");
                    if (stateProp != null)
                    {
                        foreach (var cmd in commanders)
                        {
                            var state = stateProp.GetValue(cmd);
                            if (state == null) continue;
                            var isDirectOther = state.GetType().GetProperty("IsDirectControllingOther")?.GetValue(state) as bool?;
                            if (isDirectOther == true)
                                report.Add("  OK: CommanderPrototype — DirectControl active, controlled unit should have input enabled");
                            else
                                report.Add("  OK: CommanderPrototype — original player has input");
                        }
                    }
                }
            }
        }

        private static void CheckRootMovement(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Root Movement ---");

            var motorType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CharacterMovementMotor");
            if (motorType == null)
            {
                report.Add("  WARNING: CharacterMovementMotor type not found (expected at Assets/Scripts/Character/)");
                warnings++;
                return;
            }
            report.Add("  OK: CharacterMovementMotor type exists");

            var combatCtrlType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CombatController");
            var aiType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "SimpleCombatAI");
            var navBridgeType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "NavigationAgentBridge");

            int playersWithMotor = 0;
            int playersMissingMotor = 0;
            int aiWithMotor = 0;
            int aiMissingMotor = 0;
            int rootMotionViolations = 0;
            int frozenXZ = 0;
            int gravityOn = 0;
            int controllerOnVisual = 0;
            int aiOnVisual = 0;

            if (combatCtrlType != null)
            {
                var allCtrls = Object.FindObjectsOfType(combatCtrlType);
                foreach (var ctrl in allCtrls)
                {
                    var mono = ctrl as MonoBehaviour;
                    if (mono == null) continue;

                    if (mono.transform.parent != null && mono.gameObject.name == "Visual")
                    {
                        report.Add($"  WARNING: CombatController on Visual child '{mono.transform.parent.name}/Visual' — must be on PrefabRoot");
                        controllerOnVisual++;
                        warnings++;
                    }

                    if (mono.GetComponent(motorType) != null) playersWithMotor++;
                    else
                    {
                        playersMissingMotor++;
                        report.Add($"  WARNING: '{mono.gameObject.name}' has CombatController but no CharacterMovementMotor");
                        warnings++;
                    }

                    var animator = mono.GetComponent<Animator>();
                    if (animator != null && animator.applyRootMotion)
                    {
                        report.Add($"  WARNING: '{mono.gameObject.name}' Animator.applyRootMotion=true — will fight gameplay root movement");
                        rootMotionViolations++;
                        warnings++;
                    }

                    var rb = mono.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        if ((rb.constraints & RigidbodyConstraints.FreezePositionX) != 0 ||
                            (rb.constraints & RigidbodyConstraints.FreezePositionZ) != 0)
                        {
                            report.Add($"  WARNING: '{mono.gameObject.name}' Rigidbody freezes X/Z position — root cannot move");
                            frozenXZ++;
                            warnings++;
                        }
                        if (rb.useGravity && !rb.isKinematic)
                        {
                            report.Add($"  WARNING: '{mono.gameObject.name}' Rigidbody non-kinematic with gravity — may sink/fall");
                            gravityOn++;
                            warnings++;
                        }
                    }
                }
            }

            if (aiType != null)
            {
                var allAI = Object.FindObjectsOfType(aiType);
                foreach (var a in allAI)
                {
                    var mono = a as MonoBehaviour;
                    if (mono == null) continue;

                    if (mono.gameObject.name == "Visual" && mono.transform.parent != null)
                    {
                        report.Add($"  WARNING: SimpleCombatAI on Visual child '{mono.transform.parent.name}/Visual' — must be on PrefabRoot");
                        aiOnVisual++;
                        warnings++;
                    }

                    if (mono.GetComponent(motorType) != null) aiWithMotor++;
                    else
                    {
                        aiMissingMotor++;
                        report.Add($"  WARNING: AI '{mono.gameObject.name}' missing CharacterMovementMotor");
                        warnings++;
                    }

                    if (navBridgeType != null && mono.GetComponent(navBridgeType) == null)
                    {
                        report.Add($"  WARNING: AI '{mono.gameObject.name}' missing NavigationAgentBridge");
                        warnings++;
                    }

                    var animator = mono.GetComponent<Animator>();
                    if (animator != null && animator.applyRootMotion)
                    {
                        report.Add($"  WARNING: AI '{mono.gameObject.name}' Animator.applyRootMotion=true");
                        rootMotionViolations++;
                        warnings++;
                    }
                }
            }

            // Visual child localPosition sanity
            int visualOffsetIssues = 0;
            if (combatCtrlType != null)
            {
                foreach (var ctrl in Object.FindObjectsOfType(combatCtrlType))
                {
                    var mono = ctrl as MonoBehaviour;
                    if (mono == null) continue;
                    var visual = mono.transform.Find("Visual");
                    if (visual != null && visual.localPosition.y < -0.5f)
                    {
                        report.Add($"  WARNING: '{mono.gameObject.name}/Visual' localPosition.y={visual.localPosition.y:F2} (sunken)");
                        visualOffsetIssues++;
                        warnings++;
                    }
                }
            }

            report.Add($"  Players with motor: {playersWithMotor}, missing: {playersMissingMotor}");
            report.Add($"  AI with motor: {aiWithMotor}, missing: {aiMissingMotor}");
            if (rootMotionViolations == 0) report.Add("  OK: No applyRootMotion=true violations");
            if (frozenXZ == 0) report.Add("  OK: No Rigidbody freezing X/Z");
            if (gravityOn == 0) report.Add("  OK: No prototype Rigidbody with gravity+non-kinematic");
            if (controllerOnVisual == 0 && aiOnVisual == 0) report.Add("  OK: All controllers on PrefabRoot, not Visual");
            if (visualOffsetIssues == 0) report.Add("  OK: No Visual children sunken below -0.5y");
        }

        private static void CheckAnimationClipBindings(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Animation Clip Bindings ---");

            var clipGuids = AssetDatabase.FindAssets("t:AnimationClip");
            int clipsScanned = 0;
            int rootBindingViolations = 0;
            var problems = new List<string>();

            foreach (var guid in clipGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.StartsWith("Assets/")) continue;

                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) continue;
                clipsScanned++;

                var bindings = AnimationUtility.GetCurveBindings(clip);
                foreach (var b in bindings)
                {
                    if (b.type != typeof(Transform)) continue;
                    var prop = b.propertyName;
                    bool isPositionOrEuler =
                        prop.StartsWith("localPosition") ||
                        prop.StartsWith("m_LocalPosition") ||
                        prop.StartsWith("localEulerAngles") ||
                        prop.StartsWith("m_LocalEulerAngles");
                    if (!isPositionOrEuler) continue;

                    if (string.IsNullOrEmpty(b.path))
                    {
                        rootBindingViolations++;
                        problems.Add($"    {path} :: '{prop}' bound to ROOT (path='') — must be bound to 'Visual' or another child");
                    }
                }
            }

            report.Add($"  Scanned {clipsScanned} AnimationClip asset(s)");
            if (rootBindingViolations == 0)
            {
                report.Add("  OK: No animation clip binds Transform position/rotation to root path");
            }
            else
            {
                report.Add($"  WARNING: {rootBindingViolations} root-path binding(s) found:");
                foreach (var p in problems)
                    report.Add(p);
                warnings += rootBindingViolations;
            }
        }

        private static void CheckGameplayScriptAttachment(List<string> report, ref int warnings)
        {
            report.Add("");
            report.Add("--- Gameplay Script Attachment ---");

            var combatCtrlType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CombatController");
            var aiType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "SimpleCombatAI");
            var combatantType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "Combatant");
            var navBridgeType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "NavigationAgentBridge");

            int violations = 0;
            var localReport = report;

            System.Action<System.Type, string> checkType = (t, label) =>
            {
                if (t == null) return;
                foreach (var obj in Object.FindObjectsOfType(t))
                {
                    var mono = obj as MonoBehaviour;
                    if (mono == null) continue;
                    var go = mono.gameObject;
                    if (go.name == "Visual" || go.name == "Collision" || go.name == "Marker")
                    {
                        localReport.Add($"  WARNING: {label} attached to '{go.transform.parent?.name}/{go.name}' — must be on PrefabRoot, not child");
                        violations++;
                    }
                }
            };

            checkType(combatCtrlType, "CombatController");
            checkType(aiType, "SimpleCombatAI");
            checkType(combatantType, "Combatant");
            checkType(navBridgeType, "NavigationAgentBridge");

            warnings += violations;

            if (violations == 0)
                report.Add("  OK: All gameplay scripts attached to PrefabRoot, not Visual/Collision/Marker children");
        }

        private static void CheckCombatReadability(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Combat Readability ---");

            // 1. Type existence (compile-level)
            var presenterType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CombatantHealthBarPresenter");
            var damageNumType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "DamageNumberFeedback");
            var hitFlashType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "HitFlashFeedback");
            var broadcasterType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CombatFeedbackBroadcaster");

            if (presenterType == null) { report.Add("  ERROR: CombatantHealthBarPresenter type missing"); errors++; }
            else report.Add("  OK: CombatantHealthBarPresenter type exists");

            if (damageNumType == null) { report.Add("  ERROR: DamageNumberFeedback type missing"); errors++; }
            else report.Add("  OK: DamageNumberFeedback type exists");

            if (hitFlashType == null) { report.Add("  ERROR: HitFlashFeedback type missing"); errors++; }
            else report.Add("  OK: HitFlashFeedback type exists");

            if (broadcasterType == null) { report.Add("  ERROR: CombatFeedbackBroadcaster type missing"); errors++; }
            else report.Add("  OK: CombatFeedbackBroadcaster type exists");

            // 2. CombatPrototype: enemies have presenter
            CheckSceneEnemiesHaveHealthBar("Assets/Scenes/CombatPrototype.unity", "CombatPrototype", report, ref warnings);
            CheckSceneEnemiesHaveHealthBar("Assets/Scenes/CommanderPrototype.unity", "CommanderPrototype", report, ref warnings);
        }

        private static void CheckSceneEnemiesHaveHealthBar(string scenePath, string label, List<string> report, ref int warnings)
        {
            if (!File.Exists(scenePath))
            {
                report.Add($"  SKIP: {label} scene not found at {scenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                int enemiesChecked = 0;
                int enemiesMissing = 0;
                int maxHpZero = 0;
                foreach (var go in scene.GetRootGameObjects())
                {
                    foreach (var ai in go.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (ai == null) continue;
                        if (ai.GetType().Name != "SimpleCombatAI") continue;
                        enemiesChecked++;
                        var presenter = ai.GetComponent("CombatantHealthBarPresenter");
                        if (presenter == null) enemiesMissing++;

                        var combatantComp = ai.GetComponent("Combatant") as MonoBehaviour;
                        if (combatantComp != null)
                        {
                            var stats = combatantComp.GetType().GetProperty("Stats");
                            if (stats != null)
                            {
                                var statsVal = stats.GetValue(combatantComp);
                                var maxHp = statsVal?.GetType().GetField("maxHealth")?.GetValue(statsVal);
                                if (maxHp is float hp && hp <= 0f) maxHpZero++;
                            }
                        }
                    }
                }
                if (enemiesChecked == 0)
                {
                    report.Add($"  WARNING: {label} has no AI units");
                    warnings++;
                }
                else if (enemiesMissing > 0)
                {
                    report.Add($"  WARNING: {label} {enemiesMissing}/{enemiesChecked} enemies missing CombatantHealthBarPresenter");
                    warnings++;
                }
                else
                {
                    report.Add($"  OK: {label} all {enemiesChecked} enemies have health bar");
                }

                if (maxHpZero > 0)
                {
                    report.Add($"  WARNING: {label} {maxHpZero} units have maxHealth=0 at edit-time (will calc at runtime)");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CheckCombatBalance(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Combat Balance ---");

            // 1. CombatTuningConfig exists and is valid
            var configPath = "Assets/Data/Combat/CombatTuningConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<CombatTuningConfigSO>(configPath);
            if (config == null)
            {
                report.Add("  ERROR: CombatTuningConfig.asset missing");
                errors++;
            }
            else
            {
                report.Add("  OK: CombatTuningConfig.asset exists");
                if (!config.Validate(out var firstError))
                {
                    report.Add($"  ERROR: CombatTuningConfig invalid: {firstError}");
                    errors++;
                }
                else
                {
                    report.Add("  OK: CombatTuningConfig values valid");
                }

                if (config.playerAttackDamage > 0f)
                    report.Add($"  OK: playerAttackDamage override = {config.playerAttackDamage}");
                if (config.enemyAttackDamage > 0f)
                    report.Add($"  OK: enemyAttackDamage override = {config.enemyAttackDamage}");
                if (config.playerAttackRange > 0f)
                    report.Add($"  OK: playerAttackRange override = {config.playerAttackRange}");
                if (config.enemyAttackRange > 0f)
                    report.Add($"  OK: enemyAttackRange override = {config.enemyAttackRange}");
            }

            // 2. Player has hit feedback (HitFlashFeedback)
            CheckScenePlayerHasHitFeedback("Assets/Scenes/CombatPrototype.unity", "CombatPrototype", report, ref warnings);

            // 3. CombatPrototypeDebugController exists in CombatPrototype
            CheckSceneHasDebugController("Assets/Scenes/CombatPrototype.unity", report, ref warnings);

            // 4. Enemy AI attack feedback (windup marker)
            CheckSceneEnemyAIHasWindupFeedback("Assets/Scenes/CombatPrototype.unity", "CombatPrototype", report, ref warnings);
        }

        private static void CheckScenePlayerHasHitFeedback(string scenePath, string label, List<string> report, ref int warnings)
        {
            if (!File.Exists(scenePath)) { report.Add($"  SKIP: {label} not found for player feedback check"); return; }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                bool foundPlayer = false;
                bool hasFlash = false;
                foreach (var go in scene.GetRootGameObjects())
                {
                    var ctrl = go.GetComponentInChildren<CombatController>(true);
                    if (ctrl == null) continue;
                    foundPlayer = true;
                    if (ctrl.GetComponent<HitFlashFeedback>() != null)
                        hasFlash = true;
                }
                if (!foundPlayer)
                {
                    report.Add($"  WARNING: {label} no player (CombatController) found");
                    warnings++;
                }
                else if (!hasFlash)
                {
                    report.Add($"  WARNING: {label} player missing HitFlashFeedback");
                    warnings++;
                }
                else
                {
                    report.Add($"  OK: {label} player has HitFlashFeedback");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CheckSceneHasDebugController(string scenePath, List<string> report, ref int warnings)
        {
            if (!File.Exists(scenePath)) { report.Add("  SKIP: CombatPrototype not found for debug controller check"); return; }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var type = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.Name == "CombatPrototypeDebugController");
                if (type == null)
                {
                    report.Add("  WARNING: CombatPrototypeDebugController type not found");
                    warnings++;
                    return;
                }

                bool found = false;
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.GetComponentInChildren(type, true) != null)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    report.Add("  OK: CombatPrototypeDebugController present in CombatPrototype");
                else
                {
                    report.Add("  WARNING: CombatPrototypeDebugController missing from CombatPrototype");
                    warnings++;
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CheckSceneEnemyAIHasWindupFeedback(string scenePath, string label, List<string> report, ref int warnings)
        {
            if (!File.Exists(scenePath)) { report.Add($"  SKIP: {label} not found for AI feedback check"); return; }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                int aiCount = 0;
                int aiWithIndicator = 0;
                foreach (var go in scene.GetRootGameObjects())
                {
                    foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (mb == null) continue;
                        if (mb.GetType().Name != "SimpleCombatAI") continue;
                        aiCount++;
                        var indicatorField = mb.GetType().GetField("_showAttackIndicator",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (indicatorField != null && (bool)indicatorField.GetValue(mb))
                            aiWithIndicator++;
                    }
                }
                if (aiCount == 0)
                {
                    report.Add($"  WARNING: {label} no AI units");
                    warnings++;
                }
                else if (aiWithIndicator < aiCount)
                {
                    report.Add($"  WARNING: {label} {aiCount - aiWithIndicator}/{aiCount} AI units have attack indicator disabled");
                    warnings++;
                }
                else
                {
                    report.Add($"  OK: {label} all {aiCount} AI units have attack indicator enabled");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CheckEncounterReliability(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Encounter Reliability ---");

            // 1. aiStopDistance <= attackRange or warning
            var config = AssetDatabase.LoadAssetAtPath<CombatTuningConfigSO>("Assets/Data/Combat/CombatTuningConfig.asset");
            if (config != null && config.aiStopDistance > 0f)
            {
                // Check against typical attack range (2.2 + roleWeight*0.3 for Common = 2.5)
                var typicalRange = 2.5f;
                if (config.aiStopDistance > typicalRange)
                {
                    report.Add($"  WARNING: aiStopDistance ({config.aiStopDistance}) > typical attackRange ({typicalRange}) — AI may stop out of attack range");
                    warnings++;
                }
                else
                    report.Add($"  OK: aiStopDistance ({config.aiStopDistance}) <= typical attackRange ({typicalRange})");
            }
            else
                report.Add("  OK: aiStopDistance uses fallback (attackRange*0.8)");

            // 2. NavMesh mode report for CombatPrototype
            if (File.Exists("Assets/Scenes/CombatPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CombatPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    var bridgeType = System.AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                        .FirstOrDefault(t => t.Name == "NavigationAgentBridge");
                    int withBridge = 0;
                    int withNavAgent = 0;
                    foreach (var go in scene.GetRootGameObjects())
                    {
                        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
                        {
                            if (mb == null) continue;
                            if (mb.GetType().Name != "SimpleCombatAI") continue;
                            withBridge++;
                            if (mb.GetComponent<UnityEngine.AI.NavMeshAgent>() != null) withNavAgent++;
                        }
                    }
                    if (withBridge == 0)
                        report.Add("  WARNING: CombatPrototype has no AI units with NavigationAgentBridge");
                    else
                    {
                        report.Add($"  OK: CombatPrototype {withBridge} AI units, {withNavAgent} have NavMeshAgent");
                        if (withNavAgent < withBridge)
                            report.Add($"  NOTE: {withBridge - withNavAgent} AI units will use fallback movement (no NavMeshAgent)");
                    }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            // 3. CommanderPrototype: EncounterSpawnPoint count
            if (File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    int spawnPoints = 0;
                    int encounterRuntimes = 0;
                    foreach (var go in scene.GetRootGameObjects())
                    {
                        spawnPoints += go.GetComponentsInChildren<EncounterSpawnPoint>(true).Length;
                        encounterRuntimes += go.GetComponentsInChildren<EncounterRuntime>(true).Length;
                    }
                    if (encounterRuntimes > 0)
                        report.Add($"  OK: CommanderPrototype has {encounterRuntimes} EncounterRuntime(s)");
                    else
                    {
                        report.Add("  WARNING: CommanderPrototype has no EncounterRuntime");
                        warnings++;
                    }
                    if (spawnPoints > 0)
                        report.Add($"  OK: CommanderPrototype has {spawnPoints} EncounterSpawnPoint(s)");
                    else
                    {
                        report.Add("  WARNING: CommanderPrototype has no EncounterSpawnPoint — dynamic waves cannot spawn");
                        warnings++;
                    }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void CheckEncounterPersistence(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Encounter Persistence ---");

            var snapshotType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "EncounterSnapshot");
            if (snapshotType == null)
            {
                report.Add("  ERROR: EncounterSnapshot type missing (check Save/GameSaveData.cs)");
                errors++;
            }
            else
                report.Add("  OK: EncounterSnapshot type exists");

            var encounterType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "EncounterRuntime");
            if (encounterType != null)
            {
                var requiredMethods = new[] { "GetSnapshot", "RestoreSnapshot", "StartEncounter", "CompleteEncounter", "ResetEncounter", "ClearSpawnedUnits", "DespawnDeadUnits" };
                foreach (var m in requiredMethods)
                {
                    if (encounterType.GetMethod(m) != null)
                        report.Add($"  OK: EncounterRuntime.{m} exists");
                    else
                    {
                        report.Add($"  ERROR: EncounterRuntime.{m} missing");
                        errors++;
                    }
                }
                var props = new[] { "HasStarted", "HasCompleted", "LastOutcome", "TotalSpawnedCount", "SpawnedWaveIds", "NeedsRestartAfterLoad" };
                foreach (var p in props)
                {
                    if (encounterType.GetProperty(p) != null)
                        report.Add($"  OK: EncounterRuntime.{p} exists");
                    else
                    {
                        report.Add($"  WARNING: EncounterRuntime.{p} missing");
                        warnings++;
                    }
                }
            }

            var saveDataType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "GameSaveData");
            if (saveDataType != null)
            {
                var field = saveDataType.GetField("encounterSnapshots");
                if (field != null)
                    report.Add("  OK: GameSaveData.encounterSnapshots field exists");
                else
                {
                    report.Add("  ERROR: GameSaveData.encounterSnapshots field missing");
                    errors++;
                }
            }

            // Validate the EncounterSnapshot type carries needsRestartAfterLoad
            if (snapshotType != null)
            {
                var nrField = snapshotType.GetField("needsRestartAfterLoad");
                if (nrField != null)
                    report.Add("  OK: EncounterSnapshot.needsRestartAfterLoad field exists");
                else
                {
                    report.Add("  WARNING: EncounterSnapshot.needsRestartAfterLoad field missing");
                    warnings++;
                }
            }

            // Validate SaveLoadManager logs the dynamic-units limitation on restore.
            try
            {
                var saveLoadPath = System.IO.Path.Combine("Assets", "Scripts", "Save", "SaveLoadManager.cs");
                if (System.IO.File.Exists(saveLoadPath))
                {
                    var src = System.IO.File.ReadAllText(saveLoadPath);
                    if (src.Contains("Dynamic units are not fully serialized"))
                        report.Add("  OK: SaveLoadManager logs dynamic-units serialization limitation on restore");
                    else
                    {
                        report.Add("  WARNING: SaveLoadManager missing dynamic-units serialization warning");
                        warnings++;
                    }
                }
            }
            catch { /* validator must not crash on IO */ }

            // Validate MissionChainService duplicate guard.
            var missionChainType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "MissionChainService");
            if (missionChainType != null)
            {
                var record = missionChainType.GetMethod("RecordMissionResult");
                if (record != null && System.Linq.Enumerable.Any(record.GetParameters(),
                    p => p.Name == "allowDuplicate"))
                    report.Add("  OK: MissionChainService.RecordMissionResult has allowDuplicate guard");
                else
                {
                    report.Add("  WARNING: MissionChainService.RecordMissionResult missing allowDuplicate guard");
                    warnings++;
                }
            }

            // Reference the design doc as a soft check.
            var designPath = System.IO.Path.Combine("Assets", "Docs", "ENCOUNTER_PERSISTENCE_DESIGN.md");
            if (System.IO.File.Exists(designPath))
                report.Add("  OK: ENCOUNTER_PERSISTENCE_DESIGN.md exists");
            else
            {
                report.Add("  WARNING: ENCOUNTER_PERSISTENCE_DESIGN.md missing");
                warnings++;
            }

            var triggerType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "MissionTriggerZone");
            if (triggerType != null)
            {
                var startedProp = triggerType.GetProperty("MissionStarted");
                var completedProp = triggerType.GetProperty("MissionCompleted");
                if (startedProp != null && completedProp != null)
                    report.Add("  OK: MissionTriggerZone has MissionStarted + MissionCompleted guards");
                else
                {
                    report.Add("  WARNING: MissionTriggerZone missing started/completed guards");
                    warnings++;
                }
            }
        }

        private static void CheckDemoFlow(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Demo Flow ---");

            if (typeof(DemoFlowManager) != null) report.Add("  OK: DemoFlowManager type exists");
            if (typeof(DemoFlowHud) != null) report.Add("  OK: DemoFlowHud type exists");

            var go = new GameObject("ValidatorDemoFlow");
            try
            {
                var manager = go.AddComponent<DemoFlowManager>();
                manager.RefreshFromMissionChain((MissionChainState)null);
                if (manager.GetNextMissionId() == DemoFlowManager.ConvoyMissionId)
                    report.Add("  OK: DemoFlow returns Mission 1 for missing chain");
                else
                {
                    report.Add("  ERROR: DemoFlow missing-chain fallback is not Mission 1");
                    errors++;
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }

            var debugType = typeof(PrototypeDebugController);
            if (debugType.GetMethod("TeleportPlayerToCityGateDisputeArea") != null)
                report.Add("  OK: F8 CityGate teleport hook exists");
            else
            {
                report.Add("  ERROR: F8 CityGate teleport hook missing");
                errors++;
            }

            if (File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    var hasManager = false;
                    var hasHud = false;
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        hasManager |= root.GetComponentsInChildren<DemoFlowManager>(true).Length > 0;
                        hasHud |= root.GetComponentsInChildren<DemoFlowHud>(true).Length > 0;
                    }

                    if (hasManager) report.Add("  OK: CommanderPrototype contains DemoFlowManager");
                    else { report.Add("  ERROR: CommanderPrototype missing DemoFlowManager"); errors++; }
                    if (hasHud) report.Add("  OK: CommanderPrototype contains DemoFlowHud");
                    else { report.Add("  ERROR: CommanderPrototype missing DemoFlowHud"); errors++; }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void CheckPlayableDemoReadability(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Playable Demo Readability ---");

            var requiredHudTypes = new[]
            {
                typeof(DemoFlowHud), typeof(MissionObjectiveHud), typeof(MissionResultSummaryPanel),
                typeof(CommanderControlHintPanel), typeof(CommanderDebugHud)
            };
            foreach (var type in requiredHudTypes)
                report.Add($"  OK: {type.Name} type exists");

            var shortcuts = DemoFlowHud.BuildShortcutHelpLines(false);
            var shortcutText = string.Join(" ", shortcuts);
            var expectedShortcuts = new[] { "1", "2", "3", "F7", "F8", "F5", "F9", "F10", "Tab/Q", "E", "R", "Left Click", "Space" };
            foreach (var key in expectedShortcuts)
            {
                if (shortcutText.Contains(key)) report.Add($"  OK: shortcut help includes {key}");
                else { report.Add($"  ERROR: shortcut help missing {key}"); errors++; }
            }

            if (typeof(CommanderPrototypeRuntime).GetMethod("OnMissionCompleted") != null)
                report.Add("  OK: CommanderPrototypeRuntime mission completion hook exists");
            else { report.Add("  ERROR: CommanderPrototypeRuntime mission completion hook missing"); errors++; }

            if (typeof(PrototypeDebugController).GetMethod("TeleportPlayerToCityGateDisputeArea") != null)
                report.Add("  OK: F8 teleport hook exists");
            else { report.Add("  ERROR: F8 teleport hook missing"); errors++; }

            var checklistPath = Path.Combine("Assets", "Docs", "MANUAL_DEMO_VALIDATION_CHECKLIST.md");
            if (File.Exists(checklistPath)) report.Add("  OK: MANUAL_DEMO_VALIDATION_CHECKLIST.md exists");
            else { report.Add("  ERROR: MANUAL_DEMO_VALIDATION_CHECKLIST.md missing"); errors++; }
        }

        private static void CheckHudLayout(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- HUD Layout ---");

            var defaultPanels = new Dictionary<string, Rect>
            {
                { "DemoFlow", DebugUILayout.GetDemoFlowRect(1280, 720) },
                { "MissionObjective", DebugUILayout.GetMissionObjectiveRect(1280, 720) },
                { "ControlHint", DebugUILayout.GetControlHintRect(1280, 720) },
                { "CommanderHud", DebugUILayout.GetCommanderHudRect(1280, 720) },
                { "MissionResultSummary", DebugUILayout.GetMissionResultSummaryRect(1280, 720) }
            };

            foreach (var panel in defaultPanels)
            {
                if (panel.Value.width > 0f && panel.Value.height > 0f)
                    report.Add($"  OK: {panel.Key} rect positive ({panel.Value})");
                else { report.Add($"  ERROR: {panel.Key} rect has non-positive size ({panel.Value})"); errors++; }
            }

            if (defaultPanels["DemoFlow"].xMax <= defaultPanels["ControlHint"].xMin &&
                defaultPanels["MissionObjective"].xMax <= defaultPanels["CommanderHud"].xMin)
                report.Add("  OK: default layout separates left guidance from right commander panels");
            else { report.Add("  ERROR: default HUD columns overlap heavily at 1280x720"); errors++; }

            if (!DebugUILayout.OverlapsHeavily(defaultPanels["CommanderHud"], defaultPanels["MissionResultSummary"]))
                report.Add("  OK: commander and result panels do not overlap heavily");
            else { report.Add("  ERROR: commander and result panels overlap heavily"); errors++; }

            if (DebugUILayout.IsCompact(800))
                report.Add("  OK: compact layout enabled below 1024 width");
            else { report.Add("  ERROR: compact layout not enabled below 1024 width"); errors++; }

            var compactPanels = new[]
            {
                DebugUILayout.GetDemoFlowRect(800, 600),
                DebugUILayout.GetMissionObjectiveRect(800, 600),
                DebugUILayout.GetControlHintRect(800, 600),
                DebugUILayout.GetMissionResultSummaryRect(800, 600)
            };
            foreach (var rect in compactPanels)
            {
                if (rect.width > 0f && rect.height > 0f && rect.x >= 0f && rect.y >= 0f)
                    continue;
                report.Add($"  ERROR: compact rect unsafe ({rect})");
                errors++;
            }
        }

        private static void CheckMissionMarkerCoverage(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Mission Marker Coverage ---");

            var required = new Dictionary<string, string[]>
            {
                { "Convoy marker", new[] { "Convoy", "Convoy_Objective" } },
                { "EnergyNode marker", new[] { "Energy Node", "Energy_Node" } },
                { "Border marker", new[] { "Border Retaliation", "Area_BorderRetaliation" } },
                { "RaiderSpawn marker", new[] { "Raider Spawn", "BorderSpawnPoint_Beast" } },
                { "CityGate marker", new[] { "City Gate Mission Area", "Area_CityGateDispute" } },
                { "CityGateCore marker", new[] { "CityGateCore", "CityGateCore_Objective" } },
                { "BeastNegotiator marker", new[] { "BeastNegotiator" } },
                { "BeastRaiderSpawn marker", new[] { "BeastRaider Spawn", "CityGateSpawnPoint_Beast" } },
                { "Low-rank controllable marker", new[] { "Low-Rank Ally", "Press E to Control", "MechaGateGuard" } },
                { "High-rank denied marker", new[] { "High-Rank Unit", "Tactical Command Only", "MechaHardliner" } },
                { "Allied defense marker", new[] { "Allied Defense Point", "Border_ObjectiveMarker" } }
            };

            if (!File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                report.Add("  ERROR: CommanderPrototype.unity missing; cannot inspect marker coverage");
                errors++;
                return;
            }

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
            try
            {
                var text = new List<string>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                        text.Add(tr.name);
                    foreach (var marker in root.GetComponentsInChildren<WorldMarker>(true))
                    {
                        if (!string.IsNullOrEmpty(marker.CustomLabel)) text.Add(marker.CustomLabel);
                        var fallback = WorldMarker.BuildReadableLabel(marker.gameObject.name);
                        if (!string.IsNullOrEmpty(fallback)) text.Add(fallback);
                    }
                }

                foreach (var item in required)
                {
                    var found = item.Value.Any(token => text.Any(value => value.Contains(token)));
                    if (found) report.Add($"  OK: {item.Key} exists");
                    else { report.Add($"  ERROR: {item.Key} missing"); errors++; }
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CheckMissionAuthoring(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Mission Authoring ---");

            CheckMissionDefinitionAsset(report, ref errors, "Assets/Data/Missions/ConvoyEnergyConflict.asset", DemoFlowManager.ConvoyMissionId, "Convoy Energy Conflict");
            CheckMissionDefinitionAsset(report, ref errors, "Assets/Data/Missions/BorderRetaliation.asset", DemoFlowManager.BorderMissionId, "Border Retaliation");
            CheckMissionDefinitionAsset(report, ref errors, "Assets/Data/Missions/CityGateDispute.asset", DemoFlowManager.CityGateMissionId, "City Gate Dispute");
        }

        private static void CheckMissionDefinitionAsset(List<string> report, ref int errors, string path, string missionId, string displayName)
        {
            var asset = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
            if (asset == null)
            {
                report.Add($"  ERROR: {path} missing");
                errors++;
                return;
            }

            if (asset.MissionId == missionId) report.Add($"  OK: {displayName} missionId");
            else { report.Add($"  ERROR: {path} missionId '{asset.MissionId}' expected '{missionId}'"); errors++; }
            if (!string.IsNullOrEmpty(asset.DisplayName)) report.Add($"  OK: {displayName} displayName");
            else { report.Add($"  ERROR: {path} missing displayName"); errors++; }
            if (asset.DefaultObjectives != null && asset.DefaultObjectives.Count > 0) report.Add($"  OK: {displayName} objectives authored");
            else { report.Add($"  ERROR: {path} has no objectives"); errors++; }
            if (asset.OutcomeConsequences != null && asset.OutcomeConsequences.Count > 0) report.Add($"  OK: {displayName} outcomes authored");
            else { report.Add($"  ERROR: {path} has no outcomes"); errors++; }
        }

        private static void CheckCommanderActionReadability(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Commander Action Readability ---");

            if (typeof(CommanderActionType).IsEnum) report.Add("  OK: CommanderActionType enum exists");
            if (typeof(CommanderActionPresenter) != null) report.Add("  OK: CommanderActionPresenter type exists");

            var state = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "Validator Low",
                LastDirectControlAllowed = true,
                LastTacticalCommandAllowed = true,
                LastSyncAssistAllowed = true,
                LastDefendObjectiveAllowed = true,
                LastFocusFireAllowed = true,
                LastObjectiveTargetName = "CityGateCore",
                LastFocusTargetName = "BeastRaider_01"
            };
            var descriptors = CommanderActionPresenter.BuildDescriptors(state);
            var expectedActions = new[]
            {
                CommanderActionType.DirectControl,
                CommanderActionType.TacticalCommand,
                CommanderActionType.SyncAssist,
                CommanderActionType.DefendObjective,
                CommanderActionType.FocusFire
            };
            if (descriptors.Count == expectedActions.Length && expectedActions.All(action => descriptors.Any(d => d.ActionType == action)))
                report.Add("  OK: DirectControl / TacticalCommand / SyncAssist / DefendObjective / FocusFire descriptors available");
            else
            {
                report.Add("  ERROR: CommanderActionPresenter did not build 5 expected descriptors");
                errors++;
            }

            if (typeof(CommanderDebugHud) != null && typeof(CommanderControlHintPanel) != null)
                report.Add("  OK: Commander HUD and hint panel can access action descriptions");

            if (File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    var hasLow = false;
                    var hasHigh = false;
                    foreach (var root in scene.GetRootGameObjects())
                    foreach (var marker in root.GetComponentsInChildren<WorldMarker>(true))
                    {
                        var label = marker.CustomLabel ?? string.Empty;
                        hasLow |= label.Contains("Low-Rank") || label.Contains("MechaGateGuard") || label.Contains("[LOW]");
                        hasHigh |= label.Contains("High-Rank") || label.Contains("Tactical Command Only") || label.Contains("[HIGH]") || label.Contains("[DENIED]");
                    }

                    if (hasLow) report.Add("  OK: low-rank controllable marker exists");
                    else { report.Add("  WARNING: low-rank controllable marker missing"); warnings++; }
                    if (hasHigh) report.Add("  OK: high-rank denied marker exists");
                    else { report.Add("  WARNING: high-rank denied marker missing"); warnings++; }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void CheckCommanderActionExpansion(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Commander Action Expansion ---");

            if (System.Enum.IsDefined(typeof(CommanderActionType), "DefendObjective"))
                report.Add("  OK: CommanderActionType.DefendObjective exists");
            else { report.Add("  ERROR: CommanderActionType.DefendObjective missing"); errors++; }

            if (System.Enum.IsDefined(typeof(CommanderActionType), "FocusFire"))
                report.Add("  OK: CommanderActionType.FocusFire exists");
            else { report.Add("  ERROR: CommanderActionType.FocusFire missing"); errors++; }

            if (System.Enum.IsDefined(typeof(CommanderCommandType), "DefendObjective"))
                report.Add("  OK: CommanderCommandType.DefendObjective exists");
            else { report.Add("  ERROR: CommanderCommandType.DefendObjective missing"); errors++; }

            if (System.Enum.IsDefined(typeof(CommanderCommandType), "FocusFire"))
                report.Add("  OK: CommanderCommandType.FocusFire exists");
            else { report.Add("  ERROR: CommanderCommandType.FocusFire missing"); errors++; }

            var descriptors = CommanderActionPresenter.BuildDescriptors(new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "Validator Target",
                LastDefendObjectiveAllowed = true,
                LastFocusFireAllowed = true,
                LastObjectiveTargetName = "CityGateCore",
                LastFocusTargetName = "BeastRaider_01"
            });
            if (descriptors.Any(d => d.ActionType == CommanderActionType.DefendObjective)
                && descriptors.Any(d => d.ActionType == CommanderActionType.FocusFire))
                report.Add("  OK: CommanderActionPresenter describes DefendObjective / FocusFire");
            else { report.Add("  ERROR: CommanderActionPresenter missing expansion descriptors"); errors++; }

            if (typeof(CommanderControlController).GetMethod("TryIssueDefendObjective", System.Type.EmptyTypes) != null)
                report.Add("  OK: CommanderControlController.TryIssueDefendObjective exists");
            else { report.Add("  ERROR: TryIssueDefendObjective missing"); errors++; }

            if (typeof(CommanderControlController).GetMethod("TryIssueFocusFire", System.Type.EmptyTypes) != null)
                report.Add("  OK: CommanderControlController.TryIssueFocusFire exists");
            else { report.Add("  ERROR: TryIssueFocusFire missing"); errors++; }

            if (typeof(TacticalCommandState).GetMethod("SetDefendObjective") != null
                && typeof(TacticalCommandState).GetMethod("SetFocusFire") != null)
                report.Add("  OK: TacticalCommandState supports defend/focus payloads");
            else { report.Add("  ERROR: TacticalCommandState missing defend/focus methods"); errors++; }

            var aiType = typeof(SimpleCombatAI);
            if (aiType.GetMethod("SetDefendObjective") != null && aiType.GetMethod("SetFocusFireTarget") != null)
                report.Add("  OK: SimpleCombatAI supports defend objective and focus fire");
            else { report.Add("  ERROR: SimpleCombatAI missing command helpers"); errors++; }

            var shortcutText = string.Join(" ", DemoFlowHud.BuildShortcutHelpLines(false));
            foreach (var key in new[] { "G", "F", "1", "2", "3", "F7", "F8", "F5", "F9", "F10" })
            {
                if (shortcutText.Contains(key)) report.Add($"  OK: shortcut help includes {key}");
                else { report.Add($"  ERROR: shortcut help missing {key}"); errors++; }
            }

            if (File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    var text = new List<string>();
                    foreach (var root in scene.GetRootGameObjects())
                    foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                    {
                        text.Add(tr.name);
                        var marker = tr.GetComponent<WorldMarker>();
                        if (marker != null && !string.IsNullOrEmpty(marker.CustomLabel)) text.Add(marker.CustomLabel);
                        var fallback = WorldMarker.BuildReadableLabel(tr.gameObject.name);
                        if (!string.IsNullOrEmpty(fallback)) text.Add(fallback);
                    }

                    if (text.Any(v => v.Contains("Low-Rank") || v.Contains("MechaGateGuard"))) report.Add("  OK: commandable low-rank ally exists");
                    else { report.Add("  WARNING: commandable low-rank ally marker missing"); warnings++; }
                    if (text.Any(v => v.Contains("Objective") || v.Contains("CityGateCore") || v.Contains("Convoy"))) report.Add("  OK: defend objective marker exists");
                    else { report.Add("  WARNING: defend objective marker missing"); warnings++; }
                    if (text.Any(v => v.Contains("BeastRaider") || v.Contains("Raider"))) report.Add("  OK: hostile focus target exists");
                    else { report.Add("  WARNING: hostile focus target missing"); warnings++; }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void CheckAIBehaviorProfiles(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- AI Behavior Profiles ---");

            if (typeof(AIBehaviorProfileSO) != null && typeof(AIBehaviorProfileType).IsEnum)
                report.Add("  OK: AIBehaviorProfileSO / AIBehaviorProfileType runtime types exist");

            var required = new[]
            {
                AIBehaviorProfileType.AggressiveRaider,
                AIBehaviorProfileType.DefensiveGuard,
                AIBehaviorProfileType.Negotiator,
                AIBehaviorProfileType.Hardliner,
                AIBehaviorProfileType.CommanderUnit,
                AIBehaviorProfileType.NeutralCivilian
            };

            foreach (var type in required)
            {
                var path = $"Assets/Data/AIProfiles/{type}.asset";
                var profile = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>(path);
                if (profile == null)
                {
                    report.Add($"  ERROR: Missing AI behavior profile asset: {path}");
                    errors++;
                    continue;
                }

                if (profile.Validate(out var error))
                    report.Add($"  OK: {type} profile validates ({profile.DisplayLabel})");
                else
                {
                    report.Add($"  ERROR: {type} profile invalid: {error}");
                    errors++;
                }
            }

            var simpleAiProfile = typeof(SimpleCombatAI).GetProperty("BehaviorProfile");
            if (simpleAiProfile != null)
                report.Add("  OK: SimpleCombatAI exposes BehaviorProfile");
            else
            {
                report.Add("  ERROR: SimpleCombatAI.BehaviorProfile missing");
                errors++;
            }

            var negotiator = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>("Assets/Data/AIProfiles/Negotiator.asset");
            var guard = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>("Assets/Data/AIProfiles/DefensiveGuard.asset");
            var raider = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>("Assets/Data/AIProfiles/AggressiveRaider.asset");
            var hardliner = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>("Assets/Data/AIProfiles/Hardliner.asset");
            var commander = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>("Assets/Data/AIProfiles/CommanderUnit.asset");

            if (negotiator != null && !negotiator.canInitiateCombat) report.Add("  OK: Negotiator cannot initiate combat"); else { report.Add("  ERROR: Negotiator can initiate combat"); errors++; }
            if (negotiator != null && !negotiator.respondsToFocusFire) report.Add("  OK: Negotiator ignores FocusFire responder role"); else { report.Add("  ERROR: Negotiator responds to FocusFire"); errors++; }
            if (guard != null && guard.respondsToDefendObjective && guard.maxChaseDistanceFromHome > 0f && guard.guardLeashRadius > 0f) report.Add("  OK: DefensiveGuard can defend objective and has guard leash/chase limit"); else { report.Add("  ERROR: DefensiveGuard missing defend/leash/chase semantics"); errors++; }
            if (raider != null && raider.canInitiateCombat && raider.prefersObjectiveTargets && raider.objectivePressureWeight > raider.hostileUnitWeight) report.Add("  OK: AggressiveRaider can pressure objectives over generic hostiles"); else { report.Add("  ERROR: AggressiveRaider missing objective pressure tuning"); errors++; }
            if (guard != null && raider != null && guard.maxChaseDistanceFromHome < raider.maxChaseDistanceFromHome) report.Add("  OK: DefensiveGuard chase limit is more conservative than Raider"); else { report.Add("  ERROR: DefensiveGuard chase limit is not more conservative than Raider"); errors++; }
            if (hardliner != null && hardliner.prefersProtectedTargets && hardliner.canAttackNeutral && hardliner.hardlinerEscalationBias > 0f) report.Add("  OK: Hardliner can target protected/neutral unit with escalation bias"); else { report.Add("  ERROR: Hardliner missing escalation semantics"); errors++; }
            if (commander != null && commander.respondsToTacticalCommand && commander.respondsToDefendObjective && commander.respondsToFocusFire) report.Add("  OK: CommanderUnit responds to tactical commands"); else { report.Add("  ERROR: CommanderUnit missing command response semantics"); errors++; }

            var monitorType = typeof(AIBehaviorScenarioMonitor);
            if (monitorType != null && monitorType.GetMethod("BuildScenarioSummary") != null)
                report.Add("  OK: AIBehaviorScenarioMonitor exposes behavior summaries for validation/tests");
            else
            {
                report.Add("  ERROR: AIBehaviorScenarioMonitor missing summary API");
                errors++;
            }

            if (typeof(CommanderActionPresenter).GetMethod("BuildProfileSummary") != null
                && typeof(CommanderActionPresenter).GetMethod("BuildBehaviorSummary") != null
                && typeof(CommanderActionPresenter).GetMethod("BuildProfileSuggestion", new[] { typeof(SimpleCombatAI) }) != null)
                report.Add("  OK: HUD/presenter can display profile label, behavior, and suggestion");
            else
            {
                report.Add("  ERROR: HUD/presenter missing profile behavior text helpers");
                errors++;
            }

            var presenterMethod = typeof(CommanderActionPresenter).GetMethod("BuildDescriptors");
            if (presenterMethod != null)
                report.Add("  OK: HUD/presenter can display profile-aware action suggestions");
        }

        private static void CheckCityGateDispute(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- City Gate Dispute (Mission 3) ---");

            var runtimeType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CityGateDisputeRuntime");
            if (runtimeType != null)
            {
                report.Add("  OK: CityGateDisputeRuntime type exists");

                var requiredProps = new[] { "Phase", "Encounter", "CityGateCore", "BeastNegotiator", "BeastRaidersDefeated", "NegotiatorSurvived", "CoreSurvived", "MechaCasualties", "BeastCasualties", "IsInitialized" };
                foreach (var p in requiredProps)
                {
                    if (runtimeType.GetProperty(p) != null)
                        report.Add($"  OK: CityGateDisputeRuntime.{p} exists");
                    else
                    {
                        report.Add($"  WARNING: CityGateDisputeRuntime.{p} missing");
                        warnings++;
                    }
                }

                var initializeMethod = runtimeType.GetMethod("Initialize", new[] { typeof(LuoLuoTripGameContext) });
                if (initializeMethod != null)
                    report.Add("  OK: CityGateDisputeRuntime.Initialize(context) exists");
                else
                {
                    report.Add("  ERROR: CityGateDisputeRuntime.Initialize(context) missing");
                    errors++;
                }

                var resolveMethod = runtimeType.GetMethod("ResolveOutcome", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (resolveMethod != null)
                    report.Add("  OK: CityGateDisputeRuntime.ResolveOutcome static method exists");
                else
                {
                    report.Add("  WARNING: CityGateDisputeRuntime.ResolveOutcome missing");
                    warnings++;
                }
            }
            else
            {
                report.Add("  ERROR: CityGateDisputeRuntime type missing");
                errors++;
            }

            var outcomeType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "MissionOutcomeType");
            if (outcomeType != null && outcomeType.IsEnum)
            {
                var requiredOutcomes = new[] { "BalancedMediation", "MechaSuppression", "BeastNegotiation", "FailedEscalation", "PartialContainment" };
                foreach (var name in requiredOutcomes)
                {
                    if (System.Enum.Parse(outcomeType, name) != null)
                        report.Add($"  OK: MissionOutcomeType.{name} exists");
                    else
                    {
                        report.Add($"  ERROR: MissionOutcomeType.{name} missing");
                        errors++;
                    }
                }
            }

            var chainType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "MissionChainService");
            if (chainType != null)
            {
                var recordMethod = chainType.GetMethod("RecordMissionResult");
                if (recordMethod != null && System.Linq.Enumerable.Any(recordMethod.GetParameters(),
                    p => p.Name == "allowDuplicate"))
                    report.Add("  OK: MissionChainService.RecordMissionResult has allowDuplicate guard");
                else
                {
                    report.Add("  WARNING: MissionChainService.RecordMissionResult missing allowDuplicate guard");
                    warnings++;
                }
            }

            var missionAsset = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/CityGateDispute.asset");
            if (missionAsset != null && missionAsset.MissionId == "city_gate_dispute")
                report.Add("  OK: CityGateDispute MissionDefinitionSO exists");
            else
            {
                report.Add("  ERROR: Assets/Data/Missions/CityGateDispute.asset missing or invalid");
                errors++;
            }

            var debugType = typeof(PrototypeDebugController);
            if (debugType.GetMethod("TeleportPlayerToCityGateDisputeArea") != null)
                report.Add("  OK: PrototypeDebugController exposes F8 CityGate teleport method");
            else
            {
                report.Add("  ERROR: PrototypeDebugController.TeleportPlayerToCityGateDisputeArea missing");
                errors++;
            }

            if (File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    bool hasCityGate = false;
                    bool hasCore = false;
                    bool hasNegotiator = false;
                    bool hasSpawn = false;
                    bool hasSummary = false;
                    bool hasMonitor = false;
                    bool hasProfileLabels = false;
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        if (root.name.Contains("CityGate")) hasCityGate = true;
                        if (root.GetComponentsInChildren<MissionResultSummaryPanel>(true).Length > 0) hasSummary = true;
                        if (root.GetComponentsInChildren<AIBehaviorScenarioMonitor>(true).Length > 0) hasMonitor = true;
                        foreach (var ai in root.GetComponentsInChildren<SimpleCombatAI>(true))
                        {
                            var label = ai.BehaviorProfile != null ? ai.BehaviorProfile.DisplayLabel : string.Empty;
                            if (label.Contains("Raider: Aggressive") || label.Contains("Guard: Defensive") || label.Contains("Negotiator: Non-combatant") || label.Contains("Hardliner: Escalation risk") || label.Contains("CommanderUnit: Tactical only"))
                                hasProfileLabels = true;
                        }
                        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                        {
                            if (tr.name.Contains("CityGateCore")) hasCore = true;
                            if (tr.name.Contains("BeastNegotiator")) hasNegotiator = true;
                            if (tr.name.Contains("CityGateSpawnPoint_Beast") || tr.name.Contains("BeastRaider")) hasSpawn = true;
                        }
                        foreach (var marker in root.GetComponentsInChildren<WorldMarker>(true))
                        {
                            var label = marker.CustomLabel ?? string.Empty;
                            if (label.Contains("CityGateCore")) hasCore = true;
                            if (label.Contains("BeastNegotiator")) hasNegotiator = true;
                            if (label.Contains("BeastRaiders")) hasSpawn = true;
                        }
                    }
                    if (hasCityGate) report.Add("  OK: CommanderPrototype contains CityGate scene objects/marker");
                    else { report.Add("  ERROR: CommanderPrototype missing CityGate marker/object"); errors++; }
                    if (hasCore) report.Add("  OK: CityGateCore marker exists");
                    else { report.Add("  ERROR: CityGateCore marker missing"); errors++; }
                    if (hasNegotiator) report.Add("  OK: BeastNegotiator marker exists");
                    else { report.Add("  ERROR: BeastNegotiator marker missing"); errors++; }
                    if (hasSpawn) report.Add("  OK: BeastRaider spawn marker exists");
                    else { report.Add("  WARNING: BeastRaider spawn marker missing"); warnings++; }
                    if (hasSummary) report.Add("  OK: outcome summary display exists");
                    else { report.Add("  ERROR: MissionResultSummaryPanel missing"); errors++; }
                    if (hasMonitor) report.Add("  OK: CityGateDispute has AIBehaviorScenarioMonitor");
                    else { report.Add("  ERROR: CityGateDispute missing AIBehaviorScenarioMonitor"); errors++; }
                    if (hasProfileLabels) report.Add("  OK: CityGate key units expose readable AI profile labels");
                    else { report.Add("  ERROR: CityGate key unit profile labels missing"); errors++; }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            var designPath = System.IO.Path.Combine("Assets", "Docs", "CITY_GATE_DISPUTE_DESIGN.md");
            if (System.IO.File.Exists(designPath))
                report.Add("  OK: CITY_GATE_DISPUTE_DESIGN.md exists");
            else
            {
                report.Add("  WARNING: CITY_GATE_DISPUTE_DESIGN.md missing");
                warnings++;
            }

            var checklistPath = System.IO.Path.Combine("Assets", "Docs", "MANUAL_DEMO_VALIDATION_CHECKLIST.md");
            if (System.IO.File.Exists(checklistPath) && System.IO.File.ReadAllText(checklistPath).Contains("AI behavior tuning validation"))
                report.Add("  OK: manual checklist includes AI behavior tuning validation");
            else
            {
                report.Add("  ERROR: manual checklist missing AI behavior tuning validation section");
                errors++;
            }
        }

        private static void CheckCommanderControlUsability(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Commander Control Usability ---");

            var controlType = typeof(CommanderControlController);
            var selectorType = typeof(CommanderTargetSelector);
            var stateType = typeof(CommanderControlRuntimeState);

            report.Add(controlType != null ? "  OK: CommanderControlController type exists" : "  ERROR: CommanderControlController type missing");
            report.Add(selectorType != null ? "  OK: CommanderTargetSelector type exists" : "  ERROR: CommanderTargetSelector type missing");

            var diagnosticFields = new[]
            {
                "LastControlAttemptTime", "LastControlResult", "LastControlRejectReason",
                "LastSelectedTargetName", "LastSelectedTargetRank", "LastSelectedTargetRequiredLevel",
                "LastSelectedTargetTrust", "LastSelectedTargetIsLeader", "LastSelectedTargetAllowDirectControl",
                "LastSelectedTargetAllowTacticalCommand", "LastCommanderLevel", "LastInputRoute", "LastSuggestion"
            };
            foreach (var field in diagnosticFields)
            {
                if (stateType.GetField(field) != null)
                    report.Add($"  OK: CommanderControlRuntimeState.{field} exists");
                else
                {
                    report.Add($"  ERROR: CommanderControlRuntimeState.{field} missing");
                    errors++;
                }
            }

            if (controlType.GetMethod("TryInteract") != null)
                report.Add("  OK: CommanderControlController.TryInteract is callable");
            else
            {
                report.Add("  ERROR: CommanderControlController.TryInteract missing/public-inaccessible");
                errors++;
            }

            if (controlType.GetMethod("HasSelectedTarget") != null)
                report.Add("  OK: CommanderControlController.HasSelectedTarget exists for E-priority checks");
            else
            {
                report.Add("  WARNING: CommanderControlController.HasSelectedTarget missing");
                warnings++;
            }

            if (selectorType.GetMethod("TrySelectTarget") != null && selectorType.GetMethod("GetCandidates") != null)
                report.Add("  OK: CommanderTargetSelector exposes selection and candidate APIs");
            else
            {
                report.Add("  ERROR: CommanderTargetSelector selection/candidate APIs missing");
                errors++;
            }

            var service = new ControlPermissionService();
            var commander = CommanderProfile.CreateDefault();
            var lowRank = new ControlPermissionRequest
            {
                Commander = commander,
                Target = CharacterControlInfo.FromCharacterData(CharacterData.Create("validator_low", "Validator Low", SubFactionId.MotorIronRiders, CharacterRole.Minion)),
                CurrentControlledUnitCount = 0,
                FactionTrust = 40
            };
            var lowResult = service.Evaluate(lowRank);
            if (lowResult.Mode == ControlMode.DirectControl)
                report.Add("  OK: ControlPermissionService allows low-rank direct control sample");
            else
            {
                report.Add($"  ERROR: low-rank direct control sample denied ({lowResult.Reason})");
                errors++;
            }

            var leaderData = CharacterData.Create("validator_leader", "Validator Leader", SubFactionId.MotorIronRiders, CharacterRole.CityLord);
            var leader = new ControlPermissionRequest
            {
                Commander = commander,
                Target = CharacterControlInfo.FromCharacterData(leaderData),
                CurrentControlledUnitCount = 0,
                FactionTrust = 80
            };
            var leaderResult = service.Evaluate(leader);
            if (leaderResult.Mode == ControlMode.Denied)
                report.Add("  OK: ControlPermissionService denies leader direct control sample");
            else
            {
                report.Add($"  ERROR: leader sample unexpectedly allowed ({leaderResult.Mode})");
                errors++;
            }

            if (typeof(CommanderDebugHud).GetMethod("SetRuntimeState") != null && typeof(CommanderControlHintPanel).GetMethod("SetRuntimeState") != null)
                report.Add("  OK: Commander debug/hint feedback surfaces accept runtime diagnostics");
            else
            {
                report.Add("  ERROR: commander denial feedback surfaces missing runtime-state setters");
                errors++;
            }

            if (File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    int controllers = 0;
                    int selectors = 0;
                    int lowRankControllable = 0;
                    int highRankDenied = 0;
                    int hintPanels = 0;
                    int debugHuds = 0;

                    foreach (var root in scene.GetRootGameObjects())
                    {
                        controllers += root.GetComponentsInChildren<CommanderControlController>(true).Length;
                        selectors += root.GetComponentsInChildren<CommanderTargetSelector>(true).Length;
                        hintPanels += root.GetComponentsInChildren<CommanderControlHintPanel>(true).Length;
                        debugHuds += root.GetComponentsInChildren<CommanderDebugHud>(true).Length;

                        foreach (var entity in root.GetComponentsInChildren<CharacterEntity>(true))
                        {
                            var data = entity.Data;
                            if (data == null) continue;
                            if (data.IsAlive && !data.IsHeroOrLeader && data.AllowDirectControl && data.RequiredCommanderLevel <= commander.CommanderLevel && data.CommandRank <= commander.MaxDirectControlRank)
                                lowRankControllable++;
                            if (data.IsHeroOrLeader || data.Role == CharacterRole.CityLord || data.Role == CharacterRole.WarKing || !data.AllowDirectControl || data.CommandRank > commander.MaxDirectControlRank)
                                highRankDenied++;
                        }
                    }

                    if (controllers > 0) report.Add($"  OK: CommanderPrototype has {controllers} CommanderControlController(s)");
                    else { report.Add("  ERROR: CommanderPrototype missing CommanderControlController"); errors++; }
                    if (selectors > 0) report.Add($"  OK: CommanderPrototype has {selectors} CommanderTargetSelector(s)");
                    else { report.Add("  ERROR: CommanderPrototype missing CommanderTargetSelector"); errors++; }
                    if (lowRankControllable > 0) report.Add($"  OK: CommanderPrototype has {lowRankControllable} low-rank controllable unit(s)");
                    else { report.Add("  ERROR: CommanderPrototype missing low-rank controllable unit"); errors++; }
                    if (highRankDenied > 0) report.Add($"  OK: CommanderPrototype has {highRankDenied} high-rank/denied unit example(s)");
                    else { report.Add("  ERROR: CommanderPrototype missing high-rank denied unit example"); errors++; }
                    if (hintPanels > 0 && debugHuds > 0) report.Add("  OK: CommanderPrototype has CommanderDebugHud and CommanderControlHintPanel");
                    else { report.Add("  ERROR: CommanderPrototype missing commander feedback HUD/hint panel"); errors++; }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void CheckPlayerAttackUsability(List<string> report, ref int errors, ref int warnings)
        {
            report.Add("");
            report.Add("--- Player Attack Usability ---");

            var combatControllerType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CombatController");
            if (combatControllerType == null)
            {
                report.Add("  ERROR: CombatController type missing");
                errors++;
                return;
            }

            var diagnosticProps = new[] { "LastAttackAttemptTime", "LastAttackResult", "LastAttackRejectReason", "LastAttackTargetName", "LastAttackDistance", "LastAttackRange", "LastAttackState" };
            foreach (var p in diagnosticProps)
            {
                if (combatControllerType.GetProperty(p) != null)
                    report.Add($"  OK: CombatController.{p} exists");
                else
                {
                    report.Add($"  ERROR: CombatController.{p} missing");
                    errors++;
                }
            }

            if (combatControllerType.GetMethod("AttemptAttack") != null)
                report.Add("  OK: CombatController.AttemptAttack exists");
            else
            {
                report.Add("  ERROR: CombatController.AttemptAttack missing");
                errors++;
            }

            var debugType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "PrototypeDebugController");
            if (debugType != null)
                report.Add("  OK: PrototypeDebugController type exists");
            else
            {
                report.Add("  ERROR: PrototypeDebugController type missing");
                errors++;
            }

            var proceduralType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "ProceduralCombatAnimator");
            if (proceduralType != null)
            {
                report.Add("  OK: ProceduralCombatAnimator fallback exists");
                if (proceduralType.GetProperty("VisualLocalOffset") != null)
                    report.Add("  OK: ProceduralCombatAnimator exposes VisualLocalOffset");
            }
            else
            {
                report.Add("  ERROR: ProceduralCombatAnimator type missing");
                errors++;
            }

            if (File.Exists("Assets/Scenes/CommanderPrototype.unity"))
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/CommanderPrototype.unity", OpenSceneMode.Additive);
                try
                {
                    int debugControllers = 0;
                    int alivePlayers = 0;
                    int rootMotionIssues = 0;
                    int visualMissing = 0;
                    int animFallbackMissing = 0;

                    foreach (var go in scene.GetRootGameObjects())
                    {
                        debugControllers += go.GetComponentsInChildren<PrototypeDebugController>(true).Length;
                        foreach (var ctrl in go.GetComponentsInChildren<CombatController>(true))
                        {
                            var c = ctrl.GetComponent<Combatant>();
                            if (c != null && c.CurrentHealth > 0f) alivePlayers++;
                            var animator = ctrl.GetComponent<Animator>();
                            if (animator != null && animator.applyRootMotion) rootMotionIssues++;
                            if (ctrl.transform.Find("Visual") == null) visualMissing++;
                            if (ctrl.GetComponent<ProceduralCombatAnimator>() == null && ctrl.GetComponent<AnimatorCombatBridge>() == null) animFallbackMissing++;
                        }
                    }

                    if (debugControllers > 0)
                        report.Add($"  OK: CommanderPrototype has {debugControllers} PrototypeDebugController(s)");
                    else
                    {
                        report.Add("  WARNING: CommanderPrototype missing PrototypeDebugController (recreate scene)");
                        warnings++;
                    }

                    if (alivePlayers > 0)
                        report.Add("  OK: CommanderPrototype player starts alive");
                    else
                    {
                        report.Add("  WARNING: CommanderPrototype has no alive CombatController player in scene asset");
                        warnings++;
                    }

                    if (rootMotionIssues == 0) report.Add("  OK: player Animator.applyRootMotion=false");
                    else { report.Add($"  ERROR: {rootMotionIssues} player Animator(s) have applyRootMotion=true"); errors += rootMotionIssues; }

                    if (visualMissing == 0) report.Add("  OK: player Visual child exists");
                    else { report.Add($"  WARNING: {visualMissing} player(s) missing Visual child"); warnings += visualMissing; }

                    if (animFallbackMissing == 0) report.Add("  OK: player has animation bridge or procedural fallback");
                    else { report.Add($"  WARNING: {animFallbackMissing} player(s) missing combat animation feedback"); warnings += animFallbackMissing; }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }
    }
}
#endif

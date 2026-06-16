#if UNITY_EDITOR
using LuoLuoTrip.Save;
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
            if (!File.Exists(path))
            {
                report.Add("  WARNING: CombatTuningConfig.asset missing (expected at Assets/Data/Combat/)");
                warnings++;
            }
            else
            {
                report.Add("  OK: CombatTuningConfig.asset exists");
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

            var shakeType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "CameraShakeService");
            if (shakeType != null && camGo.GetComponent(shakeType) != null)
            {
                report.Add("  WARNING: CameraShakeService serialized on Main Camera — will be added at runtime by CombatHitFeedbackHub. Remove from scene to avoid Awake-ordering duplicate.");
                warnings++;
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
    }
}
#endif

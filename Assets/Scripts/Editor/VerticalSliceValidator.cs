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
            CheckServiceLifecycle(report, ref warnings);
            CheckNavMeshSetup(report, ref warnings);
            CheckEncounterWaveConfig(report, ref warnings);
            CheckInputOwnership(report, ref warnings);
            CheckRootMovement(report, ref warnings);
            CheckAnimationClipBindings(report, ref warnings);
            CheckGameplayScriptAttachment(report, ref warnings);
            CheckCombatReadability(report, ref errors, ref warnings);

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
    }
}
#endif

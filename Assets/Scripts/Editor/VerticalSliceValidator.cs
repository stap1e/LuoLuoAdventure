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
                typeof(ConvoyEnergyConflictRuntime)
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

            var hudTypes = new[] { "CommanderDebugHud", "FactionStandingDebugPanel", "MissionResultDebugPanel", "MissionObjectiveHud" };
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
    }
}
#endif

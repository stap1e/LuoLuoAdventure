#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    public static class ProjectCompatibilityChecker
    {
        private const string Separator = "========================================";

        [MenuItem("LuoLuoTrip/Tools/Compatibility/Run Project Compatibility Check")]
        public static void RunCheck()
        {
            var report = new List<string>();
            report.Add(Separator);
            report.Add("LuoLuoTrip Project Compatibility Report");
            report.Add($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.Add(Separator);

            report.Add("");
            report.Add("--- Unity Version ---");
            report.Add($"Unity Version: {Application.unityVersion}");
            report.Add($"ProjectVersion.txt: 2022.3.62f3");
            report.Add($"Recommended: Unity 2022.3.62f3 LTS");

            report.Add("");
            report.Add("--- Package Manifest ---");
            CheckPackages(report);

            report.Add("");
            report.Add("--- Assembly Definitions ---");
            CheckAssemblyDefinitions(report);

            report.Add("");
            report.Add("--- Runtime/Editor API Separation ---");
            CheckEditorApiSeparation(report);

            report.Add("");
            report.Add("--- Missing Scripts ---");
            CheckMissingScripts(report);

            report.Add("");
            report.Add("--- Placeholder Prefab Hierarchy ---");
            CheckPlaceholderPrefabs(report);

            report.Add("");
            report.Add("--- Scene Files ---");
            CheckScenes(report);

            report.Add("");
            report.Add("--- Orphaned .meta Files ---");
            CheckOrphanedMetaFiles(report);

            report.Add("");
            report.Add(Separator);
            report.Add("Compatibility check complete.");
            report.Add(Separator);

            var text = string.Join("\n", report);
            Debug.Log(text);

            var path = Path.Combine(Application.dataPath, "..", "CompatibilityCheck_Report.txt");
            File.WriteAllText(path, text);
            Debug.Log($"Report saved to: {path}");
        }

        private static void CheckPackages(List<string> report)
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                var content = File.ReadAllText(manifestPath);
                var lines = content.Split('\n').Where(l => l.Contains("\"com.unity.")).ToList();
                foreach (var line in lines)
                {
                    report.Add($"  {line.Trim().TrimEnd(',')}");
                }
            }
            else
            {
                report.Add("  manifest.json NOT FOUND");
            }
        }

        private static void CheckAssemblyDefinitions(List<string> report)
        {
            var asmdefFiles = Directory.GetFiles(Application.dataPath, "*.asmdef", SearchOption.AllDirectories);
            foreach (var asmdef in asmdefFiles)
            {
                var relative = MakeRelative(asmdef);
                var content = File.ReadAllText(asmdef);
                var name = ExtractJsonField(content, "name");
                var includePlatforms = ExtractJsonArray(content, "includePlatforms");
                var refs = ExtractJsonArray(content, "references");

                report.Add($"  {relative}");
                report.Add($"    Name: {name}");
                report.Add($"    Platforms: {(includePlatforms.Count == 0 ? "All" : string.Join(", ", includePlatforms))}");
                report.Add($"    References: {(refs.Count == 0 ? "None" : string.Join(", ", refs))}");

                var isEditor = includePlatforms.Contains("Editor");
                var hasEditorRef = refs.Any(r => r.EndsWith(".Editor"));
                if (!isEditor && hasEditorRef)
                    report.Add("    ERROR: Runtime assembly references Editor assembly!");
            }
        }

        private static void CheckEditorApiSeparation(List<string> report)
        {
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

            var editorKeywords = new[] { "using UnityEditor", "UnityEditor.", "AssetDatabase", "MenuItem", "SerializedObject" };
            var issues = new List<string>();

            foreach (var script in runtimeScripts)
            {
                var content = File.ReadAllText(script);
                foreach (var keyword in editorKeywords)
                {
                    if (content.Contains(keyword))
                    {
                        issues.Add($"  FOUND: {MakeRelative(script)} contains '{keyword}'");
                    }
                }
            }

            if (issues.Count == 0)
                report.Add("  OK: No UnityEditor references in runtime scripts");
            else
                foreach (var issue in issues)
                    report.Add(issue);
        }

        private static void CheckMissingScripts(List<string> report)
        {
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
                report.Add("  OK: No missing scripts found in loaded scenes");
            else
                report.Add($"  Total missing scripts: {missingCount}");
        }

        private static void CheckPlaceholderPrefabs(List<string> report)
        {
            var prefabDir = Path.Combine(Application.dataPath, "Art", "Placeholders", "Prefabs");
            if (!Directory.Exists(prefabDir))
            {
                report.Add("  Placeholder prefab directory not found (run Generate Placeholder Assets)");
                return;
            }

            var prefabs = Directory.GetFiles(prefabDir, "PH_*.prefab");
            if (prefabs.Length == 0)
            {
                report.Add("  No PH_*.prefab files found (run Generate Placeholder Assets)");
                return;
            }

            var expectedChildren = new[] { "Visual", "Collision", "Marker" };

            foreach (var prefabPath in prefabs)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(MakeRelative(prefabPath));
                if (asset == null)
                {
                    report.Add($"  {Path.GetFileName(prefabPath)}: Could not load prefab");
                    continue;
                }

                var childNames = new HashSet<string>();
                for (int i = 0; i < asset.transform.childCount; i++)
                    childNames.Add(asset.transform.GetChild(i).name);

                var missing = expectedChildren.Where(e => !childNames.Contains(e)).ToList();
                if (missing.Count == 0)
                    report.Add($"  {asset.name}: OK (Visual/Collision/Marker present)");
                else
                    report.Add($"  {asset.name}: MISSING children: {string.Join(", ", missing)}");
            }
        }

        private static void CheckScenes(List<string> report)
        {
            var sceneDir = Path.Combine(Application.dataPath, "Scenes");
            if (!Directory.Exists(sceneDir))
            {
                report.Add("  Scenes directory not found");
                return;
            }

            var expectedScenes = new[] { "CombatPrototype.unity", "CommanderPrototype.unity" };
            foreach (var scene in expectedScenes)
            {
                var fullPath = Path.Combine(sceneDir, scene);
                report.Add($"  {scene}: {(File.Exists(fullPath) ? "EXISTS" : "MISSING (generate via Setup menu)")}");
            }
        }

        private static void CheckOrphanedMetaFiles(List<string> report)
        {
            var metaFiles = Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories);
            var orphaned = new List<string>();

            foreach (var meta in metaFiles)
            {
                var assetPath = meta.Substring(0, meta.Length - 5);
                if (!File.Exists(assetPath) && !Directory.Exists(assetPath))
                    orphaned.Add(MakeRelative(meta));
            }

            if (orphaned.Count == 0)
                report.Add("  OK: No orphaned .meta files");
            else
                foreach (var o in orphaned)
                    report.Add($"  ORPHANED: {o}");
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

        private static string MakeRelative(string absolutePath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;
            return absolutePath.Replace(projectRoot, "");
        }

        private static string ExtractJsonField(string json, string fieldName)
        {
            var search = $"\"{fieldName}\":";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return "";
            var start = json.IndexOf('"', idx + search.Length) + 1;
            var end = json.IndexOf('"', start);
            return json.Substring(start, end - start);
        }

        private static List<string> ExtractJsonArray(string json, string fieldName)
        {
            var result = new List<string>();
            var search = $"\"{fieldName}\":";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
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
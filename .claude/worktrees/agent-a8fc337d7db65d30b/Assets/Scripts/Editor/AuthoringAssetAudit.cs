#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LuoLuoTrip.AI;
using LuoLuoTrip.Audio;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Feedback;
using UnityEditor;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    public enum AuthoringAssetKind
    {
        MissionDefinition,
        AIBehaviorProfile,
        CombatTuningConfig,
        RuntimeResource
    }

    public sealed class RequiredAuthoringAsset
    {
        public string Path;
        public Type AssetType;
        public AuthoringAssetKind Kind;
        public string Label;
        public string RegenerateCommand;
        public bool RuntimeResource;
    }

    public struct GitIgnoreCheckResult
    {
        public bool WasChecked;
        public bool IsIgnored;
        public string Detail;

        public static GitIgnoreCheckResult Checked(bool isIgnored, string detail)
        {
            return new GitIgnoreCheckResult { WasChecked = true, IsIgnored = isIgnored, Detail = detail ?? string.Empty };
        }

        public static GitIgnoreCheckResult Skipped(string detail)
        {
            return new GitIgnoreCheckResult { WasChecked = false, IsIgnored = false, Detail = detail ?? string.Empty };
        }
    }

    public sealed class AuthoringAssetAuditIssue
    {
        public string Path;
        public string Message;
        public bool IsError;
    }

    public sealed class AuthoringAssetAuditResult
    {
        public readonly List<AuthoringAssetAuditIssue> Issues = new List<AuthoringAssetAuditIssue>();
        public int Errors => Issues.FindAll(i => i.IsError).Count;
        public int Warnings => Issues.FindAll(i => !i.IsError).Count;
        public bool Passed => Errors == 0;
    }

    public static class AuthoringAssetAudit
    {
        public const string CreateAIProfilesCommand = "LuoLuoTrip/Setup/Create AI Behavior Profiles";
        public const string CreateMissionDataCommand = "LuoLuoTrip/Setup/Create Mission Prototype Data";
        public const string CreateCombatTuningCommand = "LuoLuoTrip/Setup/Create Combat Tuning Config";
        public const string CreateCommanderSceneCommand = "LuoLuoTrip/Setup/Create Commander Mission Prototype Scene";

        public static IReadOnlyList<RequiredAuthoringAsset> RequiredAssets => BuildRequiredAssets();

        public static AuthoringAssetAuditResult AuditRequiredAssets(bool checkGit = true, Func<string, GitIgnoreCheckResult> gitIgnoreChecker = null)
        {
            var result = new AuthoringAssetAuditResult();
            var checker = gitIgnoreChecker ?? CheckGitIgnore;

            foreach (var required in RequiredAssets)
                AuditAsset(required, result, checkGit, checker);

            return result;
        }

        public static GitIgnoreCheckResult CheckGitIgnore(string path)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"check-ignore -v -- \"{path}\"",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                        return GitIgnoreCheckResult.Skipped("git process did not start");

                    var stdout = process.StandardOutput.ReadToEnd();
                    var stderr = process.StandardError.ReadToEnd();
                    if (!process.WaitForExit(5000))
                    {
                        try { process.Kill(); } catch { }
                        return GitIgnoreCheckResult.Skipped("git check-ignore timed out");
                    }

                    if (process.ExitCode == 0)
                    {
                        var lines = stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var last = lines.Length > 0 ? lines[lines.Length - 1] : string.Empty;
                        var parts = last.Split(new[] { ':' }, 4);
                        var pattern = parts.Length >= 3 ? parts[2].Trim() : last;
                        var isNegatedAllow = pattern.StartsWith("!");
                        return GitIgnoreCheckResult.Checked(!isNegatedAllow, last.Trim());
                    }
                    if (process.ExitCode == 1)
                        return GitIgnoreCheckResult.Checked(false, string.Empty);

                    return GitIgnoreCheckResult.Skipped(string.IsNullOrWhiteSpace(stderr) ? $"git check-ignore exited {process.ExitCode}" : stderr.Trim());
                }
            }
            catch (Exception ex)
            {
                return GitIgnoreCheckResult.Skipped(ex.Message);
            }
        }

        private static IReadOnlyList<RequiredAuthoringAsset> BuildRequiredAssets()
        {
            var assets = new List<RequiredAuthoringAsset>
            {
                Mission("Assets/Data/Missions/ConvoyEnergyConflict.asset", "Convoy Energy Conflict"),
                Mission("Assets/Data/Missions/BorderRetaliation.asset", "Border Retaliation"),
                Mission("Assets/Data/Missions/CityGateDispute.asset", "City Gate Dispute"),
                Combat("Assets/Data/Combat/CombatTuningConfig.asset", "Combat tuning authoring asset", false),
                Combat("Assets/Resources/CombatTuningConfig.asset", "Combat tuning Resources copy", true),
                Resource<SubFactionDatabaseSO>("Assets/Resources/SubFactionDatabase.asset", "SubFactionDatabase Resources asset", "LuoLuoTrip/Setup/Generate All Sub Faction Configs"),
                Resource<AudioFeedbackProfileSO>("Assets/Resources/AudioFeedbackProfile.asset", "AudioFeedbackProfile Resources asset", "LuoLuoTrip/Setup/Create Audio Feedback Profile"),
                Resource<WorldMarkerProfileSO>("Assets/Resources/WorldMarkerProfile.asset", "WorldMarkerProfile Resources asset", "LuoLuoTrip/Setup/Create World Marker Profile")
            };

            foreach (AIBehaviorProfileType type in Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                assets.Add(Profile($"Assets/Data/AIProfiles/{type}.asset", $"{type} authoring profile", false));
                assets.Add(Profile($"Assets/Resources/AIProfiles/{type}.asset", $"{type} Resources profile", true));
            }

            return assets;
        }

        private static RequiredAuthoringAsset Mission(string path, string label)
        {
            return new RequiredAuthoringAsset
            {
                Path = path,
                AssetType = typeof(MissionDefinitionSO),
                Kind = AuthoringAssetKind.MissionDefinition,
                Label = label,
                RegenerateCommand = CreateMissionDataCommand
            };
        }

        private static RequiredAuthoringAsset Profile(string path, string label, bool runtimeResource)
        {
            return new RequiredAuthoringAsset
            {
                Path = path,
                AssetType = typeof(AIBehaviorProfileSO),
                Kind = AuthoringAssetKind.AIBehaviorProfile,
                Label = label,
                RegenerateCommand = CreateAIProfilesCommand,
                RuntimeResource = runtimeResource
            };
        }

        private static RequiredAuthoringAsset Combat(string path, string label, bool runtimeResource)
        {
            return new RequiredAuthoringAsset
            {
                Path = path,
                AssetType = typeof(CombatTuningConfigSO),
                Kind = AuthoringAssetKind.CombatTuningConfig,
                Label = label,
                RegenerateCommand = CreateCombatTuningCommand,
                RuntimeResource = runtimeResource
            };
        }

        private static RequiredAuthoringAsset Resource<T>(string path, string label, string command) where T : UnityEngine.Object
        {
            return new RequiredAuthoringAsset
            {
                Path = path,
                AssetType = typeof(T),
                Kind = AuthoringAssetKind.RuntimeResource,
                Label = label,
                RegenerateCommand = command,
                RuntimeResource = true
            };
        }

        private static void AuditAsset(RequiredAuthoringAsset required, AuthoringAssetAuditResult result, bool checkGit, Func<string, GitIgnoreCheckResult> checker)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(required.Path);
            if (asset == null)
            {
                Add(result, required.Path, $"Missing {required.Label}. Regenerate with {required.RegenerateCommand}.", true);
                return;
            }

            if (!File.Exists(required.Path + ".meta"))
                Add(result, required.Path + ".meta", $"Missing meta file for {required.Label}; reimport/regenerate with {required.RegenerateCommand}.", true);

            ValidateContents(required, asset, result);

            if (!checkGit || checker == null) return;
            CheckIgnore(required.Path, result, checker);
            CheckIgnore(required.Path + ".meta", result, checker);
        }

        private static void ValidateContents(RequiredAuthoringAsset required, UnityEngine.Object asset, AuthoringAssetAuditResult result)
        {
            switch (required.Kind)
            {
                case AuthoringAssetKind.MissionDefinition:
                    var mission = asset as MissionDefinitionSO;
                    if (mission == null)
                    {
                        Add(result, required.Path, "Asset is not a MissionDefinitionSO.", true);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(mission.MissionId)) Add(result, required.Path, "MissionDefinitionSO missing missionId.", true);
                    if (string.IsNullOrWhiteSpace(mission.DisplayName)) Add(result, required.Path, "MissionDefinitionSO missing displayName.", true);
                    if (mission.DefaultObjectives == null || mission.DefaultObjectives.Count == 0) Add(result, required.Path, "MissionDefinitionSO missing objectives.", true);
                    if (mission.OutcomeConsequences == null || mission.OutcomeConsequences.Count == 0) Add(result, required.Path, "MissionDefinitionSO missing outcomes.", true);
                    break;
                case AuthoringAssetKind.AIBehaviorProfile:
                    var profile = asset as AIBehaviorProfileSO;
                    if (profile == null)
                    {
                        Add(result, required.Path, "Asset is not an AIBehaviorProfileSO.", true);
                        return;
                    }
                    if (!profile.Validate(out var profileError)) Add(result, required.Path, $"AIBehaviorProfileSO invalid: {profileError}", true);
                    break;
                case AuthoringAssetKind.CombatTuningConfig:
                    var tuning = asset as CombatTuningConfigSO;
                    if (tuning == null)
                    {
                        Add(result, required.Path, "Asset is not a CombatTuningConfigSO.", true);
                        return;
                    }
                    if (!tuning.Validate(out var tuningError)) Add(result, required.Path, $"CombatTuningConfigSO invalid: {tuningError}", true);
                    break;
                case AuthoringAssetKind.RuntimeResource:
                    if (!required.AssetType.IsInstanceOfType(asset))
                        Add(result, required.Path, $"Asset is not a {required.AssetType.Name}.", true);
                    break;
            }
        }

        private static void CheckIgnore(string path, AuthoringAssetAuditResult result, Func<string, GitIgnoreCheckResult> checker)
        {
            var git = checker(path);
            if (!git.WasChecked)
            {
                Add(result, path, $"Git ignore check skipped: {git.Detail}", false);
                return;
            }

            if (git.IsIgnored)
                Add(result, path, $"Required authoring asset is git-ignored: {git.Detail}", true);
        }

        private static void Add(AuthoringAssetAuditResult result, string path, string message, bool isError)
        {
            result.Issues.Add(new AuthoringAssetAuditIssue { Path = path, Message = message, IsError = isError });
        }
    }
}
#endif

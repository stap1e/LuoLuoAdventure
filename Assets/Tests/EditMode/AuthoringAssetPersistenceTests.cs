using System.Linq;
using LuoLuoTrip.Editor;
using NUnit.Framework;
using UnityEditor;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AuthoringAssetPersistenceTests
    {
        [Test]
        public void RequiredAuthoringAssets_ExistWithMetaFiles()
        {
            foreach (var required in AuthoringAssetAudit.RequiredAssets)
            {
                Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(required.Path), Is.Not.Null, required.Path);
                Assert.That(System.IO.File.Exists(required.Path + ".meta"), Is.True, required.Path + ".meta");
            }
        }

        [Test]
        public void RequiredAuthoringAssets_AreNotGitIgnored_WhenGitAvailable()
        {
            foreach (var required in AuthoringAssetAudit.RequiredAssets)
            {
                AssertNotIgnored(required.Path);
                AssertNotIgnored(required.Path + ".meta");
            }
        }

        [Test]
        public void AuditRequiredAssets_GitUnavailable_DoesNotFailCoreValidation()
        {
            var result = AuthoringAssetAudit.AuditRequiredAssets(
                checkGit: true,
                gitIgnoreChecker: _ => GitIgnoreCheckResult.Skipped("git unavailable in test"));

            Assert.That(result.Errors, Is.EqualTo(0), string.Join("\n", result.Issues.Select(i => i.Message)));
            Assert.That(result.Warnings, Is.GreaterThan(0));
        }

        [Test]
        public void AuditRequiredAssets_ReportsIgnoredAssetAsError()
        {
            var result = AuthoringAssetAudit.AuditRequiredAssets(
                checkGit: true,
                gitIgnoreChecker: path => path.EndsWith("CityGateDispute.asset")
                    ? GitIgnoreCheckResult.Checked(true, ".gitignore:sample")
                    : GitIgnoreCheckResult.Checked(false, string.Empty));

            Assert.That(result.Errors, Is.GreaterThan(0));
            Assert.That(result.Issues.Any(i => i.Path.Contains("CityGateDispute.asset") && i.Message.Contains("git-ignored")), Is.True);
        }

        private static void AssertNotIgnored(string path)
        {
            var result = AuthoringAssetAudit.CheckGitIgnore(path);
            if (!result.WasChecked)
                Assert.Inconclusive($"git check-ignore unavailable for {path}: {result.Detail}");
            Assert.That(result.IsIgnored, Is.False, result.Detail);
        }
    }
}

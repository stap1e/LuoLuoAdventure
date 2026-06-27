using System.Linq;
using LuoLuoTrip.Editor;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AuthoringValidatorTests
    {
        [Test]
        public void RequiredAuthoringAssetAudit_PassesForCurrentProjectAssets()
        {
            var result = AuthoringAssetAudit.AuditRequiredAssets(checkGit: false);
            Assert.That(result.Errors, Is.EqualTo(0), string.Join("\n", result.Issues.Select(i => i.Message)));
        }

        [Test]
        public void RequiredAuthoringAssetAudit_MissingAssetMessageIsActionable()
        {
            var missing = AuthoringAssetAudit.RequiredAssets.First(a => a.Path.Contains("CityGateDispute.asset"));
            Assert.That(missing.RegenerateCommand, Is.EqualTo(AuthoringAssetAudit.CreateMissionDataCommand));
        }

        [Test]
        public void RequiredAuthoringAssetAudit_GitUnavailableIsWarningOnly()
        {
            var result = AuthoringAssetAudit.AuditRequiredAssets(
                checkGit: true,
                gitIgnoreChecker: _ => GitIgnoreCheckResult.Skipped("simulated unavailable git"));

            Assert.That(result.Errors, Is.EqualTo(0), string.Join("\n", result.Issues.Select(i => i.Message)));
            Assert.That(result.Issues.Any(i => !i.IsError && i.Message.Contains("skipped")), Is.True);
        }
    }
}

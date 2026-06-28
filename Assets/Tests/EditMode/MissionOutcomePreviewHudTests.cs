using System.Collections.Generic;
using System.Linq;
using LuoLuoTrip.UI;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionOutcomePreviewHudTests
    {
        [Test]
        public void BuildDisplayLines_MissingPreviewSafe()
        {
            var lines = new List<string>();

            MissionOutcomePreviewHud.BuildDisplayLines(null, lines);

            Assert.That(lines, Is.Not.Empty);
            Assert.That(string.Join("\n", lines), Does.Contain("No preview data"));
        }

        [Test]
        public void BuildDisplayLines_ContainsLikelyOutcomeAndSuggestion()
        {
            var preview = new MissionOutcomePreviewService().BuildCityGatePreview(false, true, false, 0, 0);
            var lines = new List<string>();

            MissionOutcomePreviewHud.BuildDisplayLines(preview, lines, 3);
            var text = string.Join("\n", lines);

            Assert.That(text, Does.Contain("Likely Outcome"));
            Assert.That(text, Does.Contain("FailedEscalation"));
            Assert.That(text, Does.Contain("G DefendObjective"));
            Assert.That(text, Does.Contain("Preview only"));
        }

        [Test]
        public void BuildDisplayLines_TopRisksLimitedToThree()
        {
            var preview = new MissionOutcomePreviewService().BuildCityGatePreview(false, false, false, 5, 5);
            var lines = new List<string>();

            MissionOutcomePreviewHud.BuildDisplayLines(preview, lines, 3);

            Assert.That(lines.Count(l => l.StartsWith("Risk:")), Is.EqualTo(3));
        }

        [Test]
        public void CompactPreviewLayout_IsSafe()
        {
            var preview = DebugUILayout.GetMissionOutcomePreviewRect(800, 600);
            var objective = DebugUILayout.GetMissionObjectiveRect(800, 600);

            Assert.That(preview.width, Is.GreaterThan(0f));
            Assert.That(preview.height, Is.GreaterThan(0f));
            Assert.That(preview.x, Is.GreaterThanOrEqualTo(0f));
            Assert.That(preview.y, Is.GreaterThanOrEqualTo(0f));
            Assert.That(DebugUILayout.OverlapsHeavily(preview, objective), Is.False);
        }
    }
}

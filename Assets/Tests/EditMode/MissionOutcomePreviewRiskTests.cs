using System.Collections.Generic;
using System.Linq;
using LuoLuoTrip.UI;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionOutcomePreviewRiskTests
    {
        [Test]
        public void CoreDestroyed_CreatesCriticalDefendRisk()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(false, true, true, 0, 0);

            var risk = preview.risks.FirstOrDefault(r => r.riskId == "citygate_core_destroyed");
            Assert.That(risk, Is.Not.Null);
            Assert.That(risk.isCritical, Is.True);
            Assert.That(risk.suggestion, Does.Contain("G DefendObjective"));
        }

        [Test]
        public void NegotiatorDead_CreatesProtectRisk()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, false, true, 0, 0);

            var risk = preview.risks.FirstOrDefault(r => r.riskId == "citygate_negotiator_dead");
            Assert.That(risk, Is.Not.Null);
            Assert.That(risk.displayName, Does.Contain("BeastNegotiator"));
            Assert.That(risk.suggestion, Does.Contain("Protect Negotiator"));
        }

        [Test]
        public void HighCasualties_CreateHardlinerEscalationRisk()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, true, true, 5, 5);

            var risk = preview.risks.FirstOrDefault(r => r.riskId == "citygate_escalation");
            Assert.That(risk, Is.Not.Null);
            Assert.That(risk.isCritical, Is.True);
            Assert.That(risk.suggestion, Does.Contain("FocusFire Hardliner"));
        }

        [Test]
        public void RaidersActive_CreateFocusFireRisk()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, true, false, 0, 0);

            var risk = preview.risks.FirstOrDefault(r => r.riskId == "citygate_raiders");
            Assert.That(risk, Is.Not.Null);
            Assert.That(risk.suggestion, Does.Contain("F FocusFire"));
        }

        [Test]
        public void HudDisplay_LimitsTopRisksToThree()
        {
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(false, false, false, 5, 5);
            var lines = new List<string>();

            MissionOutcomePreviewHud.BuildDisplayLines(preview, lines, 3);

            Assert.That(lines.Count(l => l.StartsWith("Risk:")), Is.EqualTo(3));
        }
    }
}

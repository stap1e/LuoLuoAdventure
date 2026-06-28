using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateMissionChainTests
    {
        [Test]
        public void CityGateDispute_RecordsToChain()
        {
            var chain = new MissionChainService();
            chain.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100);
            chain.RecordMissionResult("border_retaliation", MissionOutcomeType.BalancedResolution, 300);
            chain.RecordMissionResult("city_gate_dispute", MissionOutcomeType.BalancedMediation, 350);

            Assert.That(chain.State.CompletedMissions.Count, Is.EqualTo(3));
            Assert.That(chain.HasCompleted("city_gate_dispute"), Is.True);
        }

        [Test]
        public void CityGateDispute_DuplicateOutcome_Guarded()
        {
            var chain = new MissionChainService();
            chain.RecordMissionResult("city_gate_dispute", MissionOutcomeType.BalancedMediation, 350);

            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Warning,
                new System.Text.RegularExpressions.Regex("Skip duplicate mission outcome"));
            chain.RecordMissionResult("city_gate_dispute", MissionOutcomeType.MechaSuppression, 250);

            Assert.That(chain.State.CompletedMissions.Count, Is.EqualTo(1),
                "Duplicate mission outcome must not be appended");
        }

        [Test]
        public void CityGateDispute_AllowsDuplicateWithAllowDuplicate()
        {
            var chain = new MissionChainService();
            chain.RecordMissionResult("city_gate_dispute", MissionOutcomeType.BalancedMediation, 350);
            chain.RecordMissionResult("city_gate_dispute", MissionOutcomeType.MechaSuppression, 250,
                allowDuplicate: true);

            Assert.That(chain.State.CompletedMissions.Count, Is.EqualTo(2));
            Assert.That(chain.GetLastOutcome("city_gate_dispute"), Is.EqualTo(MissionOutcomeType.MechaSuppression));
        }

        [Test]
        public void CityGateDispute_ModifierGeneratedFromBorderOutcome()
        {
            var chain = new MissionChainService();
            chain.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.MechaVictory, 100);
            chain.RecordMissionResult("border_retaliation", MissionOutcomeType.MechaVictory, 200);

            var modifier = chain.BuildMissionModifiers("city_gate_dispute");
            Assert.That(modifier.ModifierId, Is.EqualTo("citygate_hardliner_pressure"));
            Assert.That(modifier.SourceMissionId, Is.EqualTo("border_retaliation"));
        }

        [Test]
        public void CityGateDispute_ModifierCeasefireFromBalancedBorder()
        {
            var chain = new MissionChainService();
            chain.RecordMissionResult("convoy_energy_conflict", MissionOutcomeType.BalancedResolution, 300);
            chain.RecordMissionResult("border_retaliation", MissionOutcomeType.BalancedResolution, 300);

            var modifier = chain.BuildMissionModifiers("city_gate_dispute");
            Assert.That(modifier.ModifierId, Is.EqualTo("citygate_ceasefire_fragile"));
            Assert.That(modifier.CeasefireActive, Is.True);
        }

        [Test]
        public void CityGateDispute_XPNotDuplicated()
        {
            var commander = CommanderProfile.CreateDefault();
            var xpBefore = commander.Experience;

            var chain = new MissionChainService();
            chain.RecordMissionResult("city_gate_dispute", MissionOutcomeType.BalancedMediation, 350);

            // Simulate consequence applying XP once
            commander.AddExperience(350);
            var xpAfterFirst = commander.Experience;
            Assert.That(xpAfterFirst - xpBefore, Is.GreaterThanOrEqualTo(350));

            // Duplicate record should be blocked, so XP is NOT applied again
            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Warning,
                new System.Text.RegularExpressions.Regex("Skip duplicate mission outcome"));
            chain.RecordMissionResult("city_gate_dispute", MissionOutcomeType.BalancedMediation, 350);

            // XP stays the same — no double application
            Assert.That(commander.Experience, Is.EqualTo(xpAfterFirst));
        }
    }
}

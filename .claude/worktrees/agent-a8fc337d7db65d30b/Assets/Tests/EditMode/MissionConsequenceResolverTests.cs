using System.Linq;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionConsequenceResolverTests
    {
        [Test]
        public void MechaVictory_IncreasesMechaTrust_AndBeastHostility()
        {
            var state = CreateState(MissionOutcomeType.MechaVictory);
            var consequence = MissionConsequenceResolver.Resolve(state);

            var mechaDelta = consequence.FactionDeltas.FirstOrDefault(d => GameConstants.IsMotorSubFaction(d.FactionId));
            var beastDelta = consequence.FactionDeltas.FirstOrDefault(d => GameConstants.IsBeastSubFaction(d.FactionId));

            Assert.That(mechaDelta.TrustDelta, Is.GreaterThan(0));
            Assert.That(beastDelta.HostilityDelta, Is.GreaterThan(0));
        }

        [Test]
        public void BeastVictory_IncreasesBeastTrust_AndMechaHostility()
        {
            var state = CreateState(MissionOutcomeType.BeastVictory);
            var consequence = MissionConsequenceResolver.Resolve(state);

            var beastDelta = consequence.FactionDeltas.FirstOrDefault(d => GameConstants.IsBeastSubFaction(d.FactionId));
            var mechaDelta = consequence.FactionDeltas.FirstOrDefault(d => GameConstants.IsMotorSubFaction(d.FactionId));

            Assert.That(beastDelta.TrustDelta, Is.GreaterThan(0));
            Assert.That(mechaDelta.HostilityDelta, Is.GreaterThan(0));
        }

        [Test]
        public void BalancedResolution_LowersHostility()
        {
            var state = CreateState(MissionOutcomeType.BalancedResolution);
            var consequence = MissionConsequenceResolver.Resolve(state);

            foreach (var delta in consequence.FactionDeltas)
            {
                Assert.That(delta.HostilityDelta, Is.LessThanOrEqualTo(0));
            }
        }

        [Test]
        public void HighCasualties_IncreasesWarExhaustion()
        {
            var state = CreateState(MissionOutcomeType.MechaVictory);
            state.MechaCasualties = 5;
            state.BeastCasualties = 3;

            var consequence = MissionConsequenceResolver.Resolve(state);

            var hasWarExhaustion = consequence.FactionDeltas.Any(d => d.WarExhaustionDelta > 0);
            Assert.That(hasWarExhaustion, Is.True);
        }

        [Test]
        public void PlayerRetreat_DecreasesRespect()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "test_retreat",
                PlayerRetreated = true,
                Outcome = MissionOutcomeType.Failed
            };

            var consequence = MissionConsequenceResolver.Resolve(state);

            foreach (var delta in consequence.FactionDeltas)
            {
                Assert.That(delta.RespectDelta, Is.LessThan(0));
            }
        }

        [Test]
        public void BalancedResolution_GivesMoreXP()
        {
            var balanced = CreateState(MissionOutcomeType.BalancedResolution);
            var mechaVictory = CreateState(MissionOutcomeType.MechaVictory);

            var balancedConsequence = MissionConsequenceResolver.Resolve(balanced);
            var mechaConsequence = MissionConsequenceResolver.Resolve(mechaVictory);

            Assert.That(balancedConsequence.CommanderExperienceDelta,
                Is.GreaterThan(mechaConsequence.CommanderExperienceDelta));
        }

        [Test]
        public void Consequence_HasSummaryText()
        {
            var state = CreateState(MissionOutcomeType.MechaVictory);
            var consequence = MissionConsequenceResolver.Resolve(state);

            Assert.That(consequence.SummaryText, Is.Not.Empty);
        }

        private static MissionRuntimeState CreateState(MissionOutcomeType outcome)
        {
            return new MissionRuntimeState
            {
                MissionId = "test_mission",
                Outcome = outcome,
                Objectives =
                {
                    new MissionObjective
                    {
                        ObjectiveId = "obj_1",
                        Description = "Test objective",
                        IsCompleted = true,
                        Progress = 1,
                        RequiredProgress = 1
                    }
                }
            };
        }
    }
}

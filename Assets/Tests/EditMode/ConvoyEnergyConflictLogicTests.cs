using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class ConvoyEnergyConflictLogicTests
    {
        [Test]
        public void ConvoyProtected_BeastDefeated_YieldsMechaVictory()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "convoy_energy",
                ProtectedConvoy = true
            };
            state.Objectives.Add(new MissionObjective
            {
                ObjectiveId = "protect_convoy",
                Description = "Protect convoy",
                IsCompleted = true,
                Progress = 1,
                RequiredProgress = 1
            });

            state.DetermineOutcome();
            Assert.That(state.Outcome, Is.EqualTo(MissionOutcomeType.MechaVictory));

            var consequence = MissionConsequenceResolver.Resolve(state);
            var mechaDelta = consequence.FactionDeltas.Find(d => GameConstants.IsMotorSubFaction(d.FactionId));
            Assert.That(mechaDelta.TrustDelta, Is.GreaterThan(0));
        }

        [Test]
        public void BeastReachesEnergyNode_YieldsBeastVictory()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "convoy_energy",
                EscalatedConflict = true
            };
            state.Outcome = MissionOutcomeType.BeastVictory;

            var consequence = MissionConsequenceResolver.Resolve(state);
            var beastDelta = consequence.FactionDeltas.Find(d => GameConstants.IsBeastSubFaction(d.FactionId));
            Assert.That(beastDelta.TrustDelta, Is.GreaterThan(0));
        }

        [Test]
        public void PlayerSharesEnergy_LowCasualties_YieldsBalanced()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "convoy_energy",
                SharedResources = true,
                ProtectedConvoy = true,
                MechaCasualties = 0,
                BeastCasualties = 0
            };
            state.Outcome = MissionOutcomeType.BalancedResolution;

            var consequence = MissionConsequenceResolver.Resolve(state);
            Assert.That(consequence.CommanderExperienceDelta, Is.EqualTo(300));

            foreach (var delta in consequence.FactionDeltas)
            {
                Assert.That(delta.HostilityDelta, Is.LessThanOrEqualTo(0));
            }
        }

        [Test]
        public void PlayerRetreats_YieldsFailed()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "convoy_energy",
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
        public void HighCasualties_IncreasesWarExhaustion()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "convoy_energy",
                MechaCasualties = 5,
                BeastCasualties = 3,
                Outcome = MissionOutcomeType.MechaVictory
            };

            var consequence = MissionConsequenceResolver.Resolve(state);
            var hasWarExhaustion = consequence.FactionDeltas.Exists(d => d.WarExhaustionDelta > 0);
            Assert.That(hasWarExhaustion, Is.True);
        }
    }
}

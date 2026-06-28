using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class ConvoyEnergyConflictPhaseTests
    {
        [Test]
        public void MissionPhase_Inactive_ByDefault()
        {
            Assert.That(MissionPhase.Inactive, Is.Not.EqualTo(MissionPhase.Active));
        }

        [Test]
        public void MechaVictory_WhenConvoyProtectedAndBeastsDefeated()
        {
            var state = new MissionRuntimeState
            {
                MissionId = "convoy_energy",
                ProtectedConvoy = true,
                MechaCasualties = 0,
                BeastCasualties = 2
            };
            state.Outcome = MissionOutcomeType.MechaVictory;

            var consequence = MissionConsequenceResolver.Resolve(state);
            Assert.That(consequence.Outcome, Is.EqualTo(MissionOutcomeType.MechaVictory));

            var mechaDelta = consequence.FactionDeltas.Find(d => GameConstants.IsMotorSubFaction(d.FactionId));
            Assert.That(mechaDelta.TrustDelta, Is.GreaterThan(0));
        }

        [Test]
        public void BeastVictory_WhenConvoyDestroyed()
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
        public void BalancedResolution_LowCasualties()
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
        }

        [Test]
        public void Failed_WhenPlayerRetreats()
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
    }
}

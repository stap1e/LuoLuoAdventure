using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateDisputeOutcomeTests
    {
        [Test]
        public void BalancedMediation_GeneratesConsequenceWithHostilityReduction()
        {
            var state = new MissionRuntimeState { Outcome = MissionOutcomeType.BalancedMediation };
            var consequence = MissionConsequenceResolver.Resolve(state);
            Assert.That(consequence.FactionDeltas.Count, Is.GreaterThan(0));
            bool hasHostilityReduction = false;
            foreach (var d in consequence.FactionDeltas)
            {
                if (d.HostilityDelta < 0) { hasHostilityReduction = true; break; }
            }
            Assert.That(hasHostilityReduction, Is.True, "BalancedMediation must reduce hostility");
            Assert.That(consequence.CommanderExperienceDelta, Is.EqualTo(350));
        }

        [Test]
        public void MechaSuppression_GeneratesMechaTrustAndBeastHostility()
        {
            var state = new MissionRuntimeState { Outcome = MissionOutcomeType.MechaSuppression };
            var consequence = MissionConsequenceResolver.Resolve(state);
            int mechaTrust = 0, beastHostility = 0;
            foreach (var d in consequence.FactionDeltas)
            {
                if (GameConstants.IsMotorSubFaction(d.FactionId)) mechaTrust += d.TrustDelta;
                if (GameConstants.IsBeastSubFaction(d.FactionId)) beastHostility += d.HostilityDelta;
            }
            Assert.That(mechaTrust, Is.GreaterThan(0), "Mecha trust must increase");
            Assert.That(beastHostility, Is.GreaterThan(0), "Beast hostility must increase");
            Assert.That(consequence.CommanderExperienceDelta, Is.EqualTo(250));
        }

        [Test]
        public void BeastNegotiation_GeneratesBeastHostilityReduction()
        {
            var state = new MissionRuntimeState { Outcome = MissionOutcomeType.BeastNegotiation };
            var consequence = MissionConsequenceResolver.Resolve(state);
            int beastHostility = 0;
            foreach (var d in consequence.FactionDeltas)
            {
                if (GameConstants.IsBeastSubFaction(d.FactionId)) beastHostility += d.HostilityDelta;
            }
            Assert.That(beastHostility, Is.LessThan(0), "Beast hostility must decrease");
            Assert.That(consequence.CommanderExperienceDelta, Is.EqualTo(250));
        }

        [Test]
        public void FailedEscalation_GeneratesHostilityIncrease()
        {
            var state = new MissionRuntimeState { Outcome = MissionOutcomeType.FailedEscalation };
            var consequence = MissionConsequenceResolver.Resolve(state);
            bool hasHostilityIncrease = false;
            foreach (var d in consequence.FactionDeltas)
            {
                if (d.HostilityDelta > 0) { hasHostilityIncrease = true; break; }
            }
            Assert.That(hasHostilityIncrease, Is.True, "FailedEscalation must increase hostility");
            Assert.That(consequence.CommanderExperienceDelta, Is.EqualTo(30));
        }

        [Test]
        public void PartialContainment_GeneratesSmallPenalty()
        {
            var state = new MissionRuntimeState { Outcome = MissionOutcomeType.PartialContainment };
            var consequence = MissionConsequenceResolver.Resolve(state);
            Assert.That(consequence.CommanderExperienceDelta, Is.EqualTo(100));
            bool hasPenalty = false;
            foreach (var d in consequence.FactionDeltas)
            {
                if (d.TrustDelta < 0 || d.RespectDelta < 0) { hasPenalty = true; break; }
            }
            Assert.That(hasPenalty, Is.True, "PartialContainment must have small penalty");
        }

        [Test]
        public void BalancedMediation_LowersHostility_Below40_WithDefaultStart()
        {
            var reputation = new FactionReputationService();
            reputation.InitializeDefaultPolitics();
            var state = new MissionRuntimeState { Outcome = MissionOutcomeType.BalancedMediation };
            var consequence = MissionConsequenceResolver.Resolve(state);
            reputation.ApplyDeltas(consequence.FactionDeltas);
            var beastStanding = reputation.GetStanding(SubFactionId.BeastIronClaw);
            Assert.That(beastStanding.Hostility, Is.LessThan(40),
                "BalancedMediation should push BeastIronClaw hostility below 40 threshold");
        }
    }
}

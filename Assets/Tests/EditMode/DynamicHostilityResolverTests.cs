using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DynamicHostilityResolverTests
    {
        [Test]
        public void HostilityAboveThreshold_ReturnsHostile()
        {
            var reputation = new FactionReputationService();
            reputation.InitializeDefaultPolitics();

            var relationship = new FactionRelationshipService();
            var service = new DynamicFactionHostilityService(reputation, relationship);

            var delta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: 50);
            reputation.ApplyDelta(delta);

            Assert.That(service.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.True);
        }

        [Test]
        public void BalancedResolution_LowersHostility_ReturnsNonHostile()
        {
            var reputation = new FactionReputationService();
            reputation.InitializeDefaultPolitics();

            var relationship = new FactionRelationshipService();
            var service = new DynamicFactionHostilityService(reputation, relationship);

            var delta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: 50);
            reputation.ApplyDelta(delta);
            Assert.That(service.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.True);

            var reduceDelta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: -30);
            reputation.ApplyDelta(reduceDelta);
            Assert.That(service.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.False);
        }

        [Test]
        public void StaticAndDynamicHostility_Combine()
        {
            var reputation = new FactionReputationService();
            reputation.InitializeDefaultPolitics();

            var relationship = new FactionRelationshipService();
            var service = new DynamicFactionHostilityService(reputation, relationship);

            var motorIron = SubFactionId.MotorIronRiders;
            var beastClaw = SubFactionId.BeastIronClaw;

            Assert.That(relationship.Matrix.IsHostile(motorIron, beastClaw), Is.True);
            Assert.That(service.IsHostileBetweenFactions(motorIron, beastClaw), Is.True);
        }

        [Test]
        public void ShouldAttackPlayer_WhenTrustVeryLow()
        {
            var reputation = new FactionReputationService();
            reputation.InitializeDefaultPolitics();

            var relationship = new FactionRelationshipService();
            var service = new DynamicFactionHostilityService(reputation, relationship);

            var delta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, trust: -60);
            reputation.ApplyDelta(delta);

            Assert.That(service.ShouldAttackPlayer(SubFactionId.BeastIronClaw), Is.True);
        }

        [Test]
        public void FactionTrust_PersistsAcrossDeltaApplications()
        {
            var reputation = new FactionReputationService();
            reputation.InitializeDefaultPolitics();

            reputation.ApplyDelta(FactionStandingDelta.Create(SubFactionId.MotorIronRiders, trust: 10));
            reputation.ApplyDelta(FactionStandingDelta.Create(SubFactionId.MotorIronRiders, trust: 5));

            var standing = reputation.GetStanding(SubFactionId.MotorIronRiders);
            var initial = reputation.GetStanding(SubFactionId.MotorIronRiders);
            Assert.That(standing.Trust, Is.GreaterThan(0));
        }
    }
}

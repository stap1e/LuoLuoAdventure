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
            // Design (Phase 3): BalancedResolution lowers mainstream hostility below
            // the IsFactionHostileToPlayer threshold (Hostility >= 40) but does NOT
            // zero it out. Extremist/rogue/retaliation units may remain.
            var reputation = new FactionReputationService();
            reputation.InitializeDefaultPolitics();

            var relationship = new FactionRelationshipService();
            var service = new DynamicFactionHostilityService(reputation, relationship);

            // BeastIronClaw starts at Hostility=20. Raise to 70 to simulate Mission 1
            // confrontation. BalancedResolution applies a strong de-escalation
            // (mainstream hostility -50) bringing it to 20: clearly below threshold
            // but still non-zero.
            reputation.ApplyDelta(FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: 50));
            Assert.That(service.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.True,
                "Setup: faction is hostile before BalancedResolution");

            reputation.ApplyDelta(FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: -50));
            var finalStanding = reputation.GetStanding(SubFactionId.BeastIronClaw);

            Assert.That(service.IsHostileToPlayer(SubFactionId.BeastIronClaw), Is.False,
                "BalancedResolution must lower mainstream hostility below threshold");
            Assert.That(finalStanding.Hostility, Is.GreaterThan(0),
                "BalancedResolution must NOT zero out hostility (extremists remain)");
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

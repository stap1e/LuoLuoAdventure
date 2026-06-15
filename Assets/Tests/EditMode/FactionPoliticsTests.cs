using System.Linq;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class FactionPoliticsTests
    {
        [Test]
        public void InitializeDefaultPolitics_IncludesAllSubFactions()
        {
            var service = new FactionReputationService();
            service.InitializeDefaultPolitics();

            foreach (SubFactionId id in System.Enum.GetValues(typeof(SubFactionId)))
            {
                var standing = service.GetStanding(id);
                Assert.That(standing.FactionId, Is.EqualTo(id));
            }
        }

        [Test]
        public void ApplyDelta_ChangesTrustAndHostility()
        {
            var service = new FactionReputationService();
            service.InitializeDefaultPolitics();

            var before = service.GetStanding(SubFactionId.MotorIronRiders);
            var delta = FactionStandingDelta.Create(SubFactionId.MotorIronRiders, trust: 20, hostility: -10);
            service.ApplyDelta(delta);

            var after = service.GetStanding(SubFactionId.MotorIronRiders);
            Assert.That(after.Trust, Is.EqualTo(before.Trust + 20));
            Assert.That(after.Hostility, Is.EqualTo(before.Hostility - 10));
        }

        [Test]
        public void Values_ClampedToRange()
        {
            var service = new FactionReputationService();
            service.InitializeDefaultPolitics();

            var delta = FactionStandingDelta.Create(SubFactionId.MotorIronRiders, trust: 500);
            service.ApplyDelta(delta);

            var standing = service.GetStanding(SubFactionId.MotorIronRiders);
            Assert.That(standing.Trust, Is.LessThanOrEqualTo(FactionStanding.MaxValue));
        }

        [Test]
        public void GetMainRaceTrust_CalculatesAverage()
        {
            var service = new FactionReputationService();
            service.InitializeDefaultPolitics();

            var motorTrust = service.GetMainRaceTrust(MainRace.MotorTribe);
            var beastTrust = service.GetMainRaceTrust(MainRace.BeastTribe);

            Assert.That(motorTrust, Is.GreaterThan(beastTrust));
        }

        [Test]
        public void IsFactionHostileToPlayer_WhenHostilityHigh()
        {
            var service = new FactionReputationService();
            service.InitializeDefaultPolitics();

            var delta = FactionStandingDelta.Create(SubFactionId.BeastIronClaw, hostility: 50);
            service.ApplyDelta(delta);

            Assert.That(service.IsFactionHostileToPlayer(SubFactionId.BeastIronClaw), Is.True);
        }

        [Test]
        public void FactionPoliticsState_Snapshot_Restores()
        {
            var state = new FactionPoliticsState();
            state.InitializeAll();
            state.ApplyDelta(FactionStandingDelta.Create(SubFactionId.MotorIronRiders, trust: 30));

            var snapshot = state.CreateSnapshot();
            var restored = new FactionPoliticsState();
            restored.RestoreFromSnapshot(snapshot);

            Assert.That(restored.GetStanding(SubFactionId.MotorIronRiders).Trust,
                Is.EqualTo(state.GetStanding(SubFactionId.MotorIronRiders).Trust));
        }
    }
}

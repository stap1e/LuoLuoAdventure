using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class FactionRelationshipMatrixTests
    {
        [Test]
        public void CreateDefault_UsesSymmetricRelationships()
        {
            var matrix = FactionRelationshipMatrix.CreateDefault();

            var forward = matrix.Get(SubFactionId.MotorIronRiders, SubFactionId.BeastIronClaw);
            var reverse = matrix.Get(SubFactionId.BeastIronClaw, SubFactionId.MotorIronRiders);

            Assert.That(forward, Is.EqualTo(reverse));
        }

        [Test]
        public void Set_ClampsRelationshipIntoValidRange()
        {
            var matrix = FactionRelationshipMatrix.CreateDefault();

            matrix.Set(SubFactionId.MotorIronRiders, SubFactionId.BeastIronClaw, 999);
            Assert.That(matrix.Get(SubFactionId.MotorIronRiders, SubFactionId.BeastIronClaw), Is.EqualTo(GameConstants.MaxRelationshipValue));

            matrix.Set(SubFactionId.MotorIronRiders, SubFactionId.BeastIronClaw, -999);
            Assert.That(matrix.Get(SubFactionId.MotorIronRiders, SubFactionId.BeastIronClaw), Is.EqualTo(GameConstants.MinRelationshipValue));
        }

        [Test]
        public void CreateDefault_UsesExpectedSameRaceAndCrossRaceDefaults()
        {
            var matrix = FactionRelationshipMatrix.CreateDefault();

            Assert.That(matrix.Get(SubFactionId.MotorIronRiders, SubFactionId.MotorStormGang), Is.EqualTo(GameConstants.DefaultSameRaceRelationship));
            Assert.That(matrix.Get(SubFactionId.MotorIronRiders, SubFactionId.BeastShadowFang), Is.EqualTo(GameConstants.DefaultCrossRaceRelationship));
            Assert.That(matrix.Get(SubFactionId.MotorIronRiders, SubFactionId.MotorIronRiders), Is.EqualTo(GameConstants.SelfRelationship));
        }
    }
}

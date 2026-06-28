using LuoLuoTrip;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateGuidanceTests
    {
        [Test]
        public void CityGateMissionId_DisplaysAsMissionThree()
        {
            Assert.That(MissionObjectiveHud.DisplayMissionName("city_gate_dispute"), Is.EqualTo("Mission 3: City Gate Dispute"));
        }
    }
}

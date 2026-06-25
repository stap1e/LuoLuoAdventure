using NUnit.Framework;
using UnityEditor;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionDefinitionAssetTests
    {
        [TestCase("Assets/Data/Missions/ConvoyEnergyConflict.asset", DemoFlowManager.ConvoyMissionId, "Convoy Energy Conflict")]
        [TestCase("Assets/Data/Missions/BorderRetaliation.asset", DemoFlowManager.BorderMissionId, "Border Retaliation")]
        [TestCase("Assets/Data/Missions/CityGateDispute.asset", DemoFlowManager.CityGateMissionId, "City Gate Dispute")]
        public void MissionDefinitionAsset_Exists_WithObjectivesAndOutcomes(string path, string missionId, string displayName)
        {
            var asset = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);

            Assert.That(asset, Is.Not.Null, path);
            Assert.That(asset.MissionId, Is.EqualTo(missionId));
            Assert.That(asset.DisplayName, Is.EqualTo(displayName));
            Assert.That(asset.DefaultObjectives, Is.Not.Null.And.Not.Empty);
            Assert.That(asset.OutcomeConsequences, Is.Not.Null.And.Not.Empty);
        }
    }
}

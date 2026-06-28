using NUnit.Framework;
using UnityEditor;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateMissionDefinitionTests
    {
        private const string MissionPath = "Assets/Data/Missions/CityGateDispute.asset";

        [Test]
        public void CityGateDisputeAsset_Exists_WithExpectedIdentity()
        {
            var asset = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(MissionPath);

            Assert.That(asset, Is.Not.Null, "CityGateDispute MissionDefinitionSO must be generated and checked in.");
            Assert.That(asset.MissionId, Is.EqualTo("city_gate_dispute"));
            Assert.That(asset.DisplayName, Is.EqualTo("City Gate Dispute"));
            Assert.That(asset.Description, Is.Not.Empty);
        }

        [Test]
        public void CityGateDisputeAsset_ObjectivesAreDemoChecklist()
        {
            var asset = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(MissionPath);

            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.DefaultObjectives, Has.Count.EqualTo(4));
            Assert.That(asset.DefaultObjectives.Exists(o => o.ObjectiveId == "protect_core" && o.Description.Contains("CityGateCore")), Is.True);
            Assert.That(asset.DefaultObjectives.Exists(o => o.ObjectiveId == "protect_negotiator" && o.Description.Contains("BeastNegotiator")), Is.True);
            Assert.That(asset.DefaultObjectives.Exists(o => o.ObjectiveId == "defeat_raiders" && o.Description.Contains("BeastRaiders")), Is.True);
            Assert.That(asset.DefaultObjectives.Exists(o => o.ObjectiveId == "keep_casualties_low" && o.Description.Contains("casualties")), Is.True);
        }

        [Test]
        public void CityGateDisputeAsset_OutcomesAreAuthored()
        {
            var asset = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(MissionPath);

            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.OutcomeConsequences.Exists(o => o.Outcome == MissionOutcomeType.BalancedMediation), Is.True);
            Assert.That(asset.OutcomeConsequences.Exists(o => o.Outcome == MissionOutcomeType.MechaSuppression), Is.True);
            Assert.That(asset.OutcomeConsequences.Exists(o => o.Outcome == MissionOutcomeType.BeastNegotiation), Is.True);
            Assert.That(asset.OutcomeConsequences.Exists(o => o.Outcome == MissionOutcomeType.FailedEscalation), Is.True);
            Assert.That(asset.OutcomeConsequences.Exists(o => o.Outcome == MissionOutcomeType.PartialContainment), Is.True);
        }
    }
}

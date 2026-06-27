using LuoLuoTrip.Editor;
using NUnit.Framework;
using UnityEditor;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionDefinitionAssetIdempotencyTests
    {
        [TestCase("Assets/Data/Missions/ConvoyEnergyConflict.asset")]
        [TestCase("Assets/Data/Missions/BorderRetaliation.asset")]
        [TestCase("Assets/Data/Missions/CityGateDispute.asset")]
        public void MissionAssets_HaveRequiredAuthoringFields(string path)
        {
            var mission = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
            Assert.That(mission, Is.Not.Null, path);
            Assert.That(mission.MissionId, Is.Not.Empty);
            Assert.That(mission.DisplayName, Is.Not.Empty);
            Assert.That(mission.DefaultObjectives, Is.Not.Null.And.Not.Empty);
            Assert.That(mission.OutcomeConsequences, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void CreateMissionPrototypeData_PreservesExistingDisplayName()
        {
            const string path = "Assets/Data/Missions/CityGateDispute.asset";
            var mission = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
            Assert.That(mission, Is.Not.Null, path);

            var originalDisplayName = mission.DisplayName;
            mission.DisplayName = "City Gate Dispute - Tuned Test Name";
            EditorUtility.SetDirty(mission);
            AssetDatabase.SaveAssets();

            try
            {
                LuoLuoTripSetupMenu.CreateMissionPrototypeData();
                var reloaded = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
                Assert.That(reloaded.DisplayName, Is.EqualTo("City Gate Dispute - Tuned Test Name"));
            }
            finally
            {
                mission.DisplayName = originalDisplayName;
                EditorUtility.SetDirty(mission);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

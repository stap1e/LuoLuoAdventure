using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEditor;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DemoFlowValidatorTests
    {
        [Test]
        public void DemoFlowTypes_AreDetectable()
        {
            Assert.That(typeof(DemoFlowManager), Is.Not.Null);
            Assert.That(typeof(DemoFlowHud), Is.Not.Null);
        }

        [Test]
        public void MissionAuthoringAssets_AreDetectable()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/ConvoyEnergyConflict.asset"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/BorderRetaliation.asset"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>("Assets/Data/Missions/CityGateDispute.asset"), Is.Not.Null);
        }

        [Test]
        public void CommanderActionReadabilityTypes_AreDetectable()
        {
            Assert.That(typeof(CommanderActionType).IsEnum, Is.True);
            Assert.That(typeof(CommanderActionPresenter), Is.Not.Null);
            Assert.That(CommanderActionPresenter.BuildDescriptors(new CommanderControlRuntimeState()), Has.Count.EqualTo(5));
        }
    }
}

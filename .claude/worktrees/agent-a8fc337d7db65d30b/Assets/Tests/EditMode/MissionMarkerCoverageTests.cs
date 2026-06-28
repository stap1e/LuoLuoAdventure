using System.Linq;
using LuoLuoTrip.Feedback;
using NUnit.Framework;
using UnityEditor.SceneManagement;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionMarkerCoverageTests
    {
        private const string ScenePath = "Assets/Scenes/CommanderPrototype.unity";

        [TestCase("Convoy marker", "Convoy")]
        [TestCase("EnergyNode marker", "Energy Node")]
        [TestCase("Border marker", "Border Retaliation")]
        [TestCase("RaiderSpawn marker", "Raider Spawn")]
        [TestCase("Allied defense marker", "Allied Defense Point")]
        [TestCase("CityGate marker", "City Gate Mission Area")]
        [TestCase("CityGateCore marker", "CityGateCore")]
        [TestCase("BeastNegotiator marker", "BeastNegotiator")]
        [TestCase("BeastRaiderSpawn marker", "BeastRaider Spawn")]
        [TestCase("Low-rank controllable marker", "Low-Rank Ally")]
        [TestCase("Low-rank command marker", "Can Receive Commands")]
        [TestCase("High-rank denied marker", "High-Rank Unit: Tactical Command Only")]
        public void RequiredDemoMarkers_AreReadable(string markerName, string expectedText)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var allText = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<UnityEngine.Transform>(true))
                    .SelectMany(transform =>
                    {
                        var marker = transform.GetComponent<WorldMarker>();
                        return new[]
                        {
                            transform.name,
                            marker?.CustomLabel,
                            WorldMarker.BuildReadableLabel(transform.gameObject.name)
                        };
                    })
                    .Where(value => !string.IsNullOrEmpty(value))
                    .ToList();

                Assert.That(allText.Any(value => value.Contains(expectedText)), Is.True, $"Missing {markerName}: {expectedText}");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ReadableMarkerFallbacks_CoverGeneratedSetupNames()
        {
            Assert.That(WorldMarker.BuildReadableLabel("Convoy_Objective"), Is.EqualTo("Convoy"));
            Assert.That(WorldMarker.BuildReadableLabel("Energy_Node"), Is.EqualTo("Energy Node"));
            Assert.That(WorldMarker.BuildReadableLabel("BorderSpawnPoint_Beast"), Is.EqualTo("Raider Spawn"));
            Assert.That(WorldMarker.BuildReadableLabel("CityGateSpawnPoint_Beast"), Is.EqualTo("BeastRaider Spawn"));
        }
    }
}

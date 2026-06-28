using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterModifierTests
    {
        [Test]
        public void ApplyMissionModifier_UpdatesHostilityMultiplier()
        {
            var go = new GameObject("Encounter");
            try
            {
                var encounter = go.AddComponent<EncounterRuntime>();
                encounter.Initialize(new EncounterDefinition { encounterId = "test" });
                var mod = new MissionModifier { BeastHostilityMultiplier = 1.5f, InitialHostilityOffset = -15f };
                encounter.ApplyMissionModifier(mod);
                Assert.That(encounter.Definition.BeastHostilityMultiplier, Is.EqualTo(1.5f));
                Assert.That(encounter.Definition.InitialHostilityOffset, Is.EqualTo(-15f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

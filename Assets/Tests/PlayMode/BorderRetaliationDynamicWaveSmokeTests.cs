using System.Collections;
using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class BorderRetaliationDynamicWaveSmokeTests
    {
        [UnityTest]
        public IEnumerator MechaVictoryModifier_ProducesMoreBeastWaves()
        {
            var go = new GameObject("BorderRet");
            try
            {
                var br = go.AddComponent<BorderRetaliationRuntime>();
                var modField = br.GetType().GetField("_modifier",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var mod = new MissionModifier
                {
                    ModifierId = "border_beast_retaliation",
                    BeastHostilityMultiplier = 1.5f,
                    MechaSupportMultiplier = 1f
                };
                modField.SetValue(br, mod);

                var encGo = new GameObject("Enc");
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition());
                var encField = br.GetType().GetField("_encounter",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                encField.SetValue(br, enc);

                // Call ConfigureDynamicWaves via reflection
                var method = br.GetType().GetMethod("ConfigureDynamicWaves",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(br, null);

                yield return null;

                Assert.GreaterOrEqual(enc.Waves.Count, 2, "MechaVictory should produce >= 2 beast waves");

                Object.DestroyImmediate(encGo);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [UnityTest]
        public IEnumerator BalancedResolutionModifier_ProducesFewerWaves()
        {
            var go = new GameObject("BorderRet");
            try
            {
                var br = go.AddComponent<BorderRetaliationRuntime>();
                var modField = br.GetType().GetField("_modifier",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var mod = new MissionModifier
                {
                    ModifierId = "border_ceasefire",
                    BeastHostilityMultiplier = 0.6f,
                    MechaSupportMultiplier = 0.8f
                };
                modField.SetValue(br, mod);

                var encGo = new GameObject("Enc");
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition());
                var encField = br.GetType().GetField("_encounter",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                encField.SetValue(br, enc);

                var method = br.GetType().GetMethod("ConfigureDynamicWaves",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(br, null);

                yield return null;

                Assert.LessOrEqual(enc.Waves.Count, 2, "BalancedResolution should produce <= 2 waves");

                Object.DestroyImmediate(encGo);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

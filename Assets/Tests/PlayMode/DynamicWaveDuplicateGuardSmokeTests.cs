using System.Collections;
using System.Reflection;
using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class DynamicWaveDuplicateGuardSmokeTests
    {
        [UnityTest]
        public IEnumerator ConfigureDynamicWaves_CalledTwice_DoesNotStackWaves()
        {
            var go = new GameObject("BorderRet");
            try
            {
                var br = go.AddComponent<BorderRetaliationRuntime>();
                var modField = br.GetType().GetField("_modifier",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                var mod = new MissionModifier
                {
                    ModifierId = "border_beast_retaliation",
                    BeastHostilityMultiplier = 1.5f,
                    MechaSupportMultiplier = 1f
                };
                modField.SetValue(br, mod);

                var encGo = new GameObject("Enc");
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "border_retaliation" });
                var encField = br.GetType().GetField("_encounter",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                encField.SetValue(br, enc);

                var method = br.GetType().GetMethod("ConfigureDynamicWaves",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                method.Invoke(br, null);
                int firstCount = enc.Waves.Count;
                Assert.GreaterOrEqual(firstCount, 1, "First Configure must produce waves");

                method.Invoke(br, null);
                int secondCount = enc.Waves.Count;

                Assert.That(secondCount, Is.EqualTo(firstCount), "ConfigureDynamicWaves must be idempotent");

                yield return null;

                Object.DestroyImmediate(encGo);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [UnityTest]
        public IEnumerator SpawnWave_DuplicateWaveId_DoesNotSpawnAgain()
        {
            var encGo = new GameObject("Enc");
            try
            {
                var enc = encGo.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition { encounterId = "dup_test" });
                var wave = new EncounterWave
                {
                    waveId = "beast_dup",
                    faction = SubFactionId.BeastIronClaw,
                    role = CharacterRole.Minion,
                    unitCount = 1
                };
                enc.SpawnWave(wave);
                int firstSpawnedCount = enc.TotalSpawnedCount;

                yield return null;

                int second = enc.SpawnWave(wave);
                Assert.That(second, Is.EqualTo(0), "Duplicate wave ID must return 0 spawns");
                Assert.That(enc.TotalSpawnedCount, Is.EqualTo(firstSpawnedCount));
            }
            finally { Object.DestroyImmediate(encGo); }
        }

        [UnityTest]
        public IEnumerator MissionTriggerZone_AfterCompletion_ForceStartIsNoOp()
        {
            var go = new GameObject("TriggerZone");
            try
            {
                var zone = go.AddComponent<MissionTriggerZone>();
                zone.ForceStart();
                zone.MarkCompleted();

                yield return null;

                zone.ForceStart();
                Assert.That(zone.MissionCompleted, Is.True);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

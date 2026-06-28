using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterWaveSpawnTests
    {
        [Test]
        public void EncounterWave_DefaultBehavior_IsChase()
        {
            var wave = new EncounterWave();
            Assert.AreEqual(SpawnBehavior.Chase, wave.initialBehavior);
        }

        [Test]
        public void EncounterWave_DefaultIsHostile_True()
        {
            var wave = new EncounterWave();
            Assert.IsTrue(wave.isHostile);
        }

        [Test]
        public void EncounterWave_IsReady_WhenNotSpawned()
        {
            var wave = new EncounterWave { waveId = "test", faction = SubFactionId.BeastIronClaw, unitCount = 3 };
            Assert.IsTrue(wave.IsReady);
        }

        [Test]
        public void EncounterWave_NotReady_WhenSpawned()
        {
            var wave = new EncounterWave { waveId = "test", faction = SubFactionId.BeastIronClaw, unitCount = 3, spawned = true };
            Assert.IsFalse(wave.IsReady);
        }

        [Test]
        public void EncounterSpawnPoint_GetRandomSpawnPosition_StaysInRadius()
        {
            var go = new GameObject("Spawn");
            go.transform.position = new Vector3(10f, 0f, 10f);
            var sp = go.AddComponent<EncounterSpawnPoint>();
            var radiusField = sp.GetType().GetField("_spawnRadius",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (radiusField != null) radiusField.SetValue(sp, 3f);

            for (int i = 0; i < 20; i++)
            {
                var pos = sp.GetRandomSpawnPosition();
                var dist = Vector3.Distance(new Vector3(10f, 0f, 10f), pos);
                Assert.LessOrEqual(dist, 3.1f, "Spawn position should be within radius");
            }
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EncounterSpawnPoint_GetRandomSpawnPosition_CustomRadius()
        {
            var go = new GameObject("Spawn");
            go.transform.position = Vector3.zero;
            var sp = go.AddComponent<EncounterSpawnPoint>();

            for (int i = 0; i < 20; i++)
            {
                var pos = sp.GetRandomSpawnPosition(5f);
                var dist = Vector3.Distance(Vector3.zero, pos);
                Assert.LessOrEqual(dist, 5.1f);
            }
            Object.DestroyImmediate(go);
        }
    }
}

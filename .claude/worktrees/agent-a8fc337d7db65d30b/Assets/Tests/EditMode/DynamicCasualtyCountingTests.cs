using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DynamicCasualtyCountingTests
    {
        [Test]
        public void CountCasualties_IncludesSpawnedUnits()
        {
            var go = new GameObject("Enc");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition());

                var unitGo = new GameObject("Unit");
                var entity = unitGo.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("u", "U", SubFactionId.BeastIronClaw, CharacterRole.Common, 3));
                var handle = EncounterUnitHandle.FromEntity(entity);

                var field = enc.GetType().GetField("_spawnedUnits",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = field.GetValue(enc) as System.Collections.Generic.List<EncounterUnitHandle>;
                list.Add(handle);

                Assert.AreEqual(0, enc.CountCasualties(SubFactionId.BeastIronClaw));

                entity.Data.IsAlive = false;
                Assert.AreEqual(1, enc.CountCasualties(SubFactionId.BeastIronClaw));

                Object.DestroyImmediate(unitGo);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void AreAllRaidUnitsDefeated_IncludesSpawnedUnits()
        {
            var go = new GameObject("Enc");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(new EncounterDefinition());

                var unitGo = new GameObject("Unit");
                var entity = unitGo.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("u", "U", SubFactionId.BeastIronClaw, CharacterRole.Common, 3));
                var handle = EncounterUnitHandle.FromEntity(entity);

                var field = enc.GetType().GetField("_spawnedUnits",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = field.GetValue(enc) as System.Collections.Generic.List<EncounterUnitHandle>;
                list.Add(handle);

                Assert.IsFalse(enc.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw));

                entity.Data.IsAlive = false;
                Assert.IsTrue(enc.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw));

                Object.DestroyImmediate(unitGo);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Clear_RemovesSpawnedUnits()
        {
            var go = new GameObject("Enc");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();

                var field = enc.GetType().GetField("_spawnedUnits",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = field.GetValue(enc) as System.Collections.Generic.List<EncounterUnitHandle>;
                list.Add(EncounterUnitHandle.FromEntity(null) ?? new EncounterUnitHandle());

                enc.Clear();

                Assert.AreEqual(0, list.Count);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

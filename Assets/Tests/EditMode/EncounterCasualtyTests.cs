using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterCasualtyTests
    {
        [Test]
        public void CountCasualties_BySubFaction_ReturnsCorrectCount()
        {
            var go = new GameObject("Encounter");
            try
            {
                var encounter = go.AddComponent<EncounterRuntime>();
                var u1 = new GameObject("B1");
                var entity1 = u1.AddComponent<CharacterEntity>();
                entity1.Bind(CharacterData.Create("b1", "Beast1", SubFactionId.BeastIronClaw, CharacterRole.Minion));
                entity1.Data.IsAlive = false;
                encounter.RegisterUnit(entity1);

                var u2 = new GameObject("B2");
                var entity2 = u2.AddComponent<CharacterEntity>();
                entity2.Bind(CharacterData.Create("b2", "Beast2", SubFactionId.BeastIronClaw, CharacterRole.Minion));

                encounter.RegisterUnit(entity2);
                Assert.That(encounter.CountCasualties(SubFactionId.BeastIronClaw), Is.EqualTo(1));
                Object.DestroyImmediate(u2);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetAliveUnits_FiltersByRace()
        {
            var go = new GameObject("Encounter");
            try
            {
                var encounter = go.AddComponent<EncounterRuntime>();
                var u1 = new GameObject("M1");
                var entity1 = u1.AddComponent<CharacterEntity>();
                entity1.Bind(CharacterData.Create("m1", "Mecha1", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                encounter.RegisterUnit(entity1);

                var u2 = new GameObject("B1");
                var entity2 = u2.AddComponent<CharacterEntity>();
                entity2.Bind(CharacterData.Create("b1", "Beast1", SubFactionId.BeastIronClaw, CharacterRole.Minion));
                encounter.RegisterUnit(entity2);

                var alive = encounter.GetAliveUnits(MainRace.BeastTribe);
                Assert.That(alive.Count, Is.EqualTo(1));
                Object.DestroyImmediate(u2);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

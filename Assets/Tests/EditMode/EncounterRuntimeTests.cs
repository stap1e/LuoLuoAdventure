using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterRuntimeTests
    {
        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Encounter");
            _go.AddComponent<EncounterRuntime>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void RegisterUnit_AddsToUnits()
        {
            var encounter = _go.GetComponent<EncounterRuntime>();
            var unitGo = new GameObject("Unit");
            try
            {
                var entity = unitGo.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("m1", "TestMecha", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                encounter.RegisterUnit(entity);
                Assert.That(encounter.Units.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(unitGo);
            }
        }

        [Test]
        public void UnregisterUnit_RemovesFromUnits()
        {
            var encounter = _go.GetComponent<EncounterRuntime>();
            var unitGo = new GameObject("Unit");
            try
            {
                var entity = unitGo.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("m1", "TestMecha", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                encounter.RegisterUnit(entity);
                encounter.UnregisterUnit(entity);
                Assert.That(encounter.Units.Count, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(unitGo);
            }
        }

        [Test]
        public void AreAllRaidUnitsDefeated_TrueWhenAllDead()
        {
            var encounter = _go.GetComponent<EncounterRuntime>();
            var unitGo = new GameObject("BeastUnit");
            try
            {
                var entity = unitGo.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("b1", "TestBeast", SubFactionId.BeastIronClaw, CharacterRole.Minion));
                entity.Data.IsAlive = false;
                encounter.RegisterUnit(entity);
                Assert.That(encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(unitGo);
            }
        }

        [Test]
        public void CountCasualties_CountsDeadByRace()
        {
            var encounter = _go.GetComponent<EncounterRuntime>();
            var unitGo = new GameObject("MechaUnit");
            try
            {
                var entity = unitGo.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("m1", "TestMecha", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                encounter.RegisterUnit(entity);
                entity.Data.IsAlive = false;
                Assert.That(encounter.CountCasualties(MainRace.MotorTribe), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(unitGo);
            }
        }
    }
}

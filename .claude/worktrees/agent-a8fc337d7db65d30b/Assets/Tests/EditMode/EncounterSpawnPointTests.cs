using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class EncounterSpawnPointTests
    {
        private GameObject _spawnGo;

        [TearDown]
        public void TearDown()
        {
            if (_spawnGo != null)
                Object.DestroyImmediate(_spawnGo);
            var all = Object.FindObjectsOfType<CharacterEntity>();
            foreach (var e in all)
                if (e != null && e.gameObject != null)
                    Object.DestroyImmediate(e.gameObject);
        }

        [Test]
        public void GetSpawnPosition_ReturnsTransformPosition()
        {
            _spawnGo = new GameObject("Spawn");
            _spawnGo.transform.position = new Vector3(5f, 0f, 3f);
            var sp = _spawnGo.AddComponent<EncounterSpawnPoint>();
            Assert.That(sp.GetSpawnPosition(), Is.EqualTo(new Vector3(5f, 0f, 3f)));
        }

        [Test]
        public void GetRandomSpawnPosition_WithinRadius()
        {
            _spawnGo = new GameObject("Spawn");
            _spawnGo.transform.position = Vector3.zero;
            var sp = _spawnGo.AddComponent<EncounterSpawnPoint>();
            for (int i = 0; i < 50; i++)
            {
                var pos = sp.GetRandomSpawnPosition();
                var dist = Vector3.Distance(pos, Vector3.zero);
                Assert.That(dist, Is.LessThan(3f));
            }
        }

        [Test]
        public void SpawnUnit_CreatesGameObjectWithRequiredComponents()
        {
            _spawnGo = new GameObject("Spawn");
            _spawnGo.transform.position = Vector3.zero;
            var sp = _spawnGo.AddComponent<EncounterSpawnPoint>();

            var data = CharacterData.Create("test_1", "TestUnit", SubFactionId.BeastIronClaw, CharacterRole.Minion);
            var unitGo = sp.SpawnUnit(data);

            Assert.That(unitGo, Is.Not.Null);
            Assert.That(unitGo.GetComponent<CharacterEntity>(), Is.Not.Null);
            Assert.That(unitGo.GetComponent<Combat.Combatant>(), Is.Not.Null);
            Assert.That(unitGo.GetComponent<SimpleCombatAI>(), Is.Not.Null);
            Assert.That(unitGo.GetComponent<NavigationAgentBridge>(), Is.Not.Null);
        }

        [Test]
        public void SpawnUnit_BindsCharacterData()
        {
            _spawnGo = new GameObject("Spawn");
            _spawnGo.transform.position = Vector3.zero;
            var sp = _spawnGo.AddComponent<EncounterSpawnPoint>();

            var data = CharacterData.Create("test_2", "SpawnedUnit", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            var unitGo = sp.SpawnUnit(data);

            var entity = unitGo.GetComponent<CharacterEntity>();
            Assert.That(entity.Data, Is.Not.Null);
            Assert.That(entity.Data.Faction, Is.EqualTo(SubFactionId.MotorIronRiders));
        }
    }
}

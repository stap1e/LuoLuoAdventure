using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CharacterRuntimeRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            CharacterRuntimeRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            CharacterRuntimeRegistry.Clear();
        }

        [Test]
        public void Register_AddsCharacter()
        {
            var go = new GameObject("test_reg");
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData("test_1", "Test", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));

            CharacterRuntimeRegistry.Register(entity);
            Assert.That(CharacterRuntimeRegistry.Count, Is.EqualTo(1));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Unregister_RemovesCharacter()
        {
            var go = new GameObject("test_unreg");
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData("test_2", "Test", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));

            CharacterRuntimeRegistry.Register(entity);
            CharacterRuntimeRegistry.Unregister(entity);
            Assert.That(CharacterRuntimeRegistry.Count, Is.EqualTo(0));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void QueryCharactersInRadius_ReturnsNearby()
        {
            var go1 = new GameObject("near");
            go1.transform.position = Vector3.zero;
            var entity1 = go1.AddComponent<CharacterEntity>();
            entity1.Bind(new CharacterData("near_1", "Near", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));

            var go2 = new GameObject("far");
            go2.transform.position = new Vector3(100f, 0f, 0f);
            var entity2 = go2.AddComponent<CharacterEntity>();
            entity2.Bind(new CharacterData("far_1", "Far", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));

            CharacterRuntimeRegistry.Register(entity1);
            CharacterRuntimeRegistry.Register(entity2);

            var result = CharacterRuntimeRegistry.QueryCharactersInRadius(Vector3.zero, 15f);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(entity1));

            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }

        [Test]
        public void QueryBySubFaction_ReturnsCorrectFaction()
        {
            var go1 = new GameObject("mecha");
            var entity1 = go1.AddComponent<CharacterEntity>();
            entity1.Bind(new CharacterData("m_1", "Mecha", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));

            var go2 = new GameObject("beast");
            var entity2 = go2.AddComponent<CharacterEntity>();
            entity2.Bind(new CharacterData("b_1", "Beast", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));

            CharacterRuntimeRegistry.Register(entity1);
            CharacterRuntimeRegistry.Register(entity2);

            var mechas = CharacterRuntimeRegistry.QueryBySubFaction(SubFactionId.MotorIronRiders);
            Assert.That(mechas.Count, Is.EqualTo(1));

            var beasts = CharacterRuntimeRegistry.QueryBySubFaction(SubFactionId.BeastIronClaw);
            Assert.That(beasts.Count, Is.EqualTo(1));

            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }

        [Test]
        public void DoubleRegister_DoesNotDuplicate()
        {
            var go = new GameObject("dbl");
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData("dbl_1", "Dbl", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));

            CharacterRuntimeRegistry.Register(entity);
            CharacterRuntimeRegistry.Register(entity);
            Assert.That(CharacterRuntimeRegistry.Count, Is.EqualTo(1));

            Object.DestroyImmediate(go);
        }
    }
}

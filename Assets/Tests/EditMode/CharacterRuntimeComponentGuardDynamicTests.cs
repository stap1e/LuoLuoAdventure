using LuoLuoTrip;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CharacterRuntimeComponentGuardDynamicTests
    {
        [TearDown]
        public void TearDown()
        {
            CharacterRuntimeComponentGuard.ResetWarnings();
        }

        [Test]
        public void EnsureForAI_AddsHealthBar()
        {
            var go = new GameObject("AI");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("a", "A", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsTrue(result.HealthBarAdded);
                Assert.IsNotNull(go.GetComponent<CombatantHealthBarPresenter>());
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EnsureForAI_AddsHitFlash()
        {
            var go = new GameObject("AI");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("a", "A", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsTrue(result.HitFlashAdded);
                Assert.IsNotNull(go.GetComponent<HitFlashFeedback>());
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EnsureForAI_AddsNavBridge()
        {
            var go = new GameObject("AI");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("a", "A", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsTrue(result.NavBridgeAdded);
                Assert.IsNotNull(go.GetComponent<LuoLuoTrip.AI.NavigationAgentBridge>());
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EnsureForAI_DoesNotDuplicateExistingComponents()
        {
            var go = new GameObject("AI");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("a", "A", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                go.AddComponent<CombatantHealthBarPresenter>();
                go.AddComponent<HitFlashFeedback>();

                var result = CharacterRuntimeComponentGuard.EnsureForAI(go);
                Assert.IsFalse(result.HealthBarAdded, "Should not add duplicate health bar");
                Assert.IsFalse(result.HitFlashAdded, "Should not add duplicate hit flash");
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatTuningConfigTests
    {
        [Test]
        public void DefaultConfig_HasValidTimingValues()
        {
            var config = CombatTuningConfigSO.Default;

            Assert.That(config.playerAttackWindup, Is.GreaterThan(0f));
            Assert.That(config.playerAttackActive, Is.GreaterThan(0f));
            Assert.That(config.playerAttackRecovery, Is.GreaterThan(0f));
            Assert.That(config.dodgeDuration, Is.GreaterThan(0f));
            Assert.That(config.dodgeInvulnerableDuration, Is.GreaterThan(0f));
            Assert.That(config.staggerDuration, Is.GreaterThan(0f));
            Assert.That(config.aiAttackWindupDelay, Is.GreaterThan(0f));
            Assert.That(config.syncAssistDuration, Is.GreaterThan(0f));
        }

        [Test]
        public void DefaultConfig_DodgeInvulnerable_NotExceedDodgeDuration()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.That(config.dodgeInvulnerableDuration, Is.LessThanOrEqualTo(config.dodgeDuration));
        }

        [Test]
        public void DefaultConfig_SyncAssistBonuses_ArePositive()
        {
            var config = CombatTuningConfigSO.Default;
            Assert.That(config.syncAssistAttackBonus, Is.GreaterThanOrEqualTo(0f));
            Assert.That(config.syncAssistDefenseBonus, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void ApplyToCombatant_SetsTimingValues()
        {
            var go = new GameObject("TestCombatant");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("test", "Test", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var combatant = go.AddComponent<Combatant>();

                var config = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
                config.playerAttackWindup = 0.5f;
                config.playerAttackActive = 0.3f;
                config.playerAttackRecovery = 0.4f;
                config.dodgeDuration = 0.5f;
                config.dodgeDistance = 5f;
                config.dodgeInvulnerableDuration = 0.4f;
                config.staggerDuration = 1.5f;

                config.ApplyTo(combatant);

                Assert.That(combatant.AttackWindup, Is.EqualTo(0.5f));
                Assert.That(combatant.AttackRecovery, Is.EqualTo(0.4f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void LoadOrDefault_ReturnsNonNull()
        {
            var config = CombatTuningConfigSO.LoadOrDefault();
            Assert.That(config, Is.Not.Null);
        }

        [Test]
        public void LoadOrDefault_ReturnsDefault_WhenNoAssetExists()
        {
            var config = CombatTuningConfigSO.LoadOrDefault();
            Assert.That(config, Is.SameAs(CombatTuningConfigSO.Default));
        }
    }
}

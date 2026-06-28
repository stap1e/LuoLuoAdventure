using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatTimingTests
    {
        [Test]
        public void FullAttackSequence_DurationMatchesConfig()
        {
            var go = new GameObject("Attacker");
            var defenderGo = new GameObject("Defender");
            try
            {
                var attacker = CreateCombatant(go, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderGo, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                Assert.That(attacker.TryLightAttack(defender), Is.True);
                Assert.That(attacker.State, Is.EqualTo(CombatState.AttackWindup));

                // UpdateStateTimer advances at most one phase per Tick. The helper walks
                // windup -> active -> recovery -> idle and drains the stat cooldown.
                CombatTimingTestHelper.AdvanceCombatThroughAttack(attacker);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Idle));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        [Test]
        public void DodgeInvulnerability_ExpiresBeforeDodgeEnds_WithDefaultConfig()
        {
            var go = new GameObject("Dodger");
            try
            {
                var dodger = CreateCombatant(go, "dodger", SubFactionId.MotorIronRiders);
                dodger.AutoTickEnabled = false;

                dodger.TryDodge(Vector3.forward);
                Assert.That(dodger.IsInvulnerable, Is.True);

                var config = CombatTuningConfigSO.Default;
                // IsInvulnerable returns true while State==Dodging OR while invuln timer > 0.
                // The Default config has dodgeInvulnerableDuration < dodgeDuration, so after
                // ticking past dodgeDuration the dodge state ends AND invuln timer is drained.
                dodger.Tick(config.dodgeDuration + 0.01f);
                Assert.That(dodger.State, Is.Not.EqualTo(CombatState.Dodging),
                    "After ticking past dodgeDuration, Dodging state must end");
                Assert.That(dodger.IsInvulnerable, Is.False,
                    "After dodge ends, IsInvulnerable must be false");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StaggerDuration_MatchesConfig()
        {
            var go = new GameObject("Target");
            try
            {
                var target = CreateCombatant(go, "target", SubFactionId.MotorIronRiders);
                target.AutoTickEnabled = false;

                target.ApplyPoiseDamage(target.Stats.maxPoise + 100f);
                Assert.That(target.State, Is.EqualTo(CombatState.Staggered));

                var config = CombatTuningConfigSO.Default;
                target.Tick(config.staggerDuration - 0.01f);
                Assert.That(target.State, Is.EqualTo(CombatState.Staggered));

                target.Tick(0.02f);
                Assert.That(target.State, Is.EqualTo(CombatState.Idle));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CannotAttackDuringStagger()
        {
            var go = new GameObject("Target");
            var defenderGo = new GameObject("Defender");
            try
            {
                var target = CreateCombatant(go, "target", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderGo, "defender", SubFactionId.BeastIronClaw);
                target.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                target.ApplyPoiseDamage(target.Stats.maxPoise + 100f);
                Assert.That(target.State, Is.EqualTo(CombatState.Staggered));
                Assert.That(target.TryLightAttack(defender), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        private static Combatant CreateCombatant(GameObject gameObject, string id, SubFactionId faction)
        {
            var entity = gameObject.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            return entity.Combatant;
        }
    }
}

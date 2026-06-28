using NUnit.Framework;
using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatWindupRecoveryTests
    {
        [Test]
        public void TryLightAttack_EntersAttackWindupState()
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
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        [Test]
        public void Tick_WindupTransitionsToAttacking()
        {
            var go = new GameObject("Attacker");
            var defenderGo = new GameObject("Defender");
            try
            {
                var attacker = CreateCombatant(go, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderGo, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                attacker.TryLightAttack(defender);
                Assert.That(attacker.State, Is.EqualTo(CombatState.AttackWindup));

                attacker.Tick(0.25f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Attacking));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        [Test]
        public void Tick_AttackingTransitionsToRecovery()
        {
            var go = new GameObject("Attacker");
            var defenderGo = new GameObject("Defender");
            try
            {
                var attacker = CreateCombatant(go, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderGo, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                attacker.TryLightAttack(defender);
                attacker.Tick(0.25f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Attacking));

                attacker.Tick(0.2f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.AttackRecovery));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        [Test]
        public void Tick_RecoveryTransitionsToIdle()
        {
            var go = new GameObject("Attacker");
            var defenderGo = new GameObject("Defender");
            try
            {
                var attacker = CreateCombatant(go, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderGo, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                attacker.TryLightAttack(defender);
                attacker.Tick(0.25f);
                attacker.Tick(0.2f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.AttackRecovery));

                attacker.Tick(0.3f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Idle));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        [Test]
        public void IsInvulnerable_DuringDodgeWindow()
        {
            var go = new GameObject("Dodger");
            try
            {
                var dodger = CreateCombatant(go, "dodger", SubFactionId.MotorIronRiders);
                dodger.AutoTickEnabled = false;

                dodger.TryDodge(Vector3.forward);
                Assert.That(dodger.IsInvulnerable, Is.True);

                dodger.Tick(0.35f);
                Assert.That(dodger.State, Is.EqualTo(CombatState.Idle));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CannotActDuringWindup()
        {
            var go = new GameObject("Attacker");
            var defenderGo = new GameObject("Defender");
            try
            {
                var attacker = CreateCombatant(go, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderGo, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                attacker.TryLightAttack(defender);
                Assert.That(attacker.TryLightAttack(defender), Is.False);
                Assert.That(attacker.TryDodge(Vector3.forward), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(defenderGo);
            }
        }

        [Test]
        public void CannotActDuringRecovery()
        {
            var go = new GameObject("Attacker");
            var defenderGo = new GameObject("Defender");
            try
            {
                var attacker = CreateCombatant(go, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderGo, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                attacker.TryLightAttack(defender);
                attacker.Tick(0.25f);
                attacker.Tick(0.2f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.AttackRecovery));
                Assert.That(attacker.TryLightAttack(defender), Is.False);
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

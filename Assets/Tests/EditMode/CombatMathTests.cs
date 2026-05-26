using NUnit.Framework;
using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatMathTests
    {
        [Test]
        public void Calculate_ReturnsAtLeastOneDamageAgainstHighDefense()
        {
            var attackerObject = new GameObject("Attacker");
            var defenderObject = new GameObject("Defender");

            try
            {
                var attackerEntity = attackerObject.AddComponent<CharacterEntity>();
                attackerEntity.Bind(new CharacterData("attacker", "Attacker", SubFactionId.MotorStormGang, CharacterRole.Common, 5));
                var attacker = attackerEntity.Combatant;

                var defenderEntity = defenderObject.AddComponent<CharacterEntity>();
                defenderEntity.Bind(new CharacterData("defender", "Defender", SubFactionId.BeastThunderHide, CharacterRole.Common, 5));
                var defender = defenderEntity.Combatant;

                var boostedDefense = defender.Stats;
                boostedDefense.defense = attacker.Stats.attackPower * 10f;
                defender.InitializeForTests(boostedDefense);

                var result = DamageCalculator.Calculate(attacker, defender);

                Assert.That(result.finalDamage, Is.EqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(attackerObject);
                Object.DestroyImmediate(defenderObject);
            }
        }

        [Test]
        public void Calculate_FlagsFatalWhenDamageDepletesHealth()
        {
            var attackerObject = new GameObject("Attacker");
            var defenderObject = new GameObject("Defender");

            try
            {
                var attackerEntity = attackerObject.AddComponent<CharacterEntity>();
                attackerEntity.Bind(new CharacterData("attacker", "Attacker", SubFactionId.BeastIronClaw, CharacterRole.WarKing, 50));
                var attacker = attackerEntity.Combatant;

                var defenderEntity = defenderObject.AddComponent<CharacterEntity>();
                defenderEntity.Bind(new CharacterData("defender", "Defender", SubFactionId.MotorNightRunners, CharacterRole.Minion, 1));
                var defender = defenderEntity.Combatant;

                defender.InitializeForTests(defender.Stats, health: 1f);
                var result = DamageCalculator.Calculate(attacker, defender);

                Assert.That(result.wasFatal, Is.True);
                Assert.That(defender.IsAlive, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(attackerObject);
                Object.DestroyImmediate(defenderObject);
            }
        }

        [Test]
        public void CombatStatsCalculator_IncreasesPowerWithHigherLevelRole()
        {
            var minion = new CharacterData("minion", "Minion", SubFactionId.MotorIronRiders, CharacterRole.Minion, 1);
            var warKing = new CharacterData("leader", "Leader", SubFactionId.MotorIronRiders, CharacterRole.WarKing, 20);

            var minionStats = CombatStatsCalculator.Calculate(minion);
            var warKingStats = CombatStatsCalculator.Calculate(warKing);

            Assert.That(warKingStats.maxHealth, Is.GreaterThan(minionStats.maxHealth));
            Assert.That(warKingStats.attackPower, Is.GreaterThan(minionStats.attackPower));
            Assert.That(warKingStats.attackRange, Is.GreaterThanOrEqualTo(minionStats.attackRange));
        }

        [Test]
        public void Tick_EndsAttackStateAfterWindupAndActiveFrames()
        {
            var attackerObject = new GameObject("Attacker");
            var defenderObject = new GameObject("Defender");

            try
            {
                var attacker = CreateCombatant(attackerObject, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderObject, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                Assert.That(attacker.TryLightAttack(defender), Is.True);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Attacking));

                attacker.Tick(0.2f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Attacking));

                attacker.Tick(0.3f);
                Assert.That(attacker.State, Is.EqualTo(CombatState.Idle));
            }
            finally
            {
                Object.DestroyImmediate(attackerObject);
                Object.DestroyImmediate(defenderObject);
            }
        }

        [Test]
        public void Tick_BlocksAttackUntilCooldownExpires()
        {
            var attackerObject = new GameObject("Attacker");
            var defenderObject = new GameObject("Defender");

            try
            {
                var attacker = CreateCombatant(attackerObject, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderObject, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                Assert.That(attacker.TryLightAttack(defender), Is.True);
                attacker.Tick(0.5f);
                Assert.That(attacker.TryLightAttack(defender), Is.False);

                attacker.Tick(1f);
                Assert.That(attacker.TryLightAttack(defender), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(attackerObject);
                Object.DestroyImmediate(defenderObject);
            }
        }

        [Test]
        public void Tick_MovesAndEndsDodgeDeterministically()
        {
            var dodgerObject = new GameObject("Dodger");

            try
            {
                var dodger = CreateCombatant(dodgerObject, "dodger", SubFactionId.MotorIronRiders);
                dodger.AutoTickEnabled = false;
                var startPosition = dodger.transform.position;

                Assert.That(dodger.TryDodge(Vector3.forward), Is.True);
                Assert.That(dodger.State, Is.EqualTo(CombatState.Dodging));

                dodger.Tick(0.1f);
                Assert.That(dodger.transform.position.z, Is.GreaterThan(startPosition.z));
                Assert.That(dodger.State, Is.EqualTo(CombatState.Dodging));

                dodger.Tick(0.3f);
                Assert.That(dodger.State, Is.EqualTo(CombatState.Idle));
            }
            finally
            {
                Object.DestroyImmediate(dodgerObject);
            }
        }

        [Test]
        public void Tick_DoesNotRecoverStaminaWhileAttacking()
        {
            var attackerObject = new GameObject("Attacker");
            var defenderObject = new GameObject("Defender");

            try
            {
                var attacker = CreateCombatant(attackerObject, "attacker", SubFactionId.MotorIronRiders);
                var defender = CreateCombatant(defenderObject, "defender", SubFactionId.BeastIronClaw);
                attacker.AutoTickEnabled = false;
                defender.AutoTickEnabled = false;

                var beforeAttack = attacker.CurrentStamina;
                Assert.That(attacker.TryLightAttack(defender), Is.True);
                var afterAttack = attacker.CurrentStamina;

                attacker.Tick(0.2f);
                Assert.That(attacker.CurrentStamina, Is.EqualTo(afterAttack));

                attacker.Tick(0.3f);
                attacker.Tick(0.5f);
                Assert.That(attacker.CurrentStamina, Is.GreaterThan(afterAttack));
                Assert.That(attacker.CurrentStamina, Is.LessThanOrEqualTo(beforeAttack));
            }
            finally
            {
                Object.DestroyImmediate(attackerObject);
                Object.DestroyImmediate(defenderObject);
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

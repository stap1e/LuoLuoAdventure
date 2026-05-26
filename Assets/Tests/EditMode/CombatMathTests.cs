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
    }
}

using System;

namespace LuoLuoTrip.Combat
{
    public struct DamageResult
    {
        public float finalDamage;
        public float poiseDamage;
        public bool wasPoiseBroken;
        public bool wasFatal;
    }

    public static class DamageCalculator
    {
        public static DamageResult Calculate(Combatant attacker, Combatant defender, float attackMultiplier = 1f)
        {
            if (attacker == null || defender == null || !defender.IsAlive)
            {
                return new DamageResult();
            }

            var atk = attacker.Stats.attackPower * attackMultiplier;
            var raw = Math.Max(1f, atk - defender.Stats.defense * 0.5f);
            var poiseDmg = attacker.Stats.poiseDamage * attackMultiplier;

            var wasPoiseBroken = defender.ApplyPoiseDamage(poiseDmg);
            var wasFatal = defender.ApplyHealthDamage(raw);

            return new DamageResult
            {
                finalDamage = raw,
                poiseDamage = poiseDmg,
                wasPoiseBroken = wasPoiseBroken,
                wasFatal = wasFatal
            };
        }
    }
}

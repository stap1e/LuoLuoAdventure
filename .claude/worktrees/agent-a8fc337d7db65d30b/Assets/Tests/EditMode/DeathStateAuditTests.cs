using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DeathStateAuditTests
    {
        private Combatant CreateCombatant(GameObject go, string id, SubFactionId faction)
        {
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            return entity.Combatant;
        }

        [Test]
        public void Death_FiresOnDeath_OnlyOnce()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;

                int deathCount = 0;
                c.OnDeath += _ => deathCount++;

                c.ApplyHealthDamage(99999f);
                c.ApplyHealthDamage(99999f);
                c.ApplyHealthDamage(99999f);

                Assert.AreEqual(1, deathCount, "OnDeath must fire exactly once");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Dead_CannotAttack()
        {
            var atkGo = new GameObject("A");
            var defGo = new GameObject("D");
            try
            {
                var atk = CreateCombatant(atkGo, "a", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "d", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;

                atk.ApplyHealthDamage(99999f);
                Assert.IsFalse(atk.TryLightAttack(def), "Dead unit cannot attack");
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }

        [Test]
        public void Dead_CannotDodge()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;

                c.ApplyHealthDamage(99999f);
                Assert.IsFalse(c.TryDodge(Vector3.forward), "Dead unit cannot dodge");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Dead_StateIsDead()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;

                c.ApplyHealthDamage(99999f);
                Assert.AreEqual(CombatState.Dead, c.State);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Dead_DataIsAliveFalse()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;

                c.ApplyHealthDamage(99999f);
                Assert.IsFalse(c.CharacterEntity.Data.IsAlive);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Dead_CannotBeHitAgain()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;

                c.ApplyHealthDamage(99999f);
                var hp = c.CurrentHealth;
                var result = c.ApplyHealthDamage(50f);
                Assert.IsFalse(result, "Dead unit cannot be damaged again");
                Assert.AreEqual(0f, c.CurrentHealth);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void RestoreRuntimeState_RevivesDeadUnit()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;

                c.ApplyHealthDamage(99999f);
                Assert.IsFalse(c.IsAlive);

                c.RestoreRuntimeState(c.Stats.maxHealth, c.Stats.maxStamina, c.Stats.maxPoise);
                if (c.CharacterEntity?.Data != null)
                    c.CharacterEntity.Data.IsAlive = true;

                Assert.IsTrue(c.IsAlive);
                Assert.AreEqual(CombatState.Idle, c.State);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

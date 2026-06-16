using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    /// <summary>
    /// Verifies the rebuilt active-window damage flow:
    /// - TakeDamage reduces HP via DamageCalculator
    /// - OnHitLanded fires only after entering Attacking state (active window)
    /// - OnDeath fires; dead unit cannot be re-attacked / cannot act
    /// </summary>
    public class CombatDamageFeedbackTests
    {
        private Combatant CreateCombatant(GameObject go, string id, SubFactionId faction)
        {
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            return entity.Combatant;
        }

        [Test]
        public void TryLightAttack_DoesNotImmediatelyReduceHP()
        {
            var atkGo = new GameObject("Atk");
            var defGo = new GameObject("Def");
            try
            {
                var atk = CreateCombatant(atkGo, "atk", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "def", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;

                var hpBefore = def.CurrentHealth;
                Assert.IsTrue(atk.TryLightAttack(def));
                Assert.AreEqual(CombatState.AttackWindup, atk.State);
                Assert.AreEqual(hpBefore, def.CurrentHealth, "HP must not drop during windup");
            }
            finally
            {
                Object.DestroyImmediate(atkGo);
                Object.DestroyImmediate(defGo);
            }
        }

        [Test]
        public void HP_DropsOnlyAfterEnteringActiveWindow()
        {
            var atkGo = new GameObject("Atk");
            var defGo = new GameObject("Def");
            try
            {
                var atk = CreateCombatant(atkGo, "atk", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "def", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;

                var hpBefore = def.CurrentHealth;
                atk.TryLightAttack(def);
                atk.Tick(atk.AttackWindup + 0.001f);
                Assert.AreEqual(CombatState.Attacking, atk.State);
                Assert.Less(def.CurrentHealth, hpBefore, "HP must drop on entering active window");
            }
            finally
            {
                Object.DestroyImmediate(atkGo);
                Object.DestroyImmediate(defGo);
            }
        }

        [Test]
        public void OnHitLanded_FiresOnceWithDamageResult()
        {
            var atkGo = new GameObject("Atk");
            var defGo = new GameObject("Def");
            try
            {
                var atk = CreateCombatant(atkGo, "atk", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "def", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;

                int landedCount = 0;
                float lastDmg = 0f;
                atk.OnHitLanded += e => { landedCount++; lastDmg = e.Result.finalDamage; };

                atk.TryLightAttack(def);
                Assert.AreEqual(0, landedCount, "Not yet (still windup)");

                atk.Tick(atk.AttackWindup + 0.001f);
                Assert.AreEqual(1, landedCount, "One hit on entering active window");
                Assert.Greater(lastDmg, 0f);

                // Subsequent ticks during active window must not re-fire.
                atk.Tick(0.05f);
                atk.Tick(0.05f);
                Assert.AreEqual(1, landedCount, "No double-hit within same attack");
            }
            finally
            {
                Object.DestroyImmediate(atkGo);
                Object.DestroyImmediate(defGo);
            }
        }

        [Test]
        public void OutOfRangeAttack_FiresMissEvent_NoHPLoss()
        {
            var atkGo = new GameObject("Atk");
            var defGo = new GameObject("Def");
            try
            {
                var atk = CreateCombatant(atkGo, "atk", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "def", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;
                defGo.transform.position = new Vector3(50f, 0f, 0f);

                int landedCount = 0;
                float damage = -1f;
                atk.OnHitLanded += e => { landedCount++; damage = e.Result.finalDamage; };

                var hpBefore = def.CurrentHealth;
                atk.TryLightAttack(def);
                atk.Tick(atk.AttackWindup + 0.001f);

                Assert.AreEqual(1, landedCount);
                Assert.AreEqual(0f, damage, "Miss => zero damage");
                Assert.AreEqual(hpBefore, def.CurrentHealth, "Out-of-range must not change HP");
            }
            finally
            {
                Object.DestroyImmediate(atkGo);
                Object.DestroyImmediate(defGo);
            }
        }

        [Test]
        public void DeadUnit_FiresOnDeath_AndCannotAct()
        {
            var atkGo = new GameObject("Atk");
            var defGo = new GameObject("Def");
            try
            {
                var atk = CreateCombatant(atkGo, "atk", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "def", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;

                int deathFired = 0;
                def.OnDeath += _ => deathFired++;

                def.ApplyHealthDamage(99999f);
                Assert.AreEqual(1, deathFired);
                Assert.IsFalse(def.IsAlive);
                Assert.AreEqual(CombatState.Dead, def.State);

                // Dead unit cannot dodge or attack.
                Assert.IsFalse(def.TryDodge(Vector3.forward));
                Assert.IsFalse(def.TryLightAttack(atk));
            }
            finally
            {
                Object.DestroyImmediate(atkGo);
                Object.DestroyImmediate(defGo);
            }
        }
    }
}

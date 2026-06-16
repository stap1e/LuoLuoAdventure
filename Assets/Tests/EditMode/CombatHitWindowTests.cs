using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CombatHitWindowTests
    {
        private Combatant CreateCombatant(GameObject go, string id, SubFactionId faction)
        {
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            return entity.Combatant;
        }

        [Test]
        public void NoDamage_DuringWindup()
        {
            var atkGo = new GameObject("A");
            var defGo = new GameObject("D");
            try
            {
                var atk = CreateCombatant(atkGo, "a", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "d", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false; def.AutoTickEnabled = false;

                int hits = 0;
                atk.OnHitLanded += _ => hits++;
                atk.TryLightAttack(def);
                atk.Tick(atk.AttackWindup * 0.5f);
                Assert.AreEqual(0, hits);
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }

        [Test]
        public void NoDoubleHit_WithinSameAttack()
        {
            var atkGo = new GameObject("A");
            var defGo = new GameObject("D");
            try
            {
                var atk = CreateCombatant(atkGo, "a", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "d", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false; def.AutoTickEnabled = false;

                int hits = 0;
                atk.OnHitLanded += _ => hits++;
                atk.TryLightAttack(def);
                atk.Tick(atk.AttackWindup + 0.001f);
                Assert.AreEqual(1, hits);

                for (int i = 0; i < 5; i++) atk.Tick(0.01f);
                Assert.AreEqual(1, hits);

                atk.AnimEvent_OnAttackActive();
                Assert.AreEqual(1, hits);
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }

        [Test]
        public void TwoSequentialAttacks_ProduceTwoHits()
        {
            var atkGo = new GameObject("A");
            var defGo = new GameObject("D");
            try
            {
                var atk = CreateCombatant(atkGo, "a", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "d", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false; def.AutoTickEnabled = false;

                int hits = 0;
                atk.OnHitLanded += _ => hits++;

                atk.TryLightAttack(def);
                atk.Tick(atk.AttackWindup + 0.001f);
                atk.Tick(atk.AttackActive + atk.AttackRecovery + 2f);
                Assert.AreEqual(CombatState.Idle, atk.State);

                atk.TryLightAttack(def);
                atk.Tick(atk.AttackWindup + 0.001f);
                Assert.AreEqual(2, hits);
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }

        [Test]
        public void DeadTarget_Mid_Windup_FiresMiss_NoCrash()
        {
            var atkGo = new GameObject("A");
            var defGo = new GameObject("D");
            try
            {
                var atk = CreateCombatant(atkGo, "a", SubFactionId.MotorIronRiders);
                var def = CreateCombatant(defGo, "d", SubFactionId.BeastIronClaw);
                atk.AutoTickEnabled = false; def.AutoTickEnabled = false;

                float lastDmg = -1f;
                atk.OnHitLanded += e => lastDmg = e.Result.finalDamage;

                atk.TryLightAttack(def);
                def.ApplyHealthDamage(99999f); // kill mid-windup
                atk.Tick(atk.AttackWindup + 0.001f);

                Assert.AreEqual(0f, lastDmg);
                Assert.IsFalse(def.IsAlive);
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }
    }
}

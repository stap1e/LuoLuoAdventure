using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIAttackWindowTests
    {
        private Combatant CreateAI(GameObject go, string id, SubFactionId faction)
        {
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            return entity.Combatant;
        }

        [Test]
        public void AI_DamageOnlyDuringActiveWindow()
        {
            var atkGo = new GameObject("AI");
            var defGo = new GameObject("Player");
            try
            {
                var atk = CreateAI(atkGo, "ai", SubFactionId.BeastIronClaw);
                var def = CreateAI(defGo, "p", SubFactionId.MotorIronRiders);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;

                var hpBefore = def.CurrentHealth;
                atk.TryLightAttack(def);
                Assert.AreEqual(CombatState.AttackWindup, atk.State);
                Assert.AreEqual(hpBefore, def.CurrentHealth, "No damage during windup");

                atk.Tick(atk.AttackWindup + 0.001f);
                Assert.AreEqual(CombatState.Attacking, atk.State);
                Assert.Less(def.CurrentHealth, hpBefore, "Damage on entering active window");
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }

        [Test]
        public void AI_CooldownPreventsSpam()
        {
            var atkGo = new GameObject("AI");
            var defGo = new GameObject("Player");
            try
            {
                var atk = CreateAI(atkGo, "ai", SubFactionId.BeastIronClaw);
                var def = CreateAI(defGo, "p", SubFactionId.MotorIronRiders);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;

                Assert.IsTrue(atk.TryLightAttack(def));
                Assert.IsFalse(atk.TryLightAttack(def), "Cooldown blocks second attack");

                // Tick through windup -> active -> recovery -> cooldown so attacker is Idle
                // AND attack cooldown timer has fully drained.
                CombatTimingTestHelper.AdvanceCombatThroughAttack(atk);
                Assert.AreEqual(CombatState.Idle, atk.State, "After full sequence attacker is Idle");
                Assert.AreEqual(0f, atk.AttackCooldownRemaining, 1e-3f, "Cooldown timer drained");
                Assert.IsTrue(atk.TryLightAttack(def), "Can attack after cooldown");
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }

        [Test]
        public void AI_MissEmitsZeroDamageEvent()
        {
            var atkGo = new GameObject("AI");
            var defGo = new GameObject("Player");
            try
            {
                var atk = CreateAI(atkGo, "ai", SubFactionId.BeastIronClaw);
                var def = CreateAI(defGo, "p", SubFactionId.MotorIronRiders);
                atk.AutoTickEnabled = false;
                def.AutoTickEnabled = false;
                defGo.transform.position = new Vector3(100f, 0f, 0f);

                float damage = -1f;
                atk.OnHitLanded += e => damage = e.Result.finalDamage;

                atk.TryLightAttack(def);
                atk.Tick(atk.AttackWindup + 0.001f);

                Assert.AreEqual(0f, damage, "Miss produces zero damage");
            }
            finally { Object.DestroyImmediate(atkGo); Object.DestroyImmediate(defGo); }
        }

        [Test]
        public void AI_UsesEnemyTiming_FromConfig()
        {
            var go = new GameObject("AI");
            try
            {
                var c = CreateAI(go, "ai", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;

                var config = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
                config.playerAttackWindup = 0.1f;
                config.enemyAttackWindup = 0.5f;
                config.enemyAttackActiveDuration = 0.3f;
                config.enemyAttackRecovery = 0.6f;
                config.playerAttackActive = 0.05f;
                config.playerAttackRecovery = 0.1f;
                config.dodgeDuration = 0.3f;
                config.dodgeDistance = 3f;
                config.dodgeInvulnerableDuration = 0.2f;
                config.staggerDuration = 1f;

                c.ApplyTuning(config);

                Assert.AreEqual(0.5f, c.AttackWindup, "AI uses enemy windup");
                Assert.AreEqual(0.3f, c.AttackActive, "AI uses enemy active");
                Assert.AreEqual(0.6f, c.AttackRecovery, "AI uses enemy recovery");
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

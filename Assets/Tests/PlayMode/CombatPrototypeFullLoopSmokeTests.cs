using System.Collections;
using LuoLuoTrip;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Combat.Feedback;
using LuoLuoTrip.Feedback;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CombatPrototypeFullLoopSmokeTests
    {
        [UnityTest]
        public IEnumerator PlayerHitsEnemy_EnemyHPDrops_BarUpdates()
        {
            var atkGo = new GameObject("Player");
            var defGo = new GameObject("Enemy");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(defGo.transform, false);

            try
            {
                var atkE = atkGo.AddComponent<CharacterEntity>();
                atkE.Bind(new CharacterData("p", "P", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                atkGo.AddComponent<CombatController>();
                var defE = defGo.AddComponent<CharacterEntity>();
                defE.Bind(new CharacterData("e", "E", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));

                var atk = atkE.Combatant;
                var def = defE.Combatant;
                defGo.AddComponent<CombatantHealthBarPresenter>();
                defGo.AddComponent<HitFlashFeedback>();
                atkGo.AddComponent<HitFlashFeedback>();
                CombatFeedbackBroadcaster.EnsureInstance();
                DamageNumberFeedback.EnsureInstance();

                yield return null;

                var hpBefore = def.CurrentHealth;
                atk.TryLightAttack(def);

                float t = 0f;
                while (atk.State != CombatState.Attacking && t < 1f)
                {
                    yield return null;
                    t += Time.deltaTime;
                }
                yield return null;
                yield return null;

                Assert.Less(def.CurrentHealth, hpBefore, "Enemy HP must drop");
                var presenter = defGo.GetComponent<CombatantHealthBarPresenter>();
                Assert.That(presenter.Bar.Ratio, Is.LessThan(1f), "Bar ratio must update");
                Assert.That(DamageNumberFeedback.Instance.ActiveCount, Is.GreaterThan(0), "Damage number queued");
            }
            finally
            {
                Object.DestroyImmediate(atkGo);
                Object.DestroyImmediate(defGo);
                if (DamageNumberFeedback.Instance != null)
                    Object.DestroyImmediate(DamageNumberFeedback.Instance.gameObject);
                if (CombatFeedbackBroadcaster.Instance != null)
                    Object.DestroyImmediate(CombatFeedbackBroadcaster.Instance.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator EnemyHitsPlayer_PlayerHPDrops_FeedbackVisible()
        {
            var atkGo = new GameObject("Enemy");
            var defGo = new GameObject("Player");
            var defVisual = new GameObject("Visual");
            defVisual.transform.SetParent(defGo.transform, false);

            try
            {
                var atkE = atkGo.AddComponent<CharacterEntity>();
                atkE.Bind(new CharacterData("e", "E", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var defE = defGo.AddComponent<CharacterEntity>();
                defE.Bind(new CharacterData("p", "P", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                defGo.AddComponent<CombatController>();

                var atk = atkE.Combatant;
                var def = defE.Combatant;
                defGo.AddComponent<HitFlashFeedback>();
                CombatFeedbackBroadcaster.EnsureInstance();
                DamageNumberFeedback.EnsureInstance();

                yield return null;

                var hpBefore = def.CurrentHealth;
                atk.TryLightAttack(def);

                float t = 0f;
                while (atk.State != CombatState.Attacking && t < 1f)
                {
                    yield return null;
                    t += Time.deltaTime;
                }
                yield return null;
                yield return null;

                Assert.Less(def.CurrentHealth, hpBefore, "Player HP must drop when hit by enemy");
                Assert.That(DamageNumberFeedback.Instance.ActiveCount, Is.GreaterThan(0), "Damage number on player hit");
            }
            finally
            {
                Object.DestroyImmediate(atkGo);
                Object.DestroyImmediate(defGo);
                if (DamageNumberFeedback.Instance != null)
                    Object.DestroyImmediate(DamageNumberFeedback.Instance.gameObject);
                if (CombatFeedbackBroadcaster.Instance != null)
                    Object.DestroyImmediate(CombatFeedbackBroadcaster.Instance.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator EnemyDeath_StopsAI_FiresDeathFeedback()
        {
            var defGo = new GameObject("Enemy");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(defGo.transform, false);

            try
            {
                var defE = defGo.AddComponent<CharacterEntity>();
                defE.Bind(new CharacterData("e", "E", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var def = defE.Combatant;
                defGo.AddComponent<HitFlashFeedback>();
                defGo.AddComponent<SimpleCombatAI>();
                CombatFeedbackBroadcaster.EnsureInstance();
                DamageNumberFeedback.EnsureInstance();

                yield return null;

                int deathCount = 0;
                def.OnDeath += _ => deathCount++;
                def.ApplyHealthDamage(99999f);

                yield return null;
                Assert.AreEqual(1, deathCount);
                Assert.IsFalse(def.IsAlive);
                Assert.AreEqual(CombatState.Dead, def.State);
            }
            finally
            {
                Object.DestroyImmediate(defGo);
                if (DamageNumberFeedback.Instance != null)
                    Object.DestroyImmediate(DamageNumberFeedback.Instance.gameObject);
                if (CombatFeedbackBroadcaster.Instance != null)
                    Object.DestroyImmediate(CombatFeedbackBroadcaster.Instance.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator DebugController_F2_ResetsHP()
        {
            var playerGo = new GameObject("Player");
            var debugGo = new GameObject("Debug");
            try
            {
                var entity = playerGo.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("p", "P", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                playerGo.AddComponent<CombatController>();
                var player = entity.Combatant;

                var debug = debugGo.AddComponent<CombatPrototypeDebugController>();

                yield return null;

                player.ApplyHealthDamage(30f);
                yield return null;
                Assert.Less(player.CurrentHealth, player.Stats.maxHealth);

                debug.SendMessage("ResetAllHP");
                yield return null;

                Assert.AreEqual(player.Stats.maxHealth, player.CurrentHealth, 0.1f, "F2 must reset HP");
            }
            finally
            {
                Object.DestroyImmediate(playerGo);
                Object.DestroyImmediate(debugGo);
            }
        }
    }
}

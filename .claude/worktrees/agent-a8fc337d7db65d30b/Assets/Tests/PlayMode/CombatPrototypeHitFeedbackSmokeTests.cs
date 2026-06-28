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
    public class CombatPrototypeHitFeedbackSmokeTests
    {
        [UnityTest]
        public IEnumerator PlayerAttack_DropsEnemyHP_AndUpdatesBar()
        {
            var atkGo = new GameObject("Atk");
            var defGo = new GameObject("Def");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(defGo.transform, false);

            try
            {
                var atkE = atkGo.AddComponent<CharacterEntity>();
                atkE.Bind(new CharacterData("a", "A", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var defE = defGo.AddComponent<CharacterEntity>();
                defE.Bind(new CharacterData("d", "D", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));

                var atk = atkE.Combatant;
                var def = defE.Combatant;
                defGo.AddComponent<CombatantHealthBarPresenter>();
                defGo.AddComponent<HitFlashFeedback>();
                CombatFeedbackBroadcaster.EnsureInstance();
                DamageNumberFeedback.EnsureInstance();

                yield return null;

                var hpBefore = def.CurrentHealth;
                Assert.IsTrue(atk.TryLightAttack(def));

                // Wait for attack to enter active window naturally
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
                Assert.That(presenter.Bar.Ratio, Is.LessThan(1f));

                Assert.That(DamageNumberFeedback.Instance.ActiveCount, Is.GreaterThan(0));
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
        public IEnumerator KillingEnemy_StopsAI_FiresDeathFeedback()
        {
            var defGo = new GameObject("Enemy");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(defGo.transform, false);

            try
            {
                var defE = defGo.AddComponent<CharacterEntity>();
                defE.Bind(new CharacterData("d", "D", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var def = defE.Combatant;
                defGo.AddComponent<HitFlashFeedback>();
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
    }
}

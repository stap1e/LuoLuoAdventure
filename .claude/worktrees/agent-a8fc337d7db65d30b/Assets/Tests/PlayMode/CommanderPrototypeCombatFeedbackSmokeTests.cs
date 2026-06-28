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
    public class CommanderPrototypeCombatFeedbackSmokeTests
    {
        [UnityTest]
        public IEnumerator HostileTakesDamage_AndDisplaysDamageNumber()
        {
            var atkGo = new GameObject("Cmdr");
            var defGo = new GameObject("Hostile");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(defGo.transform, false);

            try
            {
                var atkE = atkGo.AddComponent<CharacterEntity>();
                atkE.Bind(new CharacterData("c", "Cmdr", SubFactionId.MotorIronRiders, CharacterRole.Common, 8));
                var defE = defGo.AddComponent<CharacterEntity>();
                defE.Bind(new CharacterData("h", "Hostile", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));

                var atk = atkE.Combatant;
                var def = defE.Combatant;
                defGo.AddComponent<CombatantHealthBarPresenter>();
                defGo.AddComponent<HitFlashFeedback>();
                CombatFeedbackBroadcaster.EnsureInstance();
                DamageNumberFeedback.EnsureInstance();

                yield return null;

                var hpBefore = def.CurrentHealth;
                atk.TryLightAttack(def);

                float t = 0f;
                while (atk.State != CombatState.Attacking && t < 1f)
                {
                    yield return null; t += Time.deltaTime;
                }
                yield return null;
                yield return null;

                Assert.Less(def.CurrentHealth, hpBefore);
                Assert.That(DamageNumberFeedback.Instance.ActiveCount, Is.GreaterThan(0));

                var presenter = defGo.GetComponent<CombatantHealthBarPresenter>();
                Assert.IsNotNull(presenter);
                Assert.That(presenter.Bar.Ratio, Is.LessThan(1f));
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
    }
}

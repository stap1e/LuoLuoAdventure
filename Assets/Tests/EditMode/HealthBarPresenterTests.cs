using LuoLuoTrip;
using LuoLuoTrip.Combat;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class HealthBarPresenterTests
    {
        private Combatant CreateCombatant(GameObject go, string id, SubFactionId faction)
        {
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            return entity.Combatant;
        }

        [Test]
        public void Presenter_AutoCreatesWorldHealthBar()
        {
            var go = new GameObject("U");
            try
            {
                CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                var p = go.AddComponent<CombatantHealthBarPresenter>();
                Assert.IsNotNull(p.Bar);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Presenter_PercentReflectsHP()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;
                var p = go.AddComponent<CombatantHealthBarPresenter>();
                p.Invoke("Awake", 0f); // ensure binding (Awake already ran)
                p.SendMessage("LateUpdate");
                Assert.AreEqual(1f, p.Bar.Ratio, 0.01f);

                c.ApplyHealthDamage(c.Stats.maxHealth * 0.5f);
                p.SendMessage("LateUpdate");
                Assert.That(p.Bar.Ratio, Is.LessThan(0.6f));
                Assert.That(p.Bar.Ratio, Is.GreaterThan(0.4f));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Presenter_MarksDeadOnZero()
        {
            var go = new GameObject("U");
            try
            {
                var c = CreateCombatant(go, "u", SubFactionId.BeastIronClaw);
                c.AutoTickEnabled = false;
                var p = go.AddComponent<CombatantHealthBarPresenter>();
                c.ApplyHealthDamage(99999f);
                p.SendMessage("LateUpdate");
                Assert.IsTrue(p.Bar.IsDead);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Presenter_MissingCombatant_DoesNotCrash()
        {
            var go = new GameObject("Empty");
            try
            {
                var p = go.AddComponent<CombatantHealthBarPresenter>();
                Assert.DoesNotThrow(() => p.SendMessage("LateUpdate"));
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

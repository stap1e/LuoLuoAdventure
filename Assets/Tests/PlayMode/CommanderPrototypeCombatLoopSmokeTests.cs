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
    public class CommanderPrototypeCombatLoopSmokeTests
    {
        [UnityTest]
        public IEnumerator DirectControlUnit_CanFightHostile()
        {
            var atkGo = new GameObject("Cmdr");
            var defGo = new GameObject("Hostile");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(defGo.transform, false);

            try
            {
                var atkE = atkGo.AddComponent<CharacterEntity>();
                atkE.Bind(new CharacterData("c", "Cmdr", SubFactionId.MotorIronRiders, CharacterRole.Common, 8));
                atkGo.AddComponent<CombatController>();
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
                    yield return null;
                    t += Time.deltaTime;
                }
                yield return null;
                yield return null;

                Assert.Less(def.CurrentHealth, hpBefore, "Hostile HP must drop");
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
        public IEnumerator HostileDeath_UpdatesEncounterCasualty()
        {
            var defGo = new GameObject("Hostile");
            var encounterGo = new GameObject("Encounter");
            try
            {
                var defE = defGo.AddComponent<CharacterEntity>();
                defE.Bind(new CharacterData("h", "Hostile", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));
                var def = defE.Combatant;

                var encounter = encounterGo.AddComponent<EncounterRuntime>();
                encounter.RegisterUnit(defE);

                yield return null;

                Assert.AreEqual(0, encounter.CountCasualties(SubFactionId.BeastIronClaw));

                def.ApplyHealthDamage(99999f);
                yield return null;

                Assert.AreEqual(1, encounter.CountCasualties(SubFactionId.BeastIronClaw),
                    "Dead unit must be counted as casualty");
            }
            finally
            {
                Object.DestroyImmediate(defGo);
                Object.DestroyImmediate(encounterGo);
            }
        }

        [UnityTest]
        public IEnumerator EnemyTiming_DifferentFromPlayer()
        {
            var playerGo = new GameObject("Player");
            var enemyGo = new GameObject("Enemy");
            try
            {
                var pE = playerGo.AddComponent<CharacterEntity>();
                pE.Bind(new CharacterData("p", "P", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                playerGo.AddComponent<CombatController>();
                var eE = enemyGo.AddComponent<CharacterEntity>();
                eE.Bind(new CharacterData("e", "E", SubFactionId.BeastIronClaw, CharacterRole.Common, 5));

                var player = pE.Combatant;
                var enemy = eE.Combatant;

                yield return null;

                // After Start, both have applied tuning. Player uses player timing, enemy uses enemy timing.
                // Default config: player windup=0.25, enemy windup=0.35
                Assert.AreEqual(0.25f, player.AttackWindup, 0.01f, "Player uses player windup");
                Assert.AreEqual(0.35f, enemy.AttackWindup, 0.01f, "Enemy uses enemy windup");
            }
            finally
            {
                Object.DestroyImmediate(playerGo);
                Object.DestroyImmediate(enemyGo);
            }
        }
    }
}

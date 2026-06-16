using System.Collections;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CombatFeelSmokeTests
    {
        [UnityTest]
        public IEnumerator AttackWindup_BlocksActions()
        {
            var go = new GameObject("Attacker");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("attacker", "Attacker", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var combatant = go.AddComponent<Combatant>();
                combatant.AutoTickEnabled = false;

                combatant.TryLightAttack(null);
                Assert.That(combatant.State, Is.EqualTo(CombatState.AttackWindup));
                Assert.That(combatant.TryDodge(Vector3.forward), Is.False);

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator Stagger_BlocksAllActions()
        {
            var go = new GameObject("Target");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("target", "Target", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var combatant = go.AddComponent<Combatant>();
                combatant.AutoTickEnabled = false;

                combatant.ApplyPoiseDamage(combatant.Stats.maxPoise + 100f);
                Assert.That(combatant.State, Is.EqualTo(CombatState.Staggered));
                Assert.That(combatant.TryLightAttack(null), Is.False);
                Assert.That(combatant.TryDodge(Vector3.forward), Is.False);

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator Dodge_MovesCharacter()
        {
            var go = new GameObject("Dodger");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("dodger", "Dodger", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var combatant = go.AddComponent<Combatant>();
                combatant.AutoTickEnabled = false;

                var startPos = go.transform.position;
                combatant.TryDodge(Vector3.forward);

                combatant.Tick(0.35f);
                Assert.That(go.transform.position.z, Is.GreaterThan(startPos.z));

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator CombatTuningConfig_AppliesToCombatant()
        {
            var go = new GameObject("TunedCombatant");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(new CharacterData("tuned", "Tuned", SubFactionId.MotorIronRiders, CharacterRole.Common, 5));
                var combatant = go.AddComponent<Combatant>();

                var config = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
                config.playerAttackWindup = 0.5f;
                config.playerAttackActive = 0.4f;
                config.playerAttackRecovery = 0.6f;

                combatant.ApplyTuning(config);
                Assert.That(combatant.AttackWindup, Is.EqualTo(0.5f));
                Assert.That(combatant.AttackRecovery, Is.EqualTo(0.6f));

                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

using System.Collections;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeAIProfileSmokeTests
    {
        [UnityTest]
        public IEnumerator CommanderPrototype_HasRequiredAIProfileDefinitions()
        {
            foreach (AIBehaviorProfileType type in System.Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                try
                {
                    AIBehaviorProfileSO.ConfigureDefaults(profile, type);
                    Assert.That(profile.Validate(out var error), Is.True, error);
                }
                finally { Object.DestroyImmediate(profile); }
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator CityGateUnits_HaveCorrectProfileLabels()
        {
            var guard = Create("MechaGateGuard", AIBehaviorProfileType.DefensiveGuard);
            var raider = Create("BeastRaider_01", AIBehaviorProfileType.AggressiveRaider);
            var negotiator = Create("BeastNegotiator", AIBehaviorProfileType.Negotiator);
            try
            {
                yield return null;

                Assert.That(guard.GetComponent<SimpleCombatAI>().CurrentBehaviorLabel, Does.Contain("Defensive"));
                Assert.That(raider.GetComponent<SimpleCombatAI>().CurrentBehaviorLabel, Does.Contain("Aggressive"));
                Assert.That(negotiator.GetComponent<SimpleCombatAI>().CurrentBehaviorLabel, Does.Contain("Non-combatant"));
            }
            finally
            {
                Object.DestroyImmediate(guard);
                Object.DestroyImmediate(raider);
                Object.DestroyImmediate(negotiator);
            }
        }

        [UnityTest]
        public IEnumerator Hud_CanDisplaySelectedTargetProfile()
        {
            var go = Create("MechaGateGuard", AIBehaviorProfileType.DefensiveGuard);
            try
            {
                yield return null;
                var ai = go.GetComponent<SimpleCombatAI>();
                Assert.That(ai.CurrentBehaviorLabel, Is.EqualTo("Guard: Defensive"));
                Assert.That(ai.LastProfileDecision, Is.Not.Empty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        private static GameObject Create(string name, AIBehaviorProfileType type)
        {
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            AIBehaviorProfileSO.ConfigureDefaults(profile, type);
            var go = new GameObject(name);
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create(name.ToLowerInvariant(), name, SubFactionId.MotorIronRiders, CharacterRole.Minion));
            var ai = go.AddComponent<SimpleCombatAI>();
            ai.BehaviorProfile = profile;
            return go;
        }
    }
}

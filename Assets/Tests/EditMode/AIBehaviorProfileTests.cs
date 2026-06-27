using LuoLuoTrip.AI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIBehaviorProfileTests
    {
        [Test]
        public void DefaultProfiles_Validate()
        {
            foreach (AIBehaviorProfileType type in System.Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                try
                {
                    AIBehaviorProfileSO.ConfigureDefaults(profile, type);
                    Assert.That(profile.Validate(out var error), Is.True, $"{type}: {error}");
                    Assert.That(profile.profileId, Is.Not.Empty);
                    Assert.That(profile.DisplayLabel, Is.Not.Empty);
                }
                finally { Object.DestroyImmediate(profile); }
            }
        }

        [Test]
        public void AggressiveRaider_CanInitiateAndPrefersObjectives()
        {
            var profile = Create(AIBehaviorProfileType.AggressiveRaider);
            try
            {
                Assert.That(profile.canInitiateCombat, Is.True);
                Assert.That(profile.prefersObjectiveTargets, Is.True);
                Assert.That(profile.maxChaseDistanceFromHome, Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void DefensiveGuard_HasLimitedChaseAndDefendResponse()
        {
            var profile = Create(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                Assert.That(profile.respondsToDefendObjective, Is.True);
                Assert.That(profile.holdPositionWhenNoTarget, Is.True);
                Assert.That(profile.maxChaseDistanceFromHome, Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void Negotiator_CannotInitiateCombat()
        {
            var profile = Create(AIBehaviorProfileType.Negotiator);
            try
            {
                Assert.That(profile.canInitiateCombat, Is.False);
                Assert.That(profile.respondsToFocusFire, Is.False);
                Assert.That(profile.canRetreat, Is.True);
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void Hardliner_CanTargetProtectedUnit()
        {
            var profile = Create(AIBehaviorProfileType.Hardliner);
            try
            {
                Assert.That(profile.canAttackNeutral, Is.True);
                Assert.That(profile.prefersProtectedTargets, Is.True);
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void CommanderUnit_RespondsToTacticalCommands()
        {
            var profile = Create(AIBehaviorProfileType.CommanderUnit);
            try
            {
                Assert.That(profile.respondsToTacticalCommand, Is.True);
                Assert.That(profile.respondsToDefendObjective, Is.True);
                Assert.That(profile.respondsToFocusFire, Is.True);
            }
            finally { Object.DestroyImmediate(profile); }
        }

        private static AIBehaviorProfileSO Create(AIBehaviorProfileType type)
        {
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            AIBehaviorProfileSO.ConfigureDefaults(profile, type);
            return profile;
        }
    }
}

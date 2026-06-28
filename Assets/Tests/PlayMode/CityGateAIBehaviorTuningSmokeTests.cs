using System.Collections;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CityGateAIBehaviorTuningSmokeTests
    {
        [UnityTest]
        public IEnumerator BeastNegotiator_DoesNotInitiateCombatDuringObservationWindow()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.Negotiator);
            try
            {
                fixture.AI.CombatantQuery = () => new[] { fixture.SelfCombatant, fixture.EnemyCombatant };
                yield return null;
                fixture.AI.ForceRefreshTargetForTests();
                yield return null;

                Assert.That(fixture.AI.CanInitiateCombat, Is.False);
                Assert.That(fixture.AI.CurrentTarget, Is.Null);
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator BeastRaider_SelectsObjectiveOrProtectedTarget()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.AggressiveRaider);
            try
            {
                fixture.AI.ProtectedTarget = fixture.Objective.transform;
                fixture.AI.CombatantQuery = () => new[] { fixture.SelfCombatant, fixture.EnemyCombatant, fixture.ObjectiveCombatant };
                yield return null;
                fixture.AI.ForceRefreshTargetForTests();

                Assert.That(fixture.AI.BehaviorProfile.objectivePressureWeight, Is.GreaterThan(fixture.AI.BehaviorProfile.hostileUnitWeight));
                Assert.That(fixture.AI.LastProfileDecision, Does.Contain("Objective").Or.Contain("Protected"));
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator MechaGateGuard_RemainsWithinLeashOrReturns()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                fixture.AI.SetDefendObjective(fixture.Objective.transform, 5f);
                yield return null;

                Assert.That(fixture.AI.IsDefending, Is.True);
                Assert.That(fixture.AI.DefendLeashRadius, Is.GreaterThan(0f));
                Assert.That(fixture.AI.BehaviorProfile.maxChaseDistanceFromHome, Is.LessThan(10f));
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator MechaHardliner_ReportsEscalationIntentForNegotiatorTarget()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.Hardliner);
            try
            {
                var score = AITargetSelectionUtility.ScoreTarget(fixture.SelfCombatant, fixture.ObjectiveCombatant,
                    fixture.AI.BehaviorProfile, null, fixture.Objective.transform, fixture.Self.transform.position, 20f,
                    isHostile: false, isProtectedTarget: true, isNeutralTarget: true);
                yield return null;

                Assert.That(score.IsValid, Is.True);
                Assert.That(score.Reason, Does.Contain("Escalation").Or.Contain("Protected"));
                Assert.That(fixture.AI.BehaviorProfile.maxEngageDuration, Is.GreaterThan(0f));
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator ScenarioMonitor_SummarizesCityGateUnitsWithoutExceptions()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.CommanderUnit);
            var monitorGo = new GameObject("AIBehaviorScenarioMonitor");
            try
            {
                var monitor = monitorGo.AddComponent<AIBehaviorScenarioMonitor>();
                monitor.Register(fixture.AI);
                yield return null;

                var summary = monitor.BuildScenarioSummary(refresh: false);
                Assert.That(summary, Does.Contain("CommanderUnit"));
            }
            finally
            {
                Object.Destroy(monitorGo);
                fixture.Dispose();
            }
        }
    }
}

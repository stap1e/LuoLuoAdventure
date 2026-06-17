using System.Collections;
using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CityGateBalancedMediationSmokeTests
    {
        [UnityTest]
        public IEnumerator BalancedMediation_WhenCoreAndNegotiatorAlive_AndLowCasualties()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true,
                negotiatorSurvived: true,
                raidersDefeated: true,
                mechaCasualties: 1,
                beastCasualties: 2,
                maxMechaForBalanced: 2,
                maxBeastForBalanced: 4,
                maxTotalForPartial: 8);

            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.BalancedMediation));
            yield return null;
        }

        [UnityTest]
        public IEnumerator MechaSuppression_WhenNegotiatorDead_RaidersDefeated()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true,
                negotiatorSurvived: false,
                raidersDefeated: true,
                mechaCasualties: 3,
                beastCasualties: 5,
                maxMechaForBalanced: 2,
                maxBeastForBalanced: 4,
                maxTotalForPartial: 8);

            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.MechaSuppression));
            yield return null;
        }

        [UnityTest]
        public IEnumerator FailedEscalation_WhenCoreDestroyed()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: false,
                negotiatorSurvived: true,
                raidersDefeated: false,
                mechaCasualties: 5,
                beastCasualties: 5,
                maxMechaForBalanced: 2,
                maxBeastForBalanced: 4,
                maxTotalForPartial: 8);

            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.FailedEscalation));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PartialContainment_WhenCoreSaved_ButHighCasualties()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true,
                negotiatorSurvived: true,
                raidersDefeated: true,
                mechaCasualties: 5,
                beastCasualties: 5,
                maxMechaForBalanced: 2,
                maxBeastForBalanced: 4,
                maxTotalForPartial: 12);

            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.PartialContainment));
            yield return null;
        }

        [UnityTest]
        public IEnumerator BeastNegotiation_WhenNegotiatorAlive_AndLowBeastCasualties()
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(
                coreSurvived: true,
                negotiatorSurvived: true,
                raidersDefeated: false,
                mechaCasualties: 1,
                beastCasualties: 2,
                maxMechaForBalanced: 2,
                maxBeastForBalanced: 4,
                maxTotalForPartial: 8);

            Assert.That(outcome, Is.EqualTo(MissionOutcomeType.BeastNegotiation));
            yield return null;
        }
    }
}

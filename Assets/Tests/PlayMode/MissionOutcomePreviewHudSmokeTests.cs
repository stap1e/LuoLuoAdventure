using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class MissionOutcomePreviewHudSmokeTests
    {
        [UnityTest]
        public IEnumerator MissionOutcomePreviewHud_NoActiveMissionSafe()
        {
            var go = new GameObject("MissionOutcomePreviewHudSmoke");
            try
            {
                var hud = go.AddComponent<MissionOutcomePreviewHud>();
                hud.SetPreviewService(new MissionOutcomePreviewService());
                hud.RefreshNow();
                yield return null;

                Assert.That(hud.CachedPreview, Is.Not.Null);
                var lines = new List<string>();
                MissionOutcomePreviewHud.BuildDisplayLines(hud.CachedPreview, lines);
                Assert.That(lines.Count, Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [UnityTest]
        public IEnumerator CityGatePreviewText_ShowsLikelyOutcomeAndSuggestedAction()
        {
            yield return null;
            var preview = new MissionOutcomePreviewService()
                .BuildCityGatePreview(true, true, false, 0, 0);
            var lines = new List<string>();

            MissionOutcomePreviewHud.BuildDisplayLines(preview, lines, 3);
            var text = string.Join("\n", lines);

            Assert.That(text, Does.Contain("Likely Outcome"));
            Assert.That(text, Does.Contain("BeastNegotiation"));
            Assert.That(text, Does.Contain("F FocusFire"));
        }

        [UnityTest]
        public IEnumerator Preview_DoesNotCompleteMissionOrWriteChain()
        {
            var context = new LuoLuoTripGameContext();
            context.MissionChainService.RecordMissionResult(DemoFlowManager.ConvoyMissionId, MissionOutcomeType.MechaVictory, 200);
            var countBefore = context.MissionChainService.State.CompletedMissions.Count;
            var xpBefore = context.CommanderProfile.Experience;

            yield return null;
            var preview = new MissionOutcomePreviewService().BuildPreview(DemoFlowManager.BorderMissionId, context);

            Assert.That(preview, Is.Not.Null);
            Assert.That(context.MissionChainService.State.CompletedMissions.Count, Is.EqualTo(countBefore));
            Assert.That(context.CommanderProfile.Experience, Is.EqualTo(xpBefore));
        }

        [UnityTest]
        public IEnumerator ConsequenceVisibility_ChainEffects_NoDuplicateEntries()
        {
            var context = new LuoLuoTripGameContext();
            var service = new MissionOutcomePreviewService();

            context.MissionChainService.RecordMissionResult(DemoFlowManager.ConvoyMissionId, MissionOutcomeType.BalancedResolution, 300);
            yield return null;
            var border = service.BuildPreview(DemoFlowManager.BorderMissionId, context);
            Assert.That(border.previousOutcomeEffect, Does.Contain("Border hostility reduced"));

            context.MissionChainService.RecordMissionResult(DemoFlowManager.BorderMissionId, MissionOutcomeType.BalancedResolution, 300);
            yield return null;
            var cityGate = service.BuildPreview(DemoFlowManager.CityGateMissionId, context);
            Assert.That(cityGate.previousOutcomeEffect, Does.Contain("CityGate mainstream hostility reduced"));
            Assert.That(context.MissionChainService.State.CompletedMissions.Select(e => e.MissionId).Distinct().Count(), Is.EqualTo(context.MissionChainService.State.CompletedMissions.Count));
        }
    }
}

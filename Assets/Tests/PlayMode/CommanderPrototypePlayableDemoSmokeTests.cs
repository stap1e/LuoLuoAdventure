using System.Collections;
using System.Linq;
using LuoLuoTrip.Combat;
using LuoLuoTrip.Feedback;
using LuoLuoTrip.Save;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypePlayableDemoSmokeTests
    {
        [UnityTest]
        public IEnumerator RuntimeComponents_CanExistTogetherForPlayableDemo()
        {
            var root = new GameObject("PlayableDemoSmoke");
            try
            {
                root.AddComponent<DemoFlowManager>();
                root.AddComponent<DemoFlowHud>();
                root.AddComponent<MissionObjectiveHud>();
                root.AddComponent<CommanderControlHintPanel>();
                root.AddComponent<MissionResultSummaryPanel>();
                root.AddComponent<CommanderDebugHud>();
                root.AddComponent<WorldMarkerService>();
                yield return null;

                Assert.That(Object.FindObjectOfType<DemoFlowHud>(), Is.Not.Null);
                Assert.That(Object.FindObjectOfType<MissionObjectiveHud>(), Is.Not.Null);
                Assert.That(Object.FindObjectOfType<CommanderControlHintPanel>(), Is.Not.Null);
                Assert.That(Object.FindObjectOfType<MissionResultSummaryPanel>(), Is.Not.Null);
                Assert.That(Object.FindObjectOfType<CommanderDebugHud>(), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator RequiredMissionMarkerLabels_RegisterWithWorldMarkerService()
        {
            var serviceGo = new GameObject("[WorldMarkerService]");
            var markerHosts = new GameObject("MarkerHosts");
            try
            {
                var service = serviceGo.AddComponent<WorldMarkerService>();
                var requiredObjects = new[]
                {
                    "Convoy_Objective", "Energy_Node", "Area_BorderRetaliation", "BorderSpawnPoint_Beast",
                    "Border_ObjectiveMarker", "Area_CityGateDispute", "CityGateCore_Objective",
                    "BeastNegotiator", "CityGateSpawnPoint_Beast", "MechaGateGuard", "MechaHardliner"
                };

                foreach (var name in requiredObjects)
                {
                    var go = new GameObject(name);
                    go.transform.SetParent(markerHosts.transform);
                    var marker = go.AddComponent<WorldMarker>();
                    marker.Configure(WorldMarker.InferType(name), go.transform, WorldMarker.BuildReadableLabel(name));
                }

                yield return null;

                var labels = service.Markers.Select(m => m.CustomLabel).Where(label => !string.IsNullOrEmpty(label)).ToList();
                foreach (var expected in new[] { "Convoy", "Energy Node", "Border Retaliation Area", "Raider Spawn", "Allied Defense Point", "City Gate Mission Area", "CityGateCore", "BeastNegotiator", "BeastRaider Spawn", "Low-Rank Ally: Press E to Control", "High-Rank Unit: Tactical Command Only" })
                    Assert.That(labels.Any(label => label.Contains(expected)), Is.True, $"Missing marker label {expected}");
            }
            finally
            {
                Object.DestroyImmediate(markerHosts);
                Object.DestroyImmediate(serviceGo);
            }
        }
    }

    public class CommanderPrototypeManualControlSmokeTests
    {
        [UnityTest]
        public IEnumerator ManualControlText_CoversAllowedDeniedAndNoTarget()
        {
            var lowRank = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "MechaGateGuard",
                LastDirectControlAllowed = true,
                LastTacticalCommandAllowed = true,
                LastSyncAssistAllowed = true,
                LastInputRoute = "SelectedTarget"
            };
            var lowRankText = string.Join(" | ", CommanderActionPresenter.BuildDescriptors(lowRank).ConvertAll(CommanderActionPresenter.BuildStatusLine));
            Assert.That(lowRankText, Does.Contain("DirectControl: Allowed"));

            var highRank = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "MechaHardliner",
                LastDirectControlAllowed = false,
                LastTacticalCommandAllowed = true,
                LastSyncAssistAllowed = true,
                LastControlRejectReason = "Direct control disabled",
                LastSuggestion = "Try Tactical Command or select a lower-rank unit.",
                LastInputRoute = "SelectedTarget"
            };
            var direct = CommanderActionPresenter.BuildDescriptors(highRank).Find(d => d.ActionType == CommanderActionType.DirectControl);
            Assert.That(direct.StatusText, Is.EqualTo("Denied"));
            Assert.That(direct.Suggestion, Does.Contain("Tactical Command"));

            var noTarget = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "None",
                LastControlRejectReason = "No controllable target nearby",
                LastSuggestion = "Press Tab/Q to select target or move closer to a low-rank unit.",
                LastInputRoute = "NoTarget"
            };
            var noTargetDescriptor = CommanderActionPresenter.BuildDescriptors(noTarget)[0];
            Assert.That(noTargetDescriptor.Suggestion, Does.Contain("Tab/Q"));
            Assert.That(noTarget.LastInputRoute, Is.EqualTo("NoTarget"));

            yield return null;
        }
    }

    public class CommanderPrototypeDemoShortcutSmokeTests
    {
        [UnityTest]
        public IEnumerator F8TeleportAndShortcutHandlers_AreCallable()
        {
            var player = new GameObject("Player");
            var trigger = new GameObject("CityGateDisputeTrigger");
            var debugGo = new GameObject("CommanderPrototypeDebug");
            var saveGo = new GameObject("SaveLoadManager");
            try
            {
                trigger.transform.position = new Vector3(50f, 0f, 0f);
                var entity = player.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("player", "Player", SubFactionId.MotorIronRiders, CharacterRole.Common));
                player.AddComponent<Combatant>();
                var controller = player.AddComponent<CombatController>();
                controller.SetInputEnabled(true);
                player.AddComponent<CharacterMovementMotor>();

                var debug = debugGo.AddComponent<PrototypeDebugController>();
                var save = saveGo.AddComponent<SaveLoadManager>();
                yield return null;

                debug.TeleportPlayerToCityGateDisputeArea();

                Assert.That(Vector3.Distance(player.transform.position, new Vector3(50f, 0.5f, -4f)), Is.LessThan(0.25f));
                Assert.That(typeof(PrototypeDebugController).GetMethod("TeleportPlayerToCityGateDisputeArea"), Is.Not.Null);
                Assert.That(typeof(SaveLoadManager).GetMethod("SaveGame"), Is.Not.Null);
                Assert.That(typeof(SaveLoadManager).GetMethod("LoadGame"), Is.Not.Null);
                Assert.That(typeof(SaveLoadManager).GetMethod("ClearSave"), Is.Not.Null);
                Assert.That(save, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(saveGo);
                Object.DestroyImmediate(debugGo);
                Object.DestroyImmediate(trigger);
                Object.DestroyImmediate(player);
            }
        }

        [UnityTest]
        public IEnumerator DebugCityGateOutcome_DoesNotPolluteRealChainIds()
        {
            var chain = new MissionChainService(new MissionChainState());
            chain.RecordMissionResult("test_citygate", MissionOutcomeType.BalancedMediation, 350);
            yield return null;

            Assert.That(chain.State.HasCompleted(DemoFlowManager.CityGateMissionId), Is.False);
            Assert.That(DemoFlowManager.ResolveStep(chain.State), Is.EqualTo(DemoFlowState.ConvoyAvailable));
        }
    }
}

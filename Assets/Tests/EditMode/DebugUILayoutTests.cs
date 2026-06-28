using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class DebugUILayoutTests
    {
        [Test]
        public void CommanderHud_HasValidPosition()
        {
            Assert.That(DebugUILayout.CommanderHud.x, Is.GreaterThanOrEqualTo(0f));
            Assert.That(DebugUILayout.CommanderHud.y, Is.GreaterThanOrEqualTo(0f));
            Assert.That(DebugUILayout.CommanderHud.width, Is.GreaterThan(0f));
        }

        [Test]
        public void DefaultLayout_UsesFourReadableBlocks()
        {
            var demo = DebugUILayout.GetDemoFlowRect(1280, 720);
            var objective = DebugUILayout.GetMissionObjectiveRect(1280, 720);
            var hint = DebugUILayout.GetControlHintRect(1280, 720);
            var commander = DebugUILayout.GetCommanderHudRect(1280, 720);
            var result = DebugUILayout.GetMissionResultSummaryRect(1280, 720);

            Assert.That(demo.xMax, Is.LessThanOrEqualTo(hint.xMin), "DemoFlow should stay in the left guidance block.");
            Assert.That(objective.xMax, Is.LessThanOrEqualTo(commander.xMin), "Objectives should stay left of commander panels.");
            Assert.That(demo.yMax, Is.LessThanOrEqualTo(objective.yMin), "DemoFlow should sit above objectives.");
            Assert.That(hint.yMax, Is.LessThanOrEqualTo(commander.yMin), "Commander hint should sit above commander debug HUD.");
            Assert.That(DebugUILayout.OverlapsHeavily(commander, result), Is.False, "Commander and result blocks should not heavily overlap.");
        }

        [Test]
        public void CompactLayout_IsSafeBelow1024()
        {
            Assert.That(DebugUILayout.IsCompact(800), Is.True);

            var layouts = new[]
            {
                DebugUILayout.GetDemoFlowRect(800, 600),
                DebugUILayout.GetDemoShortcutHelpRect(800, 600),
                DebugUILayout.GetMissionObjectiveRect(800, 600),
                DebugUILayout.GetControlHintRect(800, 600),
                DebugUILayout.GetCommanderHudRect(800, 600),
                DebugUILayout.GetMissionResultSummaryRect(800, 600)
            };

            foreach (var layout in layouts)
            {
                Assert.That(layout.width, Is.GreaterThan(0f));
                Assert.That(layout.height, Is.GreaterThan(0f));
                Assert.That(layout.x, Is.GreaterThanOrEqualTo(0f));
                Assert.That(layout.y, Is.GreaterThanOrEqualTo(0f));
            }
        }

        [Test]
        public void AllLayouts_HavePositiveDimensions()
        {
            var layouts = new[]
            {
                DebugUILayout.DemoFlow,
                DebugUILayout.DemoShortcutHelp,
                DebugUILayout.CommanderHud,
                DebugUILayout.ControlHint,
                DebugUILayout.MissionObjective,
                DebugUILayout.MissionResultSummary,
                DebugUILayout.MissionOutcomePreview,
                DebugUILayout.MissionChainSummary,
                DebugUILayout.FactionStanding,
                DebugUILayout.FactionDeltaToast,
                DebugUILayout.Tutorial,
                DebugUILayout.MissionResultDebug
            };

            foreach (var layout in layouts)
            {
                Assert.That(layout.width, Is.GreaterThan(0f), $"Width of {layout} should be positive");
                Assert.That(layout.height, Is.GreaterThan(0f), $"Height of {layout} should be positive");
            }
        }

        [Test]
        public void ShortcutHelp_IncludesRequiredDemoDebugKeys()
        {
            var text = string.Join("\n", DemoFlowHud.BuildShortcutHelpLines(false));

            foreach (var key in new[] { "1", "2", "3", "F7", "F8", "F5", "F9", "F10", "Tab/Q", "E", "R", "Left Click", "Space" })
                Assert.That(text, Does.Contain(key));
            Assert.That(text, Does.Contain("DEMO / DEBUG"));
        }

        [Test]
        public void WithOffset_ReturnsCorrectRect()
        {
            var baseRect = DebugUILayout.CommanderHud;
            var offset = DebugUILayout.WithOffset(baseRect, 10f, 20f);

            Assert.That(offset.x, Is.EqualTo(baseRect.x + 10f));
            Assert.That(offset.y, Is.EqualTo(baseRect.y + 20f));
            Assert.That(offset.width, Is.EqualTo(baseRect.width));
            Assert.That(offset.height, Is.EqualTo(baseRect.height));
        }
    }
}

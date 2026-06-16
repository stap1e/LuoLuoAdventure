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
        public void ControlHint_DoesNotOverlapCommanderHud()
        {
            var hint = DebugUILayout.ControlHint;
            var hud = DebugUILayout.CommanderHud;

            var hintBottom = hint.y + hint.height;
            var hudTop = hud.y;

            Assert.That(hintBottom, Is.LessThanOrEqualTo(hudTop + 1f),
                "ControlHint should be above CommanderHud");
        }

        [Test]
        public void AllLayouts_HavePositiveDimensions()
        {
            var layouts = new[]
            {
                DebugUILayout.CommanderHud,
                DebugUILayout.ControlHint,
                DebugUILayout.MissionObjective,
                DebugUILayout.MissionResultSummary,
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
        public void LeftPanelLayouts_DoNotOverlapRightPanelLayouts()
        {
            var leftLayouts = new[] { DebugUILayout.CommanderHud, DebugUILayout.ControlHint, DebugUILayout.MissionObjective, DebugUILayout.MissionChainSummary };
            var rightLayouts = new[] { DebugUILayout.FactionStanding, DebugUILayout.FactionDeltaToast, DebugUILayout.MissionResultDebug };

            foreach (var left in leftLayouts)
            {
                foreach (var right in rightLayouts)
                {
                    var leftEnd = left.x + left.width;
                    var rightStart = right.x;
                    Assert.That(leftEnd, Is.LessThan(rightStart + 10f),
                        $"Left panel ending at {leftEnd} should not overlap right panel starting at {rightStart}");
                }
            }
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

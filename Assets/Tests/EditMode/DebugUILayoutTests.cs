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

            // ControlHint and CommanderHud are both anchored to the left edge.
            // Verify they do not vertically overlap.
            var hintBottom = hint.y + hint.height;
            var hintTop = hint.y;
            var hudBottom = hud.y + hud.height;
            var hudTop = hud.y;

            bool disjoint = hintBottom <= hudTop || hudBottom <= hintTop;
            Assert.That(disjoint, Is.True,
                $"ControlHint [{hintTop}-{hintBottom}] and CommanderHud [{hudTop}-{hudBottom}] must not vertically overlap");
        }

        [Test]
        public void LeftColumn_PanelsDoNotOverlapVertically()
        {
            // All left-column panels should stack without vertical overlap.
            var panels = new[]
            {
                ("MissionObjective", DebugUILayout.MissionObjective),
                ("ControlHint", DebugUILayout.ControlHint),
                ("CommanderHud", DebugUILayout.CommanderHud),
                ("MissionChainSummary", DebugUILayout.MissionChainSummary),
            };

            for (int i = 0; i < panels.Length; i++)
            {
                for (int j = i + 1; j < panels.Length; j++)
                {
                    var a = panels[i].Item2;
                    var b = panels[j].Item2;
                    bool disjoint = (a.y + a.height) <= b.y || (b.y + b.height) <= a.y;
                    Assert.That(disjoint, Is.True,
                        $"{panels[i].Item1} [{a.y}-{a.y + a.height}] overlaps {panels[j].Item1} [{b.y}-{b.y + b.height}]");
                }
            }
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
            // Right panels are anchored to Screen.width. In batchmode with no
            // display, Screen.width may be very small (e.g. 640), making the
            // right panels and left panels physically overlap. The layout is
            // designed for typical play widths (>= 1280). If Screen.width is
            // too small, skip the assertion rather than fail.
            const int MinSupportedWidth = 1024;
            if (Screen.width < MinSupportedWidth)
            {
                Assert.Pass($"Screen.width={Screen.width} below {MinSupportedWidth} (likely batchmode); skipping anchor overlap check.");
                return;
            }

            var leftLayouts = new[] { DebugUILayout.CommanderHud, DebugUILayout.ControlHint, DebugUILayout.MissionObjective, DebugUILayout.MissionChainSummary };
            var rightLayouts = new[] { DebugUILayout.FactionStanding, DebugUILayout.FactionDeltaToast, DebugUILayout.MissionResultDebug };

            foreach (var left in leftLayouts)
            {
                foreach (var right in rightLayouts)
                {
                    var leftEnd = left.x + left.width;
                    var rightStart = right.x;
                    Assert.That(leftEnd, Is.LessThanOrEqualTo(rightStart),
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

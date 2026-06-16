using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    /// <summary>
    /// Scans AnimationClip assets to ensure no Transform position/euler curves
    /// are bound to the Animator's root path (path = ""). Such bindings would
    /// overwrite gameplay root movement (CombatController / SimpleCombatAI /
    /// Combatant.Dodge / NavigationAgentBridge fallback).
    /// </summary>
    public class AnimationClipBindingSafetyTests
    {
        [Test]
        public void NoAnimationClip_BindsTransformPositionToRootPath()
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip");
            var violations = new System.Collections.Generic.List<string>();
            int scanned = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.StartsWith("Assets/")) continue;

                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) continue;
                scanned++;

                var bindings = AnimationUtility.GetCurveBindings(clip);
                foreach (var b in bindings)
                {
                    if (b.type != typeof(Transform)) continue;
                    var prop = b.propertyName;
                    bool unsafeProp =
                        prop.StartsWith("localPosition") ||
                        prop.StartsWith("localEulerAngles");
                    if (!unsafeProp) continue;

                    if (string.IsNullOrEmpty(b.path))
                        violations.Add($"{path} :: {prop} bound to root path ''");
                }
            }

            if (violations.Count > 0)
                Assert.Fail($"Found {violations.Count} root-path Transform binding(s) (must bind to 'Visual' child):\n  " +
                            string.Join("\n  ", violations));
            else
                Assert.Pass($"Scanned {scanned} AnimationClip(s); no unsafe root-path Transform bindings.");
        }

        [Test]
        public void GeneratedPlaceholderClip_BindsToVisualPath_NotRoot()
        {
            // Create an in-memory clip mimicking generator output.
            var clip = new AnimationClip { name = "TestPlaceholder" };
            clip.SetCurve("Visual", typeof(Transform), "localPosition.y",
                AnimationCurve.Linear(0f, 0f, 0.5f, 0.05f));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.IsTrue(bindings.Any(b => b.path == "Visual" && b.propertyName == "localPosition.y"),
                "Generator pattern must bind position curves to 'Visual' path");
            Assert.IsFalse(bindings.Any(b => string.IsNullOrEmpty(b.path) && b.propertyName.StartsWith("localPosition")),
                "Generator pattern must NOT bind position curves to root path ''");
        }

        [Test]
        public void RootPathBinding_DetectionLogic_FlagsViolation()
        {
            var clip = new AnimationClip { name = "BadClip" };
            clip.SetCurve("", typeof(Transform), "localPosition.y",
                AnimationCurve.Linear(0f, 0f, 0.5f, 0.05f));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            bool foundViolation = bindings.Any(b =>
                b.type == typeof(Transform) &&
                string.IsNullOrEmpty(b.path) &&
                b.propertyName.StartsWith("localPosition"));

            Assert.IsTrue(foundViolation, "Detection logic must flag root-path localPosition binding");
        }
    }
}

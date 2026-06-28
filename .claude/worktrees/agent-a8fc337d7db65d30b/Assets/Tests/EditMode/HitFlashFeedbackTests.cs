using LuoLuoTrip.Combat.Feedback;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class HitFlashFeedbackTests
    {
        [Test]
        public void NoVisualChild_NoCrash_NoRenderers()
        {
            var go = new GameObject("U");
            try
            {
                var f = go.AddComponent<HitFlashFeedback>();
                Assert.DoesNotThrow(() => f.PlayHitFlash());
                Assert.AreEqual(0, f.RendererCount);
                Assert.IsFalse(f.IsFlashing);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void FlashTints_Visual_Renderers()
        {
            var go = new GameObject("U");
            try
            {
                var visual = new GameObject("Visual");
                visual.transform.SetParent(go.transform, false);
                var prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prim.transform.SetParent(visual.transform, false);
                Object.DestroyImmediate(prim.GetComponent<Collider>());
                var renderer = prim.GetComponent<Renderer>();
                renderer.sharedMaterial = new Material(Shader.Find("Standard"));
                var original = renderer.sharedMaterial.color;

                var f = go.AddComponent<HitFlashFeedback>();
                Assert.That(f.RendererCount, Is.GreaterThan(0));

                f.PlayHitFlash();
                Assert.IsTrue(f.IsFlashing);
                Assert.AreNotEqual(original, renderer.sharedMaterial.color);

                f.RestoreImmediate();
                Assert.IsFalse(f.IsFlashing);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void DeathFlash_AppliesDifferentTint()
        {
            var go = new GameObject("U");
            try
            {
                var visual = new GameObject("Visual");
                visual.transform.SetParent(go.transform, false);
                var prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prim.transform.SetParent(visual.transform, false);
                Object.DestroyImmediate(prim.GetComponent<Collider>());
                prim.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard"));

                var f = go.AddComponent<HitFlashFeedback>();
                Assert.DoesNotThrow(() => f.PlayDeathFlash());
                Assert.IsTrue(f.IsFlashing);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

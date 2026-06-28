using LuoLuoTrip.AI;
using LuoLuoTrip.Editor;
using NUnit.Framework;
using UnityEditor;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIProfileAssetIdempotencyTests
    {
        [Test]
        public void CreateAIBehaviorProfiles_PreservesExistingProfileTuning()
        {
            const string path = "Assets/Data/AIProfiles/DefensiveGuard.asset";
            var profile = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>(path);
            Assert.That(profile, Is.Not.Null, path);

            var originalChase = profile.chaseRadius;
            var originalFocus = profile.respondsToFocusFire;
            profile.chaseRadius = 42.5f;
            profile.respondsToFocusFire = !originalFocus;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            try
            {
                LuoLuoTripSetupMenu.CreateAIBehaviorProfiles();
                var reloaded = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>(path);
                Assert.That(reloaded.chaseRadius, Is.EqualTo(42.5f));
                Assert.That(reloaded.respondsToFocusFire, Is.EqualTo(!originalFocus));
            }
            finally
            {
                profile.chaseRadius = originalChase;
                profile.respondsToFocusFire = originalFocus;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }
        }

        [Test]
        public void RequiredProfileAssets_Validate()
        {
            foreach (AIBehaviorProfileType type in System.Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                foreach (var prefix in new[] { "Assets/Data/AIProfiles", "Assets/Resources/AIProfiles" })
                {
                    var path = $"{prefix}/{type}.asset";
                    var profile = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>(path);
                    Assert.That(profile, Is.Not.Null, path);
                    Assert.That(profile.Validate(out var error), Is.True, $"{path}: {error}");
                }
            }
        }
    }
}

using LuoLuoTrip.AI;
using LuoLuoTrip.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIBehaviorTuningConfigTests
    {
        [Test]
        public void NewBehaviorTuningFields_ValidateWithDefaults()
        {
            foreach (AIBehaviorProfileType type in System.Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                try
                {
                    AIBehaviorProfileSO.ConfigureDefaults(profile, type);

                    Assert.That(profile.Validate(out var error), Is.True, $"{type}: {error}");
                    Assert.That(profile.objectivePressureWeight, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(profile.protectedTargetPressureWeight, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(profile.neutralTargetPressureWeight, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(profile.hostileUnitWeight, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(profile.retreatDistance, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(profile.retreatTriggerRadius, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(profile.guardLeashRadius, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(profile.guardReturnSpeedMultiplier, Is.GreaterThan(0f));
                    Assert.That(profile.decisionRefreshInterval, Is.GreaterThanOrEqualTo(0f));
                }
                finally { Object.DestroyImmediate(profile); }
            }
        }

        [Test]
        public void InvalidNegativeRadius_FailsValidation()
        {
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            try
            {
                AIBehaviorProfileSO.ConfigureDefaults(profile, AIBehaviorProfileType.DefensiveGuard);
                profile.retreatTriggerRadius = -0.1f;

                Assert.That(profile.Validate(out var error), Is.False);
                Assert.That(error, Does.Contain("distance tuning"));
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void RecommendedDefaults_ExpressProfileSemantics()
        {
            var raider = Create(AIBehaviorProfileType.AggressiveRaider);
            var guard = Create(AIBehaviorProfileType.DefensiveGuard);
            var negotiator = Create(AIBehaviorProfileType.Negotiator);
            var hardliner = Create(AIBehaviorProfileType.Hardliner);
            var commander = Create(AIBehaviorProfileType.CommanderUnit);
            try
            {
                Assert.That(raider.objectivePressureWeight, Is.GreaterThan(raider.hostileUnitWeight));
                Assert.That(raider.maxChaseDistanceFromHome, Is.GreaterThan(guard.maxChaseDistanceFromHome));
                Assert.That(guard.guardLeashRadius, Is.GreaterThan(0f));
                Assert.That(negotiator.canInitiateCombat, Is.False);
                Assert.That(negotiator.respondsToFocusFire, Is.False);
                Assert.That(hardliner.hardlinerEscalationBias, Is.GreaterThan(0f));
                Assert.That(commander.respondsToTacticalCommand, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(raider);
                Object.DestroyImmediate(guard);
                Object.DestroyImmediate(negotiator);
                Object.DestroyImmediate(hardliner);
                Object.DestroyImmediate(commander);
            }
        }

        [Test]
        public void SetupMenu_DoesNotOverwriteTunedBehaviorFields()
        {
            const string path = "Assets/Data/AIProfiles/AggressiveRaider.asset";
            var profile = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>(path);
            Assert.That(profile, Is.Not.Null, path);

            var originalObjective = profile.objectivePressureWeight;
            var originalLeash = profile.guardLeashRadius;
            profile.objectivePressureWeight = 37.5f;
            profile.guardLeashRadius = 6.25f;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            try
            {
                LuoLuoTripSetupMenu.CreateAIBehaviorProfiles();
                var reloaded = AssetDatabase.LoadAssetAtPath<AIBehaviorProfileSO>(path);
                Assert.That(reloaded.objectivePressureWeight, Is.EqualTo(37.5f));
                Assert.That(reloaded.guardLeashRadius, Is.EqualTo(6.25f));
            }
            finally
            {
                profile.objectivePressureWeight = originalObjective;
                profile.guardLeashRadius = originalLeash;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }
        }

        private static AIBehaviorProfileSO Create(AIBehaviorProfileType type)
        {
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            AIBehaviorProfileSO.ConfigureDefaults(profile, type);
            return profile;
        }
    }
}

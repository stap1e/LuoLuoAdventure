using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIProfileHudTests
    {
        [Test]
        public void ProfileLabel_IsReadable()
        {
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            try
            {
                AIBehaviorProfileSO.ConfigureDefaults(profile, AIBehaviorProfileType.DefensiveGuard);
                Assert.That(profile.DisplayLabel, Does.Contain("Defensive"));
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void SimpleCombatAI_DefaultProfileLabel_IsReadable()
        {
            var go = new GameObject("DefaultAI");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("default", "Default", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                var ai = go.AddComponent<SimpleCombatAI>();
                InvokeAwake(ai);

                Assert.That(ai.CurrentBehaviorLabel, Is.EqualTo("Default AI"));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void CommandResponsivenessText_IsReadableThroughPresenterSuggestion()
        {
            var go = new GameObject("Guard");
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            try
            {
                AIBehaviorProfileSO.ConfigureDefaults(profile, AIBehaviorProfileType.DefensiveGuard);
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("guard", "Guard", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                var ai = go.AddComponent<SimpleCombatAI>();
                ai.BehaviorProfile = profile;

                var state = new CommanderControlRuntimeState
                {
                    SelectedTarget = entity,
                    LastSelectedTargetName = "Guard",
                    LastDirectControlAllowed = false,
                    LastTacticalCommandAllowed = true
                };

                var descriptor = CommanderActionPresenter.GetRecommendedAction(state);
                Assert.That(descriptor.Suggestion, Does.Contain("DefendObjective"));
            }
            finally
            {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ProfileBehaviorAndSuggestionText_AreReadable()
        {
            foreach (AIBehaviorProfileType type in System.Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                var go = new GameObject(type.ToString());
                var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                try
                {
                    AIBehaviorProfileSO.ConfigureDefaults(profile, type);
                    var entity = go.AddComponent<CharacterEntity>();
                    entity.Bind(CharacterData.Create(type.ToString(), type.ToString(), SubFactionId.MotorIronRiders, CharacterRole.Minion));
                    var ai = go.AddComponent<SimpleCombatAI>();
                    ai.BehaviorProfile = profile;
                    InvokeAwake(ai);

                    Assert.That(CommanderActionPresenter.BuildProfileSummary(ai), Does.Contain(profile.DisplayLabel));
                    Assert.That(CommanderActionPresenter.BuildBehaviorSummary(ai), Does.Contain("Behavior"));
                    Assert.That(CommanderActionPresenter.BuildResponseSummary(ai), Does.Contain("Responds"));
                    Assert.That(CommanderActionPresenter.BuildProfileSuggestion(ai), Is.Not.Empty);
                }
                finally
                {
                    Object.DestroyImmediate(profile);
                    Object.DestroyImmediate(go);
                }
            }
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(behaviour, null);
        }
    }
}

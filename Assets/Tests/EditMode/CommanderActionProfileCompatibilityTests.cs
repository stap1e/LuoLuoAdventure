using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderActionProfileCompatibilityTests
    {
        [Test]
        public void DefensiveGuard_AcceptsDefendObjective()
        {
            var fixture = new Fixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                fixture.AI.SetDefendObjective(fixture.Objective.transform, 5f);
                Assert.That(fixture.AI.DefendTarget, Is.EqualTo(fixture.Objective.transform));
                Assert.That(CommanderActionPresenter.BuildProfileSuggestion(fixture.AI), Does.Contain("G"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Negotiator_IgnoresFocusFireResponderRole()
        {
            var fixture = new Fixture(AIBehaviorProfileType.Negotiator);
            try
            {
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);
                Assert.That(fixture.AI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.AI.RespondsToFocusFire, Is.False);
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void AggressiveRaider_CanBeFocusFireTarget()
        {
            var fixture = new Fixture(AIBehaviorProfileType.AggressiveRaider);
            try
            {
                var score = AITargetSelectionUtility.ScoreTarget(fixture.SelfCombatant, fixture.EnemyCombatant,
                    fixture.Profile, fixture.EnemyCombatant, null, fixture.Self.transform.position, 1f, true);

                Assert.That(score.IsValid, Is.True);
                Assert.That(score.Reason, Does.Contain("Focus fire"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void CommanderUnit_TacticalCommandSuggestionVisible()
        {
            var fixture = new Fixture(AIBehaviorProfileType.CommanderUnit);
            try
            {
                Assert.That(CommanderActionPresenter.BuildProfileSuggestion(fixture.AI), Does.Contain("Tactical"));
                Assert.That(CommanderActionPresenter.BuildResponseSummary(fixture.AI), Does.Contain("Tactical Yes"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Hardliner_FocusFireSuppressionSuggestionVisible()
        {
            var fixture = new Fixture(AIBehaviorProfileType.Hardliner);
            try
            {
                Assert.That(CommanderActionPresenter.BuildProfileSuggestion(fixture.AI), Does.Contain("Escalation"));
                Assert.That(CommanderActionPresenter.BuildProfileSuggestion(fixture.AI), Does.Contain("F"));
            }
            finally { fixture.Dispose(); }
        }

        private sealed class Fixture : System.IDisposable
        {
            private readonly GameObject _root = new GameObject("CommanderActionProfileCompatibilityFixture");
            public AIBehaviorProfileSO Profile { get; }
            public CharacterEntity Self { get; }
            public Combatant SelfCombatant => Self.Combatant;
            public SimpleCombatAI AI { get; }
            public CharacterEntity Objective { get; }
            public Combatant EnemyCombatant { get; }

            public Fixture(AIBehaviorProfileType type)
            {
                Profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                AIBehaviorProfileSO.ConfigureDefaults(Profile, type);
                Self = Create("self", "Self", SubFactionId.MotorIronRiders, Vector3.zero);
                AI = Self.gameObject.AddComponent<SimpleCombatAI>();
                AI.BehaviorProfile = Profile;
                InvokeAwake(AI);
                Objective = Create("core", "CityGateCore", SubFactionId.MotorIronRiders, new Vector3(2f, 0f, 0f));
                EnemyCombatant = Create("enemy", "BeastRaider", SubFactionId.BeastIronClaw, new Vector3(3f, 0f, 0f)).Combatant;
            }

            private CharacterEntity Create(string id, string name, SubFactionId faction, Vector3 position)
            {
                var go = new GameObject(name);
                go.transform.SetParent(_root.transform);
                go.transform.position = position;
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create(id, name, faction, CharacterRole.Minion));
                return entity;
            }

            private static void InvokeAwake(MonoBehaviour behaviour)
            {
                behaviour.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(behaviour, null);
            }

            public void Dispose()
            {
                Object.DestroyImmediate(Profile);
                Object.DestroyImmediate(_root);
            }
        }
    }
}

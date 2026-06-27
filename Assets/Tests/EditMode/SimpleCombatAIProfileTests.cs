using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class SimpleCombatAIProfileTests
    {
        [TearDown]
        public void TearDown()
        {
            CharacterEntity.HostilityResolver = null;
        }

        [Test]
        public void NoProfile_KeepsDefaultBehaviorLabel()
        {
            var fixture = new Fixture(null);
            try
            {
                Assert.That(fixture.AI.BehaviorProfile, Is.Null);
                Assert.That(fixture.AI.CurrentBehaviorLabel, Is.EqualTo("Default AI"));
                Assert.That(fixture.AI.CanInitiateCombat, Is.True);
                Assert.That(fixture.AI.RespondsToFocusFire, Is.True);
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void DefensiveGuard_DoesNotChaseTooFar()
        {
            var fixture = new Fixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                Assert.That(fixture.AI.EffectiveMaxChaseDistance, Is.GreaterThan(0f));
                fixture.AI.SetDefendObjective(fixture.Objective.transform, 5f);
                Assert.That(fixture.AI.DefendRadius, Is.GreaterThanOrEqualTo(5f));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Negotiator_IgnoresFocusFireAndCannotInitiate()
        {
            var fixture = new Fixture(AIBehaviorProfileType.Negotiator);
            try
            {
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);
                Assert.That(fixture.AI.CanInitiateCombat, Is.False);
                Assert.That(fixture.AI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.AI.LastProfileDecision, Does.Contain("ignores FocusFire"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void AggressiveRaider_AcceptsObjectiveTarget()
        {
            var fixture = new Fixture(AIBehaviorProfileType.AggressiveRaider);
            try
            {
                fixture.AI.ProtectedTarget = fixture.Objective.transform;
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);
                Assert.That(fixture.AI.BehaviorProfile.prefersObjectiveTargets, Is.True);
                Assert.That(fixture.AI.ForcedAttackTarget, Is.Null, "AggressiveRaider profile ignores player FocusFire commands by design");
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void MissingTarget_IsSafe()
        {
            var fixture = new Fixture(AIBehaviorProfileType.CommanderUnit);
            try
            {
                fixture.AI.SetFocusFireTarget(null);
                fixture.AI.SetDefendObjective(null, 5f);
                Assert.That(fixture.AI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.AI.CommanderCommandStatus, Does.Contain("missing"));
            }
            finally { fixture.Dispose(); }
        }

        private sealed class Fixture : System.IDisposable
        {
            private readonly GameObject _root = new GameObject("SimpleCombatAIProfileFixture");
            private readonly AIBehaviorProfileSO _profile;
            public SimpleCombatAI AI { get; }
            public CharacterEntity Objective { get; }
            public Combatant EnemyCombatant { get; }

            public Fixture(AIBehaviorProfileType? type)
            {
                CharacterEntity.HostilityResolver = (a, b) => a != b;
                var self = Create("self", "Self", SubFactionId.MotorIronRiders, Vector3.zero);
                AI = self.gameObject.AddComponent<SimpleCombatAI>();
                if (type.HasValue)
                {
                    _profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                    AIBehaviorProfileSO.ConfigureDefaults(_profile, type.Value);
                    AI.BehaviorProfile = _profile;
                }
                InvokeAwake(AI);

                Objective = Create("core", "CityGateCore", SubFactionId.MotorIronRiders, new Vector3(2f, 0f, 0f));
                EnemyCombatant = Create("enemy", "Enemy", SubFactionId.BeastIronClaw, new Vector3(3f, 0f, 0f)).Combatant;
            }

            private CharacterEntity Create(string id, string name, SubFactionId faction, Vector3 pos)
            {
                var go = new GameObject(name);
                go.transform.SetParent(_root.transform);
                go.transform.position = pos;
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
                if (_profile != null) Object.DestroyImmediate(_profile);
                Object.DestroyImmediate(_root);
            }
        }
    }
}

using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AITargetSelectionUtilityTests
    {
        [TearDown]
        public void TearDown()
        {
            CharacterEntity.HostilityResolver = null;
        }

        [Test]
        public void AggressiveRaider_PrioritizesObjective()
        {
            var fixture = new TargetFixture(AIBehaviorProfileType.AggressiveRaider);
            try
            {
                var normal = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.Enemy, fixture.Profile, null, fixture.Objective.transform, fixture.Self.transform.position, 20f, true);
                var objective = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.Objective, fixture.Profile, null, fixture.Objective.transform, fixture.Self.transform.position, 20f, true, isObjectiveTarget: true, isProtectedTarget: true);

                Assert.That(objective.IsValid, Is.True);
                Assert.That(objective.Score, Is.GreaterThan(normal.Score));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void DefensiveGuard_PrioritizesHostileInsideDefendRadius()
        {
            var fixture = new TargetFixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                var close = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.Enemy, fixture.Profile, null, null, fixture.Self.transform.position, 6f, true);
                var far = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.FarEnemy, fixture.Profile, null, null, fixture.Self.transform.position, 6f, true);

                Assert.That(close.IsValid, Is.True);
                Assert.That(far.IsValid, Is.False);
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Negotiator_ReturnsNoAttackTarget()
        {
            var fixture = new TargetFixture(AIBehaviorProfileType.Negotiator);
            try
            {
                var score = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.Enemy, fixture.Profile, null, null, fixture.Self.transform.position, 20f, true);
                Assert.That(score.IsValid, Is.False);
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Hardliner_PrioritizesNegotiatorProtectedTarget()
        {
            var fixture = new TargetFixture(AIBehaviorProfileType.Hardliner);
            try
            {
                var normal = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.Enemy, fixture.Profile, null, fixture.Negotiator.transform, fixture.Self.transform.position, 20f, true);
                var negotiator = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.Negotiator, fixture.Profile, null, fixture.Negotiator.transform, fixture.Self.transform.position, 20f, false, isProtectedTarget: true, isNeutralTarget: true);

                Assert.That(negotiator.IsValid, Is.True);
                Assert.That(negotiator.Score, Is.GreaterThan(normal.Score));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void FocusFireForcedTarget_HasHighestPriority()
        {
            var fixture = new TargetFixture(AIBehaviorProfileType.CommanderUnit);
            try
            {
                var forced = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.FarEnemy, fixture.Profile, fixture.FarEnemy, null, fixture.Self.transform.position, 1f, true);
                var other = AITargetSelectionUtility.ScoreTarget(fixture.Self, fixture.Enemy, fixture.Profile, fixture.FarEnemy, null, fixture.Self.transform.position, 20f, true);

                Assert.That(forced.IsValid, Is.True);
                Assert.That(forced.Score, Is.GreaterThan(1000f));
                Assert.That(other.IsValid, Is.False);
            }
            finally { fixture.Dispose(); }
        }

        private sealed class TargetFixture : System.IDisposable
        {
            private readonly GameObject _root = new GameObject("AITargetSelectionFixture");
            public AIBehaviorProfileSO Profile { get; }
            public Combatant Self { get; }
            public Combatant Enemy { get; }
            public Combatant FarEnemy { get; }
            public Combatant Objective { get; }
            public Combatant Negotiator { get; }

            public TargetFixture(AIBehaviorProfileType type)
            {
                CharacterEntity.HostilityResolver = (a, b) => a != b && GameConstants.IsBeastSubFaction(b);
                Profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                AIBehaviorProfileSO.ConfigureDefaults(Profile, type);
                Self = Create("self", "Guard", SubFactionId.MotorIronRiders, Vector3.zero);
                Enemy = Create("enemy", "BeastRaider", SubFactionId.BeastIronClaw, new Vector3(2f, 0f, 0f));
                FarEnemy = Create("far", "FarBeastRaider", SubFactionId.BeastIronClaw, new Vector3(12f, 0f, 0f));
                Objective = Create("core", "CityGateCore_Objective", SubFactionId.BeastIronClaw, new Vector3(3f, 0f, 0f));
                Negotiator = Create("neg", "BeastNegotiator", SubFactionId.BeastShadowFang, new Vector3(3f, 0f, 0f));
            }

            private Combatant Create(string id, string name, SubFactionId faction, Vector3 pos)
            {
                var go = new GameObject(name);
                go.transform.SetParent(_root.transform);
                go.transform.position = pos;
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create(id, name, faction, CharacterRole.Minion));
                return entity.Combatant;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(Profile);
                Object.DestroyImmediate(_root);
            }
        }
    }
}

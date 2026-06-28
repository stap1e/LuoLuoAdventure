using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIProfileScenarioDecisionTests
    {
        [TearDown]
        public void TearDown()
        {
            CharacterEntity.HostilityResolver = null;
        }

        [Test]
        public void AggressiveRaider_PrefersObjectiveOverGenericHostile()
        {
            var fixture = new Fixture(AIBehaviorProfileType.AggressiveRaider);
            try
            {
                var generic = AITargetSelectionUtility.ScoreTarget(fixture.SelfCombatant, fixture.EnemyCombatant, fixture.Profile,
                    null, fixture.Objective.transform, fixture.Self.transform.position, 20f, true);
                var objective = AITargetSelectionUtility.ScoreTarget(fixture.SelfCombatant, fixture.ObjectiveCombatant, fixture.Profile,
                    null, fixture.Objective.transform, fixture.Self.transform.position, 20f, true, isObjectiveTarget: true, isProtectedTarget: true);

                Assert.That(objective.IsValid, Is.True);
                Assert.That(objective.Score, Is.GreaterThan(generic.Score));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void DefensiveGuard_RefusesChaseBeyondLeash()
        {
            var fixture = new Fixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                fixture.AI.SetDefendObjective(fixture.Objective.transform, 5f);

                Assert.That(fixture.AI.DefendLeashRadius, Is.GreaterThan(0f));
                Assert.That(fixture.AI.EffectiveMaxChaseDistance, Is.LessThan(18f));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Negotiator_RefusesAttackTarget()
        {
            var fixture = new Fixture(AIBehaviorProfileType.Negotiator);
            try
            {
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);

                Assert.That(fixture.AI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.AI.CanInitiateCombat, Is.False);
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Negotiator_RetreatConditionTriggersNearHostile()
        {
            var fixture = new Fixture(AIBehaviorProfileType.Negotiator);
            try
            {
                fixture.AI.CombatantQuery = () => new[] { fixture.SelfCombatant, fixture.EnemyCombatant };
                fixture.AI.TickNonCombatantBehaviorForTests();

                Assert.That(fixture.AI.IsRetreating, Is.True);
                Assert.That(fixture.AI.LastProfileDecision, Does.Contain("Retreating"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Hardliner_PrioritizesNegotiatorProtectedTarget()
        {
            var fixture = new Fixture(AIBehaviorProfileType.Hardliner);
            try
            {
                var generic = AITargetSelectionUtility.ScoreTarget(fixture.SelfCombatant, fixture.EnemyCombatant, fixture.Profile,
                    null, fixture.Negotiator.transform, fixture.Self.transform.position, 20f, true);
                var negotiator = AITargetSelectionUtility.ScoreTarget(fixture.SelfCombatant, fixture.NegotiatorCombatant, fixture.Profile,
                    null, fixture.Negotiator.transform, fixture.Self.transform.position, 20f, false, isProtectedTarget: true, isNeutralTarget: true);

                Assert.That(negotiator.IsValid, Is.True);
                Assert.That(negotiator.Score, Is.GreaterThan(generic.Score));
                Assert.That(negotiator.Reason, Does.Contain("Escalation"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void CommanderUnit_AllowsTacticalResponseButDirectControlUsesPermissionRules()
        {
            var fixture = new Fixture(AIBehaviorProfileType.CommanderUnit, CharacterRole.CityLord);
            try
            {
                Assert.That(fixture.AI.RespondsToTacticalCommand, Is.True);
                Assert.That(fixture.AI.RespondsToDefendObjective, Is.True);
                Assert.That(fixture.AI.RespondsToFocusFire, Is.True);
                Assert.That(fixture.Self.Data.AllowDirectControl, Is.False);
                Assert.That(fixture.Self.Data.IsHeroOrLeader, Is.True);
            }
            finally { fixture.Dispose(); }
        }

        private sealed class Fixture : System.IDisposable
        {
            private readonly GameObject _root = new GameObject("AIProfileScenarioDecisionFixture");
            public AIBehaviorProfileSO Profile { get; }
            public CharacterEntity Self { get; }
            public SimpleCombatAI AI { get; }
            public Combatant SelfCombatant => Self.Combatant;
            public CharacterEntity Objective { get; }
            public Combatant ObjectiveCombatant => Objective.Combatant;
            public Combatant EnemyCombatant { get; }
            public CharacterEntity Negotiator { get; }
            public Combatant NegotiatorCombatant => Negotiator.Combatant;

            public Fixture(AIBehaviorProfileType type, CharacterRole role = CharacterRole.Minion)
            {
                CharacterEntity.HostilityResolver = (a, b) => a != b && GameConstants.IsBeastSubFaction(b);
                Profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                AIBehaviorProfileSO.ConfigureDefaults(Profile, type);
                Self = Create("self", "Self", SubFactionId.MotorIronRiders, role, Vector3.zero);
                AI = Self.gameObject.AddComponent<SimpleCombatAI>();
                AI.BehaviorProfile = Profile;
                InvokeAwake(AI);
                Objective = Create("core", "CityGateCore_Objective", SubFactionId.BeastIronClaw, CharacterRole.Minion, new Vector3(2f, 0f, 0f));
                EnemyCombatant = Create("enemy", "BeastRaider", SubFactionId.BeastIronClaw, CharacterRole.Minion, new Vector3(3f, 0f, 0f)).Combatant;
                Negotiator = Create("neg", "BeastNegotiator", SubFactionId.BeastShadowFang, CharacterRole.Minion, new Vector3(2.5f, 0f, 0f));
            }

            private CharacterEntity Create(string id, string name, SubFactionId faction, CharacterRole role, Vector3 position)
            {
                var go = new GameObject(name);
                go.transform.SetParent(_root.transform);
                go.transform.position = position;
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create(id, name, faction, role));
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

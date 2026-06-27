using System.Collections;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CityGateAIBehaviorSmokeTests
    {
        [UnityTest]
        public IEnumerator BeastNegotiator_DoesNotInitiateCombat()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.Negotiator);
            try
            {
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);
                yield return null;

                Assert.That(fixture.AI.CanInitiateCombat, Is.False);
                Assert.That(fixture.AI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.AI.LastProfileDecision, Is.Not.Empty);
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator BeastRaider_TargetsObjectiveProfile()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.AggressiveRaider);
            try
            {
                fixture.AI.ProtectedTarget = fixture.Objective.transform;
                yield return null;

                Assert.That(fixture.AI.BehaviorProfile.prefersObjectiveTargets, Is.True);
                Assert.That(fixture.AI.BehaviorProfile.prefersProtectedTargets, Is.True);
                Assert.That(fixture.AI.EffectiveMaxChaseDistance, Is.GreaterThan(0f));
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator MechaGateGuard_HoldsNearDefendPoint()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                fixture.AI.SetDefendObjective(fixture.Objective.transform, 5f);
                yield return null;

                Assert.That(fixture.AI.DefendTarget, Is.EqualTo(fixture.Objective.transform));
                Assert.That(fixture.AI.EffectiveMaxChaseDistance, Is.GreaterThan(0f));
                Assert.That(fixture.AI.BehaviorProfile.holdPositionWhenNoTarget, Is.True);
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator MechaHardliner_CanSelectProtectedTarget()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.Hardliner);
            try
            {
                var score = AITargetSelectionUtility.ScoreTarget(fixture.SelfCombatant, fixture.ObjectiveCombatant,
                    fixture.AI.BehaviorProfile, null, fixture.Objective.transform, fixture.Self.transform.position, 20f,
                    isHostile: false, isProtectedTarget: true, isNeutralTarget: true);
                yield return null;

                Assert.That(score.IsValid, Is.True);
                Assert.That(score.Reason, Does.Contain("target"));
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator CityGateAIProfiles_NoExceptions()
        {
            var fixture = new AIProfileSmokeFixture(AIBehaviorProfileType.CommanderUnit);
            try
            {
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);
                yield return null;

                Assert.That(fixture.AI.ForcedAttackTarget, Is.EqualTo(fixture.EnemyCombatant));
            }
            finally { fixture.Dispose(); }
        }
    }

    internal sealed class AIProfileSmokeFixture : System.IDisposable
    {
        private readonly GameObject _root = new GameObject("AIProfileSmokeFixture");
        private readonly AIBehaviorProfileSO _profile;
        public CharacterEntity Self { get; }
        public SimpleCombatAI AI { get; }
        public Combatant SelfCombatant => Self.Combatant;
        public CharacterEntity Objective { get; }
        public Combatant ObjectiveCombatant => Objective.Combatant;
        public Combatant EnemyCombatant { get; }

        public AIProfileSmokeFixture(AIBehaviorProfileType type)
        {
            CharacterEntity.HostilityResolver = (a, b) => a != b && GameConstants.IsBeastSubFaction(b);
            _profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            AIBehaviorProfileSO.ConfigureDefaults(_profile, type);
            Self = Create("self", "Self", SubFactionId.MotorIronRiders, Vector3.zero);
            AI = Self.gameObject.AddComponent<SimpleCombatAI>();
            AI.BehaviorProfile = _profile;
            Objective = Create("core", "CityGateCore", SubFactionId.BeastShadowFang, new Vector3(3f, 0f, 0f));
            EnemyCombatant = Create("enemy", "BeastRaider", SubFactionId.BeastIronClaw, new Vector3(4f, 0f, 0f)).Combatant;
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

        public void Dispose()
        {
            CharacterEntity.HostilityResolver = null;
            Object.DestroyImmediate(_profile);
            Object.DestroyImmediate(_root);
        }
    }
}

using System.Collections;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class AIProfileCommanderActionCompatibilitySmokeTests
    {
        [UnityTest]
        public IEnumerator DefensiveGuard_CanReceiveDefendObjective()
        {
            var fixture = new CompatibilityFixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                fixture.AI.SetDefendObjective(fixture.Objective.transform, 5f);
                yield return null;

                Assert.That(fixture.AI.DefendTarget, Is.EqualTo(fixture.Objective.transform));
                Assert.That(fixture.AI.RespondsToDefendObjective, Is.True);
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator FocusFire_IgnoresNegotiatorResponder()
        {
            var fixture = new CompatibilityFixture(AIBehaviorProfileType.Negotiator);
            try
            {
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);
                yield return null;

                Assert.That(fixture.AI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.AI.RespondsToFocusFire, Is.False);
            }
            finally { fixture.Dispose(); }
        }

        [UnityTest]
        public IEnumerator CommanderUnit_HighRankStillDeniesDirectControl()
        {
            var fixture = new CompatibilityFixture(AIBehaviorProfileType.CommanderUnit, CharacterRole.CityLord);
            try
            {
                yield return null;

                Assert.That(fixture.AI.RespondsToTacticalCommand, Is.True);
                Assert.That(fixture.Entity.Data.AllowDirectControl, Is.False);
                Assert.That(fixture.Entity.Data.IsHeroOrLeader, Is.True);
            }
            finally { fixture.Dispose(); }
        }

        private sealed class CompatibilityFixture : System.IDisposable
        {
            private readonly GameObject _root = new GameObject("AIProfileCommanderCompatibilityFixture");
            private readonly AIBehaviorProfileSO _profile;
            public CharacterEntity Entity { get; }
            public SimpleCombatAI AI { get; }
            public CharacterEntity Objective { get; }
            public Combatant EnemyCombatant { get; }

            public CompatibilityFixture(AIBehaviorProfileType type, CharacterRole role = CharacterRole.Minion)
            {
                _profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                AIBehaviorProfileSO.ConfigureDefaults(_profile, type);
                Entity = Create("unit", "Unit", SubFactionId.MotorIronRiders, role);
                AI = Entity.gameObject.AddComponent<SimpleCombatAI>();
                AI.BehaviorProfile = _profile;
                Objective = Create("core", "CityGateCore", SubFactionId.MotorIronRiders, CharacterRole.Minion);
                EnemyCombatant = Create("enemy", "BeastRaider", SubFactionId.BeastIronClaw, CharacterRole.Minion).Combatant;
            }

            private CharacterEntity Create(string id, string name, SubFactionId faction, CharacterRole role)
            {
                var go = new GameObject(name);
                go.transform.SetParent(_root.transform);
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create(id, name, faction, role));
                return entity;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(_profile);
                Object.DestroyImmediate(_root);
            }
        }
    }
}

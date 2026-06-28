using System.Linq;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIBehaviorScenarioMonitorTests
    {
        [Test]
        public void Monitor_ReportsProfileLabel()
        {
            var fixture = new Fixture(AIBehaviorProfileType.DefensiveGuard);
            try
            {
                var snapshot = AIBehaviorScenarioMonitor.CreateSnapshot(fixture.AI);
                Assert.That(snapshot.ProfileLabel, Does.Contain("Guard"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Monitor_ReportsCurrentTarget()
        {
            var fixture = new Fixture(AIBehaviorProfileType.CommanderUnit);
            try
            {
                fixture.AI.SetFocusFireTarget(fixture.EnemyCombatant);
                var snapshot = AIBehaviorScenarioMonitor.CreateSnapshot(fixture.AI);

                Assert.That(snapshot.CurrentTargetName, Does.Contain("BeastRaider"));
                Assert.That(snapshot.IsFocusFireResponder, Is.True);
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Monitor_IsSafeWithMissingAIOrProfile()
        {
            var missing = AIBehaviorScenarioMonitor.CreateSnapshot(null);
            Assert.That(missing.ProfileLabel, Is.EqualTo("Default AI"));

            var go = new GameObject("DefaultAI");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("default", "Default", SubFactionId.MotorIronRiders, CharacterRole.Minion));
                var ai = go.AddComponent<SimpleCombatAI>();
                InvokeAwake(ai);

                var snapshot = AIBehaviorScenarioMonitor.CreateSnapshot(ai);
                Assert.That(snapshot.ProfileLabel, Is.EqualTo("Default AI"));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Monitor_CanSummarizeCityGateStyleUnits()
        {
            var root = new GameObject("CityGateMonitorFixture");
            try
            {
                var monitor = root.AddComponent<AIBehaviorScenarioMonitor>();
                var guard = CreateUnit(root.transform, "MechaGateGuard", AIBehaviorProfileType.DefensiveGuard);
                var raider = CreateUnit(root.transform, "BeastRaider_01", AIBehaviorProfileType.AggressiveRaider);
                monitor.Register(guard);
                monitor.Register(raider);

                var snapshots = monitor.GetSnapshots(refresh: false);
                var summary = monitor.BuildScenarioSummary(refresh: false);

                Assert.That(snapshots.Count, Is.EqualTo(2));
                Assert.That(snapshots.Any(s => s.ProfileLabel.Contains("Guard")), Is.True);
                Assert.That(summary, Does.Contain("BeastRaider_01"));
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static SimpleCombatAI CreateUnit(Transform parent, string name, AIBehaviorProfileType type)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create(name, name, SubFactionId.MotorIronRiders, CharacterRole.Minion));
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            AIBehaviorProfileSO.ConfigureDefaults(profile, type);
            var ai = go.AddComponent<SimpleCombatAI>();
            ai.BehaviorProfile = profile;
            InvokeAwake(ai);
            return ai;
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(behaviour, null);
        }

        private sealed class Fixture : System.IDisposable
        {
            private readonly GameObject _root = new GameObject("AIBehaviorScenarioMonitorFixture");
            private readonly AIBehaviorProfileSO _profile;
            public SimpleCombatAI AI { get; }
            public Combatant EnemyCombatant { get; }

            public Fixture(AIBehaviorProfileType type)
            {
                var self = Create("self", "Self", SubFactionId.MotorIronRiders);
                _profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
                AIBehaviorProfileSO.ConfigureDefaults(_profile, type);
                AI = self.gameObject.AddComponent<SimpleCombatAI>();
                AI.BehaviorProfile = _profile;
                InvokeAwake(AI);
                EnemyCombatant = Create("enemy", "BeastRaider", SubFactionId.BeastIronClaw).Combatant;
            }

            private CharacterEntity Create(string id, string name, SubFactionId faction)
            {
                var go = new GameObject(name);
                go.transform.SetParent(_root.transform);
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create(id, name, faction, CharacterRole.Minion));
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

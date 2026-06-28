using System.Collections;
using System.Linq;
using System.Reflection;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeActionExpansionSmokeTests
    {
        [UnityTest]
        public IEnumerator DefendObjective_IssuesCommandToLowRankAlly()
        {
            var fixture = new CommanderActionFixture();
            try
            {
                yield return null;

                Assert.That(fixture.Controller.TryIssueDefendObjective(fixture.Ally, fixture.Objective), Is.True);

                Assert.That(fixture.AllyAI.DefendTarget, Is.EqualTo(fixture.Objective.transform));
                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.DefendObjective));
                Assert.That(fixture.Controller.State.TacticalCommand.StatusText, Does.Contain("DefendObjective"));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator FocusFire_IssuesCommandToNearbyAllyAndClearsOnTargetDeath()
        {
            var fixture = new CommanderActionFixture();
            try
            {
                yield return null;

                Assert.That(fixture.Controller.TryIssueFocusFire(fixture.Enemy), Is.True);
                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.EqualTo(fixture.EnemyCombatant));
                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.FocusFire));

                fixture.EnemyCombatant.ApplyHealthDamage(9999f);
                fixture.Enemy.Data.IsAlive = false;
                fixture.Controller.State.TacticalCommand.ExpiresAtTime = Time.time - 0.1f;
                InvokeControllerUpdate(fixture.Controller);
                yield return null;

                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.None));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator FocusFire_ExpiryClearsTrackedResponder()
        {
            var fixture = new CommanderActionFixture();
            var previousTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 1f;
                yield return null;

                Assert.That(fixture.Controller.TryIssueFocusFire(fixture.Enemy), Is.True);
                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.EqualTo(fixture.EnemyCombatant));

                fixture.Controller.State.TacticalCommand.ExpiresAtTime = Time.time - 0.1f;
                yield return null;

                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.None));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator HighRankUnit_StillDeniesDirectControl()
        {
            var fixture = new CommanderActionFixture();
            try
            {
                yield return null;

                fixture.Selector.TrySelectTarget(fixture.Leader);
                fixture.Controller.TryInteract();
                yield return null;

                var direct = CommanderActionPresenter.BuildDescriptors(fixture.Controller.State)
                    .First(d => d.ActionType == CommanderActionType.DirectControl);
                Assert.That(direct.IsAllowed, Is.False);
                Assert.That(direct.DenialReason, Does.Contain("Leader").Or.Contain("level").Or.Contain("Rank").Or.Contain("context"));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        private static void InvokeControllerUpdate(CommanderControlController controller)
        {
            typeof(CommanderControlController)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(controller, null);
        }

        private sealed class CommanderActionFixture
        {
            private readonly GameObject _root = new GameObject("CommanderActionFixtureRoot");

            public CommanderControlController Controller { get; }
            public CommanderTargetSelector Selector { get; }
            public CharacterEntity Ally { get; }
            public SimpleCombatAI AllyAI { get; }
            public CharacterEntity Objective { get; }
            public CharacterEntity Enemy { get; }
            public Combatant EnemyCombatant { get; }
            public CharacterEntity Leader { get; }

            public CommanderActionFixture()
            {
                CharacterEntity.HostilityResolver = (a, b) => a != b && (GameConstants.IsBeastSubFaction(a) || GameConstants.IsBeastSubFaction(b));

                var player = CreateEntity("player", "Commander", SubFactionId.MotorIronRiders, CharacterRole.Minion, Vector3.zero);
                player.gameObject.AddComponent<CombatController>();
                Controller = player.gameObject.AddComponent<CommanderControlController>();
                Selector = player.gameObject.GetComponent<CommanderTargetSelector>();
                typeof(CommanderControlController)
                    .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(Controller, null);

                Ally = CreateEntity("ally", "Low-Rank Ally", SubFactionId.MotorIronRiders, CharacterRole.Minion, new Vector3(1f, 0f, 0f));
                Ally.Data.TrustToPlayer = 100;
                AllyAI = Ally.gameObject.AddComponent<SimpleCombatAI>();
                typeof(SimpleCombatAI)
                    .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(AllyAI, null);

                Objective = CreateEntity("city_gate_core", "CityGateCore", SubFactionId.MotorIronRiders, CharacterRole.Common, new Vector3(4f, 0f, 0f));
                Enemy = CreateEntity("raider", "BeastRaider_01", SubFactionId.BeastIronClaw, CharacterRole.Minion, new Vector3(3f, 0f, 0f));
                EnemyCombatant = Enemy.GetComponent<Combatant>();
                Leader = CreateEntity("city_lord", "CityLord", SubFactionId.MotorIronRiders, CharacterRole.CityLord, new Vector3(2f, 0f, 0f));
            }

            private CharacterEntity CreateEntity(string id, string displayName, SubFactionId faction, CharacterRole role, Vector3 position)
            {
                var go = new GameObject(displayName);
                go.transform.SetParent(_root.transform);
                go.transform.position = position;
                var entity = go.AddComponent<CharacterEntity>();
                var data = CharacterData.Create(id, displayName, faction, role);
                data.TrustToPlayer = 100;
                entity.Bind(data);
                return entity;
            }

            public void Dispose()
            {
                CharacterEntity.HostilityResolver = null;
                Object.DestroyImmediate(_root);
            }
        }
    }
}

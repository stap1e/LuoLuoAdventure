using System;
using System.Linq;
using System.Reflection;
using LuoLuoTrip.Combat;
using LuoLuoTrip.UI;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderActionExpansionTests
    {
        [Test]
        public void CommanderActionType_IncludesDefendObjectiveAndFocusFire()
        {
            Assert.That(Enum.IsDefined(typeof(CommanderActionType), "DefendObjective"), Is.True);
            Assert.That(Enum.IsDefined(typeof(CommanderActionType), "FocusFire"), Is.True);
            Assert.That(Enum.IsDefined(typeof(CommanderCommandType), "DefendObjective"), Is.True);
            Assert.That(Enum.IsDefined(typeof(CommanderCommandType), "FocusFire"), Is.True);
        }

        [Test]
        public void Presenter_ReturnsReadableFiveActionDescriptors()
        {
            var state = new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "CityGateCore",
                LastDefendObjectiveAllowed = true,
                LastFocusFireAllowed = true,
                LastObjectiveTargetName = "CityGateCore",
                LastFocusTargetName = "BeastRaider_01"
            };

            var descriptors = CommanderActionPresenter.BuildDescriptors(state);

            Assert.That(descriptors, Has.Count.EqualTo(5));
            Assert.That(descriptors.Any(d => d.ActionType == CommanderActionType.DefendObjective && d.DisplayName == "DefendObjective"), Is.True);
            Assert.That(descriptors.Any(d => d.ActionType == CommanderActionType.FocusFire && d.DisplayName == "FocusFire"), Is.True);
            Assert.That(descriptors.Find(d => d.ActionType == CommanderActionType.DefendObjective).TargetName, Is.EqualTo("CityGateCore"));
            Assert.That(descriptors.Find(d => d.ActionType == CommanderActionType.FocusFire).TargetName, Is.EqualTo("BeastRaider_01"));
        }

        [Test]
        public void DeniedExpansionDescriptors_HaveReasonsAndSuggestions()
        {
            var descriptors = CommanderActionPresenter.BuildDescriptors(new CommanderControlRuntimeState
            {
                LastSelectedTargetName = "None",
                LastDefendObjectiveReason = "No ally selected",
                LastFocusFireReason = "No hostile target selected"
            });

            var defend = descriptors.Find(d => d.ActionType == CommanderActionType.DefendObjective);
            var focus = descriptors.Find(d => d.ActionType == CommanderActionType.FocusFire);

            Assert.That(defend.IsAllowed, Is.False);
            Assert.That(defend.DenialReason, Is.EqualTo("No ally selected"));
            Assert.That(defend.Suggestion, Does.Contain("G"));
            Assert.That(focus.IsAllowed, Is.False);
            Assert.That(focus.DenialReason, Is.EqualTo("No hostile target selected"));
            Assert.That(focus.Suggestion, Does.Contain("F"));
        }
    }

    public class DefendObjectiveCommandTests
    {
        [Test]
        public void TryIssueDefendObjective_AssignsAllyAIAndState()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                Assert.That(fixture.Controller.TryIssueDefendObjective(fixture.Ally, fixture.Objective), Is.True);
                Assert.That(fixture.AllyAI.DefendTarget, Is.EqualTo(fixture.Objective.transform));
                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.DefendObjective));
                Assert.That(fixture.Controller.State.TacticalCommand.DefendTarget, Is.EqualTo(fixture.Objective));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void TryIssueDefendObjective_DeniesInvalidInputs()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                Assert.That(fixture.Controller.TryIssueDefendObjective(null, fixture.Objective), Is.False);
                Assert.That(fixture.Controller.State.LastDefendObjectiveReason, Is.EqualTo("No ally selected"));

                Assert.That(fixture.Controller.TryIssueDefendObjective(fixture.Ally, null), Is.False);
                Assert.That(fixture.Controller.State.LastDefendObjectiveReason, Is.EqualTo("No objective selected"));

                fixture.Ally.Data.IsAlive = false;
                Assert.That(fixture.Controller.TryIssueDefendObjective(fixture.Ally, fixture.Objective), Is.False);
                Assert.That(fixture.Controller.State.LastDefendObjectiveReason, Is.EqualTo("Ally dead"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void TryIssueDefendObjective_DeniesWhenTacticalCommandDisabled()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                fixture.Ally.Data.AllowTacticalCommand = false;

                Assert.That(fixture.Controller.TryIssueDefendObjective(fixture.Ally, fixture.Objective), Is.False);
                Assert.That(fixture.Controller.State.LastDefendObjectiveReason, Is.EqualTo("Tactical command disabled"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void ActiveDefendObjective_ClearsWhenObjectiveDead()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                Assert.That(fixture.Controller.TryIssueDefendObjective(fixture.Ally, fixture.Objective), Is.True);
                fixture.Objective.Data.IsAlive = false;

                fixture.InvokeControllerUpdate();

                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.None));
                Assert.That(fixture.AllyAI.DefendTarget, Is.Null);
            }
            finally { fixture.Dispose(); }
        }
    }

    public class FocusFireCommandTests
    {
        [Test]
        public void TryIssueFocusFire_AssignsNearbyResponders()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                Assert.That(fixture.Controller.TryIssueFocusFire(fixture.Enemy), Is.True);

                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.EqualTo(fixture.EnemyCombatant));
                Assert.That(fixture.OtherAllyAI.ForcedAttackTarget, Is.EqualTo(fixture.EnemyCombatant));
                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.FocusFire));
                Assert.That(fixture.Controller.State.TacticalCommand.ResponderCount, Is.EqualTo(2));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void TryIssueFocusFire_DeniesInvalidTargetsAndNoResponders()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                Assert.That(fixture.Controller.TryIssueFocusFire(null), Is.False);
                Assert.That(fixture.Controller.State.LastFocusFireReason, Is.EqualTo("No hostile target selected"));

                fixture.EnemyCombatant.ApplyHealthDamage(9999f);
                Assert.That(fixture.Controller.TryIssueFocusFire(fixture.Enemy), Is.False);
                Assert.That(fixture.Controller.State.LastFocusFireReason, Is.EqualTo("Target dead"));
            }
            finally { fixture.Dispose(); }

            fixture = new CommanderActionTestFixture(includeAllies: false);
            try
            {
                Assert.That(fixture.Controller.TryIssueFocusFire(fixture.Enemy), Is.False);
                Assert.That(fixture.Controller.State.LastFocusFireReason, Is.EqualTo("No nearby responders"));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void FocusFireExpiry_ClearsOnlyTrackedResponders()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                var unrelated = fixture.CreateEntity("unrelated", "Unrelated Ally", SubFactionId.MotorIronRiders, CharacterRole.Minion, new Vector3(50f, 0f, 50f));
                var unrelatedAI = unrelated.gameObject.AddComponent<SimpleCombatAI>();
                fixture.InvokeAwake(unrelatedAI);
                unrelatedAI.SetFocusFireTarget(fixture.EnemyCombatant);

                Assert.That(fixture.Controller.TryIssueFocusFire(fixture.Enemy), Is.True);
                fixture.Controller.State.TacticalCommand.ExpiresAtTime = Time.time - 0.1f;

                fixture.InvokeControllerUpdate();

                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.Null);
                Assert.That(fixture.OtherAllyAI.ForcedAttackTarget, Is.Null);
                Assert.That(unrelatedAI.ForcedAttackTarget, Is.EqualTo(fixture.EnemyCombatant));
                Assert.That(fixture.Controller.State.ActiveCommand, Is.EqualTo(CommanderCommandType.None));
            }
            finally { fixture.Dispose(); }
        }
    }

    public class TacticalCommandStateExpansionTests
    {
        [Test]
        public void DefendObjective_StateStoresTargetAndRadius()
        {
            var ally = CommanderActionTestFixture.CreateDetachedEntity("ally", "Ally", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            var objective = CommanderActionTestFixture.CreateDetachedEntity("core", "CityGateCore", SubFactionId.MotorIronRiders, CharacterRole.Minion);
            try
            {
                var state = new TacticalCommandState();
                state.SetDefendObjective(ally, objective, 6f, 2f);

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.DefendObjective));
                Assert.That(state.Target, Is.EqualTo(ally));
                Assert.That(state.DefendTarget, Is.EqualTo(objective));
                Assert.That(state.DefendRadius, Is.EqualTo(6f));
                Assert.That(state.IssuedByCommander, Is.True);
                Assert.That(state.StatusText, Does.Contain("DefendObjective"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ally.gameObject);
                UnityEngine.Object.DestroyImmediate(objective.gameObject);
            }
        }

        [Test]
        public void FocusFire_StateStoresTargetDurationAndResponders()
        {
            var target = CommanderActionTestFixture.CreateDetachedEntity("raider", "BeastRaider_01", SubFactionId.BeastIronClaw, CharacterRole.Minion);
            try
            {
                var combatant = target.GetComponent<Combatant>();
                var state = new TacticalCommandState();
                state.SetFocusFire(target, combatant, 8f, 3, 10f);

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.FocusFire));
                Assert.That(state.FocusTarget, Is.EqualTo(combatant));
                Assert.That(state.ResponderCount, Is.EqualTo(3));
                Assert.That(state.RemainingDuration(14f), Is.EqualTo(4f));
                Assert.That(state.IsExpired(17.9f), Is.False);
                Assert.That(state.IsExpired(18.1f), Is.True);
                Assert.That(state.StatusText, Does.Contain("FocusFire"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
        }

        [Test]
        public void Clear_ResetsExpansionFields()
        {
            var target = CommanderActionTestFixture.CreateDetachedEntity("raider", "BeastRaider_01", SubFactionId.BeastIronClaw, CharacterRole.Minion);
            try
            {
                var state = new TacticalCommandState();
                state.SetFocusFire(target, target.GetComponent<Combatant>(), 8f, 2, 1f);
                state.Clear();

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.None));
                Assert.That(state.FocusTarget, Is.Null);
                Assert.That(state.DefendTarget, Is.Null);
                Assert.That(state.ResponderCount, Is.EqualTo(0));
                Assert.That(state.IssuedByCommander, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
        }
    }

    public class SimpleCombatAICommanderActionTests
    {
        [Test]
        public void SetDefendObjective_StoresTargetRadiusAndClearResets()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                fixture.AllyAI.SetDefendObjective(fixture.Objective.transform, 7f);

                Assert.That(fixture.AllyAI.DefendTarget, Is.EqualTo(fixture.Objective.transform));
                Assert.That(fixture.AllyAI.DefendRadius, Is.EqualTo(7f));
                Assert.That(fixture.AllyAI.DefendLeashRadius, Is.GreaterThan(7f));
                Assert.That(fixture.AllyAI.CommanderCommandStatus, Does.Contain("Defending"));

                fixture.AllyAI.ClearDefendObjective();

                Assert.That(fixture.AllyAI.DefendTarget, Is.Null);
                Assert.That(fixture.AllyAI.DefendRadius, Is.EqualTo(0f));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void SetFocusFireTarget_StoresAndClearsForcedTarget()
        {
            var fixture = new CommanderActionTestFixture();
            try
            {
                fixture.AllyAI.SetFocusFireTarget(fixture.EnemyCombatant);
                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.EqualTo(fixture.EnemyCombatant));

                fixture.AllyAI.SetFocusFireTarget(null);
                Assert.That(fixture.AllyAI.ForcedAttackTarget, Is.Null);
            }
            finally { fixture.Dispose(); }
        }
    }

    public class CommanderActionShortcutHelpTests
    {
        [Test]
        public void ShortcutHelp_IncludesExpansionKeysAndExistingDebugKeys()
        {
            var text = string.Join("\n", DemoFlowHud.BuildShortcutHelpLines(false));

            foreach (var key in new[] { "G", "F", "1", "2", "3", "F7", "F8", "F5", "F9", "F10" })
                Assert.That(text, Does.Contain(key));
        }
    }

    internal sealed class CommanderActionTestFixture : IDisposable
    {
        private readonly GameObject _root = new GameObject("CommanderActionEditModeFixture");

        public CommanderControlController Controller { get; }
        public CharacterEntity Ally { get; private set; }
        public SimpleCombatAI AllyAI { get; private set; }
        public CharacterEntity OtherAlly { get; private set; }
        public SimpleCombatAI OtherAllyAI { get; private set; }
        public CharacterEntity Objective { get; }
        public CharacterEntity Enemy { get; }
        public Combatant EnemyCombatant { get; }

        public CommanderActionTestFixture(bool includeAllies = true)
        {
            CharacterEntity.HostilityResolver = (a, b) => a != b && (GameConstants.IsBeastSubFaction(a) || GameConstants.IsBeastSubFaction(b));

            var player = CreateEntity("player", "Commander", SubFactionId.MotorIronRiders, CharacterRole.Minion, Vector3.zero);
            player.gameObject.AddComponent<CombatController>();
            Controller = player.gameObject.AddComponent<CommanderControlController>();
            InvokeAwake(Controller);

            if (includeAllies)
            {
                Ally = CreateEntity("ally", "Low-Rank Ally", SubFactionId.MotorIronRiders, CharacterRole.Minion, new Vector3(1f, 0f, 0f));
                AllyAI = Ally.gameObject.AddComponent<SimpleCombatAI>();
                InvokeAwake(AllyAI);

                OtherAlly = CreateEntity("ally_2", "Low-Rank Ally 2", SubFactionId.MotorIronRiders, CharacterRole.Minion, new Vector3(1.5f, 0f, 0f));
                OtherAllyAI = OtherAlly.gameObject.AddComponent<SimpleCombatAI>();
                InvokeAwake(OtherAllyAI);
            }

            Objective = CreateEntity("core", "CityGateCore", SubFactionId.MotorIronRiders, CharacterRole.Minion, new Vector3(4f, 0f, 0f));
            Enemy = CreateEntity("raider", "BeastRaider_01", SubFactionId.BeastIronClaw, CharacterRole.Minion, new Vector3(3f, 0f, 0f));
            EnemyCombatant = Enemy.GetComponent<Combatant>();
        }

        public CharacterEntity CreateEntity(string id, string displayName, SubFactionId faction, CharacterRole role, Vector3 position)
        {
            var entity = CreateDetachedEntity(id, displayName, faction, role);
            entity.transform.SetParent(_root.transform);
            entity.transform.position = position;
            entity.Data.TrustToPlayer = 100;
            return entity;
        }

        public static CharacterEntity CreateDetachedEntity(string id, string displayName, SubFactionId faction, CharacterRole role)
        {
            var go = new GameObject(displayName);
            var entity = go.AddComponent<CharacterEntity>();
            var data = CharacterData.Create(id, displayName, faction, role);
            data.TrustToPlayer = 100;
            entity.Bind(data);
            return entity;
        }

        public void InvokeControllerUpdate()
        {
            typeof(CommanderControlController)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(Controller, null);
        }

        public void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(behaviour, null);
        }

        public void Dispose()
        {
            CharacterEntity.HostilityResolver = null;
            UnityEngine.Object.DestroyImmediate(_root);
        }
    }
}

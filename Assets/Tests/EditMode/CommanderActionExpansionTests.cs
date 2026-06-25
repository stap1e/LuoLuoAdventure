using System;
using System.Linq;
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
        }

        [Test]
        public void DeniedExpansionDescriptors_HaveSuggestions()
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
            Assert.That(defend.Suggestion, Does.Contain("G"));
            Assert.That(focus.IsAllowed, Is.False);
            Assert.That(focus.Suggestion, Does.Contain("F"));
        }
    }

    public class TacticalCommandStateExpansionTests
    {
        private static CharacterEntity CreateEntity(string id, string name)
        {
            var go = new GameObject(name);
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(CharacterData.Create(id, name, SubFactionId.MotorIronRiders, CharacterRole.Minion));
            return entity;
        }

        [Test]
        public void DefendObjective_StateStoresTargetAndRadius()
        {
            var ally = CreateEntity("ally", "Ally");
            var objective = CreateEntity("core", "CityGateCore");
            try
            {
                var state = new TacticalCommandState();
                state.SetDefendObjective(ally, objective, 6f, 2f);

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.DefendObjective));
                Assert.That(state.Target, Is.EqualTo(ally));
                Assert.That(state.DefendTarget, Is.EqualTo(objective));
                Assert.That(state.DefendRadius, Is.EqualTo(6f));
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
            var target = CreateEntity("raider", "BeastRaider_01");
            try
            {
                var combatant = target.GetComponent<Combatant>();
                var state = new TacticalCommandState();
                state.SetFocusFire(target, combatant, 8f, 3, 10f);

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.FocusFire));
                Assert.That(state.FocusTarget, Is.EqualTo(combatant));
                Assert.That(state.ResponderCount, Is.EqualTo(3));
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
            var target = CreateEntity("raider", "BeastRaider_01");
            try
            {
                var state = new TacticalCommandState();
                state.SetFocusFire(target, target.GetComponent<Combatant>(), 8f, 2, 1f);
                state.Clear();

                Assert.That(state.CommandType, Is.EqualTo(CommanderCommandType.None));
                Assert.That(state.FocusTarget, Is.Null);
                Assert.That(state.DefendTarget, Is.Null);
                Assert.That(state.ResponderCount, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
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
}

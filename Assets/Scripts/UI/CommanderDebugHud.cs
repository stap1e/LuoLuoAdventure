using System;
using LuoLuoTrip.AI;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class CommanderDebugHud : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;

        private CommanderProfile _profile;
        private ControlPermissionResult _lastResult;
        private CommanderControlRuntimeState _runtimeState;

        public void SetProfile(CommanderProfile profile) => _profile = profile;
        public void SetLastControlResult(ControlPermissionResult result) => _lastResult = result;
        public void SetRuntimeState(CommanderControlRuntimeState state) => _runtimeState = state;
        private static string AllowedText(bool allowed) => allowed ? "Allowed" : "Denied";

        private void OnGUI()
        {
            if (!_visible) return;
            if (_profile == null) return;

            var layout = DebugUILayout.CommanderHud;
            var x = layout.x;
            var y = layout.y;
            var width = (int)layout.width;

            GUI.Label(new Rect(x, y, width, 20), $"=== Commander (Lv.{_profile.CommanderLevel}) ===");
            y += 20;
            GUI.Label(new Rect(x, y, width, 20), $"XP: {_profile.Experience}/{CommanderLevelSystem.ExperienceForLevel(_profile.CommanderLevel + 1)} | Capacity: {_profile.CommandCapacity}");
            y += 18;
            GUI.Label(new Rect(x, y, width, 20), $"Direct Rank: {_profile.MaxDirectControlRank} | Tact Rank: {_profile.MaxTacticalCommandRank}");
            y += 18;
            GUI.Label(new Rect(x, y, width, 20), $"Sync: {_profile.BaseSyncRate:P0} | Mecha: {_profile.MechaTrust} | Beast: {_profile.BeastTrust}");
            y += 18;
            GUI.Label(new Rect(x, y, width, 20), $"Balance: {_profile.BalanceScore}");
            y += 18;

            if (_profile.CanLevelUp)
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(x, y, width, 18), "LEVEL UP AVAILABLE!");
                GUI.color = Color.white;
                y += 18;
            }

            if (_runtimeState != null)
            {
                y += 22;

                if (_runtimeState.SelectedTarget != null && _runtimeState.SelectedTarget.Data != null)
                {
                    var td = _runtimeState.SelectedTarget.Data;
                    GUI.Label(new Rect(x, y, width, 18), $"Target: {td.DisplayName} [{td.Race}/{td.Faction}]");
                    y += 18;
                    GUI.Label(new Rect(x, y, width, 18), $"  Role: {td.Role} Rank: {td.CommandRank} ReqLv: {td.RequiredCommanderLevel}");
                    y += 18;
                    GUI.Label(new Rect(x, y, width, 18), $"  Trust: {_runtimeState.LastSelectedTargetTrust} Leader: {_runtimeState.LastSelectedTargetIsLeader}");
                    y += 18;
                    var ai = _runtimeState.SelectedTarget.GetComponent<Combat.SimpleCombatAI>();
                    if (ai != null)
                    {
                        GUI.Label(new Rect(x, y, width + 120, 18), $"  {CommanderActionPresenter.BuildProfileSummary(ai)}");
                        y += 18;
                        GUI.Label(new Rect(x, y, width + 120, 18), $"  {CommanderActionPresenter.BuildBehaviorSummary(ai)}");
                        y += 18;
                        GUI.Label(new Rect(x, y, width + 120, 18), $"  {CommanderActionPresenter.BuildResponseSummary(ai)}");
                        y += 18;
                        GUI.Label(new Rect(x, y, width + 160, 18), $"  Suggestion: {CommanderActionPresenter.BuildProfileSuggestion(ai)}");
                        y += 18;
                    }
                    else
                    {
                        GUI.Label(new Rect(x, y, width, 18), "  AI Profile: Default AI");
                        y += 18;
                    }
                    var descriptors = CommanderActionPresenter.BuildDescriptors(_runtimeState, _lastResult);
                    GUI.Label(new Rect(x, y, width + 160, 18), $"  {string.Join(" | ", descriptors.ConvertAll(CommanderActionPresenter.BuildStatusLine))}");
                    y += 18;
                }
                else
                {
                    GUI.Label(new Rect(x, y, width, 18), "Target: None");
                    y += 18;
                }

                if (!string.IsNullOrEmpty(_runtimeState.LastInputRoute))
                {
                    GUI.Label(new Rect(x, y, width, 18), $"E Route: {_runtimeState.LastInputRoute}");
                    y += 18;
                }

                if (_lastResult.IsAllowed || !string.IsNullOrEmpty(_lastResult.Reason))
                {
                    var color = _lastResult.IsAllowed ? Color.green : Color.red;
                    GUI.color = color;
                    GUI.Label(new Rect(x, y, width + 120, 18), $"Control: {_lastResult.Mode} | Sync: {_lastResult.SyncRate:P0}");
                    y += 18;
                    GUI.Label(new Rect(x, y, width + 120, 18), $"Reason: {_lastResult.Reason}");
                    GUI.color = Color.white;
                    y += 18;
                }

                if (!string.IsNullOrEmpty(_runtimeState.LastSuggestion))
                {
                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(x, y, width + 160, 18), $"Suggestion: {_runtimeState.LastSuggestion}");
                    GUI.color = Color.white;
                    y += 18;
                }

                var recommended = CommanderActionPresenter.GetRecommendedAction(_runtimeState);
                if (!string.IsNullOrEmpty(recommended.DisplayName))
                {
                    GUI.color = recommended.IsAllowed ? Color.green : Color.yellow;
                    GUI.Label(new Rect(x, y, width + 160, 18), $"Recommended: {recommended.DisplayName} - {(recommended.IsAllowed ? "Allowed" : recommended.Suggestion)}");
                    GUI.color = Color.white;
                    y += 18;
                }

                if (_runtimeState.IsDirectControllingOther && _runtimeState.DirectControlledEntity != null)
                {
                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(x, y, width, 18), $"DirectControl: {_runtimeState.DirectControlledEntity.Data?.DisplayName}");
                    GUI.color = Color.white;
                    y += 18;
                }

                if (_runtimeState.HasActiveCommand)
                {
                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(x, y, width, 18), $"Command: {_runtimeState.TacticalCommand.StatusText}");
                    GUI.color = Color.white;
                    y += 18;

                    if (_runtimeState.ActiveCommand == CommanderCommandType.FocusFire)
                    {
                        GUI.Label(new Rect(x, y, width, 18), $"  Responders: {_runtimeState.TacticalCommand.ResponderCount} | Duration: {_runtimeState.TacticalCommand.RemainingDuration(Time.time):F1}s");
                        y += 18;
                    }

                    if (_runtimeState.ActiveCommand == CommanderCommandType.DefendObjective)
                    {
                        GUI.Label(new Rect(x, y, width + 80, 18), $"  Objective: {_runtimeState.LastObjectiveTargetName} | Radius: {_runtimeState.TacticalCommand.DefendRadius:F1}");
                        y += 18;
                    }

                    if (_runtimeState.CommandTarget != null)
                    {
                        var cmdAI = _runtimeState.CommandTarget.GetComponent<Combat.SimpleCombatAI>();
                        if (cmdAI != null && cmdAI.NavController != null)
                        {
                            var nav = cmdAI.NavController;
                            GUI.Label(new Rect(x, y, width, 18), $"  Nav: {nav.NavState} | Dist: {nav.DistanceToDestination:F1} | NavMesh: {nav.Bridge.UseNavMesh}");
                            y += 18;
                        }
                    }
                }

                if (_runtimeState.IsSyncAssistActive)
                {
                    GUI.color = new Color(0.5f, 1f, 1f);
                    GUI.Label(new Rect(x, y, width, 18), $"SyncAssist: {_runtimeState.SyncAssistRemainingTime:F1}s");
                    GUI.color = Color.white;
                    y += 18;
                }
            }
        }
    }
}

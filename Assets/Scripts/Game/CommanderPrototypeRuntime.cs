using LuoLuoTrip.Audio;
using LuoLuoTrip.UI;
using UnityEngine;

namespace LuoLuoTrip
{
    public class CommanderPrototypeRuntime : MonoBehaviour
    {
        [SerializeField] private CommanderDebugHud _commanderHud;
        [SerializeField] private FactionStandingDebugPanel _factionPanel;
        [SerializeField] private MissionResultDebugPanel _missionPanel;
        [SerializeField] private MissionResultSummaryPanel _summaryPanel;
        [SerializeField] private CommanderControlHintPanel _hintPanel;
        [SerializeField] private FactionDeltaToastPanel _toastPanel;
        [SerializeField] private MissionChainSummaryPanel _chainSummaryPanel;
        [SerializeField] private TutorialFlowRuntime _tutorial;

        private MissionService _missionService;
        private MissionChainService _chainService;
        private CommanderProfile _profileBefore;

        private void Start()
        {
            var context = GameBootstrap.Context;
            if (context == null) return;

            if (_commanderHud != null)
                _commanderHud.SetProfile(context.CommanderProfile);

            if (_factionPanel != null)
                _factionPanel.SetService(context.ReputationService);

            if (_chainSummaryPanel != null)
                _chainSummaryPanel.SetChainService(context.MissionChainService);

            if (_chainSummaryPanel != null)
                _chainSummaryPanel.SetProfile(context.CommanderProfile);

            _missionService = context.MissionService;
            _chainService = context.MissionChainService;
        }

        private void Update()
        {
            if (_missionService == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                TestMechaVictory();
            if (Input.GetKeyDown(KeyCode.Alpha2))
                TestBeastVictory();
            if (Input.GetKeyDown(KeyCode.Alpha3))
                TestBalancedResolution();
        }

        private void TestMechaVictory()
        {
            Debug.Log("[DEBUG TRIGGER] Key 1: Test MechaVictory (not recorded to chain)");
            _profileBefore = CloneProfile();
            _missionService.StartMission("test_mecha");
            var consequence = _missionService.CompleteMissionWithOutcome(MissionOutcomeType.MechaVictory);
            RecordAndShow(consequence, "test_mecha", MissionOutcomeType.MechaVictory);
        }

        private void TestBeastVictory()
        {
            Debug.Log("[DEBUG TRIGGER] Key 2: Test BeastVictory (not recorded to chain)");
            _profileBefore = CloneProfile();
            _missionService.StartMission("test_beast");
            var consequence = _missionService.CompleteMissionWithOutcome(MissionOutcomeType.BeastVictory);
            RecordAndShow(consequence, "test_beast", MissionOutcomeType.BeastVictory);
        }

        private void TestBalancedResolution()
        {
            Debug.Log("[DEBUG TRIGGER] Key 3: Test BalancedResolution (not recorded to chain)");
            _profileBefore = CloneProfile();
            _missionService.StartMission("test_balance");
            var consequence = _missionService.CompleteMissionWithOutcome(MissionOutcomeType.BalancedResolution);
            RecordAndShow(consequence, "test_balance", MissionOutcomeType.BalancedResolution);
        }

        public void OnMissionCompleted(string missionId, MissionOutcomeType outcome, MissionConsequence consequence)
        {
            _profileBefore = _profileBefore ?? CloneProfile();
            RecordAndShow(consequence, missionId, outcome);
        }

        private void RecordAndShow(MissionConsequence consequence, string missionId, MissionOutcomeType outcome)
        {
            if (consequence == null) return;

            if (_chainService != null && !missionId.StartsWith("test_"))
            {
                _chainService.RecordMissionResult(missionId, outcome, consequence.CommanderExperienceDelta);
            }

            if (_missionPanel != null)
                _missionPanel.ShowConsequence(consequence);

            var context = GameBootstrap.Context;
            if (_commanderHud != null)
                _commanderHud.SetProfile(context?.CommanderProfile);

            if (_factionPanel != null)
                _factionPanel.SetService(context?.ReputationService);

            if (_summaryPanel != null && context != null)
            {
                var unlocked = _chainService?.State.UnlockedMissionIds.Count > 1
                    ? _chainService.State.UnlockedMissionIds[_chainService.State.UnlockedMissionIds.Count - 1]
                    : null;
                var modifier = _chainService?.BuildMissionModifiers("border_retaliation");
                _summaryPanel.ShowSummary(missionId, consequence, _profileBefore, context.CommanderProfile, unlocked, modifier);
            }

            if (_toastPanel != null && consequence.FactionDeltas != null)
            {
                _toastPanel.ShowFactionDeltas(consequence.FactionDeltas);
                AudioFeedbackService.PlayUI(AudioEventId.FactionDelta);
            }

            if (context != null && _profileBefore != null && context.CommanderProfile.CommanderLevel > _profileBefore.CommanderLevel)
            {
                AudioFeedbackService.PlayUI(AudioEventId.LevelUp);
            }

            Debug.Log($"[Commander] Mission complete: {consequence.Outcome}, XP: +{consequence.CommanderExperienceDelta}, Level: {context?.CommanderProfile.CommanderLevel}");
            _profileBefore = null;
        }

        private CommanderProfile CloneProfile()
        {
            var context = GameBootstrap.Context;
            if (context == null) return null;
            var p = new CommanderProfile
            {
                CommanderLevel = context.CommanderProfile.CommanderLevel,
                Experience = context.CommanderProfile.Experience,
                CommandCapacity = context.CommanderProfile.CommandCapacity,
                MaxDirectControlRank = context.CommanderProfile.MaxDirectControlRank,
                MaxTacticalCommandRank = context.CommanderProfile.MaxTacticalCommandRank,
                BaseSyncRate = context.CommanderProfile.BaseSyncRate,
                MechaTrust = context.CommanderProfile.MechaTrust,
                BeastTrust = context.CommanderProfile.BeastTrust,
                BalanceScore = context.CommanderProfile.BalanceScore
            };
            return p;
        }
    }
}

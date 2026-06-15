using LuoLuoTrip.UI;
using UnityEngine;

namespace LuoLuoTrip
{
    public class CommanderPrototypeRuntime : MonoBehaviour
    {
        [SerializeField] private CommanderDebugHud _commanderHud;
        [SerializeField] private FactionStandingDebugPanel _factionPanel;
        [SerializeField] private MissionResultDebugPanel _missionPanel;

        private MissionService _missionService;

        private void Start()
        {
            var context = GameBootstrap.Context;
            if (context == null) return;

            if (_commanderHud != null)
                _commanderHud.SetProfile(context.CommanderProfile);

            if (_factionPanel != null)
                _factionPanel.SetService(context.ReputationService);

            _missionService = context.MissionService;
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
            var state = _missionService.StartMission("test_mecha");
            var consequence = _missionService.CompleteMissionWithOutcome(MissionOutcomeType.MechaVictory);
            ShowConsequence(consequence);
        }

        private void TestBeastVictory()
        {
            var state = _missionService.StartMission("test_beast");
            var consequence = _missionService.CompleteMissionWithOutcome(MissionOutcomeType.BeastVictory);
            ShowConsequence(consequence);
        }

        private void TestBalancedResolution()
        {
            var state = _missionService.StartMission("test_balance");
            var consequence = _missionService.CompleteMissionWithOutcome(MissionOutcomeType.BalancedResolution);
            ShowConsequence(consequence);
        }

        private void ShowConsequence(MissionConsequence consequence)
        {
            if (consequence == null) return;

            if (_missionPanel != null)
                _missionPanel.ShowConsequence(consequence);

            if (_commanderHud != null)
                _commanderHud.SetProfile(GameBootstrap.Context?.CommanderProfile);

            Debug.Log($"[Commander] Mission complete: {consequence.Outcome}, XP: +{consequence.CommanderExperienceDelta}");
        }
    }
}

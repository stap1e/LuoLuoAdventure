using UnityEngine;

namespace LuoLuoTrip
{
    public class DemoFlowManager : MonoBehaviour
    {
        public const string ConvoyMissionId = "convoy_energy_conflict";
        public const string BorderMissionId = "border_retaliation";
        public const string CityGateMissionId = "city_gate_dispute";

        [SerializeField] private bool _refreshEveryFrame = true;

        private MissionChainState _overrideState;
        private DemoFlowState _lastLoggedStep = DemoFlowState.NotStarted;

        public DemoFlowState CurrentStep { get; private set; } = DemoFlowState.ConvoyAvailable;
        public string CurrentStepDisplayName => GetStepDisplayName(CurrentStep);
        public string CurrentObjectiveHint => GetObjectiveHint(CurrentStep);
        public string CurrentWorldTargetName => GetWorldTargetName(CurrentStep);

        private void Start()
        {
            RefreshFromMissionChain();
            _lastLoggedStep = CurrentStep;
        }

        private void Update()
        {
            if (!_refreshEveryFrame) return;

            var previous = CurrentStep;
            RefreshFromMissionChain();
            if (previous != CurrentStep && _lastLoggedStep != CurrentStep)
            {
                Debug.Log($"[DemoFlow] Step changed: {CurrentStepDisplayName} -> {CurrentObjectiveHint}");
                _lastLoggedStep = CurrentStep;
            }
        }

        public void SetMissionChainStateForTests(MissionChainState state)
        {
            _overrideState = state;
            RefreshFromMissionChain(state);
        }

        public void ClearMissionChainStateOverride()
        {
            _overrideState = null;
            RefreshFromMissionChain();
        }

        public void RefreshFromMissionChain()
        {
            if (_overrideState != null)
            {
                RefreshFromMissionChain(_overrideState);
                return;
            }

            RefreshFromMissionChain(GameBootstrap.Context?.MissionChainService?.State);
        }

        public void RefreshFromMissionChain(MissionChainService chainService)
        {
            RefreshFromMissionChain(chainService?.State);
        }

        public void RefreshFromMissionChain(MissionChainState state)
        {
            CurrentStep = ResolveStep(state);
        }

        public string GetNextMissionId()
        {
            return CurrentStep switch
            {
                DemoFlowState.ConvoyAvailable => ConvoyMissionId,
                DemoFlowState.BorderRetaliationAvailable => BorderMissionId,
                DemoFlowState.CityGateAvailable => CityGateMissionId,
                _ => null
            };
        }

        public static DemoFlowState ResolveStep(MissionChainState state)
        {
            if (state == null)
                return DemoFlowState.ConvoyAvailable;

            if (!state.HasCompleted(ConvoyMissionId))
                return DemoFlowState.ConvoyAvailable;

            if (!state.HasCompleted(BorderMissionId))
                return DemoFlowState.BorderRetaliationAvailable;

            if (!state.HasCompleted(CityGateMissionId))
                return DemoFlowState.CityGateAvailable;

            return DemoFlowState.AllMissionsComplete;
        }

        public static string GetStepDisplayName(DemoFlowState step)
        {
            return step switch
            {
                DemoFlowState.NotStarted => "Demo Not Started",
                DemoFlowState.ConvoyAvailable => "Mission 1: Convoy Energy Conflict",
                DemoFlowState.BorderRetaliationAvailable => "Mission 2: Border Retaliation",
                DemoFlowState.CityGateAvailable => "Mission 3: City Gate Dispute",
                DemoFlowState.AllMissionsComplete => "All Missions Complete",
                _ => "Demo Flow"
            };
        }

        public static string GetObjectiveHint(DemoFlowState step)
        {
            return step switch
            {
                DemoFlowState.NotStarted => "Start the commander prototype and follow the tutorial prompts.",
                DemoFlowState.ConvoyAvailable => "Protect the convoy, share energy, and avoid excessive casualties.",
                DemoFlowState.BorderRetaliationAvailable => "Travel to the border, survive retaliation, and protect allied units.",
                DemoFlowState.CityGateAvailable => "Go to the city gate, protect CityGateCore and BeastNegotiator, then defeat raiders.",
                DemoFlowState.AllMissionsComplete => "Review border and city stability; the three-mission demo chain is complete.",
                _ => "Follow the current mission objective."
            };
        }

        public static string GetWorldTargetName(DemoFlowState step)
        {
            return step switch
            {
                DemoFlowState.ConvoyAvailable => "Convoy Mission Area",
                DemoFlowState.BorderRetaliationAvailable => "Border Retaliation Area",
                DemoFlowState.CityGateAvailable => "City Gate Mission Area",
                DemoFlowState.AllMissionsComplete => "Mission Result Summary",
                _ => "Tutorial Area"
            };
        }

        public static string DisplayMissionName(string missionId)
        {
            return missionId switch
            {
                ConvoyMissionId => "Convoy Energy Conflict",
                BorderMissionId => "Border Retaliation",
                CityGateMissionId => "City Gate Dispute",
                _ => string.IsNullOrEmpty(missionId) ? "No mission" : missionId
            };
        }
    }
}

using System.Collections.Generic;
using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip.AI
{
    public class AIBehaviorScenarioMonitor : MonoBehaviour
    {
        [SerializeField] private bool _logDebugSummaries;
        [SerializeField] private float _logInterval = 5f;

        private readonly List<SimpleCombatAI> _trackedUnits = new List<SimpleCombatAI>();
        private float _logTimer;

        public int TrackedUnitCount => _trackedUnits.Count;

        private void Awake()
        {
            RefreshTrackedUnits();
        }

        private void Update()
        {
            if (!_logDebugSummaries) return;

            _logTimer -= Time.deltaTime;
            if (_logTimer > 0f) return;

            _logTimer = Mathf.Max(1f, _logInterval);
            Debug.Log(BuildScenarioSummary());
        }

        public void RefreshTrackedUnits()
        {
            _trackedUnits.Clear();
            _trackedUnits.AddRange(GetComponentsInChildren<SimpleCombatAI>(true));
            if (_trackedUnits.Count == 0)
                _trackedUnits.AddRange(FindObjectsOfType<SimpleCombatAI>());
        }

        public void Register(SimpleCombatAI ai)
        {
            if (ai != null && !_trackedUnits.Contains(ai))
                _trackedUnits.Add(ai);
        }

        public IReadOnlyList<UnitBehaviorSnapshot> GetSnapshots(bool refresh = true)
        {
            if (refresh)
                RefreshTrackedUnits();

            var snapshots = new List<UnitBehaviorSnapshot>(_trackedUnits.Count);
            foreach (var ai in _trackedUnits)
            {
                if (ai == null) continue;
                snapshots.Add(CreateSnapshot(ai));
            }
            return snapshots;
        }

        public string BuildScenarioSummary(bool refresh = true)
        {
            var snapshots = GetSnapshots(refresh);
            if (snapshots.Count == 0)
                return "[AIBehaviorScenarioMonitor] No AI units tracked.";

            var lines = new List<string> { "[AIBehaviorScenarioMonitor] CityGate AI behavior summary:" };
            foreach (var snapshot in snapshots)
                lines.Add(snapshot.ToSummaryLine());
            return string.Join("\n", lines);
        }

        public bool TryGetSnapshot(string unitName, out UnitBehaviorSnapshot snapshot, bool refresh = true)
        {
            foreach (var item in GetSnapshots(refresh))
            {
                if (item.UnitName == unitName || item.UnitName.Contains(unitName))
                {
                    snapshot = item;
                    return true;
                }
            }

            snapshot = default;
            return false;
        }

        public static UnitBehaviorSnapshot CreateSnapshot(SimpleCombatAI ai)
        {
            if (ai == null)
                return UnitBehaviorSnapshot.Default("Missing AI");

            var targetCombatant = ai.CurrentTarget != null ? ai.CurrentTarget : ai.ForcedAttackTarget;
            var target = targetCombatant != null ? targetCombatant.name : "None";
            var profile = ai.BehaviorProfile != null ? ai.BehaviorProfile.DisplayLabel : "Default AI";
            return new UnitBehaviorSnapshot(
                ai.name,
                profile,
                ai.CurrentBehaviorLabel,
                ai.LastProfileDecision,
                target,
                ai.DistanceFromHome,
                ai.IsRetreating,
                ai.IsDefending,
                ai.IsPursuingObjective,
                ai.ForcedAttackTarget != null);
        }

        public readonly struct UnitBehaviorSnapshot
        {
            public readonly string UnitName;
            public readonly string ProfileLabel;
            public readonly string CurrentBehaviorLabel;
            public readonly string LastProfileDecision;
            public readonly string CurrentTargetName;
            public readonly float DistanceFromHome;
            public readonly bool IsRetreating;
            public readonly bool IsDefending;
            public readonly bool IsPursuingObjective;
            public readonly bool IsFocusFireResponder;

            public UnitBehaviorSnapshot(
                string unitName,
                string profileLabel,
                string currentBehaviorLabel,
                string lastProfileDecision,
                string currentTargetName,
                float distanceFromHome,
                bool isRetreating,
                bool isDefending,
                bool isPursuingObjective,
                bool isFocusFireResponder)
            {
                UnitName = string.IsNullOrEmpty(unitName) ? "Unnamed AI" : unitName;
                ProfileLabel = string.IsNullOrEmpty(profileLabel) ? "Default AI" : profileLabel;
                CurrentBehaviorLabel = string.IsNullOrEmpty(currentBehaviorLabel) ? ProfileLabel : currentBehaviorLabel;
                LastProfileDecision = string.IsNullOrEmpty(lastProfileDecision) ? CurrentBehaviorLabel : lastProfileDecision;
                CurrentTargetName = string.IsNullOrEmpty(currentTargetName) ? "None" : currentTargetName;
                DistanceFromHome = distanceFromHome;
                IsRetreating = isRetreating;
                IsDefending = isDefending;
                IsPursuingObjective = isPursuingObjective;
                IsFocusFireResponder = isFocusFireResponder;
            }

            public static UnitBehaviorSnapshot Default(string unitName) => new UnitBehaviorSnapshot(
                unitName,
                "Default AI",
                "Default AI",
                "Default AI behavior",
                "None",
                0f,
                false,
                false,
                false,
                false);

            public string ToSummaryLine()
            {
                return $"- {UnitName}: {ProfileLabel} | {LastProfileDecision} | Target={CurrentTargetName} | Home={DistanceFromHome:F1} | Retreat={IsRetreating} | Defend={IsDefending} | Objective={IsPursuingObjective} | FocusResponder={IsFocusFireResponder}";
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace LuoLuoTrip.UI
{
    public class MissionOutcomePreviewHud : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;
        [SerializeField] private float _refreshInterval = 0.25f;

        private MissionOutcomePreviewService _previewService = new MissionOutcomePreviewService();
        private MissionService _missionService;
        private MissionChainService _chainService;
        private LuoLuoTripGameContext _context;
        private MissionOutcomePreview _cachedPreview;
        private readonly List<string> _cachedLines = new List<string>();
        private float _refreshTimer;

        public MissionOutcomePreview CachedPreview => _cachedPreview;

        public void SetContext(LuoLuoTripGameContext context)
        {
            _context = context;
            _missionService = context?.MissionService;
            _chainService = context?.MissionChainService;
            RefreshNow();
        }

        public void SetPreviewService(MissionOutcomePreviewService previewService)
        {
            _previewService = previewService ?? new MissionOutcomePreviewService();
            RefreshNow();
        }

        public void SetMissionService(MissionService missionService)
        {
            _missionService = missionService;
            RefreshNow();
        }

        public void SetChainService(MissionChainService chainService)
        {
            _chainService = chainService;
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (_previewService == null)
                _previewService = new MissionOutcomePreviewService();

            var context = _context ?? GameBootstrap.Context;
            var missionId = _missionService?.ActiveMission?.MissionId;
            if (string.IsNullOrEmpty(missionId))
                missionId = ResolveNextMissionId(_chainService ?? context?.MissionChainService);

            _cachedPreview = _previewService.BuildPreview(missionId, context);
            BuildDisplayLines(_cachedPreview, _cachedLines, 3);
            _refreshTimer = _refreshInterval;
        }

        public static void BuildDisplayLines(MissionOutcomePreview preview, List<string> lines, int maxRisks = 3)
        {
            lines?.Clear();
            if (lines == null) return;

            if (preview == null)
            {
                lines.Add("Mission Outcome Preview");
                lines.Add("No preview data.");
                return;
            }

            lines.Add($"Likely Outcome: {preview.likelyOutcome}");
            lines.Add($"Confidence: {preview.confidenceLabel ?? "Projected"}");
            if (!string.IsNullOrEmpty(preview.outcomeSummary))
                lines.Add($"Effect: {preview.outcomeSummary}");
            if (!string.IsNullOrEmpty(preview.consequenceSummary) && preview.consequenceSummary != preview.outcomeSummary)
                lines.Add($"Consequence Preview: {preview.consequenceSummary}");
            lines.Add($"Commander XP if completed now: +{preview.commanderXpPreview}");

            if (!string.IsNullOrEmpty(preview.previousOutcomeEffect))
                lines.Add(preview.previousOutcomeEffect);

            var riskCount = preview.risks?.Count ?? 0;
            if (riskCount > 0)
            {
                var limit = Mathf.Min(Mathf.Max(0, maxRisks), riskCount);
                for (var i = 0; i < limit; i++)
                {
                    var risk = preview.risks[i];
                    if (risk == null) continue;
                    lines.Add($"Risk: {risk.displayName} [{risk.severity}] {risk.currentValueText} / {risk.thresholdText}");
                    if (!string.IsNullOrEmpty(risk.suggestion))
                        lines.Add($"Suggestion: {risk.suggestion}");
                }
            }
            else
            {
                lines.Add("Risk: No major risk factors.");
            }

            if (preview.consequences != null && preview.consequences.Count > 0)
            {
                var count = Mathf.Min(2, preview.consequences.Count);
                for (var i = 0; i < count; i++)
                    lines.Add($"Consequence: {preview.consequences[i].displayText}");
            }

            if (!string.IsNullOrEmpty(preview.nextMissionHint))
                lines.Add(preview.nextMissionHint);
            lines.Add("Preview only — no XP or chain writes.");
        }

        private void Start()
        {
            SetContext(GameBootstrap.Context);
        }

        private void Update()
        {
            if (!_visible) return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
                RefreshNow();
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (_cachedPreview == null || _cachedLines.Count == 0)
                RefreshNow();

            var layout = DebugUILayout.MissionOutcomePreview;
            var x = layout.x;
            var y = layout.y;
            var width = layout.width;
            var height = layout.height;
            GUI.Box(new Rect(x - 4, y - 4, width + 8, height + 8), "");
            GUI.Label(new Rect(x, y, width, 18), "Mission Outcome Preview");
            y += 20;

            var maxLines = Mathf.FloorToInt((height - 24f) / 16f);
            for (var i = 0; i < _cachedLines.Count && i < maxLines; i++)
            {
                GUI.Label(new Rect(x, y, width + 80f, 16), _cachedLines[i]);
                y += 16;
            }
        }

        private static string ResolveNextMissionId(MissionChainService chainService)
        {
            var state = chainService?.State;
            if (state == null)
                return DemoFlowManager.ConvoyMissionId;
            if (!state.HasCompleted(DemoFlowManager.ConvoyMissionId))
                return DemoFlowManager.ConvoyMissionId;
            if (!state.HasCompleted(DemoFlowManager.BorderMissionId))
                return DemoFlowManager.BorderMissionId;
            return DemoFlowManager.CityGateMissionId;
        }
    }
}

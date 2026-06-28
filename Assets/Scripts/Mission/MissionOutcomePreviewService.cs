using System.Collections.Generic;
using LuoLuoTrip.Combat;
using UnityEngine;

namespace LuoLuoTrip
{
    public class MissionOutcomePreviewService
    {
        private const int DefaultCityGateMaxMechaCasualtiesForBalanced = 2;
        private const int DefaultCityGateMaxBeastCasualtiesForBalanced = 4;
        private const int DefaultCityGateMaxTotalCasualtiesForPartial = 8;

        public MissionOutcomePreview BuildPreview(string missionId, LuoLuoTripGameContext context)
        {
            if (string.IsNullOrEmpty(missionId))
                missionId = context?.MissionService?.ActiveMission?.MissionId;

            if (string.IsNullOrEmpty(missionId))
                return MissionOutcomePreview.Unavailable(string.Empty, "No active mission preview.");

            return missionId switch
            {
                DemoFlowManager.ConvoyMissionId => BuildConvoyPreview(context),
                DemoFlowManager.BorderMissionId => BuildBorderPreview(context),
                DemoFlowManager.CityGateMissionId => BuildCityGatePreview(context),
                _ => BuildGenericPreview(missionId, context)
            };
        }

        public MissionOutcomePreview BuildConvoyPreview(LuoLuoTripGameContext context)
        {
            var state = context?.MissionService?.ActiveMission;
            var runtime = Object.FindObjectOfType<ConvoyEnergyConflictRuntime>();
            var convoy = Object.FindObjectOfType<ConvoyObjective>();
            var energy = Object.FindObjectOfType<EnergyNodeObjective>();
            var mechaCasualties = runtime?.Encounter?.CountCasualties(MainRace.MotorTribe) ?? state?.MechaCasualties ?? 0;
            var beastCasualties = runtime?.Encounter?.CountCasualties(MainRace.BeastTribe) ?? state?.BeastCasualties ?? 0;
            var raidersCleared = runtime?.Encounter != null && runtime.Encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw);

            MissionOutcomeType outcome;
            if (convoy != null && convoy.IsDestroyed)
                outcome = MissionOutcomeType.BeastVictory;
            else if (energy != null && energy.IsCapturedByBeast)
                outcome = MissionOutcomeType.BeastVictory;
            else if (energy != null && energy.IsSharedByPlayer)
                outcome = mechaCasualties + beastCasualties <= 1 ? MissionOutcomeType.BalancedResolution : MissionOutcomeType.PartialSuccess;
            else if (raidersCleared)
                outcome = MissionOutcomeType.MechaVictory;
            else if (mechaCasualties + beastCasualties > 3)
                outcome = MissionOutcomeType.PartialSuccess;
            else
                outcome = MissionOutcomeType.BalancedResolution;

            var preview = BuildFromOutcome(DemoFlowManager.ConvoyMissionId, outcome, context, mechaCasualties, beastCasualties);
            preview.confidenceLabel = runtime != null && runtime.IsActive ? "Live" : "Projected";

            if (convoy != null && convoy.HealthRatio <= 0.35f)
                AddRisk(preview, "convoy_hp_low", "Convoy HP low", convoy.HealthRatio <= 0.2f ? "Critical" : "High",
                    $"{convoy.Health:0}/{convoy.MaxHealth:0}", ">35% HP", "Use G DefendObjective on Convoy.", convoy.HealthRatio <= 0.2f);
            if (energy != null && !energy.IsSharedByPlayer && energy.BeastCaptureProgress > 0f)
                AddRisk(preview, "energy_contested", "EnergyNode contested", "Medium",
                    $"{energy.BeastCaptureProgress:0.0}/{energy.BeastCaptureTime:0.0}", "Share before Beast capture", "Move to EnergyNode and share energy.");
            if (mechaCasualties + beastCasualties > 1)
                AddRisk(preview, "convoy_casualties", "Casualties high", "Medium",
                    $"{mechaCasualties + beastCasualties}", "<=1 for balanced", "Reduce casualties and defend allies.");
            if (!raidersCleared)
                AddRisk(preview, "convoy_raiders", "Raider pressure high", "Medium",
                    "Raiders active", "Raiders cleared", "Use F FocusFire on BeastRaider.");

            return FinalizePreview(preview);
        }

        public MissionOutcomePreview BuildBorderPreview(LuoLuoTripGameContext context)
        {
            var runtime = Object.FindObjectOfType<BorderRetaliationRuntime>();
            var modifier = runtime?.CurrentModifier ?? context?.MissionChainService?.BuildMissionModifiers(DemoFlowManager.BorderMissionId);
            var encounter = runtime?.Encounter;
            var mechaCasualties = encounter?.CountCasualties(MainRace.MotorTribe) ?? context?.MissionService?.ActiveMission?.MechaCasualties ?? 0;
            var beastCasualties = encounter?.CountCasualties(MainRace.BeastTribe) ?? context?.MissionService?.ActiveMission?.BeastCasualties ?? 0;
            var raidersCleared = encounter != null && encounter.AreAllRaidUnitsDefeated(SubFactionId.BeastIronClaw);

            MissionOutcomeType outcome;
            if (modifier != null && modifier.CeasefireActive)
                outcome = mechaCasualties + beastCasualties <= 1 ? MissionOutcomeType.BalancedResolution : MissionOutcomeType.BeastVictory;
            else if (modifier != null && modifier.LowTrustMode)
                outcome = mechaCasualties >= 3 ? MissionOutcomeType.Failed : MissionOutcomeType.PartialSuccess;
            else if (modifier != null && modifier.MechaCaptainTacticalOnly)
                outcome = raidersCleared ? MissionOutcomeType.BalancedResolution : MissionOutcomeType.PartialSuccess;
            else
                outcome = mechaCasualties > 2 ? MissionOutcomeType.PartialSuccess : MissionOutcomeType.MechaVictory;

            var preview = BuildFromOutcome(DemoFlowManager.BorderMissionId, outcome, context, mechaCasualties, beastCasualties);
            preview.confidenceLabel = runtime != null && runtime.Phase == MissionPhase.Active ? "Live" : "Projected";

            if (mechaCasualties > 1)
                AddRisk(preview, "border_allied_casualties", "Allied casualties high", mechaCasualties >= 3 ? "Critical" : "High",
                    mechaCasualties.ToString(), "<=1 preferred", "Use G DefendObjective on Allied Defense Point.", mechaCasualties >= 3);
            if (!raidersCleared)
                AddRisk(preview, "border_raiders", "Raider wave not cleared", "Medium",
                    "Raiders active", "Raiders cleared", "Use F FocusFire on Raider leader.");
            if (modifier != null && modifier.LowTrustMode)
                AddRisk(preview, "border_retreat_pressure", "Retreat pressure high", "Medium",
                    modifier.Description, "Hold defense point", "Stay in mission area and keep casualties low.");

            return FinalizePreview(preview);
        }

        public MissionOutcomePreview BuildCityGatePreview(LuoLuoTripGameContext context)
        {
            var runtime = Object.FindObjectOfType<CityGateDisputeRuntime>();
            if (runtime == null)
            {
                var projected = BuildCityGatePreview(true, true, false, 0, 0, context,
                    DefaultCityGateMaxMechaCasualtiesForBalanced,
                    DefaultCityGateMaxBeastCasualtiesForBalanced,
                    DefaultCityGateMaxTotalCasualtiesForPartial);
                projected.confidenceLabel = "Projected";
                return projected;
            }

            var coreSurvived = runtime.CoreSurvived && (runtime.CityGateCore == null || runtime.CityGateCore.IsAlive);
            var negotiatorSurvived = runtime.NegotiatorSurvived && runtime.BeastNegotiator?.Data?.IsAlive != false;
            var mechaCasualties = runtime.MechaCasualties;
            var beastCasualties = runtime.BeastCasualties;

            var preview = BuildCityGatePreview(coreSurvived, negotiatorSurvived, runtime.BeastRaidersDefeated,
                mechaCasualties, beastCasualties, context,
                runtime.MaxMechaCasualtiesForBalanced,
                runtime.MaxBeastCasualtiesForBalanced,
                runtime.MaxTotalCasualtiesForPartial);
            preview.confidenceLabel = runtime.Phase == MissionPhase.Active || runtime.Phase == MissionPhase.Tension ? "Live" : "Projected";

            var coreRatio = GetHealthRatio(runtime.CityGateCore);
            if (coreRatio >= 0f && coreRatio <= 0.35f)
                AddRisk(preview, "citygate_core_hp", "CityGateCore HP low", coreRatio <= 0.2f ? "Critical" : "High",
                    $"{coreRatio:P0}", ">35% HP", "Use G DefendObjective on CityGateCore.", coreRatio <= 0.2f);

            var negotiatorCombatant = runtime.BeastNegotiator != null ? runtime.BeastNegotiator.GetComponent<Combatant>() : null;
            var negotiatorRatio = GetHealthRatio(negotiatorCombatant);
            if (!negotiatorSurvived || (negotiatorRatio >= 0f && negotiatorRatio <= 0.35f))
                AddRisk(preview, "citygate_negotiator_hp", "BeastNegotiator HP low", !negotiatorSurvived || negotiatorRatio <= 0.2f ? "Critical" : "High",
                    negotiatorRatio >= 0f ? $"{negotiatorRatio:P0}" : "Threatened", ">35% HP", "Protect Negotiator; command Guard to defend.", !negotiatorSurvived || negotiatorRatio <= 0.2f);

            AddCityGateSharedRisks(preview, coreSurvived, negotiatorSurvived, runtime.BeastRaidersDefeated,
                mechaCasualties, beastCasualties,
                runtime.MaxMechaCasualtiesForBalanced,
                runtime.MaxBeastCasualtiesForBalanced,
                runtime.MaxTotalCasualtiesForPartial);
            return FinalizePreview(preview);
        }

        public MissionOutcomePreview BuildCityGatePreview(bool coreSurvived, bool negotiatorSurvived, bool raidersDefeated,
            int mechaCasualties, int beastCasualties, LuoLuoTripGameContext context = null,
            int maxMechaForBalanced = DefaultCityGateMaxMechaCasualtiesForBalanced,
            int maxBeastForBalanced = DefaultCityGateMaxBeastCasualtiesForBalanced,
            int maxTotalForPartial = DefaultCityGateMaxTotalCasualtiesForPartial)
        {
            var outcome = CityGateDisputeRuntime.ResolveOutcome(coreSurvived, negotiatorSurvived, raidersDefeated,
                mechaCasualties, beastCasualties, maxMechaForBalanced, maxBeastForBalanced, maxTotalForPartial);
            var preview = BuildFromOutcome(DemoFlowManager.CityGateMissionId, outcome, context, mechaCasualties, beastCasualties);
            preview.confidenceLabel = "Projected";
            AddCityGateSharedRisks(preview, coreSurvived, negotiatorSurvived, raidersDefeated,
                mechaCasualties, beastCasualties, maxMechaForBalanced, maxBeastForBalanced, maxTotalForPartial);
            return FinalizePreview(preview);
        }

        public string BuildPreviousOutcomeEffectText(string missionId, MissionChainState chainState)
        {
            if (string.IsNullOrEmpty(missionId) || chainState == null)
                return "No previous outcome modifier.";

            if (missionId == DemoFlowManager.CityGateMissionId && chainState.HasCompleted(DemoFlowManager.CityGateMissionId))
                return "Previous outcome effect: CityGate result affects future stability.";

            if (missionId == DemoFlowManager.BorderMissionId)
            {
                var outcome = chainState.GetLastOutcome(DemoFlowManager.ConvoyMissionId);
                return outcome switch
                {
                    MissionOutcomeType.MechaVictory => "Previous outcome effect: Convoy MechaVictory — Beast retaliation intensified.",
                    MissionOutcomeType.BeastVictory => "Previous outcome effect: Convoy BeastVictory — Mecha support reduced.",
                    MissionOutcomeType.BalancedResolution => "Previous outcome effect: Convoy BalancedResolution — Border hostility reduced.",
                    MissionOutcomeType.PartialSuccess => "Previous outcome effect: Convoy PartialSuccess — low trust raises border tension.",
                    MissionOutcomeType.Failed => "Previous outcome effect: Convoy Failed — low trust raises border tension.",
                    _ => "No previous outcome modifier."
                };
            }

            if (missionId == DemoFlowManager.CityGateMissionId)
            {
                var outcome = chainState.GetLastOutcome(DemoFlowManager.BorderMissionId);
                return outcome switch
                {
                    MissionOutcomeType.BalancedResolution => "Previous outcome effect: Border BalancedResolution — CityGate mainstream hostility reduced.",
                    MissionOutcomeType.MechaVictory => "Previous outcome effect: Border MechaVictory — Mecha hardliner pressure increased.",
                    MissionOutcomeType.BeastVictory => "Previous outcome effect: Border BeastVictory — Beast raider confidence increased.",
                    MissionOutcomeType.PartialSuccess => "Previous outcome effect: Border Partial/Failed — CityGate tension increased.",
                    MissionOutcomeType.Failed => "Previous outcome effect: Border Partial/Failed — CityGate tension increased.",
                    _ => "No previous outcome modifier."
                };
            }

            return "No previous outcome modifier.";
        }

        public string BuildRiskSummary(MissionOutcomePreview preview)
        {
            if (preview == null || preview.risks == null || preview.risks.Count == 0)
                return "No major risk factors.";

            var parts = new List<string>();
            for (var i = 0; i < preview.risks.Count && i < 3; i++)
                parts.Add(preview.risks[i].displayName);
            return string.Join("; ", parts);
        }

        private MissionOutcomePreview BuildGenericPreview(string missionId, LuoLuoTripGameContext context)
        {
            var state = context?.MissionService?.ActiveMission;
            if (state == null || state.MissionId != missionId)
                return MissionOutcomePreview.Unavailable(missionId, "No active mission preview.");

            var clone = CloneRuntimeState(state);
            clone.DetermineOutcome();
            return FinalizePreview(BuildFromOutcome(missionId, clone.Outcome, context, clone.MechaCasualties, clone.BeastCasualties));
        }

        private MissionOutcomePreview BuildFromOutcome(string missionId, MissionOutcomeType outcome,
            LuoLuoTripGameContext context, int mechaCasualties, int beastCasualties)
        {
            var state = new MissionRuntimeState
            {
                MissionId = missionId,
                Outcome = outcome,
                MechaCasualties = mechaCasualties,
                BeastCasualties = beastCasualties,
                PlayerRetreated = outcome == MissionOutcomeType.Failed
            };
            var consequence = MissionConsequenceResolver.Resolve(state);
            var preview = new MissionOutcomePreview
            {
                missionId = missionId,
                missionDisplayName = MissionOutcomeTextLibrary.DisplayMissionName(missionId),
                likelyOutcome = consequence.Outcome,
                confidenceLabel = "Projected",
                outcomeSummary = MissionOutcomeTextLibrary.BuildOutcomeSummary(consequence.Outcome),
                consequenceSummary = MissionOutcomeTextLibrary.FormatConsequenceSummary(consequence),
                commanderXpPreview = consequence.CommanderExperienceDelta,
                nextMissionHint = MissionOutcomeTextLibrary.BuildNextHint(missionId),
                previousOutcomeEffect = BuildPreviousOutcomeEffectText(missionId, context?.MissionChainService?.State),
                isFailureLikely = consequence.Outcome == MissionOutcomeType.Failed || consequence.Outcome == MissionOutcomeType.FailedEscalation,
                isBalancedLikely = consequence.Outcome == MissionOutcomeType.BalancedResolution || consequence.Outcome == MissionOutcomeType.BalancedMediation
            };

            if (consequence.FactionDeltas != null)
            {
                foreach (var delta in consequence.FactionDeltas)
                    preview.consequences.Add(MissionConsequencePreview.FromDelta(delta));
            }

            return preview;
        }

        private static MissionRuntimeState CloneRuntimeState(MissionRuntimeState source)
        {
            var clone = new MissionRuntimeState();
            if (source == null)
                return clone;

            clone.MissionId = source.MissionId;
            clone.MechaCasualties = source.MechaCasualties;
            clone.BeastCasualties = source.BeastCasualties;
            clone.ProtectedConvoy = source.ProtectedConvoy;
            clone.SharedResources = source.SharedResources;
            clone.EscalatedConflict = source.EscalatedConflict;
            clone.PlayerRetreated = source.PlayerRetreated;
            clone.Outcome = source.Outcome;
            if (source.Objectives != null)
            {
                foreach (var objective in source.Objectives)
                {
                    if (objective == null) continue;
                    clone.Objectives.Add(new MissionObjective
                    {
                        ObjectiveId = objective.ObjectiveId,
                        Description = objective.Description,
                        IsCompleted = objective.IsCompleted,
                        IsFailed = objective.IsFailed,
                        Progress = objective.Progress,
                        RequiredProgress = objective.RequiredProgress
                    });
                }
            }
            return clone;
        }

        private static void AddCityGateSharedRisks(MissionOutcomePreview preview, bool coreSurvived, bool negotiatorSurvived,
            bool raidersDefeated, int mechaCasualties, int beastCasualties, int maxMechaForBalanced,
            int maxBeastForBalanced, int maxTotalForPartial)
        {
            if (!coreSurvived)
                AddRisk(preview, "citygate_core_destroyed", "CityGateCore destroyed", "Critical", "Destroyed", "Core alive", "Use G DefendObjective on CityGateCore.", true);
            if (!negotiatorSurvived)
                AddRisk(preview, "citygate_negotiator_dead", "BeastNegotiator dead", "Critical", "Dead", "Negotiator alive", "Protect Negotiator; command Guard to defend.", true);
            if (mechaCasualties > maxMechaForBalanced)
                AddRisk(preview, "citygate_mecha_casualties", "Mecha casualties high", "High", mechaCasualties.ToString(), $"<={maxMechaForBalanced}", "Pull allies back and use DefendObjective.");
            if (beastCasualties > maxBeastForBalanced)
                AddRisk(preview, "citygate_beast_casualties", "Beast casualties high", "High", beastCasualties.ToString(), $"<={maxBeastForBalanced}", "Avoid excess Beast casualties to preserve negotiation.");
            if (mechaCasualties + beastCasualties > maxTotalForPartial)
                AddRisk(preview, "citygate_escalation", "Hardliner escalation risk", "Critical", (mechaCasualties + beastCasualties).ToString(), $"<={maxTotalForPartial}", "FocusFire Hardliner or use TacticalCommand if available.", true);
            if (!raidersDefeated)
                AddRisk(preview, "citygate_raiders", "Raider pressure high", "Medium", "Raiders active", "Raiders cleared", "Use F FocusFire on BeastRaider.");
        }

        private static void AddRisk(MissionOutcomePreview preview, string id, string name, string severity,
            string current, string threshold, string suggestion, bool critical = false)
        {
            if (preview == null) return;
            preview.risks.Add(new MissionOutcomeRisk(id, name, severity, current, threshold, suggestion, critical));
        }

        private static MissionOutcomePreview FinalizePreview(MissionOutcomePreview preview)
        {
            if (preview == null)
                return MissionOutcomePreview.Unavailable(string.Empty, "No preview data.");

            preview.hasCriticalRisk = false;
            if (preview.risks != null)
            {
                preview.risks.Sort((a, b) => RiskRank(b).CompareTo(RiskRank(a)));
                foreach (var risk in preview.risks)
                    preview.hasCriticalRisk |= risk != null && risk.isCritical;
            }
            return preview;
        }

        private static int RiskRank(MissionOutcomeRisk risk)
        {
            if (risk == null) return 0;
            if (risk.isCritical || risk.severity == "Critical") return 3;
            if (risk.severity == "High") return 2;
            if (risk.severity == "Medium") return 1;
            return 0;
        }

        private static float GetHealthRatio(Combatant combatant)
        {
            if (combatant == null || combatant.Stats.maxHealth <= 0f)
                return -1f;
            return Mathf.Clamp01(combatant.CurrentHealth / combatant.Stats.maxHealth);
        }
    }
}

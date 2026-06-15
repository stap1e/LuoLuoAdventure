using System;
using System.Collections.Generic;

namespace LuoLuoTrip
{
    public static class MissionConsequenceResolver
    {
        public static MissionConsequence Resolve(MissionRuntimeState state)
        {
            if (state == null) return MissionConsequence.Empty(MissionOutcomeType.Failed);

            if (state.Outcome == MissionOutcomeType.Failed && !state.PlayerRetreated)
                state.DetermineOutcome();

            var consequence = new MissionConsequence
            {
                Outcome = state.Outcome,
                FactionDeltas = new List<FactionStandingDelta>()
            };

            switch (state.Outcome)
            {
                case MissionOutcomeType.MechaVictory:
                    ResolveMechaVictory(state, consequence);
                    break;
                case MissionOutcomeType.BeastVictory:
                    ResolveBeastVictory(state, consequence);
                    break;
                case MissionOutcomeType.BalancedResolution:
                    ResolveBalancedResolution(state, consequence);
                    break;
                case MissionOutcomeType.PartialSuccess:
                    ResolvePartialSuccess(state, consequence);
                    break;
                case MissionOutcomeType.Failed:
                    ResolveFailed(state, consequence);
                    break;
            }

            ApplyCasualtyEffects(state, consequence);

            consequence.CommanderExperienceDelta = CalculateExperience(state);
            consequence.SummaryText = BuildSummary(state, consequence);

            return consequence;
        }

        private static void ResolveMechaVictory(MissionRuntimeState state, MissionConsequence consequence)
        {
            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                if (GameConstants.IsMotorSubFaction(id))
                {
                    consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, trust: 15, respect: 10, fear: -5, hostility: -10));
                }
                else
                {
                    consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, trust: -5, respect: -5, fear: 5, hostility: 15));
                }
            }
        }

        private static void ResolveBeastVictory(MissionRuntimeState state, MissionConsequence consequence)
        {
            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                if (GameConstants.IsBeastSubFaction(id))
                {
                    consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, trust: 15, respect: 10, fear: -5, hostility: -10));
                }
                else
                {
                    consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, trust: -5, respect: -5, fear: 5, hostility: 15));
                }
            }
        }

        private static void ResolveBalancedResolution(MissionRuntimeState state, MissionConsequence consequence)
        {
            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, trust: 5, respect: 5, hostility: -5));
            }
        }

        private static void ResolvePartialSuccess(MissionRuntimeState state, MissionConsequence consequence)
        {
            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, respect: -5, trust: -3));
            }
        }

        private static void ResolveFailed(MissionRuntimeState state, MissionConsequence consequence)
        {
            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                if (state.PlayerRetreated)
                    consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, respect: -10, trust: -5));
                else
                    consequence.FactionDeltas.Add(FactionStandingDelta.Create(id, respect: -15, fear: -5, hostility: 5));
            }
        }

        private static void ApplyCasualtyEffects(MissionRuntimeState state, MissionConsequence consequence)
        {
            var totalCasualties = state.MechaCasualties + state.BeastCasualties;
            if (totalCasualties <= 2) return;

            var warExhaustion = Math.Min(20, totalCasualties * 3);
            var balancePenalty = -Math.Min(10, totalCasualties * 2);

            foreach (SubFactionId id in Enum.GetValues(typeof(SubFactionId)))
            {
                var race = GameConstants.GetMainRace(id);
                var casualties = race == MainRace.MotorTribe ? state.MechaCasualties : state.BeastCasualties;
                if (casualties > 0)
                {
                    var delta = FactionStandingDelta.Create(id,
                        warExhaustion: warExhaustion);
                    consequence.FactionDeltas.Add(delta);
                }
            }
        }

        private static int CalculateExperience(MissionRuntimeState state)
        {
            return state.Outcome switch
            {
                MissionOutcomeType.MechaVictory => 200,
                MissionOutcomeType.BeastVictory => 200,
                MissionOutcomeType.BalancedResolution => 300,
                MissionOutcomeType.PartialSuccess => 80,
                MissionOutcomeType.Failed => 30,
                _ => 0
            };
        }

        private static string BuildSummary(MissionRuntimeState state, MissionConsequence consequence)
        {
            var parts = new List<string>
            {
                $"Outcome: {state.Outcome}",
                $"XP: +{consequence.CommanderExperienceDelta}"
            };

            if (state.MechaCasualties > 0) parts.Add($"Mecha casualties: {state.MechaCasualties}");
            if (state.BeastCasualties > 0) parts.Add($"Beast casualties: {state.BeastCasualties}");
            if (state.PlayerRetreated) parts.Add("Player retreated");

            return string.Join(" | ", parts);
        }
    }
}

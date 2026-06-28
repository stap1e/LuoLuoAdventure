using System;

namespace LuoLuoTrip
{
    [Serializable]
    public class MissionOutcomeRisk
    {
        public string riskId;
        public string displayName;
        public string severity;
        public string currentValueText;
        public string thresholdText;
        public string suggestion;
        public bool isCritical;

        public MissionOutcomeRisk() { }

        public MissionOutcomeRisk(string riskId, string displayName, string severity,
            string currentValueText, string thresholdText, string suggestion, bool isCritical = false)
        {
            this.riskId = riskId;
            this.displayName = displayName;
            this.severity = severity;
            this.currentValueText = currentValueText;
            this.thresholdText = thresholdText;
            this.suggestion = suggestion;
            this.isCritical = isCritical;
        }
    }
}

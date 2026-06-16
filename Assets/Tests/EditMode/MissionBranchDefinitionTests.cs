using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionBranchDefinitionTests
    {
        [Test]
        public void FromModifier_MapsAllFields()
        {
            var modifier = new MissionModifier
            {
                ModifierId = "border_beast_retaliation",
                SourceMissionId = "convoy_energy_conflict",
                SourceOutcome = MissionOutcomeType.MechaVictory,
                Description = "Beast retaliation",
                BeastHostilityMultiplier = 1.5f,
                MechaSupportMultiplier = 0.8f,
                InitialHostilityOffset = 10f,
                CeasefireActive = false,
                MechaCaptainTacticalOnly = true,
                LowTrustMode = false
            };

            var def = MissionBranchDefinition.FromModifier(modifier);

            Assert.That(def.BranchId, Is.EqualTo("border_beast_retaliation"));
            Assert.That(def.SourceMissionId, Is.EqualTo("convoy_energy_conflict"));
            Assert.That(def.RequiredOutcome, Is.EqualTo(MissionOutcomeType.MechaVictory));
            Assert.That(def.Description, Is.EqualTo("Beast retaliation"));
            Assert.That(def.BeastHostilityMultiplier, Is.EqualTo(1.5f));
            Assert.That(def.MechaSupportMultiplier, Is.EqualTo(0.8f));
            Assert.That(def.InitialHostilityOffset, Is.EqualTo(10f));
            Assert.That(def.CeasefireActive, Is.False);
            Assert.That(def.MechaCaptainTacticalOnly, Is.True);
            Assert.That(def.LowTrustMode, Is.False);
        }

        [Test]
        public void ToModifier_RoundTripsFields()
        {
            var original = new MissionModifier
            {
                ModifierId = "border_mecha_distrust",
                SourceMissionId = "convoy_energy_conflict",
                SourceOutcome = MissionOutcomeType.BeastVictory,
                Description = "Mecha distrust",
                BeastHostilityMultiplier = 1f,
                MechaSupportMultiplier = 0.5f,
                MechaCaptainTacticalOnly = true
            };

            var def = MissionBranchDefinition.FromModifier(original);
            var roundTrip = def.ToModifier();

            Assert.That(roundTrip.ModifierId, Is.EqualTo(original.ModifierId));
            Assert.That(roundTrip.SourceMissionId, Is.EqualTo(original.SourceMissionId));
            Assert.That(roundTrip.SourceOutcome, Is.EqualTo(original.SourceOutcome));
            Assert.That(roundTrip.Description, Is.EqualTo(original.Description));
            Assert.That(roundTrip.BeastHostilityMultiplier, Is.EqualTo(original.BeastHostilityMultiplier));
            Assert.That(roundTrip.MechaSupportMultiplier, Is.EqualTo(original.MechaSupportMultiplier));
            Assert.That(roundTrip.MechaCaptainTacticalOnly, Is.EqualTo(original.MechaCaptainTacticalOnly));
        }

        [Test]
        public void FromModifier_Ceasefire_SetsCeasefireActive()
        {
            var modifier = new MissionModifier
            {
                ModifierId = "border_ceasefire",
                CeasefireActive = true
            };

            var def = MissionBranchDefinition.FromModifier(modifier);
            Assert.That(def.CeasefireActive, Is.True);
        }

        [Test]
        public void FromModifier_LowTrust_SetsEvacuationTimer()
        {
            var modifier = new MissionModifier
            {
                ModifierId = "border_low_trust",
                LowTrustMode = true
            };

            var def = MissionBranchDefinition.FromModifier(modifier);
            Assert.That(def.LowTrustMode, Is.True);
        }
    }
}

using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class MissionModifierEncounterTests
    {
        [Test]
        public void MechaVictory_Modifier_IncreasesBeastHostility()
        {
            var mod = new MissionModifier
            {
                ModifierId = "border_beast_retaliation",
                BeastHostilityMultiplier = 1.5f,
                MechaSupportMultiplier = 1f
            };
            Assert.Greater(mod.BeastHostilityMultiplier, 1f);
        }

        [Test]
        public void BeastVictory_Modifier_ReducesMechaSupport()
        {
            var mod = new MissionModifier
            {
                ModifierId = "border_mecha_distrust",
                BeastHostilityMultiplier = 0.8f,
                MechaSupportMultiplier = 0.5f
            };
            Assert.Less(mod.MechaSupportMultiplier, 1f);
        }

        [Test]
        public void BalancedResolution_Modifier_ReducesEnemies()
        {
            var mod = new MissionModifier
            {
                ModifierId = "border_ceasefire",
                BeastHostilityMultiplier = 0.6f,
                MechaSupportMultiplier = 0.8f
            };
            Assert.Less(mod.BeastHostilityMultiplier, 1f);
            Assert.Less(mod.MechaSupportMultiplier, 1f);
        }

        [Test]
        public void EncounterDefinition_AppliesModifier()
        {
            var def = new EncounterDefinition();
            var mod = new MissionModifier
            {
                BeastHostilityMultiplier = 2f,
                MechaSupportMultiplier = 0.5f,
                InitialHostilityOffset = -1f
            };

            var go = new GameObject("Enc");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(def);
                enc.ApplyMissionModifier(mod);

                Assert.AreEqual(2f, def.BeastHostilityMultiplier);
                Assert.AreEqual(0.5f, def.MechaSupportMultiplier);
                Assert.AreEqual(-1f, def.InitialHostilityOffset);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void GetFactionMultiplier_UsesModifierValues()
        {
            var def = new EncounterDefinition
            {
                BeastHostilityMultiplier = 1.5f,
                MechaSupportMultiplier = 0.7f
            };

            var go = new GameObject("Enc");
            try
            {
                var enc = go.AddComponent<EncounterRuntime>();
                enc.Initialize(def);

                var beastMult = enc.GetFactionMultiplier(SubFactionId.BeastIronClaw);
                var mechaMult = enc.GetFactionMultiplier(SubFactionId.MotorIronRiders);

                Assert.AreEqual(1.5f, beastMult, 0.01f);
                Assert.AreEqual(0.7f, mechaMult, 0.01f);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

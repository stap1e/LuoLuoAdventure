using LuoLuoTrip;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AIStopDistanceTests
    {
        private Combatant CreateAI(GameObject go, string id, SubFactionId faction)
        {
            var entity = go.AddComponent<CharacterEntity>();
            entity.Bind(new CharacterData(id, id, faction, CharacterRole.Common, 5));
            return entity.Combatant;
        }

        [Test]
        public void EffectiveStopDistance_UsesConfigValue()
        {
            var go = new GameObject("AI");
            try
            {
                var c = CreateAI(go, "ai", SubFactionId.BeastIronClaw);
                var ai = go.AddComponent<SimpleCombatAI>();
                ai.ApplyTuning(CombatTuningConfigSO.Default);

                Assert.AreEqual(CombatTuningConfigSO.Default.aiStopDistance, ai.EffectiveStopDistance, 0.01f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void EffectiveStopDistance_FallbacksToAttackRange_WhenZero()
        {
            var go = new GameObject("AI");
            try
            {
                var c = CreateAI(go, "ai", SubFactionId.BeastIronClaw);
                var ai = go.AddComponent<SimpleCombatAI>();
                var config = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
                config.aiStopDistance = 0f;
                ai.ApplyTuning(config);

                var expected = Mathf.Max(0.5f, c.Stats.attackRange * 0.8f);
                Assert.AreEqual(expected, ai.EffectiveStopDistance, 0.01f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void StopDistance_DoesNotExceedAttackRange()
        {
            var go = new GameObject("AI");
            try
            {
                var c = CreateAI(go, "ai", SubFactionId.BeastIronClaw);
                var ai = go.AddComponent<SimpleCombatAI>();
                var config = ScriptableObject.CreateInstance<CombatTuningConfigSO>();
                config.aiStopDistance = 1.5f;
                ai.ApplyTuning(config);

                Assert.LessOrEqual(ai.EffectiveStopDistance, c.Stats.attackRange,
                    "Stop distance should not exceed attack range for valid combat");
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CityGateAIProfileSetupTests
    {
        [Test]
        public void CityGateProfileAssignments_UseExpectedTypes()
        {
            var raider = CreateProfile(AIBehaviorProfileType.AggressiveRaider);
            var guard = CreateProfile(AIBehaviorProfileType.DefensiveGuard);
            var negotiator = CreateProfile(AIBehaviorProfileType.Negotiator);
            var hardliner = CreateProfile(AIBehaviorProfileType.Hardliner);
            var commander = CreateProfile(AIBehaviorProfileType.CommanderUnit);
            try
            {
                Assert.That(raider.profileType, Is.EqualTo(AIBehaviorProfileType.AggressiveRaider));
                Assert.That(guard.profileType, Is.EqualTo(AIBehaviorProfileType.DefensiveGuard));
                Assert.That(negotiator.profileType, Is.EqualTo(AIBehaviorProfileType.Negotiator));
                Assert.That(hardliner.profileType, Is.EqualTo(AIBehaviorProfileType.Hardliner));
                Assert.That(commander.profileType, Is.EqualTo(AIBehaviorProfileType.CommanderUnit));
            }
            finally
            {
                Object.DestroyImmediate(raider);
                Object.DestroyImmediate(guard);
                Object.DestroyImmediate(negotiator);
                Object.DestroyImmediate(hardliner);
                Object.DestroyImmediate(commander);
            }
        }

        [Test]
        public void EncounterWave_CarriesRaiderProfileToSpawnedAI()
        {
            var profile = CreateProfile(AIBehaviorProfileType.AggressiveRaider);
            var encGo = new GameObject("Encounter");
            var spawnGo = new GameObject("Spawn");
            try
            {
                var enc = encGo.AddComponent<EncounterRuntime>();
                var spawn = spawnGo.AddComponent<EncounterSpawnPoint>();
                enc.AddSpawnPoint(spawn);
                var wave = new EncounterWave
                {
                    waveId = "citygate_beast_raid_test",
                    faction = SubFactionId.BeastIronClaw,
                    role = CharacterRole.Minion,
                    unitCount = 1,
                    behaviorProfile = profile
                };

                Assert.That(enc.SpawnWave(wave), Is.EqualTo(1));
                Assert.That(enc.SpawnedUnits[0].Entity.GetComponent<SimpleCombatAI>().BehaviorProfile.profileType,
                    Is.EqualTo(AIBehaviorProfileType.AggressiveRaider));
            }
            finally
            {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(encGo);
                Object.DestroyImmediate(spawnGo);
            }
        }

        [Test]
        public void HighRank_CommanderUnitProfileKeepsDirectControlPolicySeparate()
        {
            var profile = CreateProfile(AIBehaviorProfileType.CommanderUnit);
            var go = new GameObject("CityLord_HighRank");
            try
            {
                var entity = go.AddComponent<CharacterEntity>();
                entity.Bind(CharacterData.Create("city_lord", "CityLord", SubFactionId.MotorIronRiders, CharacterRole.CityLord));
                var ai = go.AddComponent<SimpleCombatAI>();
                ai.BehaviorProfile = profile;

                Assert.That(ai.RespondsToTacticalCommand, Is.True);
                Assert.That(entity.Data.AllowDirectControl, Is.False);
                Assert.That(entity.Data.IsHeroOrLeader, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(go);
            }
        }

        private static AIBehaviorProfileSO CreateProfile(AIBehaviorProfileType type)
        {
            var profile = ScriptableObject.CreateInstance<AIBehaviorProfileSO>();
            AIBehaviorProfileSO.ConfigureDefaults(profile, type);
            return profile;
        }
    }
}

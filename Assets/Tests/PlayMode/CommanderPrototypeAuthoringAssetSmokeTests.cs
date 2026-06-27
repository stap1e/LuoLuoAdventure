using System.Collections;
using LuoLuoTrip.AI;
using LuoLuoTrip.Combat;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LuoLuoTrip.Tests.PlayMode
{
    public class CommanderPrototypeAuthoringAssetSmokeTests
    {
        [UnityTest]
        public IEnumerator CombatTuningConfig_LoadsFromResources()
        {
            var config = CombatTuningConfigSO.LoadOrDefault();
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Validate(out var error), Is.True, error);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AIProfiles_LoadFromResources()
        {
            foreach (AIBehaviorProfileType type in System.Enum.GetValues(typeof(AIBehaviorProfileType)))
            {
                var profile = AIBehaviorProfileSO.LoadDefault(type);
                Assert.That(profile, Is.Not.Null, type.ToString());
                Assert.That(profile.profileType, Is.EqualTo(type));
                Assert.That(profile.Validate(out var error), Is.True, $"{type}: {error}");
            }
            yield return null;
        }
    }
}

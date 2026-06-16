using LuoLuoTrip.Feedback;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class WorldMarkerProfileTests
    {
        [Test]
        public void DefaultEntry_HasLabelForEveryNonNoneType()
        {
            foreach (WorldMarkerType type in System.Enum.GetValues(typeof(WorldMarkerType)))
            {
                if (type == WorldMarkerType.None) continue;
                var entry = WorldMarkerProfileSO.DefaultEntry(type);
                Assert.That(entry, Is.Not.Null, $"Default entry null for {type}");
                Assert.That(entry.type, Is.EqualTo(type));
            }
        }

        [Test]
        public void DefaultEntry_AIWindupWarning_UsesRedColor()
        {
            var entry = WorldMarkerProfileSO.DefaultEntry(WorldMarkerType.AIWindupWarning);
            Assert.That(entry.color.r, Is.GreaterThan(0.7f));
            Assert.That(entry.label, Is.EqualTo("[!]"));
        }

        [Test]
        public void DefaultEntry_HostileUnit_HidesLabel()
        {
            var entry = WorldMarkerProfileSO.DefaultEntry(WorldMarkerType.HostileUnit);
            Assert.That(entry.showLabel, Is.False);
        }

        [Test]
        public void DefaultEntry_FriendlyUnit_HidesLabel()
        {
            var entry = WorldMarkerProfileSO.DefaultEntry(WorldMarkerType.FriendlyUnit);
            Assert.That(entry.showLabel, Is.False);
        }

        [Test]
        public void EnsureAllTypes_PopulatesEveryNonNoneType()
        {
            var profile = ScriptableObject.CreateInstance<WorldMarkerProfileSO>();
            try
            {
                profile.EnsureAllTypes();
                foreach (WorldMarkerType type in System.Enum.GetValues(typeof(WorldMarkerType)))
                {
                    if (type == WorldMarkerType.None) continue;
                    var entry = profile.GetEntry(type);
                    Assert.That(entry, Is.Not.Null, $"Missing entry for {type}");
                    Assert.That(entry.type, Is.EqualTo(type));
                }
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void GetEntry_FallsBackToDefault_WhenAssetEmpty()
        {
            var profile = ScriptableObject.CreateInstance<WorldMarkerProfileSO>();
            try
            {
                var entry = profile.GetEntry(WorldMarkerType.MissionObjective);
                Assert.That(entry, Is.Not.Null);
                Assert.That(entry.label, Is.EqualTo("[OBJ]"));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void EnsureAllTypes_IsIdempotent()
        {
            var profile = ScriptableObject.CreateInstance<WorldMarkerProfileSO>();
            try
            {
                profile.EnsureAllTypes();
                profile.EnsureAllTypes();
                int expected = System.Enum.GetValues(typeof(WorldMarkerType)).Length - 1;
                int actual = 0;
                foreach (WorldMarkerType t in System.Enum.GetValues(typeof(WorldMarkerType)))
                {
                    if (t == WorldMarkerType.None) continue;
                    if (profile.GetEntry(t) != null) actual++;
                }
                Assert.That(actual, Is.EqualTo(expected));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }
    }
}

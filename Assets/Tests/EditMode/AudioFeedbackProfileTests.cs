using LuoLuoTrip.Audio;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class AudioFeedbackProfileTests
    {
        [Test]
        public void EnsureAllEvents_AddsEntryForEveryNonNoneEvent()
        {
            var profile = ScriptableObject.CreateInstance<AudioFeedbackProfileSO>();
            try
            {
                profile.EnsureAllEvents();

                foreach (AudioEventId id in System.Enum.GetValues(typeof(AudioEventId)))
                {
                    if (id == AudioEventId.None) continue;
                    Assert.That(profile.HasEntry(id), Is.True, $"Missing entry for {id}");
                }
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void GetEntry_ReturnsNullForUnknownEvent()
        {
            var profile = ScriptableObject.CreateInstance<AudioFeedbackProfileSO>();
            try
            {
                Assert.That(profile.GetEntry(AudioEventId.Hit), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void PickClip_ReturnsNullWhenEntryHasNoClips()
        {
            var profile = ScriptableObject.CreateInstance<AudioFeedbackProfileSO>();
            try
            {
                profile.EnsureAllEvents();
                Assert.That(profile.PickClip(AudioEventId.Hit), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void GetVolume_DefaultsToOne()
        {
            var profile = ScriptableObject.CreateInstance<AudioFeedbackProfileSO>();
            try
            {
                profile.EnsureAllEvents();
                Assert.That(profile.GetVolume(AudioEventId.AttackStart), Is.EqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void IsSpatial_DefaultsTrue()
        {
            var profile = ScriptableObject.CreateInstance<AudioFeedbackProfileSO>();
            try
            {
                profile.EnsureAllEvents();
                Assert.That(profile.IsSpatial(AudioEventId.AttackStart), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void EnsureAllEvents_IsIdempotent()
        {
            var profile = ScriptableObject.CreateInstance<AudioFeedbackProfileSO>();
            try
            {
                profile.EnsureAllEvents();
                int firstCount = profile.Entries.Count;
                profile.EnsureAllEvents();
                Assert.That(profile.Entries.Count, Is.EqualTo(firstCount));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }
    }
}

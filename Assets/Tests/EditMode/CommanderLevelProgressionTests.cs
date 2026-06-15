using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CommanderLevelProgressionTests
    {
        [Test]
        public void ApplyXP_IncreasesExperience()
        {
            var profile = CommanderProfile.CreateDefault();
            var initialXp = profile.Experience;
            profile.AddExperience(100);

            Assert.That(profile.Experience, Is.EqualTo(initialXp + 100));
        }

        [Test]
        public void LevelUp_IncreasesLevel()
        {
            var profile = CommanderProfile.CreateDefault();
            var initialLevel = profile.CommanderLevel;

            var xpNeeded = CommanderLevelSystem.ExperienceForLevel(initialLevel + 1);
            profile.AddExperience(xpNeeded);

            Assert.That(profile.CommanderLevel, Is.GreaterThan(initialLevel));
        }

        [Test]
        public void LevelUp_IncreasesControlRank()
        {
            var profile = CommanderProfile.CreateDefault();
            var initialRank = profile.MaxDirectControlRank;

            while (profile.CommanderLevel < 10)
            {
                var xpNeeded = CommanderLevelSystem.ExperienceForLevel(profile.CommanderLevel + 1);
                profile.AddExperience(xpNeeded);
            }

            Assert.That(profile.MaxDirectControlRank, Is.GreaterThan(initialRank));
        }

        [Test]
        public void ExperienceToNextLevel_DecreasesWithXP()
        {
            var profile = CommanderProfile.CreateDefault();
            var before = profile.ExperienceToNextLevel;
            profile.AddExperience(50);
            var after = profile.ExperienceToNextLevel;

            Assert.That(after, Is.LessThan(before));
        }

        [Test]
        public void CanLevelUp_FalseWhenInsufficientXP()
        {
            var profile = CommanderProfile.CreateDefault();
            Assert.That(profile.CanLevelUp, Is.False);
        }

        [Test]
        public void CanLevelUp_TrueWhenEnoughXP()
        {
            var profile = CommanderProfile.CreateDefault();
            var xpNeeded = CommanderLevelSystem.ExperienceForLevel(profile.CommanderLevel + 1);
            profile.AddExperience(xpNeeded);
            Assert.That(profile.CanLevelUp, Is.True);
        }

        [Test]
        public void SaveRestore_PreservesLevelAndXP()
        {
            var profile = CommanderProfile.CreateDefault();
            profile.AddExperience(500);
            var savedLevel = profile.CommanderLevel;
            var savedXp = profile.Experience;

            var restored = CommanderProfile.CreateDefault();
            restored.CommanderLevel = savedLevel;
            restored.Experience = savedXp;
            restored.CommandCapacity = CommanderLevelSystem.GetCommandCapacity(savedLevel);
            restored.MaxDirectControlRank = CommanderLevelSystem.GetMaxDirectControlRank(savedLevel);
            restored.MaxTacticalCommandRank = CommanderLevelSystem.GetMaxTacticalCommandRank(savedLevel);
            restored.BaseSyncRate = CommanderLevelSystem.GetBaseSyncRate(savedLevel);
            restored.Clamp();

            Assert.That(restored.CommanderLevel, Is.EqualTo(savedLevel));
            Assert.That(restored.Experience, Is.EqualTo(savedXp));
        }
    }
}

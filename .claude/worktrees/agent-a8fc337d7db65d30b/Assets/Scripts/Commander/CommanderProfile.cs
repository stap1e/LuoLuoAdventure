using System;

namespace LuoLuoTrip
{
    [Serializable]
    public class CommanderProfile
    {
        public int CommanderLevel = 1;
        public int Experience;
        public int CommandCapacity;
        public int MaxDirectControlRank;
        public int MaxTacticalCommandRank;
        public float BaseSyncRate;
        public int MechaTrust;
        public int BeastTrust;
        public int BalanceScore;

        public int ExperienceToNextLevel => CommanderLevel < CommanderLevelSystem.MaxCommanderLevel
            ? CommanderLevelSystem.ExperienceForLevel(CommanderLevel + 1) - Experience
            : 0;

        public bool CanLevelUp => CommanderLevel < CommanderLevelSystem.MaxCommanderLevel
            && Experience >= CommanderLevelSystem.ExperienceForLevel(CommanderLevel + 1);

        public static CommanderProfile CreateDefault()
        {
            var profile = new CommanderProfile();
            ApplyLevelStats(profile);
            return profile;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;
            Experience += amount;
            while (CommanderLevel < CommanderLevelSystem.MaxCommanderLevel)
            {
                var required = CommanderLevelSystem.ExperienceForLevel(CommanderLevel + 1);
                if (Experience < required) break;
                CommanderLevel++;
            }
            ApplyLevelStats(this);
        }

        public void Clamp()
        {
            CommanderLevel = Math.Clamp(CommanderLevel, 1, CommanderLevelSystem.MaxCommanderLevel);
            CommandCapacity = Math.Clamp(CommandCapacity, 1, 50);
            MaxDirectControlRank = Math.Clamp(MaxDirectControlRank, 1, 5);
            MaxTacticalCommandRank = Math.Clamp(MaxTacticalCommandRank, 1, 5);
            BaseSyncRate = Math.Clamp(BaseSyncRate, 0f, 1f);
            MechaTrust = Math.Clamp(MechaTrust, -100, 100);
            BeastTrust = Math.Clamp(BeastTrust, -100, 100);
            BalanceScore = Math.Clamp(BalanceScore, -100, 100);
            Experience = Math.Max(0, Experience);
        }

        private static void ApplyLevelStats(CommanderProfile profile)
        {
            profile.CommandCapacity = CommanderLevelSystem.GetCommandCapacity(profile.CommanderLevel);
            profile.MaxDirectControlRank = CommanderLevelSystem.GetMaxDirectControlRank(profile.CommanderLevel);
            profile.MaxTacticalCommandRank = CommanderLevelSystem.GetMaxTacticalCommandRank(profile.CommanderLevel);
            profile.BaseSyncRate = CommanderLevelSystem.GetBaseSyncRate(profile.CommanderLevel);
            profile.Clamp();
        }
    }
}

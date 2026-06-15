namespace LuoLuoTrip
{
    public static class CommanderLevelSystem
    {
        public const int MaxCommanderLevel = 50;

        public static int ExperienceForLevel(int level)
        {
            if (level <= 1) return 0;
            return level * level * 100;
        }

        public static int GetCommandCapacity(int commanderLevel)
        {
            if (commanderLevel >= 45) return 20;
            if (commanderLevel >= 35) return 15;
            if (commanderLevel >= 20) return 10;
            if (commanderLevel >= 10) return 6;
            if (commanderLevel >= 5) return 4;
            return 2;
        }

        public static int GetMaxDirectControlRank(int commanderLevel)
        {
            if (commanderLevel >= 45) return 5;
            if (commanderLevel >= 35) return 4;
            if (commanderLevel >= 20) return 3;
            if (commanderLevel >= 10) return 2;
            return 1;
        }

        public static int GetMaxTacticalCommandRank(int commanderLevel)
        {
            if (commanderLevel >= 45) return 5;
            if (commanderLevel >= 35) return 5;
            if (commanderLevel >= 20) return 4;
            if (commanderLevel >= 10) return 3;
            return 2;
        }

        public static float GetBaseSyncRate(int commanderLevel)
        {
            if (commanderLevel >= 45) return 0.95f;
            if (commanderLevel >= 35) return 0.80f;
            if (commanderLevel >= 20) return 0.65f;
            if (commanderLevel >= 10) return 0.50f;
            if (commanderLevel >= 5) return 0.35f;
            return 0.20f;
        }
    }
}

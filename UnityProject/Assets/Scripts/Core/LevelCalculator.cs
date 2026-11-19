using System.Collections.Generic;

namespace ExpeditionCamp
{
    public static class LevelCalculator
    {
        public static int GetLevel(int totalExp, List<Dictionary<string, string>> levelExpRows)
        {
            int level = 1;
            int maxLevel = 1;
            foreach (Dictionary<string, string> row in levelExpRows)
            {
                int rowLevel = DataLoader.ReadInt(row, "level", 0);
                int requiredExp = DataLoader.ReadInt(row, "required_exp", 0);
                if (rowLevel > maxLevel)
                {
                    maxLevel = rowLevel;
                }
                if (totalExp >= requiredExp && rowLevel > level)
                {
                    level = rowLevel;
                }
            }
            if (level > maxLevel)
            {
                return maxLevel;
            }
            return level;
        }
    }
}

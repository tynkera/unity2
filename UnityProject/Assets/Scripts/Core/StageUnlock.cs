using System.Collections.Generic;

namespace ExpeditionCamp
{
    public static class StageUnlock
    {
        public static bool IsUnlocked(int stageId, HashSet<int> clearedStageIds,
            List<Dictionary<string, string>> stageRows)
        {
            Dictionary<string, string> stageRow = null;
            foreach (Dictionary<string, string> row in stageRows)
            {
                if (DataLoader.ReadInt(row, "stage_id", 0) == stageId)
                {
                    stageRow = row;
                    break;
                }
            }
            if (stageRow == null)
            {
                return false;
            }

            int previousStageId = DataLoader.ReadInt(stageRow, "prev_stage_id", 0);
            if (previousStageId == 0)
            {
                return true;
            }
            if (clearedStageIds == null)
            {
                return false;
            }
            return clearedStageIds.Contains(previousStageId);
        }
    }
}

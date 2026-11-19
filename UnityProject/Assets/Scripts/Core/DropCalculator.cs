using System;
using System.Collections.Generic;

namespace ExpeditionCamp
{
    public struct DropResult
    {
        public int ItemId;
        public int Count;
    }

    public static class DropCalculator
    {
        public const int ProbabilityBase = 10000;

        public static bool IsIndependentDropped(int prob, int roll)
        {
            return roll < prob;
        }

        public static int SelectRandomOne(List<Dictionary<string, string>> rows, int roll)
        {
            int accumulated = 0;
            foreach (Dictionary<string, string> row in rows)
            {
                if (DataLoader.ReadText(row, "drop_type") != "RANDOM_ONE")
                {
                    continue;
                }
                accumulated += DataLoader.ReadInt(row, "prob", 0);
                if (roll < accumulated)
                {
                    return DataLoader.ReadInt(row, "item_id", 0);
                }
            }
            return 0;
        }

        public static List<DropResult> CalculateDrops(List<Dictionary<string, string>> groupRows,
            Func<int, int, int> randomRange)
        {
            List<DropResult> results = new List<DropResult>();
            bool randomOneHandled = false;

            foreach (Dictionary<string, string> row in groupRows)
            {
                string dropType = DataLoader.ReadText(row, "drop_type");
                int itemId = DataLoader.ReadInt(row, "item_id", 0);
                int minCount = DataLoader.ReadInt(row, "min_count", 1);
                int maxCount = DataLoader.ReadInt(row, "max_count", 1);
                int prob = DataLoader.ReadInt(row, "prob", 0);

                if (dropType == "FIXED")
                {
                    results.Add(MakeResult(itemId, minCount, maxCount, randomRange));
                    continue;
                }

                if (dropType == "INDEPENDENT")
                {
                    int roll = randomRange(0, ProbabilityBase);
                    if (IsIndependentDropped(prob, roll))
                    {
                        results.Add(MakeResult(itemId, minCount, maxCount, randomRange));
                    }
                    continue;
                }

                if (dropType == "RANDOM_ONE" && !randomOneHandled)
                {
                    randomOneHandled = true;
                    int roll = randomRange(0, ProbabilityBase);
                    int selectedItemId = SelectRandomOne(groupRows, roll);
                    if (selectedItemId == 0)
                    {
                        continue;
                    }
                    Dictionary<string, string> selectedRow = FindRandomOneRow(groupRows, selectedItemId);
                    if (selectedRow == null)
                    {
                        continue;
                    }
                    results.Add(MakeResult(
                        selectedItemId,
                        DataLoader.ReadInt(selectedRow, "min_count", 1),
                        DataLoader.ReadInt(selectedRow, "max_count", 1),
                        randomRange));
                }
            }

            return results;
        }

        private static Dictionary<string, string> FindRandomOneRow(
            List<Dictionary<string, string>> groupRows, int itemId)
        {
            foreach (Dictionary<string, string> row in groupRows)
            {
                if (DataLoader.ReadText(row, "drop_type") != "RANDOM_ONE")
                {
                    continue;
                }
                if (DataLoader.ReadInt(row, "item_id", 0) == itemId)
                {
                    return row;
                }
            }
            return null;
        }

        private static DropResult MakeResult(int itemId, int minCount, int maxCount,
            Func<int, int, int> randomRange)
        {
            int count = minCount;
            if (maxCount > minCount)
            {
                count = randomRange(minCount, maxCount + 1);
            }
            DropResult result = new DropResult();
            result.ItemId = itemId;
            result.Count = count;
            return result;
        }
    }
}

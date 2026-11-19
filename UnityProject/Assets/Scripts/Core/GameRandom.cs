using System;
using System.Collections.Generic;

namespace ExpeditionCamp
{
    public static class GameRandom
    {
        public const string SeedArgument = "-qaSeed";

        private static Random _drop;
        private static Random _spawn;

        public static int Seed { get; private set; }
        public static bool IsFixed { get; private set; }

        public static void Initialize(int configSeed)
        {
            int argumentSeed = ReadSeedArgument();
            if (argumentSeed != 0)
            {
                SetSeed(argumentSeed);
                return;
            }
            if (configSeed != 0)
            {
                SetSeed(configSeed);
                return;
            }
            Seed = Environment.TickCount;
            IsFixed = false;
            _drop = new Random(Seed);
            _spawn = new Random(Seed + 1);
        }

        public static void SetSeed(int seed)
        {
            Seed = seed;
            IsFixed = true;
            _drop = new Random(seed);
            _spawn = new Random(seed + 1);
        }

        public static bool IsInitialized
        {
            get { return _drop != null; }
        }

        public static int DropRange(int minInclusive, int maxExclusive)
        {
            EnsureInitialized();
            return _drop.Next(minInclusive, maxExclusive);
        }

        public static float SpawnRange(float min, float max)
        {
            EnsureInitialized();
            return min + (float)_spawn.NextDouble() * (max - min);
        }

        private static void EnsureInitialized()
        {
            if (_drop == null)
            {
                Initialize(0);
            }
        }

        public static int ReadSeedArgument()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] != SeedArgument)
                {
                    continue;
                }
                int value;
                if (int.TryParse(arguments[index + 1], out value))
                {
                    return value;
                }
            }
            return 0;
        }
    }

    [Serializable]
    public class DropRecord
    {
        public int group_id;
        public int roll = -1;
        public int[] rolls = new int[0];
        public int[] item_ids = new int[0];
        public int[] counts = new int[0];
    }

    public static class DropLog
    {
        public static DropRecord Last { get; private set; }
        public static readonly List<DropRecord> History = new List<DropRecord>();

        public static List<DropResult> Roll(int groupId)
        {
            List<int> rolls = new List<int>();
            Func<int, int, int> recorder = (minInclusive, maxExclusive) =>
            {
                int value = GameRandom.DropRange(minInclusive, maxExclusive);
                if (minInclusive == 0 && maxExclusive == DropCalculator.ProbabilityBase)
                {
                    rolls.Add(value);
                }
                return value;
            };

            List<DropResult> results = DropCalculator.CalculateDrops(GameData.GetDropRows(groupId), recorder);

            DropRecord record = new DropRecord();
            record.group_id = groupId;
            record.rolls = rolls.ToArray();
            record.roll = rolls.Count > 0 ? rolls[0] : -1;
            record.item_ids = new int[results.Count];
            record.counts = new int[results.Count];
            for (int index = 0; index < results.Count; index++)
            {
                record.item_ids[index] = results[index].ItemId;
                record.counts[index] = results[index].Count;
            }
            Last = record;
            History.Add(record);
            return results;
        }

        public static void Clear()
        {
            Last = null;
            History.Clear();
        }
    }
}

using System.Collections.Generic;

namespace ExpeditionCamp
{
    public static class Inventory
    {
        private static readonly Dictionary<int, int> _counts = new Dictionary<int, int>();
        private static readonly List<int> _order = new List<int>();

        public static void AddItem(int itemId, int count)
        {
            if (count <= 0)
            {
                return;
            }

            int maxStack = GetMaxStack(itemId);
            int current = 0;
            if (_counts.TryGetValue(itemId, out current))
            {
                current = _counts[itemId];
            }
            else
            {
                _order.Add(itemId);
            }

            int next = current + count;
            if (next > maxStack)
            {
                next = maxStack;
            }
            _counts[itemId] = next;
        }

        public static int GetCount(int itemId)
        {
            int count;
            if (_counts.TryGetValue(itemId, out count))
            {
                return count;
            }
            return 0;
        }

        public static List<int> GetItemIds()
        {
            return new List<int>(_order);
        }

        public static void Clear()
        {
            _counts.Clear();
            _order.Clear();
        }

        public static int GetMaxStack(int itemId)
        {
            Dictionary<string, string> itemRow = GameData.GetItem(itemId);
            if (itemRow == null)
            {
                return int.MaxValue;
            }
            return DataLoader.ReadInt(itemRow, "max_stack", int.MaxValue);
        }
    }
}

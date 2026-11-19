using System.Collections.Generic;

namespace ExpeditionCamp
{
    public class FieldLoot
    {
        public readonly List<int> ItemOrder = new List<int>();
        public readonly Dictionary<int, int> Items = new Dictionary<int, int>();
        public readonly List<int> NewDex = new List<int>();
        public int Gold;
        public int Exp;
        public int Kills;

        public void AddItem(int itemId, int count)
        {
            if (itemId == GameData.GoldItemId)
            {
                Gold += count;
                return;
            }
            int current;
            if (!Items.TryGetValue(itemId, out current))
            {
                ItemOrder.Add(itemId);
                current = 0;
            }
            Items[itemId] = current + count;
        }

        public int GetCount(int itemId)
        {
            int count;
            if (Items.TryGetValue(itemId, out count))
            {
                return count;
            }
            return 0;
        }
    }
}

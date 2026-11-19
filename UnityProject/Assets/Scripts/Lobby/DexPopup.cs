using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ExpeditionCamp
{
    public class DexPopup : PopupBase
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private DexCardView cardPrefab;
        [SerializeField] private TMP_Text countText;

        public override void Refresh()
        {
            ClearChildren(listRoot);
            SaveSystem.EnsureLoaded();
            List<Dictionary<string, string>> monsters = GameData.GetDexMonsters();
            int registeredCount = 0;
            foreach (Dictionary<string, string> row in monsters)
            {
                int monsterId = DataLoader.ReadInt(row, "monster_id", 0);
                bool registered = SaveSystem.Data.dex.Contains(monsterId);
                if (registered)
                {
                    registeredCount++;
                }
                DexCardView card = Instantiate(cardPrefab, listRoot);
                card.Bind(monsterId, registered);
            }
            countText.text = "등록 " + registeredCount + " / " + monsters.Count;
        }
    }
}

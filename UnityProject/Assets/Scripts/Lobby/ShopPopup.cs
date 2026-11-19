using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ExpeditionCamp
{
    public class ShopPopup : PopupBase
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private ShopSlotView slotPrefab;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private LobbyController lobby;

        public override void Refresh()
        {
            ClearChildren(listRoot);
            foreach (Dictionary<string, string> row in GameData.GetVisibleShopRows())
            {
                int slotId = DataLoader.ReadInt(row, "shop_slot_id", 0);
                int itemId = DataLoader.ReadInt(row, "item_id", 0);
                int price = DataLoader.ReadInt(row, "price", 0);
                ShopSlotView slot = Instantiate(slotPrefab, listRoot);
                slot.Bind(slotId, itemId, price, () => Buy(itemId, price));
            }
            messageText.text = "";
            RefreshGold();
        }

        private void Buy(int itemId, int price)
        {
            SaveSystem.EnsureLoaded();
            if (SaveSystem.Data.gold < price)
            {
                messageText.text = "골드가 부족합니다";
                return;
            }
            SaveSystem.Data.gold -= price;
            Inventory.AddItem(itemId, 1);
            SaveSystem.Save();
            string itemName = GameData.GetString(DataLoader.ReadText(GameData.GetItem(itemId), "name_key"));
            messageText.text = itemName + " 을 구매했습니다";
            Debug.Log("[Shop] " + itemId + " 구매 · 가격 " + price + " · 남은 골드 " + SaveSystem.Data.gold);
            RefreshGold();
            if (lobby != null)
            {
                lobby.RefreshHud();
            }
        }

        private void RefreshGold()
        {
            SaveSystem.EnsureLoaded();
            goldText.text = "보유 골드 " + SaveSystem.Data.gold;
        }
    }
}

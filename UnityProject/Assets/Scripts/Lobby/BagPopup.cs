using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ExpeditionCamp
{
    public class BagPopup : PopupBase
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private ItemSlotView slotPrefab;
        [SerializeField] private TMP_Text emptyText;

        public override void Refresh()
        {
            ClearChildren(listRoot);
            List<int> itemIds = Inventory.GetItemIds();
            foreach (int itemId in itemIds)
            {
                ItemSlotView slot = Instantiate(slotPrefab, listRoot);
                slot.Bind("Bag", itemId, Inventory.GetCount(itemId));
            }
            emptyText.gameObject.SetActive(itemIds.Count == 0);
        }
    }
}

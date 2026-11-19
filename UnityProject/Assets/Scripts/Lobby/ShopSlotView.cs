using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class ShopSlotView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;

        public void Bind(int slotId, int itemId, int price, Action onBuy)
        {
            gameObject.name = "ShopSlot_" + slotId;
            icon.gameObject.name = "ShopIcon_" + slotId;
            nameText.gameObject.name = "ShopName_" + slotId;
            priceText.gameObject.name = "ShopPrice_" + slotId;
            buyButton.gameObject.name = "Btn_Buy_" + slotId;
            TMP_Text buyLabel = buyButton.GetComponentInChildren<TMP_Text>();
            if (buyLabel != null)
            {
                buyLabel.gameObject.name = "Txt_Buy_" + slotId;
            }

            Iconize(itemId);
            priceText.text = price.ToString();
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy());
        }

        private void Iconize(int itemId)
        {
            System.Collections.Generic.Dictionary<string, string> row = GameData.GetItem(itemId);
            icon.sprite = IconLoader.Load(DataLoader.ReadText(row, "icon_path"));
            nameText.text = GameData.GetString(DataLoader.ReadText(row, "name_key"));
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class ItemSlotView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;

        public void Bind(string prefix, int itemId, int count)
        {
            gameObject.name = prefix + "Slot_" + itemId;
            icon.gameObject.name = prefix + "Icon_" + itemId;
            nameText.gameObject.name = prefix + "Name_" + itemId;
            countText.gameObject.name = prefix + "Count_" + itemId;

            string iconPath = DataLoader.ReadText(GameData.GetItem(itemId), "icon_path");
            icon.sprite = IconLoader.Load(iconPath);
            nameText.text = GameData.GetString(DataLoader.ReadText(GameData.GetItem(itemId), "name_key"));
            countText.text = "x" + count;
        }
    }
}

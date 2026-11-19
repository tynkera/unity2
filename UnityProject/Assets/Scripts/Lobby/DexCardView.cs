using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class DexCardView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image frame;

        public void Bind(int monsterId, bool registered)
        {
            gameObject.name = "DexCard_" + monsterId;
            icon.gameObject.name = "DexIcon_" + monsterId;
            nameText.gameObject.name = "DexName_" + monsterId;

            System.Collections.Generic.Dictionary<string, string> row = GameData.GetMonster(monsterId);
            icon.sprite = IconLoader.Load(DataLoader.ReadText(row, "icon_path"));
            if (registered)
            {
                icon.color = Color.white;
                nameText.text = GameData.GetString(DataLoader.ReadText(row, "name_key"));
            }
            else
            {
                icon.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
                nameText.text = "???";
            }
            if (frame != null)
            {
                frame.color = IconLoader.GetGradeColor(DataLoader.ReadText(row, "grade"));
            }
        }
    }
}

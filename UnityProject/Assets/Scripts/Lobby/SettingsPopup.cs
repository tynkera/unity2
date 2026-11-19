using TMPro;
using UnityEngine;

namespace ExpeditionCamp
{
    public class SettingsPopup : PopupBase
    {
        [SerializeField] private TMP_Text infoText;

        public override void Refresh()
        {
            infoText.text = "버전 " + Application.version + "\n세이브 위치\n" + SaveSystem.SavePath;
        }
    }
}

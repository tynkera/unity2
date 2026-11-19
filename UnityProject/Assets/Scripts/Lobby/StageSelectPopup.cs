using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class StageSelectPopup : PopupBase
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private Button stageButtonPrefab;
        [SerializeField] private LobbyController lobby;
        [SerializeField] private int chapter = 1;

        public override void Refresh()
        {
            ClearChildren(listRoot);
            foreach (Dictionary<string, string> row in GameData.GetChapterStages(chapter))
            {
                int stageId = DataLoader.ReadInt(row, "stage_id", 0);
                int stageNo = DataLoader.ReadInt(row, "stage_no", 0);
                bool open = SaveSystem.IsStageUnlocked(stageId) && GameData.IsPlayableStage(stageId);

                Button button = Instantiate(stageButtonPrefab, listRoot);
                button.gameObject.name = "Btn_Stage_" + stageId;
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                label.gameObject.name = "Txt_Stage_" + stageId;
                string stageName = GameData.GetString(DataLoader.ReadText(row, "name_key"));
                label.text = chapter + "-" + stageNo + " " + stageName + (open ? "" : "  (잠김)");
                button.interactable = open;
                button.onClick.AddListener(() => lobby.StartStage(stageId));
            }
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button bagButton;
        [SerializeField] private Button dexButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private PopupBase bagPopup;
        [SerializeField] private PopupBase dexPopup;
        [SerializeField] private PopupBase shopPopup;
        [SerializeField] private PopupBase settingsPopup;
        [SerializeField] private GameObject loadingPopup;
        [SerializeField] private TMP_Text loadingText;

        private void Start()
        {
            bagButton.onClick.AddListener(bagPopup.Open);
            dexButton.onClick.AddListener(dexPopup.Open);
            shopButton.onClick.AddListener(shopPopup.Open);
            settingsButton.onClick.AddListener(settingsPopup.Open);
            loadingPopup.SetActive(false);

            GameSession.CurrentStageId = 0;
            SaveSystem.EnsureLoaded();
            SaveSystem.Save();
            RefreshHud();
        }

        public void RefreshHud()
        {
            SaveSystem.EnsureLoaded();
            goldText.text = "골드 " + SaveSystem.Data.gold;
            levelText.text = "Lv." + SaveSystem.Data.level;
        }

        public void StartStage(int stageId)
        {
            loadingPopup.SetActive(true);
            loadingPopup.transform.SetAsLastSibling();
            loadingText.text = "스테이지를 불러오는 중입니다";
            GameSession.StartStage(stageId);
        }
    }
}

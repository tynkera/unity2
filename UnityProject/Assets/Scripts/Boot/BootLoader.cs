using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExpeditionCamp
{
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private GameObject dataErrorPopup;
        [SerializeField] private TMP_Text dataErrorDetail;
        [SerializeField] private float minimumShowSecond = 0.5f;

        private IEnumerator Start()
        {
            if (dataErrorPopup != null)
            {
                dataErrorPopup.SetActive(false);
            }
            SetLoading("데이터를 불러오는 중입니다");
            yield return null;

            GameData.Reload();
            DataLoadError error = GameData.LoadError;
            if (error != null)
            {
                ShowError(error);
                yield break;
            }

            SetLoading("세이브를 불러오는 중입니다");
            SaveSystem.Load();
            yield return new WaitForSecondsRealtime(minimumShowSecond);

            SceneManager.LoadScene(GameSession.LobbyScene);
        }

        private void SetLoading(string message)
        {
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }

        private void ShowError(DataLoadError error)
        {
            Debug.LogError("[Boot] 데이터를 불러오지 못했습니다. " + error.ToMessage());
            SetLoading("");
            dataErrorPopup.SetActive(true);
            dataErrorDetail.text =
                "시트 이름: " + error.Sheet + "\n" +
                "행 번호: " + error.Row + "\n" +
                "열 이름: " + (string.IsNullOrEmpty(error.Column) ? "-" : error.Column) + "\n" +
                "내용: " + error.Reason;
        }
    }
}

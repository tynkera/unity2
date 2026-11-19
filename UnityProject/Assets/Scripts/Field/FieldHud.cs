using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class FieldHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Image hpFill;
        [SerializeField] private Toggle autoBattleToggle;

        private PlayerController _player;

        public void Bind(PlayerController player)
        {
            _player = player;
            autoBattleToggle.isOn = player.AutoBattle;
            autoBattleToggle.onValueChanged.AddListener(OnAutoBattleChanged);
            player.HpChanged += RefreshHp;
            RefreshHp();
        }

        private void OnAutoBattleChanged(bool value)
        {
            _player.AutoBattle = value;
            Debug.Log("[Field] 자동 전투 " + (value ? "켜짐" : "꺼짐"));
        }

        public void SetStage(string label)
        {
            stageText.text = label;
        }

        public void SetWave(int current, int total)
        {
            waveText.text = "웨이브 " + current + " / " + total;
        }

        private void RefreshHp()
        {
            hpText.text = "HP " + _player.Hp + " / " + _player.MaxHp;
            hpFill.fillAmount = _player.MaxHp > 0 ? (float)_player.Hp / _player.MaxHp : 0f;
        }
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class ResultPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text expText;
        [SerializeField] private TMP_Text emptyText;
        [SerializeField] private Transform listRoot;
        [SerializeField] private ItemSlotView slotPrefab;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private RectTransform panel;
        [SerializeField] private float revealSecond = 0.22f;

        private Action _onConfirm;

        private void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirm);
        }

        public void Show(bool cleared, FieldLoot loot, Action onConfirm)
        {
            _onConfirm = onConfirm;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            titleText.text = cleared ? "스테이지 클리어" : "스테이지 실패";
            titleText.color = cleared ? new Color(0.96f, 0.92f, 0.80f, 1f) : new Color(0.88f, 0.55f, 0.40f, 1f);
            if (confirmText != null)
            {
                confirmText.text = cleared ? "보상 받고 캠프로" : "캠프로 돌아가기";
            }
            if (panel != null)
            {
                StartCoroutine(Reveal());
            }
            for (int index = listRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(listRoot.GetChild(index).gameObject);
            }

            if (!cleared)
            {
                goldText.text = "골드 +0";
                expText.text = "경험치 +0";
                emptyText.text = "실패한 스테이지는 보상을 받지 않습니다";
                emptyText.gameObject.SetActive(true);
                return;
            }

            goldText.text = "골드 +" + loot.Gold;
            expText.text = "경험치 +" + loot.Exp;
            foreach (int itemId in loot.ItemOrder)
            {
                ItemSlotView slot = Instantiate(slotPrefab, listRoot);
                slot.Bind("Result", itemId, loot.GetCount(itemId));
            }
            emptyText.text = "획득한 아이템이 없습니다";
            emptyText.gameObject.SetActive(loot.ItemOrder.Count == 0);
        }

        private IEnumerator Reveal()
        {
            float elapsed = 0f;
            while (elapsed < revealSecond)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / revealSecond);
                float eased = 1f - Mathf.Pow(1f - progress, 3f);
                panel.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, eased);
                yield return null;
            }
            panel.localScale = Vector3.one;
        }

        private void OnConfirm()
        {
            if (_onConfirm != null)
            {
                _onConfirm();
            }
        }
    }
}

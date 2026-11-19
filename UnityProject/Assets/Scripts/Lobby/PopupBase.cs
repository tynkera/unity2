using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public class PopupBase : MonoBehaviour
    {
        [SerializeField] protected Button closeButton;

        protected virtual void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        public virtual void Refresh()
        {
        }

        protected static void ClearChildren(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
            {
                GameObject child = parent.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }
    }
}

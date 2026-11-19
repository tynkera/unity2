using UnityEngine;
using UnityEngine.EventSystems;

namespace ExpeditionCamp
{
    [RequireComponent(typeof(Collider2D))]
    public class NpcClick : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private PopupBase targetPopup;

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("[Lobby] " + gameObject.name + " 클릭");
            targetPopup.Open();
        }
    }
}

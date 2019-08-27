using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    class ItemDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.localPosition = Vector3.zero; // go back to where you were
        }
    }
}

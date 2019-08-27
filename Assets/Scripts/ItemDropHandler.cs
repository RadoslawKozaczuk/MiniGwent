using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    class ItemDropHandler : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            RectTransform trans = transform as RectTransform;
            if(RectTransformUtility.RectangleContainsScreenPoint(trans, Input.mousePosition))
            {
                Debug.Log("contains");
            }
            else
            {
                Debug.Log("Contains not");
            }
        }
    }
}

using Assets.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class LineUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool Outline = false;
        [HideInInspector] public LineIndicator LineIndicator;

        OutlineController _outline;

        public List<CardUI> Cards = new List<CardUI>();

        void Awake()
        {
            if(Outline)
                _outline = GetComponent<OutlineController>();
        }

        /// <summary>
        /// This removes the slot container game object.
        /// </summary>
        public void RemoveFromLine(int slotNumber)
        {
            Transform child = transform.GetChild(slotNumber);
            child.parent = null;
            Destroy(child.gameObject);

            // all cards on the right must have their NumberInLine reduced by one
            var cardsOnTheRight = Cards.TakeLast(Cards.Count - slotNumber - 1).ToList();
            cardsOnTheRight.ForEach(c => c.NumberInLine--);

            Cards.RemoveAt(slotNumber);
        }

        /// <summary>
        /// Creates a new free spot and adds card to it.
        /// </summary>
        public void PutInLine(CardUI card)
        {
            card.NumberInLine = Cards.Count;

            GameObject slot = Instantiate(GameEngine.Instance.CardContainer, transform);
            card.transform.SetParent(slot.transform);
            card.transform.localPosition = Vector3.zero;
            card.ParentLineUI = this;

            Cards.Add(card);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Outline)
                if (GameEngine.CardBeingDraged)
                    _outline.TurnPulsationOn();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Outline)
                _outline.TurnPulsationOff();
        }
    }
}

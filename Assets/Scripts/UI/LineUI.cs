using Assets.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [DisallowMultipleComponent]
    public class LineUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public CardUI this[int id]
        {
            get => Cards[id];
            set => Cards[id] = value;
        }

        public int Count => Cards.Count;

        public bool Outline = false;
        public Line LineIndicator;
        public PlayerIndicator PlayerIndicator;

        OutlineController _outline;

        public List<CardUI> Cards = new List<CardUI>();

        GameObject targetSlotIndicator;

        GridLayoutGroup _gridLayoutGroup;

        bool isMouseOver;

        void Awake()
        {
            if(Outline)
                _outline = GetComponent<OutlineController>();

            _gridLayoutGroup = GetComponent<GridLayoutGroup>();
        }

        public int TargetSlotPositionNumber; 

        void Update()
        {
            if (isMouseOver 
                && MainUIController.CardBeingDraged 
                && MainUIController.CardBeingDraged.ParentLineUI != this
                && targetSlotIndicator)
            {
                TargetSlotPositionNumber = GetTargetSlotPositionNumber();
                targetSlotIndicator.transform.SetSiblingIndex(TargetSlotPositionNumber);
            }
        }

        #region Interfaces Implementation
        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;

            CardUI card = MainUIController.CardBeingDraged;
            if (card != null
                && card.ParentLineUI != this
                && card.PlayerIndicator == PlayerIndicator)
            {
                _outline.TurnPulsationOn();

                // create empty
                targetSlotIndicator = Instantiate(MainUIController.Instance.TargetSlotIndicatorPrefab, transform);

                RecalculateSpacing();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;

            DestroyTargetSlotIndicator();

            if (Outline)
                _outline.TurnPulsationOff();
        }
        #endregion

        int GetTargetSlotPositionNumber()
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Vector3 cardPos = Cards[i].transform.parent.localPosition;
                if (MainUIController.Instance.SecondaryCanvas.ScreenToCanvasPosition(Input.mousePosition).x < cardPos.x)
                    return i;
            }

            return Cards.Count;
        }

        /// <summary>
        /// This removes the slot container game object.
        /// </summary>
        public void RemoveFromLine(int slotNumber)
        {
            Transform child = transform.GetChild(slotNumber);
            child.SetParent(MainUIController.Instance.ObjectDump);
            Destroy(child.gameObject);

            // all cards on the right must have their NumberInLine reduced by one
            var cardsOnTheRight = Cards.TakeLast(Cards.Count - slotNumber - 1).ToList();
            cardsOnTheRight.ForEach(c => c.SlotNumber--);

            Cards.RemoveAt(slotNumber);

            RecalculateSpacing();
        }

        /// <summary>
        /// Creates a new free spot and adds card to it.
        /// This method does not inform the game logic about anything.
        /// </summary>
        public void InsertCard(CardUI card)
        {
            card.SlotNumber = Cards.Count;

            GameObject slot = Instantiate(MainUIController.Instance.CardContainerPrefab, transform);
            card.transform.SetParent(slot.transform);
            card.transform.localPosition = Vector3.zero;
            card.ParentLineUI = this;
            card.transform.localScale = new Vector3(1, 1, 1);

            Cards.Add(card);

            RecalculateSpacing();
        }

        /// <summary>
        /// Creates a new free spot and adds card to it.
        /// This method does not inform the game logic about anything.
        /// </summary>
        public void InsertCard(CardUI card, int slotNumber)
        {
            GameObject slot = Instantiate(MainUIController.Instance.CardContainerPrefab, transform);
            slot.transform.SetSiblingIndex(slotNumber);

            card.transform.SetParent(slot.transform);
            card.transform.localPosition = Vector3.zero;
            card.ParentLineUI = this;
            card.SlotNumber = Cards.Count;

            if (slotNumber == Cards.Count)
                Cards.Add(card);
            else
                Cards.Insert(slotNumber, card);

            RecalculateSpacing();
        }

        public void DestroyTargetSlotIndicator()
        {
            if (targetSlotIndicator == null)
                return;

            // remove from the horizontal group and move out of the map
            Transform dump = MainUIController.Instance.ObjectDump.transform;
            targetSlotIndicator.transform.SetParent(dump);
            targetSlotIndicator.transform.position = dump.position;

            Destroy(targetSlotIndicator);
            targetSlotIndicator = null;

            RecalculateSpacing();
        }

        void RecalculateSpacing() 
            => _gridLayoutGroup.spacing = new Vector2((GameLogic.MAX_NUMBER_OF_CARDS_IN_LINE - Cards.Count) * 3, 0);
    }
}

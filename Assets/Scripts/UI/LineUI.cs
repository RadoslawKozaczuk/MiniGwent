using Assets.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class LineUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public CardUI this[int id]
        {
            get => Cards[id];
            set => Cards[id] = value;
        }

        public int Count => Cards.Count;

        public bool Outline = false;
        [HideInInspector] public Line LineIndicator;

        OutlineController _outline;

        public List<CardUI> Cards = new List<CardUI>();

        GameObject targetSlotIndicator;

        bool isMouseOver;

        void Awake()
        {
            if(Outline)
                _outline = GetComponent<OutlineController>();
        }

        public int TargetSlotPositionNumber; 

        void Update()
        {
            if (isMouseOver 
                && GameEngine.CardBeingDraged 
                && GameEngine.CardBeingDraged.ParentLineUI != this
                && targetSlotIndicator)
            {
                TargetSlotPositionNumber = GetTargetSlotPositionNumber();
                targetSlotIndicator.transform.SetSiblingIndex(TargetSlotPositionNumber);
            }
        }

        int GetTargetSlotPositionNumber()
        {
            var mousePos = Input.mousePosition;
            for (int i = 0; i < Cards.Count; i++)
            {
                Vector3 cardPos = Camera.main.transform.InverseTransformPoint(Cards[i].transform.position);
                if (mousePos.x < cardPos.x)
                    return i;
            }

            return Cards.Count;
        }

        /// <summary>
        /// This removes the slot container game object.
        /// </summary>
        public void RemoveFromLine(int slotNumber, bool informInternalLogic)
        {
            Transform child = transform.GetChild(slotNumber);
            child.SetParent(GameEngine.Instance.ObjectDump);
            Destroy(child.gameObject);

            // all cards on the right must have their NumberInLine reduced by one
            var cardsOnTheRight = Cards.TakeLast(Cards.Count - slotNumber - 1).ToList();
            cardsOnTheRight.ForEach(c => c.NumberInLine--);

            Cards.RemoveAt(slotNumber);

            // inform the game logic about it
            if(informInternalLogic)
                GameEngine.Instance.GameLogic.RemoveCardFromLine(LineIndicator, slotNumber);
        }

        /// <summary>
        /// Creates a new free spot and adds card to it.
        /// This method does not inform the game logic about anything.
        /// </summary>
        public void InsertCard(CardUI card)
        {
            card.NumberInLine = Cards.Count;

            GameObject slot = Instantiate(GameEngine.Instance.CardContainerPrefab, transform);
            card.transform.SetParent(slot.transform);
            card.transform.localPosition = Vector3.zero;
            card.ParentLineUI = this;

            Cards.Add(card);
        }

        /// <summary>
        /// Creates a new free spot and adds card to it.
        /// This method does not inform the game logic about anything.
        /// </summary>
        public void InsertCard(CardUI card, int slotNumber)
        {
            GameObject slot = Instantiate(GameEngine.Instance.CardContainerPrefab, transform);
            slot.transform.SetSiblingIndex(slotNumber);

            card.transform.SetParent(slot.transform);
            card.transform.localPosition = Vector3.zero;
            card.ParentLineUI = this;
            card.NumberInLine = Cards.Count;

            if (slotNumber == Cards.Count)
                Cards.Add(card);
            else
                Cards.Insert(slotNumber, card);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;

            // dragujesz karte z innej lini na tą
            if (GameEngine.CardBeingDraged && GameEngine.CardBeingDraged.ParentLineUI != this)
            {
                _outline.TurnPulsationOn();

                // (pozniej) is valid target
                // podswietla ci gdzie karta wyladuje 
                // czyli musi ja zinstacjonowac i wlaczyc jej podswietlenie

                // stworz empty
                targetSlotIndicator = Instantiate(GameEngine.Instance.TargetSlotIndicatorPrefab, transform);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;

            DestroyTargetSlotIndicator();

            if (Outline)
                _outline.TurnPulsationOff();
        }

        public void DestroyTargetSlotIndicator()
        {
            if (targetSlotIndicator == null)
                return;

            // remove from the horizontal group and move out of the map
            Transform dump = GameEngine.Instance.ObjectDump.transform;
            targetSlotIndicator.transform.SetParent(dump);
            targetSlotIndicator.transform.position = dump.position;

            Destroy(targetSlotIndicator);
            targetSlotIndicator = null;
        }
    }
}

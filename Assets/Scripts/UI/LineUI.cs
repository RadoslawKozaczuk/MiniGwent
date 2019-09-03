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

        public List<CardUI> Cards = new List<CardUI>();
        public LineIndicator LineIndicator;
        public PlayerIndicator PlayerIndicator;
        public int TargetSlotPositionNumber;

        [SerializeField] OutlineController _outline;
        GameObject _targetSlotIndicator;
        GridLayoutGroup _gridLayoutGroup;
        bool _isMouseOver;

        #region Unity life-cycle methods
        void Awake()
        {
            _gridLayoutGroup = GetComponent<GridLayoutGroup>();
        }

        void Update()
        {
            if (_isMouseOver 
                && MainUIController.CardBeingDraged 
                && MainUIController.CardBeingDraged.ParentLineUI != this
                && _targetSlotIndicator)
            {
                TargetSlotPositionNumber = GetTargetSlotPositionNumber();
                _targetSlotIndicator.transform.SetSiblingIndex(TargetSlotPositionNumber);
            }
        }
        #endregion

        #region Interfaces Implementation
        public void OnPointerEnter(PointerEventData eventData)
        {
            MainUIController.MouseHoveringOverPopulatedAllyLine = PlayerIndicator == PlayerIndicator.Bot;
            MainUIController.MouseHoveringOverPopulatedEnemyLine = PlayerIndicator == PlayerIndicator.Top 
                && LineIndicator != LineIndicator.TopDeck
                && Cards.Count > 0;

            MainUIController.LineMouseOver = this;
            _isMouseOver = true;

            CardUI card = MainUIController.CardBeingDraged;
            if (card != null && card.ParentLineUI != this && card.PlayerIndicator == PlayerIndicator) // on drag
            {
                _outline.TurnPulsationOn();

                // create empty
                _targetSlotIndicator = Instantiate(MainUIController.Instance.TargetSlotIndicatorPrefab, transform);

                RecalculateSpacing();
            }
            else if((MainUIController.AllyLineSelectMode && MainUIController.MouseHoveringOverPopulatedAllyLine)
                || (MainUIController.EnemyLineSelectMode && MainUIController.MouseHoveringOverPopulatedEnemyLine))
                _outline.TurnPulsationOn(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MainUIController.MouseHoveringOverPopulatedAllyLine = false;
            MainUIController.MouseHoveringOverPopulatedEnemyLine = false;

            MainUIController.LineMouseOver = null;
            _isMouseOver = false;

            DestroyTargetSlotIndicator();

            _outline.TurnPulsationOff();
        }
        #endregion

        /// <summary>
        /// This removes the slot container game object.
        /// </summary>
        public void RemoveFromLine(int slotNumber)
        {
            Transform child = transform.GetChild(slotNumber);
            child.SetParent(MainUIController.Instance.ObjectDump);
            Destroy(child.gameObject);

            // all cards on the right must have their SlotNumber reduced by one
            var cardsOnTheRight = Cards.GetLast(Cards.Count - slotNumber - 1).ToList();
            cardsOnTheRight.ForEach(c => c.SlotNumber--);

            Cards.RemoveAt(slotNumber);

            RecalculateSpacing();
        }

        /// <summary>
        /// Creates a new free spot and adds card to it.
        /// This method does not inform the game logic about anything.
        /// </summary>
        public void AddCard(CardUI card)
        {
            GameObject slot = Instantiate(MainUIController.Instance.CardContainerPrefab, transform);
            card.transform.SetParent(slot.transform);
            card.transform.localPosition = Vector3.zero;
            card.ParentLineUI = this;
            card.transform.localScale = new Vector3(1, 1, 1);
            card.SlotNumber = Cards.Count;

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
            card.SlotNumber = slotNumber;

            if (Cards.Count == 0)
                Cards.Add(card);
            else
                Cards.Insert(slotNumber, card);

            Cards.AllOnTheRight(slotNumber, c => c.SlotNumber++);

            RecalculateSpacing();
        }

        public void DestroyTargetSlotIndicator()
        {
            if (_targetSlotIndicator == null)
                return;

            // remove from the horizontal group and move out of the map
            Transform dump = MainUIController.Instance.ObjectDump.transform;
            _targetSlotIndicator.transform.SetParent(dump);
            _targetSlotIndicator.transform.position = dump.position;

            Destroy(_targetSlotIndicator);
            _targetSlotIndicator = null;

            RecalculateSpacing();
        }

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

        void RecalculateSpacing() 
            => _gridLayoutGroup.spacing = new Vector2((GameLogic.MAX_NUMBER_OF_CARDS_IN_LINE - Cards.Count) * 3, 0);
    }
}

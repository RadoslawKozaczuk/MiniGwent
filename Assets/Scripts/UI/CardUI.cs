﻿using Assets.Core;
using Assets.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region Properties
        bool _hidden;
        /// <summary>
        /// When set to true card back is shown.
        /// </summary>
        public bool Hidden
        {
            get => _hidden;
            set
            {
                _front.SetActive(!value);
                _back.SetActive(value);
                _hidden = value;
            }
        }

        int _currentStrength;
        public int CurrentStrength
        {
            get => _currentStrength;
            set
            {
                _currentStrength = value;
                if (CurrentStrength < DefaultStrength)
                    _stengthText.text = $"STR: <color=red>{CurrentStrength}</color>";
                else if (CurrentStrength == DefaultStrength)
                    _stengthText.text = $"STR: {CurrentStrength}";
                else
                    _stengthText.text = $"STR: <color=green>{CurrentStrength}</color>";
            }
        }
        #endregion

        public Image Image;
        public int Id;
        public int DefaultStrength;
        
        /// <summary>
        /// Indicates whether that card should be able to be dragged.
        /// </summary>
        public bool Draggable;
        public LineUI ParentLineUI;

        // this corresponds both to the sibling index int the hierarchy as well as the index in the table in GameLogic
        public int SlotNumber;
        public PlayerIndicator PlayerIndicator;
        public TextMeshProUGUI TitleText;

        [SerializeField] GameObject _front;
        [SerializeField] GameObject _back;
        [SerializeField] TextMeshProUGUI _stengthText;
        [SerializeField] OutlineController _outlineController;
        Transform _preDragLocation;

        #region Interface Implementation
        public void OnPointerEnter(PointerEventData eventData)
        {
            MainUIController.MouseHoveringOverPopulatedAllyLine = PlayerIndicator == PlayerIndicator.Bot
                && ParentLineUI.LineIndicator != LineIndicator.BotDeck;
            MainUIController.MouseHoveringOverAllyUnit = PlayerIndicator == PlayerIndicator.Bot;
            MainUIController.MouseHoveringOverPopulatedEnemyLine 
                = PlayerIndicator == PlayerIndicator.Top && ParentLineUI.LineIndicator != LineIndicator.TopDeck;
            MainUIController.MouseHoveringOverEnemyUnit
                = PlayerIndicator == PlayerIndicator.Top && ParentLineUI.LineIndicator != LineIndicator.TopDeck;

            MainUIController.CardMouseOver = this;

            if (!MainUIController.CardBeingDraged && !Hidden)
            {
                _outlineController.TurnPulsationOn(MainUIController.SingleTargetSelectMode && MainUIController.MouseHoveringOverEnemyUnit);
                MainUIController.Instance.CardInfoPanel.gameObject.SetActive(true);
                MainUIController.Instance.CardInfoPanel.SetInfoForCard(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MainUIController.MouseHoveringOverPopulatedAllyLine = false;
            MainUIController.MouseHoveringOverAllyUnit = false;
            MainUIController.MouseHoveringOverPopulatedEnemyLine = false;
            MainUIController.MouseHoveringOverEnemyUnit = false;

            MainUIController.CardMouseOver = null;

            _outlineController.TurnPulsationOff();
            MainUIController.Instance.CardInfoPanel.gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Draggable || MainUIController.BlockDragAction || MainUIController.SingleTargetSelectMode)
                return;

            _preDragLocation = transform.parent;
            
            // move to secondary canvas in order to be displayed always on top
            transform.SetParent(MainUIController.Instance.SecondaryCanvas.transform, true);

            MainUIController.CardBeingDraged = this;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Draggable || MainUIController.BlockDragAction || MainUIController.SingleTargetSelectMode)
                return;

            transform.localPosition = MainUIController.Instance.SecondaryCanvas.ScreenToCanvasPosition(Input.mousePosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            MainUIController.CardBeingDraged = null;

            if (!Draggable || MainUIController.BlockDragAction || MainUIController.SingleTargetSelectMode)
                return;

            LineUI targetLine = eventData.pointerCurrentRaycast.gameObject.GetComponent<LineUI>();
            if (targetLine 
                && targetLine != ParentLineUI // not dropped on the same line
                && PlayerIndicator == targetLine.PlayerIndicator) 
            {
                MainUIController.Instance.HandleInterfaceMoveCardRequest(
                    ParentLineUI.LineIndicator, 
                    SlotNumber, 
                    targetLine.LineIndicator, 
                    targetLine.TargetSlotPositionNumber);

                // every card once moved becomes non drag-able anymore
                Draggable = false;

                targetLine.DestroyTargetSlotIndicator();
            }
            else // dropped somewhere else or on the same line
            {
                transform.SetParent(_preDragLocation);
                transform.localPosition = Vector3.zero; // go back where you were
            }
        }
        #endregion
    }
}

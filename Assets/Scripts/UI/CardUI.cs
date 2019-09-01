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
                UpdateStrengthText();
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

        [SerializeField] GameObject _front;
        [SerializeField] GameObject _back;
        public TextMeshProUGUI TitleText;
        [SerializeField] TextMeshProUGUI _stengthText;
        [SerializeField] OutlineController _outlineController;

        // predrag stuff
        Transform _preDragLocation;
        public LineUI ParentLineUI;
        public Canvas mainCanvas;
        public Canvas secondaryCanvas;

        // this corresponds both to the sibling index int the hierarchy as well as the index in the table in GameLogic
        public int SlotNumber;
        public PlayerIndicator PlayerIndicator;

        #region Interface Implementation
        public void OnPointerEnter(PointerEventData eventData)
        {
            // if nothing is being dragged
            if(!MainUIController.CardBeingDraged && !_hidden)
            {
                _outlineController.TurnPulsationOn();
                MainUIController.Instance.CardInfoPanel.gameObject.SetActive(true);
                MainUIController.Instance.CardInfoPanel.LoadDataForId(Id);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outlineController.TurnPulsationOff();
            MainUIController.Instance.CardInfoPanel.gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Draggable || MainUIController.BlockDragAction)
                return;

            _preDragLocation = transform.parent;
            
            // move to secondary canvas in order to be displayed always on top
            transform.SetParent(secondaryCanvas.transform, true);

            MainUIController.CardBeingDraged = this;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Draggable || MainUIController.BlockDragAction)
                return;

            transform.localPosition = secondaryCanvas.ScreenToCanvasPosition(Input.mousePosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Draggable || MainUIController.BlockDragAction)
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

            MainUIController.CardBeingDraged = null;
        }
        #endregion

        void UpdateStrengthText()
        {
            if (CurrentStrength < DefaultStrength)
                _stengthText.text = $"STR: <color=red>{CurrentStrength}</color>";
            else if (CurrentStrength == DefaultStrength)
                _stengthText.text = $"STR: {CurrentStrength}";
            else
                _stengthText.text = $"STR: <color=red>{CurrentStrength}</color>";
        }
    }
}
